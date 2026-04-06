using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Monthly_Excel.Scripts;
using Monthly_Excel.Utils;

namespace Monthly_Excel.Processors
{
    public class BlogCleanerProcessor : IDisposable
    {
        private static readonly HttpClient SharedHttpClient = CreateHttpClient();
        private readonly WebView2 _webView;
        private readonly Action<string> _setStatus;
        private string? _sessionFolderPath;

        private bool _initialized;
        private bool _isPageNavigationCompleted;
        private bool _navigationHandlerAttached;

        public BlogCleanerProcessor(WebView2 webView, Action<string> setStatus)
        {
            _webView = webView;
            _setStatus = setStatus;
        }

        public async Task InitializeAsync()
        {
            if (_initialized && _webView.CoreWebView2 != null)
                return;

            try
            {
                _setStatus("브라우저 초기화 중...");

                if (_webView.CoreWebView2 == null)
                {
                    WebView2TempManager.CleanupOldSessionFolders(TimeSpan.FromHours(12));
                    WebView2TempManager.CleanupAllExcept(_sessionFolderPath);
                    _sessionFolderPath ??= WebView2TempManager.CreateSessionFolder();
                    var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: _sessionFolderPath);
                    try
                    {
                        await _webView.EnsureCoreWebView2Async(environment);
                    }
                    catch (InvalidOperationException ex) when (
                        ex.Message.Contains("already initialized with a different CoreWebView2Environment", StringComparison.OrdinalIgnoreCase))
                    {
                        await _webView.EnsureCoreWebView2Async();
                    }
                }

                if (_webView.CoreWebView2 != null)
                {
                    if (!_navigationHandlerAttached)
                    {
                        _webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                        _navigationHandlerAttached = true;
                    }

                    _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                    _webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
                    _webView.CoreWebView2.Settings.IsZoomControlEnabled = true;
                }
                else
                {
                    _setStatus("브라우저 엔진 초기화에 실패했습니다.");
                    return;
                }

                _initialized = true;
                _setStatus("브라우저 초기화 완료");

                // Avoid blank initial screen: navigate after WebView2 is initialized.
                if (_webView.CoreWebView2 != null &&
                    string.Equals(_webView.CoreWebView2.Source, "about:blank", StringComparison.OrdinalIgnoreCase))
                {
                    _isPageNavigationCompleted = false;
                    _webView.CoreWebView2.Navigate("https://blog.naver.com");
                    _setStatus("기본 블로그 페이지 여는 중...");
                }
            }
            catch (Exception ex)
            {
                _setStatus($"초기화 실패: {ex.Message}");
            }
        }

        public async Task OpenAsync(string url)
        {
            if (_webView.CoreWebView2 == null)
                await InitializeAsync();

            if (_webView.CoreWebView2 == null)
            {
                _setStatus("브라우저가 아직 준비되지 않았습니다.");
                return;
            }

            try
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    _setStatus("올바른 URL 형식이 아닙니다.");
                    return;
                }

                await ClearNavigationCacheAsync();
                _isPageNavigationCompleted = false;
                _webView.Source = uri;
                _setStatus("페이지 여는 중...");
            }
            catch (Exception ex)
            {
                _setStatus($"페이지 열기 실패: {ex.Message}");
            }
        }

        public async Task CleanAsync()
        {
            if (_webView.CoreWebView2 == null)
            {
                await InitializeAsync();
            }

            if (_webView.CoreWebView2 == null)
            {
                _setStatus("브라우저가 아직 준비되지 않았습니다.");
                return;
            }

            try
            {
                var waitTime = _isPageNavigationCompleted ? TimeSpan.FromSeconds(5) : TimeSpan.FromSeconds(8);
                if (!await WaitForContentReadyAsync(waitTime))
                {
                    _setStatus("본문 영역이 아직 로드되지 않았습니다. 페이지가 완전히 뜬 뒤 다시 시도하세요.");
                    return;
                }

                _setStatus("HTML 정리 중...");

                string cleanScript = BlogCleanerScriptProvider.GetCleanScript();
                await _webView.CoreWebView2.ExecuteScriptAsync(cleanScript);

                string rightClickScript = BlogCleanerScriptProvider.GetEnableRightClickScript();
                await _webView.CoreWebView2.ExecuteScriptAsync(rightClickScript);

                _setStatus("정리 완료");
            }
            catch (Exception ex)
            {
                _setStatus($"정리 실패: {ex.Message}");
            }
        }

        public async Task RefreshAsync()
        {
            if (_webView.CoreWebView2 == null)
            {
                await InitializeAsync();
            }

            if (_webView.CoreWebView2 == null)
            {
                _setStatus("브라우저가 아직 준비되지 않았습니다.");
                return;
            }

            try
            {
                _setStatus("새로고침 중...");
                _isPageNavigationCompleted = false;
                _webView.CoreWebView2.Reload();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _setStatus($"새로고침 실패: {ex.Message}");
            }
        }

        public async Task SaveImagesAsync(string downloadPath)
        {
            if (_webView.CoreWebView2 == null)
            {
                await InitializeAsync();
            }

            if (_webView.CoreWebView2 == null)
            {
                _setStatus("브라우저가 아직 준비되지 않았습니다.");
                return;
            }

            try
            {
                _setStatus("이미지 수집 중...");

                string collectScript = BlogCleanerScriptProvider.GetCollectImageInfosScript();
                string result = "";

                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    result = await _webView.CoreWebView2.ExecuteScriptAsync(collectScript);

                    if (!string.IsNullOrEmpty(result) && result != "[]" && result != "\"[]\"")
                        break;

                    if (attempt < 3)
                    {
                        _setStatus($"이미지 확인 중... ({attempt}/3)");
                        await Task.Delay(800);
                    }
                }

                if (string.IsNullOrEmpty(result))
                {
                    _setStatus("이미지 수집 실패");
                    return;
                }

                // ExecuteScriptAsync는 JSON 문자열로 반환됨
                // 예: "[]" 또는 "[{...}]" 형태
                // 한번 더 deserialize해서 따옴표 제거
                string jsonResult = result;
                try
                {
                    jsonResult = JsonSerializer.Deserialize<string>(result) ?? result;
                }
                catch
                {
                    // 이미 JSON 형태일 수 있음
                }

                if (jsonResult == "[]" || jsonResult == "" || jsonResult == "\"[]\"")
                {
                    _setStatus("이미지를 찾을 수 없습니다. 본문 iframe 또는 실제 이미지 요소가 없는 페이지일 수 있습니다.");
                    return;
                }

                var images = JsonSerializer.Deserialize<List<ImageInfo>>(jsonResult);

                if (images == null || images.Count == 0)
                {
                    _setStatus("이미지를 찾을 수 없습니다. 다른 블로그를 시도해보세요.");
                    return;
                }

                _setStatus($"이미지 {images.Count}개 찾음, 다운로드 시작...");

                Directory.CreateDirectory(downloadPath);

                int downloaded = 0;
                foreach (var image in images)
                {
                    try
                    {
                        if (string.IsNullOrEmpty(image.src))
                            continue;

                        string fileName = $"{image.idx:D4}_{SanitizeFileName(image.alt)}.jpg";
                        string filePath = Path.Combine(downloadPath, fileName);

                        _setStatus($"다운로드 중... ({downloaded + 1}/{images.Count})");

                        using (var response = await SharedHttpClient.GetAsync(image.src))
                        {
                            if (response.IsSuccessStatusCode)
                            {
                                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                                {
                                    await response.Content.CopyToAsync(fs);
                                    downloaded++;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // 개별 이미지 실패는 계속 진행
                        continue;
                    }
                }

                _setStatus($"다운로드 완료! {downloaded}개 이미지가 {downloadPath}에 저장되었습니다.");
            }
            catch (Exception ex)
            {
                _setStatus($"이미지 다운로드 실패: {ex.Message}");
            }
        }

        private string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                fileName = fileName.Replace(c, '_');
            }

            if (fileName.Length > 50)
                fileName = fileName.Substring(0, 50);

            return fileName;
        }

        private async Task ClearNavigationCacheAsync()
        {
            if (_webView.CoreWebView2?.Profile == null)
            {
                return;
            }

            try
            {
                _setStatus("이전 캐시 정리 중...");

                var kinds =
                    CoreWebView2BrowsingDataKinds.DiskCache |
                    CoreWebView2BrowsingDataKinds.BrowsingHistory |
                    CoreWebView2BrowsingDataKinds.AllDomStorage;

                await _webView.CoreWebView2.Profile.ClearBrowsingDataAsync(kinds);
            }
            catch (Exception ex)
            {
                _setStatus($"캐시 정리 실패: {ex.Message}");
            }
        }

        private class ImageInfo
        {
            public int idx { get; set; }
            public string src { get; set; } = string.Empty;
            public string alt { get; set; } = string.Empty;
            public int width { get; set; }
            public int height { get; set; }
        }

        public async Task EnableRightClickAsync()
        {
            if (_webView.CoreWebView2 == null)
                return;

            try
            {
                string script = BlogCleanerScriptProvider.GetEnableRightClickScript();
                await _webView.CoreWebView2.ExecuteScriptAsync(script);
                _setStatus("우클릭 허용 적용");
            }
            catch (Exception ex)
            {
                _setStatus($"우클릭 허용 실패: {ex.Message}");
            }
        }

        private async void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                _isPageNavigationCompleted = false;
                _setStatus("페이지 로드 실패");
                return;
            }

            _isPageNavigationCompleted = true;
            _setStatus("페이지 로드 완료");

            await EnableRightClickAsync();
        }

        private async Task<bool> WaitForContentReadyAsync(TimeSpan timeout)
        {
            if (_webView.CoreWebView2 == null)
            {
                return false;
            }

            var deadline = DateTime.UtcNow + timeout;

            while (DateTime.UtcNow < deadline)
            {
                if (await IsContentReadyAsync())
                {
                    return true;
                }

                await Task.Delay(250);
            }

            return false;
        }

        private async Task<bool> IsContentReadyAsync()
        {
            if (_webView.CoreWebView2 == null)
            {
                return false;
            }

            const string checkScript = """
(() => {
    try {
        const mainFrame = document.querySelector('#mainFrame');
        const doc = (mainFrame && mainFrame.contentDocument) ? mainFrame.contentDocument : document;
        const hasContainer =
            !!doc.querySelector('.se-main-container') ||
            !!doc.querySelector('.post-view') ||
            !!doc.querySelector('[id^="post-view"]');
        return hasContainer;
    } catch (_) {
        return false;
    }
})();
""";

            var result = await _webView.CoreWebView2.ExecuteScriptAsync(checkScript);
            return string.Equals(result, "true", StringComparison.OrdinalIgnoreCase);
        }

        public void Dispose()
        {
            if (_webView?.CoreWebView2 != null && _navigationHandlerAttached)
            {
                _webView.CoreWebView2.NavigationCompleted -= CoreWebView2_NavigationCompleted;
                _navigationHandlerAttached = false;
            }

            if (!string.IsNullOrWhiteSpace(_sessionFolderPath))
            {
                WebView2TempManager.TryDeleteDirectory(_sessionFolderPath);
            }
        }

        private static HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            return client;
        }
    }
}
