using System;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Monthly_Excel.Scripts;

namespace Monthly_Excel.Processors
{
    public class BlogCleanerProcessor : IDisposable
    {
        private readonly WebView2 _webView;
        private readonly Action<string> _setStatus;

        private bool _initialized;

        public BlogCleanerProcessor(WebView2 webView, Action<string> setStatus)
        {
            _webView = webView;
            _setStatus = setStatus;
        }

        public async Task InitializeAsync()
        {
            if (_initialized)
                return;

            try
            {
                _setStatus("브라우저 초기화 중...");

                await _webView.EnsureCoreWebView2Async();

                if (_webView.CoreWebView2 != null)
                {
                    _webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
                    _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true;
                    _webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
                    _webView.CoreWebView2.Settings.IsZoomControlEnabled = true;
                }

                _initialized = true;
                _setStatus("브라우저 초기화 완료");
            }
            catch (Exception ex)
            {
                _setStatus($"초기화 실패: {ex.Message}");
            }
        }

        public async Task OpenAsync(string url)
        {
            if (!_initialized)
                await InitializeAsync();

            try
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    _setStatus("올바른 URL 형식이 아닙니다.");
                    return;
                }

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
            if (!_initialized || _webView.CoreWebView2 == null)
            {
                _setStatus("브라우저가 아직 준비되지 않았습니다.");
                return;
            }

            try
            {
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
            if (!_initialized || _webView.CoreWebView2 == null)
            {
                _setStatus("브라우저가 아직 준비되지 않았습니다.");
                return;
            }

            try
            {
                _setStatus("새로고침 중...");
                _webView.CoreWebView2.Reload();
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _setStatus($"새로고침 실패: {ex.Message}");
            }
        }

        public async Task EnableRightClickAsync()
        {
            if (!_initialized || _webView.CoreWebView2 == null)
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
                _setStatus("페이지 로드 실패");
                return;
            }

            _setStatus("페이지 로드 완료");

            await EnableRightClickAsync();
        }

        public void Dispose()
        {
            if (_webView?.CoreWebView2 != null)
            {
                _webView.CoreWebView2.NavigationCompleted -= CoreWebView2_NavigationCompleted;
            }
        }
    }
}