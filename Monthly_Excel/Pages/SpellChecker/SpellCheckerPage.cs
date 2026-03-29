using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Monthly_Excel.UI;

namespace Monthly_Excel.Pages.SpellChecker
{
    public class SpellCheckerPage : UserControl
    {
        private const string SpellCheckerUrl = "https://www.saramin.co.kr/zf_user/tools/character-counter";

        private readonly Label _statusLabel;
        private readonly Label _zoomLabel;
        private readonly NumericUpDown _zoomInput;
        private readonly Label _zoomUnitLabel;
        private readonly CheckBox _saveZoomCheckBox;
        private readonly WebView2 _spellCheckerWebView;
        private bool _isApplyingFocusView;
        private bool _suppressZoomEvent;

        public event EventHandler? ZoomPreferenceChanged;

        public SpellCheckerPage()
        {
            AppTheme.ApplyPage(this);

            var surface = AppTheme.CreateSurfacePanel();
            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = AppTheme.SurfaceBackground
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var topBar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 6)
            };
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380F));

            _statusLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0),
                Text = "맞춤법 도구 로딩 중..."
            };
            AppTheme.StyleStatusLabel(_statusLabel);

            var zoomPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                Margin = new Padding(0)
            };

            _saveZoomCheckBox = new CheckBox
            {
                AutoSize = true,
                Margin = new Padding(8, 10, 0, 0),
                Text = "저장"
            };
            _saveZoomCheckBox.CheckedChanged += SaveZoomCheckBox_CheckedChanged;

            _zoomUnitLabel = new Label
            {
                AutoSize = true,
                Margin = new Padding(4, 10, 8, 0),
                Text = "%"
            };
            AppTheme.StyleStatusLabel(_zoomUnitLabel);

            _zoomInput = new NumericUpDown
            {
                Width = 74,
                Minimum = 50,
                Maximum = 300,
                Increment = 5,
                DecimalPlaces = 0,
                Value = 100,
                Margin = new Padding(0, 6, 0, 0),
                TextAlign = HorizontalAlignment.Right
            };
            _zoomInput.ValueChanged += ZoomInput_ValueChanged;

            _zoomLabel = new Label
            {
                AutoSize = true,
                Margin = new Padding(0, 9, 8, 0),
                Text = "화면 비율"
            };
            AppTheme.StyleStatusLabel(_zoomLabel);

            zoomPanel.Controls.Add(_saveZoomCheckBox);
            zoomPanel.Controls.Add(_zoomUnitLabel);
            zoomPanel.Controls.Add(_zoomInput);
            zoomPanel.Controls.Add(_zoomLabel);

            _spellCheckerWebView = new WebView2
            {
                Dock = DockStyle.Fill,
                ZoomFactor = 1D,
                DefaultBackgroundColor = Color.White
            };

            topBar.Controls.Add(_statusLabel, 0, 0);
            topBar.Controls.Add(zoomPanel, 1, 0);
            layout.Controls.Add(topBar, 0, 0);
            layout.Controls.Add(_spellCheckerWebView, 0, 1);
            surface.Controls.Add(layout);
            Controls.Add(surface);

            Load += SpellCheckerPage_Load;
        }

        private async void SpellCheckerPage_Load(object? sender, EventArgs e)
        {
            await EnsureInitializedAsync();
        }

        public async Task InitializeAsync()
        {
            await EnsureInitializedAsync();
        }

        public (int ZoomPercent, bool SaveEnabled) GetZoomPreference()
        {
            return ((int)_zoomInput.Value, _saveZoomCheckBox.Checked);
        }

        public void ApplyZoomPreference(int zoomPercent, bool saveEnabled)
        {
            int safeZoom = Math.Clamp(zoomPercent, (int)_zoomInput.Minimum, (int)_zoomInput.Maximum);

            _suppressZoomEvent = true;
            try
            {
                _zoomInput.Value = safeZoom;
                _saveZoomCheckBox.Checked = saveEnabled;
            }
            finally
            {
                _suppressZoomEvent = false;
            }

            ApplyZoomFactor((int)_zoomInput.Value);
        }

        private async Task EnsureInitializedAsync()
        {
            if (_spellCheckerWebView.CoreWebView2 != null)
            {
                return;
            }

            try
            {
                await _spellCheckerWebView.EnsureCoreWebView2Async();
                var core = _spellCheckerWebView.CoreWebView2;
                if (core == null)
                {
                    _statusLabel.Text = "브라우저 엔진 준비 실패";
                    return;
                }

                core.Settings.AreDefaultContextMenusEnabled = true;
                core.Settings.AreDevToolsEnabled = true;
                core.Settings.IsZoomControlEnabled = true;
                core.NavigationCompleted += SpellCheckerWebView_NavigationCompleted;
                _spellCheckerWebView.Source = new Uri(SpellCheckerUrl, UriKind.Absolute);
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"초기화 실패: {ex.Message}";
            }
        }

        private async void SpellCheckerWebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                _statusLabel.Text = "페이지 로드 실패";
                return;
            }

            _statusLabel.Text = "맞춤법 영역 정리 중...";
            await ApplyFocusedSpellCheckerViewAsync();
        }

        private async Task ApplyFocusedSpellCheckerViewAsync()
        {
            if (_isApplyingFocusView || _spellCheckerWebView.CoreWebView2 == null)
            {
                return;
            }

            _isApplyingFocusView = true;

            try
            {
                var script = BuildFocusScript();
                string result = "target-not-found";

                for (int attempt = 0; attempt < 7; attempt++)
                {
                    result = await _spellCheckerWebView.CoreWebView2.ExecuteScriptAsync(script);
                    if (result.Contains("ok", StringComparison.OrdinalIgnoreCase))
                    {
                        _statusLabel.Text = "맞춤법 영역 표시 완료";
                        return;
                    }

                    await Task.Delay(300);
                }

                _statusLabel.Text = "영역 자동 정리 실패 (원본 페이지 표시)";
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"영역 적용 실패: {ex.Message}";
            }
            finally
            {
                _isApplyingFocusView = false;
            }
        }

        private void ZoomInput_ValueChanged(object? sender, EventArgs e)
        {
            ApplyZoomFactor((int)_zoomInput.Value);
            RaiseZoomPreferenceChanged();
        }

        private void SaveZoomCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            RaiseZoomPreferenceChanged();
        }

        private void ApplyZoomFactor(int zoomPercent)
        {
            var zoomFactor = Math.Clamp(zoomPercent / 100d, 0.5d, 3.0d);
            _spellCheckerWebView.ZoomFactor = zoomFactor;
        }

        private void RaiseZoomPreferenceChanged()
        {
            if (_suppressZoomEvent)
            {
                return;
            }

            ZoomPreferenceChanged?.Invoke(this, EventArgs.Empty);
        }

        private static string BuildFocusScript()
        {
            return """
(() => {
  const removeSelectors = [
    '.wrap_title_recruit.title_type2',
    '.banner_job_pass',
    '.banner_page_bottom',
    '.wrap_recommend_slide.type02.hot_slide',
    '.wrap_footer',
    'header',
    '#header',
    '.header',
    '.wrap_header'
  ];

  removeSelectors.forEach((selector) => {
    document.querySelectorAll(selector).forEach((el) => el.remove());
  });

  const content =
    document.querySelector('.content') ||
    document.querySelector('#content') ||
    document.querySelector('main');

  if (!content) {
    return 'target-not-found';
  }

  content.style.setProperty('margin-top', '0', 'important');
  content.style.setProperty('margin-left', '0', 'important');
  content.style.setProperty('margin-right', '0', 'important');
  content.style.setProperty('padding-top', '0', 'important');
  content.style.setProperty('padding-left', '0', 'important');
  content.style.setProperty('padding-right', '0', 'important');

  const style = document.createElement('style');
  style.id = 'codex-spellchecker-focus-style';
  style.textContent = `
    html, body {
      margin: 0 !important;
      padding: 0 !important;
      background: #f5f7fa !important;
      overflow-y: auto !important;
      overflow-x: auto !important;
    }
    * { box-sizing: border-box !important; }
  `;

  const oldStyle = document.getElementById('codex-spellchecker-focus-style');
  if (oldStyle) oldStyle.remove();
  document.head.appendChild(style);

  window.scrollTo({ top: 0, left: 0, behavior: 'auto' });

  return 'ok';
})();
""";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _spellCheckerWebView.CoreWebView2 != null)
            {
                _spellCheckerWebView.CoreWebView2.NavigationCompleted -= SpellCheckerWebView_NavigationCompleted;
            }

            base.Dispose(disposing);
        }
    }
}
