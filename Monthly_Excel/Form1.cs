using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Monthly_Excel.Controls;
using Monthly_Excel.Handlers;
using Monthly_Excel.Pages.BlogCleaner;
using Monthly_Excel.Pages.Crawling;
using Monthly_Excel.Pages.ImageConverter;
using Monthly_Excel.Pages.Keyword;
using Monthly_Excel.Pages.Settings;
using Monthly_Excel.Pages.SpellChecker;
using Monthly_Excel.UI;

namespace Monthly_Excel
{
    public partial class Form1 : Form
    {
        private readonly Size _defaultFormSize = new(532, 328);
        private readonly Size _settingsFormSize = new(562, 338);
        private readonly Size _imageConverterFormSize = new(1180, 760);
        private readonly Size _blogFormSize = new(1400, 900);
        private readonly Size _spellCheckerFormSize = new(1200, 860);
        private bool _isPreloading;
        private int _tabTransitionVersion;
        private readonly Panel _loadingOverlay;
        private readonly Label _loadingLabel;
        private readonly CircularLoaderControl _loadingSpinner;
        private readonly StatusStrip _statusStrip;
        private readonly ToolStripStatusLabel _versionStatusLabel;

        private readonly CrawlingPage _crawlingPage;
        private readonly KeywordPage _keywordPage;
        private readonly BlogCleanerPage _blogCleanerPage;
        private readonly ImageConverterPage _imageConverterPage;
        private readonly SpellCheckerPage _spellCheckerPage;
        private readonly SettingsPage _settingsPage;
        private readonly List<ManagedTab> _managedTabs = new();
        private static readonly JsonSerializerOptions TabSettingsJsonOptions = new() { WriteIndented = true };

        private readonly CrawlingEventHandler _crawlingHandler;
        private readonly KeywordEventHandler _keywordHandler;
        private readonly BlogCleanerHandler _blogCleanerHandler;

        public Form1()
        {
            InitializeComponent();
            ApplyTheme();
            EnableDoubleBuffer(this);
            EnableDoubleBuffer(tabControl);
            (_loadingOverlay, _loadingLabel, _loadingSpinner) = CreateLoadingOverlay();
            (_statusStrip, _versionStatusLabel) = CreateStatusStrip();

            (_crawlingPage, _keywordPage, _blogCleanerPage, _imageConverterPage, _spellCheckerPage, _settingsPage) = CreatePages();
            AttachPages();
            (_crawlingHandler, _keywordHandler, _blogCleanerHandler) = CreateHandlers();

            BindEvents();
            InitializeTabSettings();
        }

        private (CrawlingPage CrawlingPage, KeywordPage KeywordPage, BlogCleanerPage BlogCleanerPage, ImageConverterPage ImageConverterPage, SpellCheckerPage SpellCheckerPage, SettingsPage SettingsPage) CreatePages()
        {
            return (
                new CrawlingPage { Dock = DockStyle.Fill },
                new KeywordPage { Dock = DockStyle.Fill },
                new BlogCleanerPage { Dock = DockStyle.Fill },
                new ImageConverterPage { Dock = DockStyle.Fill },
                new SpellCheckerPage { Dock = DockStyle.Fill },
                new SettingsPage { Dock = DockStyle.Fill }
            );
        }

        private void AttachPages()
        {
            tabControl.SuspendLayout();
            tabPageCrawling.SuspendLayout();
            tabPageKeyword.SuspendLayout();
            tabPageBlogCleaner.SuspendLayout();
            tabPageImageConverter.SuspendLayout();
            tabPageSpellChecker.SuspendLayout();
            tabPageSettings.SuspendLayout();

            tabPageCrawling.Controls.Add(_crawlingPage);
            tabPageKeyword.Controls.Add(_keywordPage);
            tabPageBlogCleaner.Controls.Add(_blogCleanerPage);
            tabPageImageConverter.Controls.Add(_imageConverterPage);
            tabPageSpellChecker.Controls.Add(_spellCheckerPage);
            tabPageSettings.Controls.Add(_settingsPage);

            tabPageSettings.ResumeLayout();
            tabPageSpellChecker.ResumeLayout();
            tabPageImageConverter.ResumeLayout();
            tabPageBlogCleaner.ResumeLayout();
            tabPageKeyword.ResumeLayout();
            tabPageCrawling.ResumeLayout();
            tabControl.ResumeLayout();

            Controls.Add(_loadingOverlay);
            _loadingOverlay.BringToFront();
            Controls.Add(_statusStrip);
            _statusStrip.BringToFront();
        }

        private (CrawlingEventHandler CrawlingHandler, KeywordEventHandler KeywordHandler, BlogCleanerHandler BlogCleanerHandler) CreateHandlers()
        {
            return (
                new CrawlingEventHandler(
                    _crawlingPage.LabelStatus,
                    _crawlingPage.ProgressBar,
                    _crawlingPage.ButtonUpload,
                    _crawlingPage.ButtonDownload,
                    _crawlingPage.ButtonTemplateDownload),
                new KeywordEventHandler(_keywordPage.InputKeywordBox, _keywordPage.LeftListBox, _keywordPage.RightListBox),
                new BlogCleanerHandler(_blogCleanerPage.BlogUrlTextBox, _blogCleanerPage.LabelBlogStatus, _blogCleanerPage.BlogWebView)
            );
        }

        private void BindEvents()
        {
            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

            _crawlingPage.ButtonUpload.Click += _crawlingHandler.OnUploadClicked;
            _crawlingPage.ButtonDownload.Click += _crawlingHandler.OnDownloadClicked;
            _crawlingPage.ButtonTemplateDownload.Click += _crawlingHandler.OnTemplateDownloadClicked;

            _keywordPage.ConvertButton.Click += _keywordHandler.OnConvertClicked;
            _keywordPage.CopyLeftButton.Click += _keywordHandler.OnCopyLeftClicked;
            _keywordPage.CopyRightButton.Click += _keywordHandler.OnCopyRightClicked;

            _blogCleanerPage.ButtonBlogOpen.Click += ButtonBlogOpen_Click;
            _blogCleanerPage.ButtonBlogClean.Click += ButtonBlogClean_Click;
            _blogCleanerPage.ButtonBlogRefresh.Click += ButtonBlogRefresh_Click;
            _blogCleanerPage.ButtonBlogDownloadImages.Click += ButtonBlogDownloadImages_Click;
            _blogCleanerPage.BlogUrlTextBox.KeyDown += BlogUrlTextBox_KeyDown;
            _settingsPage.ApplyRequested += SettingsPage_ApplyRequested;
            _settingsPage.ResetRequested += SettingsPage_ResetRequested;
            _spellCheckerPage.ZoomPreferenceChanged += SpellCheckerPage_ZoomPreferenceChanged;
        }

        private void InitializeTabSettings()
        {
            _managedTabs.Clear();
            _managedTabs.Add(new ManagedTab("crawling", tabPageCrawling.Text, tabPageCrawling));
            _managedTabs.Add(new ManagedTab("keyword", tabPageKeyword.Text, tabPageKeyword));
            _managedTabs.Add(new ManagedTab("blog", tabPageBlogCleaner.Text, tabPageBlogCleaner));
            _managedTabs.Add(new ManagedTab("image", tabPageImageConverter.Text, tabPageImageConverter));
            _managedTabs.Add(new ManagedTab("spell", tabPageSpellChecker.Text, tabPageSpellChecker));

            var persisted = LoadTabSettings();
            ApplyPersistedTabSettings(persisted);
            if (persisted?.SpellCheckerZoom?.SaveEnabled == true)
            {
                _spellCheckerPage.ApplyZoomPreference(
                    persisted.SpellCheckerZoom.ZoomPercent,
                    persisted.SpellCheckerZoom.SaveEnabled
                );
            }
            else
            {
                _spellCheckerPage.ApplyZoomPreference(100, false);
            }

            _settingsPage.SetTabs(
                _managedTabs
                    .Select(tab => new SettingsTabState(tab.Key, tab.Title, tab.Visible))
                    .ToList()
            );

            ApplyTabLayoutFromSettings();
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            await PreloadPagesAsync();
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _crawlingHandler.CancelDownload();
            _blogCleanerHandler.Dispose();
        }

        private async void TabControl_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isPreloading)
            {
                return;
            }

            await ShowTabTransitionAsync();
        }

        private async Task PreloadPagesAsync()
        {
            _isPreloading = true;

            SuspendLayout();
            tabControl.SuspendLayout();

            try
            {
                ApplyCurrentTabLayout(centerForm: false);
                await _blogCleanerHandler.InitializeAsync();
                await _spellCheckerPage.InitializeAsync();
            }
            finally
            {
                tabControl.ResumeLayout(true);
                ResumeLayout(true);
                _isPreloading = false;
                ApplyCurrentTabLayout();
            }
        }

        private void ApplyCurrentTabLayout(bool centerForm = true)
        {
            var targetSize = GetCurrentTabSize();
            SuspendLayout();
            tabControl.SuspendLayout();

            try
            {
                if (Size != targetSize)
                {
                    Size = targetSize;
                }

                UpdateLoadingOverlayBounds();

                if (centerForm && !_isPreloading)
                {
                    CenterToScreen();
                }
            }
            finally
            {
                tabControl.ResumeLayout(true);
                ResumeLayout(true);
            }
        }

        private Size GetCurrentTabSize()
        {
            if (tabControl.SelectedTab == tabPageBlogCleaner)
            {
                return _blogFormSize;
            }

            if (tabControl.SelectedTab == tabPageImageConverter)
            {
                return _imageConverterFormSize;
            }

            if (tabControl.SelectedTab == tabPageSpellChecker)
            {
                return _spellCheckerFormSize;
            }

            if (tabControl.SelectedTab == tabPageSettings)
            {
                return _settingsFormSize;
            }

            return _defaultFormSize;
        }

        private void SettingsPage_ApplyRequested(object? sender, EventArgs e)
        {
            var settings = _settingsPage.GetTabs();
            var updatedTabs = new List<ManagedTab>();

            foreach (var setting in settings)
            {
                var currentTab = _managedTabs.FirstOrDefault(tab => tab.Key == setting.Key);
                if (currentTab == null)
                {
                    continue;
                }

                currentTab.Visible = setting.Visible;
                updatedTabs.Add(currentTab);
            }

            _managedTabs.Clear();
            _managedTabs.AddRange(updatedTabs);

            ApplyTabLayoutFromSettings();
            SaveTabSettings();
        }

        private void SpellCheckerPage_ZoomPreferenceChanged(object? sender, EventArgs e)
        {
            SaveTabSettings();
        }

        private void SettingsPage_ResetRequested(object? sender, EventArgs e)
        {
            _managedTabs.Clear();
            _managedTabs.Add(new ManagedTab("crawling", tabPageCrawling.Text, tabPageCrawling));
            _managedTabs.Add(new ManagedTab("keyword", tabPageKeyword.Text, tabPageKeyword));
            _managedTabs.Add(new ManagedTab("blog", tabPageBlogCleaner.Text, tabPageBlogCleaner));
            _managedTabs.Add(new ManagedTab("image", tabPageImageConverter.Text, tabPageImageConverter));
            _managedTabs.Add(new ManagedTab("spell", tabPageSpellChecker.Text, tabPageSpellChecker));

            _spellCheckerPage.ApplyZoomPreference(100, false);
            _settingsPage.SetTabs(
                _managedTabs
                    .Select(tab => new SettingsTabState(tab.Key, tab.Title, tab.Visible))
                    .ToList()
            );

            ApplyTabLayoutFromSettings();

            try
            {
                var path = GetTabSettingsPath();
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                SaveTabSettings();
            }
        }

        private void ApplyTabLayoutFromSettings()
        {
            var previousSelection = tabControl.SelectedTab;

            tabControl.SuspendLayout();
            try
            {
                tabControl.TabPages.Clear();

                foreach (var tab in _managedTabs.Where(tab => tab.Visible))
                {
                    tabControl.TabPages.Add(tab.TabPage);
                }

                tabControl.TabPages.Add(tabPageSettings);

                if (previousSelection != null && tabControl.TabPages.Contains(previousSelection))
                {
                    tabControl.SelectedTab = previousSelection;
                }
                else
                {
                    tabControl.SelectedTab = tabPageSettings;
                }
            }
            finally
            {
                tabControl.ResumeLayout(true);
            }

            ApplyCurrentTabLayout();
        }

        private void ApplyPersistedTabSettings(PersistedTabLayout? persisted)
        {
            if (persisted == null || persisted.Tabs.Count == 0)
            {
                return;
            }

            var defaultTabs = _managedTabs.ToList();
            var map = defaultTabs.ToDictionary(tab => tab.Key, tab => tab, StringComparer.OrdinalIgnoreCase);
            var ordered = new List<ManagedTab>();

            foreach (var pref in persisted.Tabs)
            {
                if (!map.TryGetValue(pref.Key, out var tab))
                {
                    continue;
                }

                tab.Visible = pref.Visible;
                ordered.Add(tab);
                map.Remove(pref.Key);
            }

            foreach (var tab in defaultTabs)
            {
                if (map.ContainsKey(tab.Key))
                {
                    ordered.Add(tab);
                }
            }

            _managedTabs.Clear();
            _managedTabs.AddRange(ordered);
        }

        private void SaveTabSettings()
        {
            try
            {
                var path = GetTabSettingsPath();
                var directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                var data = new PersistedTabLayout
                {
                    Tabs = _managedTabs
                        .Select(tab => new PersistedTabItem { Key = tab.Key, Visible = tab.Visible })
                        .ToList(),
                    SpellCheckerZoom = BuildSpellCheckerZoomSetting()
                };

                var json = JsonSerializer.Serialize(data, TabSettingsJsonOptions);
                File.WriteAllText(path, json);
            }
            catch
            {
                // 설정 저장 실패는 앱 동작을 막지 않음
            }
        }

        private static PersistedTabLayout? LoadTabSettings()
        {
            try
            {
                var path = GetTabSettingsPath();
                if (!File.Exists(path))
                {
                    return null;
                }

                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<PersistedTabLayout>(json);
            }
            catch
            {
                return null;
            }
        }

        private static string GetTabSettingsPath()
        {
            var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            return Path.Combine(root, "Monthly_Excel", "tab-settings.json");
        }

        private PersistedSpellCheckerZoom BuildSpellCheckerZoomSetting()
        {
            var (zoomPercent, saveEnabled) = _spellCheckerPage.GetZoomPreference();
            return new PersistedSpellCheckerZoom
            {
                ZoomPercent = saveEnabled ? zoomPercent : 100,
                SaveEnabled = saveEnabled
            };
        }

        private (Panel Overlay, Label Label, CircularLoaderControl Spinner) CreateLoadingOverlay()
        {
            var overlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(236, 241, 246),
                Visible = false
            };
            EnableDoubleBuffer(overlay);

            var spinner = new CircularLoaderControl();
            var label = new Label
            {
                AutoSize = false,
                Size = new Size(180, 28),
                Text = "로딩 중...",
                TextAlign = ContentAlignment.MiddleCenter,
                Font = AppTheme.TitleFont,
                ForeColor = AppTheme.TextPrimary,
                BackColor = Color.Transparent
            };

            overlay.Controls.Add(spinner);
            overlay.Controls.Add(label);
            overlay.Resize += (_, _) => CenterLoadingElements();

            return (overlay, label, spinner);
        }

        private void ApplyTheme()
        {
            BackColor = AppTheme.AppBackground;
            Font = AppTheme.BodyFont;
            ForeColor = AppTheme.TextPrimary;
            tabControl.Font = AppTheme.BodyFont;
        }

        private (StatusStrip StatusStrip, ToolStripStatusLabel VersionLabel) CreateStatusStrip()
        {
            var statusStrip = new StatusStrip
            {
                Dock = DockStyle.Bottom,
                SizingGrip = false,
                BackColor = AppTheme.SurfaceMuted
            };

            var fillerLabel = new ToolStripStatusLabel
            {
                Spring = true
            };

            var versionLabel = new ToolStripStatusLabel
            {
                Text = $"v{GetDisplayVersion()}",
                ForeColor = AppTheme.TextMuted,
                TextAlign = ContentAlignment.MiddleRight
            };

            statusStrip.Items.Add(fillerLabel);
            statusStrip.Items.Add(versionLabel);

            return (statusStrip, versionLabel);
        }

        private static string GetDisplayVersion()
        {
            var informationalVersion =
                Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion;

            if (!string.IsNullOrWhiteSpace(informationalVersion))
            {
                var separatorIndex = informationalVersion.IndexOf('+');
                return separatorIndex >= 0
                    ? informationalVersion[..separatorIndex]
                    : informationalVersion;
            }

            return Application.ProductVersion;
        }

        private async Task ShowTabTransitionAsync()
        {
            var transitionVersion = ++_tabTransitionVersion;
            ShowLoadingOverlay();

            await Task.Yield();
            ApplyCurrentTabLayout();
            await Task.Delay(140);

            if (transitionVersion != _tabTransitionVersion || IsDisposed)
            {
                return;
            }

            HideLoadingOverlay();
        }

        private void ShowLoadingOverlay()
        {
            UpdateLoadingOverlayBounds();
            _loadingLabel.Text = "로딩 중...";
            _loadingOverlay.Visible = true;
            _loadingOverlay.BringToFront();
            _loadingSpinner.Visible = true;
            tabControl.Enabled = false;
            CenterLoadingElements();
        }

        private void HideLoadingOverlay()
        {
            tabControl.Enabled = true;
            _loadingSpinner.Visible = false;
            _loadingOverlay.Visible = false;
        }

        private void UpdateLoadingOverlayBounds()
        {
            _loadingOverlay.Bounds = ClientRectangle;
            CenterLoadingElements();
        }

        private void CenterLoadingElements()
        {
            var centerX = _loadingOverlay.ClientSize.Width / 2;
            var centerY = _loadingOverlay.ClientSize.Height / 2;

            _loadingSpinner.Location = new Point(centerX - (_loadingSpinner.Width / 2), centerY - 34);
            _loadingLabel.Location = new Point(centerX - (_loadingLabel.Width / 2), _loadingSpinner.Bottom + 10);
        }

        private static void EnableDoubleBuffer(Control control)
        {
            typeof(Control)
                .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(control, true);
        }

        private async void ButtonBlogOpen_Click(object? sender, EventArgs e)
        {
            await _blogCleanerHandler.OpenAsync();
        }

        private async void ButtonBlogClean_Click(object? sender, EventArgs e)
        {
            await _blogCleanerHandler.CleanAsync();
        }

        private async void ButtonBlogRefresh_Click(object? sender, EventArgs e)
        {
            await _blogCleanerHandler.RefreshAsync();
        }

        private async void ButtonBlogDownloadImages_Click(object? sender, EventArgs e)
        {
            await _blogCleanerHandler.SaveImagesAsync();
        }

        private async void BlogUrlTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

            e.SuppressKeyPress = true;
            await _blogCleanerHandler.OpenAsync();
        }

        private sealed class ManagedTab
        {
            public ManagedTab(string key, string title, TabPage tabPage)
            {
                Key = key;
                Title = title;
                TabPage = tabPage;
                Visible = true;
            }

            public string Key { get; }
            public string Title { get; }
            public TabPage TabPage { get; }
            public bool Visible { get; set; }
        }

        private sealed class PersistedTabLayout
        {
            public List<PersistedTabItem> Tabs { get; set; } = new();
            public PersistedSpellCheckerZoom? SpellCheckerZoom { get; set; }
        }

        private sealed class PersistedTabItem
        {
            public string Key { get; set; } = string.Empty;
            public bool Visible { get; set; } = true;
        }

        private sealed class PersistedSpellCheckerZoom
        {
            public int ZoomPercent { get; set; } = 100;
            public bool SaveEnabled { get; set; }
        }
    }
}
