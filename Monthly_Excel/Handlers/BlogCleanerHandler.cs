using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;
using Monthly_Excel.Processors;

namespace Monthly_Excel.Handlers
{
    public class BlogCleanerHandler : IDisposable
    {
        private readonly TextBox _urlTextBox;
        private readonly Label _statusLabel;
        private readonly WebView2 _webView;
        private readonly BlogCleanerProcessor _processor;

        public BlogCleanerHandler(
            TextBox urlTextBox,
            Label statusLabel,
            WebView2 webView)
        {
            _urlTextBox = urlTextBox;
            _statusLabel = statusLabel;
            _webView = webView;

            _processor = new BlogCleanerProcessor(
                _webView,
                SetStatus
            );
        }

        public async Task InitializeAsync()
        {
            await _processor.InitializeAsync();
            SetStatus("준비 완료");
        }

        public async Task OpenAsync()
        {
            var url = _urlTextBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(url))
            {
                SetStatus("URL을 입력하세요.");
                _urlTextBox.Focus();
                return;
            }

            await _processor.OpenAsync(url);
        }

        public async Task CleanAsync()
        {
            await _processor.CleanAsync();
        }

        public async Task RefreshAsync()
        {
            await _processor.RefreshAsync();
        }

        private void SetStatus(string message)
        {
            if (_statusLabel.InvokeRequired)
            {
                _statusLabel.Invoke(new Action(() => _statusLabel.Text = message));
                return;
            }

            _statusLabel.Text = message;
        }

        public void Dispose()
        {
            _processor.Dispose();
        }
    }
}