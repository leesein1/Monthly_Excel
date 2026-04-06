using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Monthly_Excel.UI;

namespace Monthly_Excel.Pages.Inspector
{
    public class InspectorPage : UserControl
    {
        private const string CharacterCounterUrl = "https://www.saramin.co.kr/zf_user/tools/character-counter";
        private const string TranslatorUrl = "https://papago.naver.com/";

        private enum InspectorToolMode
        {
            CharacterCounter,
            Translator
        }

        private readonly Label _statusLabel;
        private readonly RadioButton _characterCounterRadioButton;
        private readonly RadioButton _translatorRadioButton;
        private readonly Label _zoomLabel;
        private readonly NumericUpDown _zoomInput;
        private readonly Label _zoomUnitLabel;
        private readonly CheckBox _saveZoomCheckBox;
        private readonly Panel _webViewHost;
        private readonly WebView2 _characterCounterWebView;
        private readonly WebView2 _translatorWebView;

        private bool _isApplyingCharacterCounterView;
        private bool _suppressZoomEvent;
        private bool _isInitialized;
        private bool _isCharacterCounterReady;
        private bool _isTranslatorReady;
        private InspectorToolMode _currentMode = InspectorToolMode.CharacterCounter;

        public event EventHandler? ZoomPreferenceChanged;

        public InspectorPage()
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
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 6)
            };
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 380F));

            var toolSelectorPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(0),
                Padding = new Padding(4, 7, 0, 0)
            };

            _characterCounterRadioButton = new RadioButton
            {
                AutoSize = true,
                Text = "글자수",
                Checked = true,
                Margin = new Padding(0, 0, 16, 0)
            };
            _characterCounterRadioButton.CheckedChanged += ToolRadioButton_CheckedChanged;

            _translatorRadioButton = new RadioButton
            {
                AutoSize = true,
                Text = "번역기",
                Margin = new Padding(0)
            };
            _translatorRadioButton.CheckedChanged += ToolRadioButton_CheckedChanged;

            toolSelectorPanel.Controls.Add(_characterCounterRadioButton);
            toolSelectorPanel.Controls.Add(_translatorRadioButton);

            _statusLabel = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0),
                Text = "검사기 준비 중..."
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

            _webViewHost = new Panel
            {
                Dock = DockStyle.Fill
            };

            // Preload both tools once and switch visibility instantly on radio changes.
            _characterCounterWebView = CreateWebView();
            _translatorWebView = CreateWebView();
            _translatorWebView.Visible = false;

            _webViewHost.Controls.Add(_translatorWebView);
            _webViewHost.Controls.Add(_characterCounterWebView);

            topBar.Controls.Add(toolSelectorPanel, 0, 0);
            topBar.Controls.Add(_statusLabel, 1, 0);
            topBar.Controls.Add(zoomPanel, 2, 0);
            layout.Controls.Add(topBar, 0, 0);
            layout.Controls.Add(_webViewHost, 0, 1);
            surface.Controls.Add(layout);
            Controls.Add(surface);

            Load += InspectorPage_Load;
        }

        private async void InspectorPage_Load(object? sender, EventArgs e)
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

        private WebView2 CreateWebView()
        {
            return new WebView2
            {
                Dock = DockStyle.Fill,
                ZoomFactor = 1D,
                DefaultBackgroundColor = Color.White
            };
        }

        private async Task EnsureInitializedAsync()
        {
            if (_isInitialized)
            {
                return;
            }

            _isInitialized = true;

            try
            {
                _statusLabel.Text = "검사기 로딩 중...";

                // Initialize both WebViews up front so tool switching does not trigger a fresh load.
                await InitializeCharacterCounterWebViewAsync();
                await InitializeTranslatorWebViewAsync();

                ApplyZoomFactor((int)_zoomInput.Value);
                UpdateVisibleTool();
            }
            catch (Exception ex)
            {
                _statusLabel.Text = $"초기화 실패: {ex.Message}";
            }
        }

        private async Task InitializeCharacterCounterWebViewAsync()
        {
            await _characterCounterWebView.EnsureCoreWebView2Async();
            var core = _characterCounterWebView.CoreWebView2;
            if (core == null)
            {
                throw new InvalidOperationException("글자수 브라우저 엔진 준비 실패");
            }

            ConfigureWebView(core);
            core.NavigationCompleted += CharacterCounterWebView_NavigationCompleted;
            core.Navigate(CharacterCounterUrl);
        }

        private async Task InitializeTranslatorWebViewAsync()
        {
            await _translatorWebView.EnsureCoreWebView2Async();
            var core = _translatorWebView.CoreWebView2;
            if (core == null)
            {
                throw new InvalidOperationException("번역기 브라우저 엔진 준비 실패");
            }

            ConfigureWebView(core);
            core.NavigationCompleted += TranslatorWebView_NavigationCompleted;
            core.Navigate(TranslatorUrl);
        }

        private static void ConfigureWebView(CoreWebView2 core)
        {
            core.Settings.AreDefaultContextMenusEnabled = true;
            core.Settings.AreDevToolsEnabled = true;
            core.Settings.IsZoomControlEnabled = true;
        }

        private async void CharacterCounterWebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                if (_currentMode == InspectorToolMode.CharacterCounter)
                {
                    _statusLabel.Text = "글자수 페이지 로드 실패";
                }

                return;
            }

            if (_currentMode == InspectorToolMode.CharacterCounter)
            {
                _statusLabel.Text = "글자수 영역 정리 중...";
            }

            await ApplyFocusedCharacterCounterViewAsync();
            _isCharacterCounterReady = true;

            if (_currentMode == InspectorToolMode.CharacterCounter)
            {
                _statusLabel.Text = "글자수 도구 준비 완료";
            }
        }

        private void TranslatorWebView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                if (_currentMode == InspectorToolMode.Translator)
                {
                    _statusLabel.Text = "Papago 번역기 로드 실패";
                }

                return;
            }

            _isTranslatorReady = true;

            if (_currentMode == InspectorToolMode.Translator)
            {
                _statusLabel.Text = "Papago 번역기 준비 완료";
            }
        }

        private async Task ApplyFocusedCharacterCounterViewAsync()
        {
            if (_isApplyingCharacterCounterView || _characterCounterWebView.CoreWebView2 == null)
            {
                return;
            }

            _isApplyingCharacterCounterView = true;

            try
            {
                var script = BuildFocusScript();
                string result = "target-not-found";

                for (int attempt = 0; attempt < 7; attempt++)
                {
                    result = await _characterCounterWebView.CoreWebView2.ExecuteScriptAsync(script);
                    if (result.Contains("ok", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    await Task.Delay(300);
                }

                if (_currentMode == InspectorToolMode.CharacterCounter)
                {
                    _statusLabel.Text = "글자수 화면 정리 실패 (원본 페이지 표시)";
                }
            }
            catch (Exception ex)
            {
                if (_currentMode == InspectorToolMode.CharacterCounter)
                {
                    _statusLabel.Text = $"글자수 화면 적용 실패: {ex.Message}";
                }
            }
            finally
            {
                _isApplyingCharacterCounterView = false;
            }
        }

        private void ToolRadioButton_CheckedChanged(object? sender, EventArgs e)
        {
            if (!_characterCounterRadioButton.Checked && !_translatorRadioButton.Checked)
            {
                return;
            }

            _currentMode = _characterCounterRadioButton.Checked
                ? InspectorToolMode.CharacterCounter
                : InspectorToolMode.Translator;

            UpdateVisibleTool();
        }

        private void UpdateVisibleTool()
        {
            // Keep the loaded pages alive and only swap the visible WebView.
            bool showCharacterCounter = _currentMode == InspectorToolMode.CharacterCounter;

            _characterCounterWebView.Visible = showCharacterCounter;
            _translatorWebView.Visible = !showCharacterCounter;

            if (!_isInitialized)
            {
                _statusLabel.Text = "검사기 준비 중...";
                return;
            }

            if (showCharacterCounter)
            {
                _characterCounterWebView.BringToFront();
                _statusLabel.Text = _isCharacterCounterReady
                    ? "글자수 도구 준비 완료"
                    : "글자수 도구 로딩 중...";
                return;
            }

            _translatorWebView.BringToFront();
            _statusLabel.Text = _isTranslatorReady
                ? "Papago 번역기 준비 완료"
                : "Papago 번역기 로딩 중...";
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
            _characterCounterWebView.ZoomFactor = zoomFactor;
            _translatorWebView.ZoomFactor = zoomFactor;
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
  style.id = 'codex-inspector-focus-style';
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

  const oldStyle = document.getElementById('codex-inspector-focus-style');
  if (oldStyle) oldStyle.remove();
  document.head.appendChild(style);

  window.scrollTo({ top: 0, left: 0, behavior: 'auto' });

  return 'ok';
})();
""";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_characterCounterWebView.CoreWebView2 != null)
                {
                    _characterCounterWebView.CoreWebView2.NavigationCompleted -= CharacterCounterWebView_NavigationCompleted;
                }

                if (_translatorWebView.CoreWebView2 != null)
                {
                    _translatorWebView.CoreWebView2.NavigationCompleted -= TranslatorWebView_NavigationCompleted;
                }
            }

            base.Dispose(disposing);
        }
    }
}
