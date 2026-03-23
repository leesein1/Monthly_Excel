using System;
using System.Drawing;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Monthly_Excel.Handlers;
using Monthly_Excel.Pages.BlogCleaner;
using Monthly_Excel.Pages.Crawling;
using Monthly_Excel.Pages.ImageConverter;
using Monthly_Excel.Pages.Keyword;

namespace Monthly_Excel
{
    public partial class Form1 : Form
    {
        private readonly Size _defaultFormSize = new(532, 328);
        private readonly Size _imageConverterFormSize = new(1180, 760);
        private readonly Size _blogFormSize = new(1400, 900);
        private bool _isPreloading;
        private int _tabTransitionVersion;
        private readonly Panel _loadingOverlay;
        private readonly Panel _loadingCard;
        private readonly Label _loadingLabel;
        private readonly ProgressBar _loadingProgressBar;

        private readonly CrawlingPage _crawlingPage;
        private readonly KeywordPage _keywordPage;
        private readonly BlogCleanerPage _blogCleanerPage;
        private readonly ImageConverterPage _imageConverterPage;

        private readonly CrawlingEventHandler _crawlingHandler;
        private readonly KeywordEventHandler _keywordHandler;
        private readonly BlogCleanerHandler _blogCleanerHandler;

        public Form1()
        {
            InitializeComponent();
            EnableDoubleBuffer(this);
            EnableDoubleBuffer(tabControl);
            (_loadingOverlay, _loadingCard, _loadingLabel, _loadingProgressBar) = CreateLoadingOverlay();

            (_crawlingPage, _keywordPage, _blogCleanerPage, _imageConverterPage) = CreatePages();
            AttachPages();
            (_crawlingHandler, _keywordHandler, _blogCleanerHandler) = CreateHandlers();

            BindEvents();
        }

        private (CrawlingPage CrawlingPage, KeywordPage KeywordPage, BlogCleanerPage BlogCleanerPage, ImageConverterPage ImageConverterPage) CreatePages()
        {
            return (
                new CrawlingPage { Dock = DockStyle.Fill },
                new KeywordPage { Dock = DockStyle.Fill },
                new BlogCleanerPage { Dock = DockStyle.Fill },
                new ImageConverterPage { Dock = DockStyle.Fill }
            );
        }

        private void AttachPages()
        {
            tabControl.SuspendLayout();
            tabPageCrawling.SuspendLayout();
            tabPageKeyword.SuspendLayout();
            tabPageBlogCleaner.SuspendLayout();
            tabPageImageConverter.SuspendLayout();

            tabPageCrawling.Controls.Add(_crawlingPage);
            tabPageKeyword.Controls.Add(_keywordPage);
            tabPageBlogCleaner.Controls.Add(_blogCleanerPage);
            tabPageImageConverter.Controls.Add(_imageConverterPage);

            tabPageImageConverter.ResumeLayout();
            tabPageBlogCleaner.ResumeLayout();
            tabPageKeyword.ResumeLayout();
            tabPageCrawling.ResumeLayout();
            tabControl.ResumeLayout();

            Controls.Add(_loadingOverlay);
            _loadingOverlay.BringToFront();
        }

        private (CrawlingEventHandler CrawlingHandler, KeywordEventHandler KeywordHandler, BlogCleanerHandler BlogCleanerHandler) CreateHandlers()
        {
            return (
                new CrawlingEventHandler(_crawlingPage.LabelStatus, _crawlingPage.ProgressBar),
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
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            await PreloadPagesAsync();
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
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

            return _defaultFormSize;
        }

        private (Panel Overlay, Panel Card, Label Label, ProgressBar ProgressBar) CreateLoadingOverlay()
        {
            var overlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(245, 247, 250),
                Visible = false
            };
            EnableDoubleBuffer(overlay);

            var card = new Panel
            {
                Size = new Size(220, 92),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var label = new Label
            {
                Dock = DockStyle.Top,
                Height = 44,
                Text = "로딩 중...",
                TextAlign = ContentAlignment.BottomCenter,
                Font = new Font(Font.FontFamily, 10F, FontStyle.Bold)
            };

            var progressBar = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 18,
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 24
            };

            card.Controls.Add(progressBar);
            card.Controls.Add(label);
            overlay.Controls.Add(card);
            overlay.Resize += (_, _) => CenterLoadingCard();

            return (overlay, card, label, progressBar);
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
            tabControl.Enabled = false;
            CenterLoadingCard();
        }

        private void HideLoadingOverlay()
        {
            tabControl.Enabled = true;
            _loadingOverlay.Visible = false;
        }

        private void UpdateLoadingOverlayBounds()
        {
            _loadingOverlay.Bounds = ClientRectangle;
            CenterLoadingCard();
        }

        private void CenterLoadingCard()
        {
            var x = Math.Max(0, (_loadingOverlay.ClientSize.Width - _loadingCard.Width) / 2);
            var y = Math.Max(0, (_loadingOverlay.ClientSize.Height - _loadingCard.Height) / 2);
            _loadingCard.Location = new Point(x, y);
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
    }
}
