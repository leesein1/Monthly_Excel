using System;
using System.Drawing;
using System.Windows.Forms;
using Monthly_Excel.Handlers;
using Monthly_Excel.Pages;

namespace Monthly_Excel
{
    public partial class Form1 : Form
    {
        private readonly Size _defaultFormSize = new(532, 328);
        private readonly Size _defaultMinSize = new(516, 289);
        private readonly Size _imageConverterFormSize = new(1180, 760);
        private readonly Size _imageConverterMinSize = new(960, 640);
        private readonly Size _blogFormSize = new(1400, 900);
        private readonly Size _blogMinSize = new(1200, 800);

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

            _crawlingPage = new CrawlingPage { Dock = DockStyle.Fill };
            _keywordPage = new KeywordPage { Dock = DockStyle.Fill };
            _blogCleanerPage = new BlogCleanerPage { Dock = DockStyle.Fill };
            _imageConverterPage = new ImageConverterPage { Dock = DockStyle.Fill };

            tabPageCrawling.Controls.Add(_crawlingPage);
            tabPageKeyword.Controls.Add(_keywordPage);
            tabPageBlogCleaner.Controls.Add(_blogCleanerPage);
            tabPageImageConverter.Controls.Add(_imageConverterPage);

            _crawlingHandler = new CrawlingEventHandler(_crawlingPage.LabelStatus, _crawlingPage.ProgressBar);
            _keywordHandler = new KeywordEventHandler(_keywordPage.InputKeywordBox, _keywordPage.LeftListBox, _keywordPage.RightListBox);
            _blogCleanerHandler = new BlogCleanerHandler(_blogCleanerPage.BlogUrlTextBox, _blogCleanerPage.LabelBlogStatus, _blogCleanerPage.BlogWebView);

            BindEvents();
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
            ResizeForCurrentTab();
            await _blogCleanerHandler.InitializeAsync();
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _blogCleanerHandler.Dispose();
        }

        private void TabControl_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ResizeForCurrentTab();
        }

        private void ResizeForCurrentTab()
        {
            if (tabControl.SelectedTab == tabPageBlogCleaner)
            {
                MinimumSize = _blogMinSize;
                Size = _blogFormSize;
            }
            else if (tabControl.SelectedTab == tabPageImageConverter)
            {
                MinimumSize = _imageConverterMinSize;
                Size = _imageConverterFormSize;
            }
            else
            {
                MinimumSize = _defaultMinSize;
                Size = _defaultFormSize;
            }

            CenterToScreen();
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
