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

            _wait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromSeconds(30), TimeSpan.FromMilliseconds(200));
            _frameWait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromSeconds(20), TimeSpan.FromMilliseconds(200));
            _shortWait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(200));
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

                if (TryHandleDeletedPostAlert(result))
                {
                    return result;
                }

                _frameWait.Until(ExpectedConditions.FrameToBeAvailableAndSwitchToIt("cafe_main"));

                result.Title = GetTextOrDefault(By.CssSelector("h3.title_text"), "[제목 없음]");
                result.Views = ParseInt(
                    GetTextOrDefault(By.CssSelector(".article_info span:nth-child(2)"), "0")
                        .Replace("조회", string.Empty)
                        .Replace(",", string.Empty)
                        .Trim()
                );

                string rawDate = GetTextOrDefault(By.CssSelector(".article_info span:nth-child(1)"), string.Empty).Trim();
                string dateOnly = (rawDate.Split(' ').FirstOrDefault() ?? string.Empty)
                    .Replace(".", "-")
                    .Trim('-');

                if (TryParseDate(dateOnly, out DateTime writtenDate))
                {
                    result.WrittenDate = writtenDate;
                }

                result.Comments = await GetCommentCountAsync();
            }
            catch
            {
                result.Title = "[크롤링 실패]";
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
                    alertText.Contains("존재하지", StringComparison.OrdinalIgnoreCase))
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
                await Task.Delay(1500);

                string commentText = TryGetCommentText(".button_comment > strong.num");
                if (string.IsNullOrWhiteSpace(commentText))
                {
                    commentText = TryGetCommentText(".comment_area strong.num");
                }

                commentText = commentText.Replace(",", string.Empty).Trim();
                return int.TryParse(commentText, out int comments) ? comments : 0;
            }
            catch
            {
                return 0;
            }
        }

        private string TryGetCommentText(string selector)
        {
            try
            {
                return _driver.FindElement(By.CssSelector(selector)).Text?.Trim() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private string GetTextOrDefault(By by, string fallback)
        {
            try
            {
                var element = _wait.Until(ExpectedConditions.ElementIsVisible(by));
                var text = element?.Text?.Trim();
                return string.IsNullOrWhiteSpace(text) ? fallback : text;
            }
            catch
            {
                return fallback;
            }
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
