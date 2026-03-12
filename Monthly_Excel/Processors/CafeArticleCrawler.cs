using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Monthly_Excel.Models;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace Monthly_Excel.Processors
{
    internal sealed class CafeArticleCrawler : IDisposable
    {
        private static readonly string[] TitleSelectors =
        {
            "h3.title_text",
            ".article_header .title_text",
            ".ArticleContentBox .title_text"
        };

        private static readonly string[] MetaInfoSelectors =
        {
            ".article_info > span",
            ".ArticleContentBox .article_info > span"
        };

        private static readonly string[] CommentSelectors =
        {
            ".button_comment > strong.num",
            ".comment_area strong.num",
            ".CommentBox .comment_count",
            ".CommentItemCount"
        };

        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly WebDriverWait _frameWait;
        private readonly WebDriverWait _shortWait;

        public CafeArticleCrawler()
        {
            var service = ChromeDriverService.CreateDefaultService();
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
            options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            options.PageLoadStrategy = PageLoadStrategy.None;

            _driver = new ChromeDriver(service, options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;

            _wait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromSeconds(12), TimeSpan.FromMilliseconds(200));
            _frameWait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromSeconds(12), TimeSpan.FromMilliseconds(200));
            _shortWait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromSeconds(2), TimeSpan.FromMilliseconds(200));
        }

        public async Task<CrawlResult> CrawlAsync(string url, string? keyword, int columnIndex)
        {
            var result = new CrawlResult
            {
                Url = url,
                RawKeyword = keyword?.Trim() ?? string.Empty,
                ColumnIndex = columnIndex
            };

            try
            {
                _driver.Navigate().GoToUrl(url);
                WaitForDocumentReady();

                if (TryHandleDeletedPostAlert(result))
                {
                    return result;
                }

                _frameWait.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt("cafe_main"));
                WaitForArticleReady();

                result.Title = GetTextOrDefault("[제목 없음]", TitleSelectors);

                string[] metaTexts = GetMetaTexts();
                result.Views = ParseInt(ExtractViewsText(metaTexts));

                string rawDate = ExtractDateText(metaTexts);
                string dateOnly = (rawDate.Split(' ').FirstOrDefault() ?? string.Empty)
                    .Replace(".", "-")
                    .Trim('-');

                if (TryParseDate(dateOnly, out DateTime writtenDate))
                {
                    result.WrittenDate = writtenDate;
                }

                result.Comments = await GetCommentCountAsync();
            }
            catch (Exception exception)
            {
                result.Title = $"[크롤링 실패] {exception.GetType().Name}";
            }
            finally
            {
                TryResetFrame();
            }

            return result;
        }

        private bool TryHandleDeletedPostAlert(CrawlResult result)
        {
            try
            {
                var alert = _shortWait.Until(ExpectedConditions.AlertIsPresent());
                if (alert is not IAlert activeAlert)
                {
                    return false;
                }

                string alertText = activeAlert.Text ?? string.Empty;

                if (alertText.Contains("삭제", StringComparison.OrdinalIgnoreCase) ||
                    alertText.Contains("존재", StringComparison.OrdinalIgnoreCase))
                {
                    result.Title = "[삭제된 글]";
                    activeAlert.Accept();
                    return true;
                }

                activeAlert.Dismiss();
            }
            catch (WebDriverTimeoutException)
            {
            }

            return false;
        }

        private async Task<int> GetCommentCountAsync()
        {
            try
            {
                await Task.Delay(400);
                string commentText = GetTextOrDefault(string.Empty, CommentSelectors);
                commentText = commentText.Replace(",", string.Empty).Trim();
                return int.TryParse(commentText, out int comments) ? comments : 0;
            }
            catch
            {
                return 0;
            }
        }

        private void WaitForDocumentReady()
        {
            try
            {
                _shortWait.Until(driver =>
                {
                    if (driver is not IJavaScriptExecutor js)
                    {
                        return true;
                    }

                    string state = js.ExecuteScript("return document.readyState")?.ToString() ?? string.Empty;
                    return state.Equals("interactive", StringComparison.OrdinalIgnoreCase) ||
                           state.Equals("complete", StringComparison.OrdinalIgnoreCase);
                });
            }
            catch (WebDriverTimeoutException)
            {
            }
        }

        private void WaitForArticleReady()
        {
            try
            {
                _wait.Until(_ => TitleSelectors.Any(selector => FindVisibleElement(selector) != null));
            }
            catch (WebDriverTimeoutException)
            {
            }
        }

        private string GetTextOrDefault(string fallback, params string[] selectors)
        {
            foreach (string selector in selectors)
            {
                string text = FindVisibleElement(selector)?.Text?.Trim() ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            return fallback;
        }

        private string[] GetMetaTexts()
        {
            foreach (string selector in MetaInfoSelectors)
            {
                var texts = _driver.FindElements(By.CssSelector(selector))
                    .Select(element => element.Text?.Trim() ?? string.Empty)
                    .Where(text => !string.IsNullOrWhiteSpace(text))
                    .ToArray();

                if (texts.Length > 0)
                {
                    return texts;
                }
            }

            return Array.Empty<string>();
        }

        private IWebElement? FindVisibleElement(string selector)
        {
            return _driver.FindElements(By.CssSelector(selector))
                .FirstOrDefault(element => element.Displayed);
        }

        private static string ExtractViewsText(string[] metaTexts)
        {
            string candidate = metaTexts.FirstOrDefault(text => text.Contains("조회", StringComparison.OrdinalIgnoreCase))
                ?? metaTexts.Skip(1).FirstOrDefault()
                ?? string.Empty;

            return candidate
                .Replace("조회", string.Empty)
                .Replace(",", string.Empty)
                .Trim();
        }

        private static string ExtractDateText(string[] metaTexts)
        {
            return metaTexts.FirstOrDefault(text =>
                    text.Contains(".", StringComparison.Ordinal) ||
                    text.Contains("-", StringComparison.Ordinal))
                ?? metaTexts.FirstOrDefault()
                ?? string.Empty;
        }

        private void TryResetFrame()
        {
            try
            {
                _driver.SwitchTo().DefaultContent();
            }
            catch
            {
            }
        }

        private static bool TryParseDate(string input, out DateTime value)
        {
            var styles = DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces;

            if (DateTime.TryParseExact(input, new[] { "yyyy-M-d", "yyyy-MM-dd" }, CultureInfo.InvariantCulture, styles, out value))
            {
                return true;
            }

            return DateTime.TryParse(input, CultureInfo.CurrentCulture, styles, out value);
        }

        private static int ParseInt(string input) =>
            int.TryParse(input, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : 0;

        public void Dispose()
        {
            _driver.Dispose();
        }
    }
}
