using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Monthly_Excel; // KeywordProcessor.ProcessKeywords 사용 가정
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

public class CrawlResult
{
    public string Url { get; set; } = "";
    public string Title { get; set; } = "";
    public int Views { get; set; }
    public DateTime? WrittenDate { get; set; }
    public int Comments { get; set; }

    // 원래 엑셀 컬럼 인덱스(B=2, C=3…)
    public int Col { get; set; }

    // 키워드 원문(보조 소스)
    public string RawKeyword { get; set; } = "";
}

public static class CrawlingProcessor
{
    public static async Task SaveUrlsWithCrawlInfoToExcel(string savePath, List<string> urlList, List<string> keywordList)
    {
        if (urlList == null || urlList.Count == 0)
            throw new ArgumentException("urlList 비어있음");

        // ===== 1) 먼저 '원본 영역'을 한 번에 써두고(링크/키워드), 크롤링은 병렬로만 수행 =====
        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("크롤링결과");

        // 원본 헤더
        ws.Cell(4, 1).Value = "링크";
        ws.Cell(5, 1).Value = "키워드";
        ws.Cell(6, 1).Value = "글제목";
        ws.Cell(7, 1).Value = "조회수";
        ws.Cell(8, 1).Value = "작성일";
        ws.Cell(9, 1).Value = "댓글수";

        for (int i = 0; i < urlList.Count; i++)
        {
            int col = 2 + i;
            string url = urlList[i]?.Trim();
            string keyword = (keywordList != null && i < keywordList.Count) ? keywordList[i]?.Trim() : null;

            if (string.IsNullOrWhiteSpace(url)) continue;

            var linkCell = ws.Cell(4, col);
            linkCell.Value = url;
            linkCell.SetHyperlink(new XLHyperlink(url));
            linkCell.Style.Font.Underline = XLFontUnderlineValues.Single;
            linkCell.Style.Font.FontColor = XLColor.Blue;

            ws.Cell(5, col).Value = keyword;
        }

        // ===== 2) URL 인덱스들을 파티셔닝하여 병렬 크롤링 =====
        int workerCount = (urlList.Count < 3) ? 1 : 3;  // 요구사항: 3보다 작으면 1, 아니면 3
        var partitions = MakeContiguousPartitions(urlList.Count, workerCount); // 10 -> [0..2], [3..5], [6..9] (3,3,4)

        var resultsBag = new ConcurrentBag<CrawlResult>();

        var tasks = partitions.Select(part =>
            Task.Run(async () =>
            {
                // 워커마다 독립된 ChromeDriver
                using var service = ChromeDriverService.CreateDefaultService();
                service.HideCommandPromptWindow = true;

                var options = new ChromeOptions();
                options.AddArgument("--headless=new");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--disable-extensions");
                options.AddArgument("--disable-notifications");
                options.AddArgument("--mute-audio");
                options.AddArgument("--window-size=1280,900");
                options.AddArgument("--blink-settings=imagesEnabled=false");
                options.AddArgument("--disable-blink-features=AutomationControlled");
                options.PageLoadStrategy = PageLoadStrategy.None;
                options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

                using IWebDriver driver = new ChromeDriver(service, options);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;

                var wait = new WebDriverWait(new SystemClock(), driver, TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(200));
                var frameWait = new WebDriverWait(new SystemClock(), driver, TimeSpan.FromSeconds(20), TimeSpan.FromMilliseconds(200));
                var shortWait = new WebDriverWait(new SystemClock(), driver, TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(200));

                foreach (int i in part)
                {
                    int col = 2 + i;
                    string url = urlList[i]?.Trim();
                    string keyword = (keywordList != null && i < keywordList.Count) ? keywordList[i]?.Trim() : null;

                    if (string.IsNullOrWhiteSpace(url)) continue;

                    var result = new CrawlResult
                    {
                        Url = url,
                        Col = col,
                        RawKeyword = keyword ?? ""
                    };

                    try
                    {
                        driver.Navigate().GoToUrl(url);

                        // 삭제 알럿 감지
                        try
                        {
                            var alert = shortWait.Until(ExpectedConditions.AlertIsPresent());
                            string alertText = alert.Text;
                            if (alertText.Contains("삭제") || alertText.Contains("존재하지"))
                            {
                                result.Title = "[삭제된 글]";
                                alert.Accept();
                                resultsBag.Add(result);
                                continue;
                            }
                            alert.Dismiss();
                        }
                        catch (WebDriverTimeoutException)
                        {
                            // 알럿 없음 → 진행
                        }

                        // iframe 진입
                        frameWait.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt("cafe_main"));

                        // 제목
                        string title = GetTextOrDefault(driver, By.CssSelector("h3.title_text"), "[제목 없음]", wait);
                        result.Title = title;

                        // 조회수
                        int views = ParseInt(
                            GetTextOrDefault(driver, By.CssSelector(".article_info span:nth-child(2)"), "0", wait)
                                .Replace("조회", "").Replace(",", "").Trim()
                        );
                        result.Views = views;

                        // 작성일
                        string rawDate = GetTextOrDefault(driver, By.CssSelector(".article_info span:nth-child(1)"), "", wait).Trim();
                        string dateOnly = (rawDate.Split(' ').FirstOrDefault() ?? "").Replace(".", "-").Trim('-');

                        if (TryParseDate(dateOnly, out DateTime dt))
                        {
                            result.WrittenDate = dt;
                        }

                        // 댓글 수
                        int comments = 0;
                        try
                        {
                            await Task.Delay(1500);
                            string commentText = "";
                            try
                            {
                                var el = driver.FindElement(By.CssSelector(".button_comment > strong.num"));
                                commentText = el?.Text?.Trim();
                            }
                            catch { }

                            if (string.IsNullOrEmpty(commentText))
                            {
                                try
                                {
                                    var el2 = driver.FindElement(By.CssSelector(".comment_area strong.num"));
                                    commentText = el2?.Text?.Trim();
                                }
                                catch { }
                            }
                            if (!string.IsNullOrEmpty(commentText))
                            {
                                commentText = commentText.Replace(",", "").Trim();
                                comments = int.TryParse(commentText, out int c) ? c : 0;
                            }
                        }
                        catch
                        {
                            comments = 0;
                        }

                        result.Comments = comments;

                        Console.WriteLine($"✅ [worker:{Task.CurrentId}] idx {i + 1} 크롤링 성공");
                    }
                    catch (Exception ex)
                    {
                        result.Title = "[크롤링 실패]";
                        Console.WriteLine($"❌ [worker:{Task.CurrentId}] idx {i + 1} 에러: {ex.Message}");
                    }
                    finally
                    {
                        try { driver.SwitchTo().DefaultContent(); } catch { }
                        resultsBag.Add(result); // 수집만
                    }
                }
            })
        ).ToArray();

        await Task.WhenAll(tasks);

        // ===== 3) 크롤링 결과를 한 번에 엑셀에 반영 =====
        var results = resultsBag.ToList();

        // 원본 영역 채우기 (제목/조회수/작성일/댓글) — 병렬 끝난 뒤에 안전하게 씀
        foreach (var r in results)
        {
            if (r == null) continue;

            ws.Cell(6, r.Col).Value = r.Title;

            ws.Cell(7, r.Col).Value = r.Views;

            if (r.WrittenDate.HasValue)
            {
                ws.Cell(8, r.Col).Value = r.WrittenDate.Value;
                ws.Cell(8, r.Col).Style.DateFormat.Format = "MM\"월\"dd\"일\"";
            }
            else
            {
                ws.Cell(8, r.Col).Value = "[작성일 파싱 실패]";
            }

            ws.Cell(9, r.Col).Value = r.Comments;
        }

        // ===== 4) 조회수 정렬 요약 =====
        var ordered = results.OrderByDescending(r => r.Views).ToList();

        string[] headers = { "순위", "작성일", "글제목", "조회수", "댓글수", "키워드", "모바일노출", "링크" };
        for (int i = 0; i < headers.Length; i++)
            ws.Cell(11, i + 1).Value = headers[i];

        int row = 12;
        for (int i = 0; i < ordered.Count; i++)
        {
            var r = ordered[i];

            ws.Cell(row, 1).Value = i + 1;

            if (r.WrittenDate.HasValue)
            {
                ws.Cell(row, 2).Value = r.WrittenDate.Value;
                ws.Cell(row, 2).Style.DateFormat.Format = "MM\"월\"dd\"일\"";
            }

            ws.Cell(row, 3).Value = r.Title;
            ws.Cell(row, 4).Value = r.Views;
            ws.Cell(row, 5).Value = r.Comments;

            // 시트(B5/C5/…) 우선, 없으면 RawKeyword 보조
            string rawInput = ws.Cell(5, r.Col).GetString();
            if (string.IsNullOrWhiteSpace(rawInput))
                rawInput = r.RawKeyword;

            if (!string.IsNullOrWhiteSpace(rawInput))
            {
                var (leftList, rightList) = KeywordProcessor.ProcessKeywords(rawInput);
                ws.Cell(row, 6).Value = string.Join(Environment.NewLine, leftList);   // 키워드
                ws.Cell(row, 7).Value = string.Join(Environment.NewLine, rightList);  // 모바일노출
            }

            var linkCell2 = ws.Cell(row, 8);
            linkCell2.Value = r.Url;
            linkCell2.SetHyperlink(new XLHyperlink(r.Url));
            linkCell2.Style.Font.Underline = XLFontUnderlineValues.Single;
            linkCell2.Style.Font.FontColor = XLColor.Blue;

            row++;
        }

        ws.Columns().Width = 11.75;
        ws.Rows().Height = 20.25;

        workbook.SaveAs(savePath);
        Console.WriteLine("📁 저장 완료 → " + savePath);
    }

    public static void SaveCrawlingTemplate(string path)
    {
        try
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("크롤링결과");

            // 원본 헤더
            ws.Cell(4, 1).Value = "링크";
            ws.Cell(5, 1).Value = "키워드";
            ws.Cell(6, 1).Value = "글제목";
            ws.Cell(7, 1).Value = "조회수";
            ws.Cell(8, 1).Value = "작성일";
            ws.Cell(9, 1).Value = "댓글수";

            // 가이드 (선택)
            ws.Cell(1, 1).Value = "📝 사용 방법";
            ws.Cell(2, 1).Value = "B열부터 링크와 키워드를 입력하고 크롤링을 실행하세요.";
            ws.Range("A1:H2").Style.Font.FontColor = XLColor.FromHtml("#666666");

            // 정렬 결과 헤더
            string[] headers = { "순위", "작성일", "글제목", "조회수", "댓글수", "키워드", "모바일노출", "링크" };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(11, i + 1).Value = headers[i];

            ws.Range("A11:H11").Style.Font.Bold = true;
            ws.Range("A11:H11").Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");

            // 서식 통일
            ws.Columns().Width = 11.75;
            ws.Rows().Height = 20.25;
            ws.Range("A4:H9").Style.Font.Bold = true;

            // 저장
            wb.SaveAs(path);
        }
        catch (IOException ioEx)
        {
            throw new IOException($"파일 저장 중 오류 발생 (파일이 열려 있을 수 있음): {ioEx.Message}");
        }
        catch (Exception ex)
        {
            throw new Exception($"템플릿 저장 실패: {ex.Message}");
        }
    }

    // ===== 유틸 =====
    // 0..(n-1) 인덱스를 연속 구간으로 workerCount개 분할 (10,3) → [0..2],[3..5],[6..9] => 3,3,4
    private static List<List<int>> MakeContiguousPartitions(int n, int workerCount)
    {
        var parts = new List<List<int>>();
        if (n <= 0 || workerCount <= 0) return parts;

        int k = Math.Min(workerCount, n);
        int q = n / k;     // base size
        int r = n % k;     // first r partitions have +1

        int start = 0;
        for (int p = 0; p < k; p++)
        {
            int size = q + (p < r ? 1 : 0);
            var list = new List<int>(size);
            for (int i = 0; i < size; i++) list.Add(start + i);
            parts.Add(list);
            start += size;
        }
        return parts;
    }

    private static string GetTextOrDefault(IWebDriver driver, By by, string fallback, WebDriverWait wait)
    {
        try
        {
            var el = wait.Until(ExpectedConditions.ElementIsVisible(by));
            var text = el?.Text?.Trim();
            return string.IsNullOrEmpty(text) ? fallback : text!;
        }
        catch { return fallback; }
    }

    private static bool TryParseDate(string input, out DateTime dt)
    {
        var styles = DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces;
        if (DateTime.TryParseExact(input, new[] { "yyyy-M-d", "yyyy-MM-dd" }, CultureInfo.InvariantCulture, styles, out dt))
            return true;
        return DateTime.TryParse(input, CultureInfo.CurrentCulture, styles, out dt);
    }

    private static int ParseInt(string s) =>
        int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : 0;

}
