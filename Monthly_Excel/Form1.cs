using System;
using System.Drawing;
using System.Windows.Forms;
using Monthly_Excel.Handlers;

namespace Monthly_Excel
{
    public partial class Form1 : Form
    {
        private readonly Size _defaultFormSize = new(532, 328);
        private readonly Size _defaultMinSize = new(516, 289);
        private readonly Size _blogFormSize = new(1400, 900);
        private readonly Size _blogMinSize = new(1200, 800);

        private readonly CrawlingEventHandler _crawlingHandler;
        private readonly KeywordEventHandler _keywordHandler;
        private readonly BlogCleanerHandler _blogCleanerHandler;

        public Form1()
        {
            InitializeComponent();

            _crawlingHandler = new CrawlingEventHandler(labelStatus, progressBar);
            _keywordHandler = new KeywordEventHandler(inputKeywordBox, leftListBox, rightListBox);
            _blogCleanerHandler = new BlogCleanerHandler(blogUrlTextBox, labelBlogStatus, blogWebView);

            BindEvents();
        }

        private void BindEvents()
        {
            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

            buttonUpload.Click += _crawlingHandler.OnUploadClicked;
            buttonDownload.Click += _crawlingHandler.OnDownloadClicked;
            buttonTemplateDownload.Click += _crawlingHandler.OnTemplateDownloadClicked;

            convertButton.Click += _keywordHandler.OnConvertClicked;
            copyLeftButton.Click += _keywordHandler.OnCopyLeftClicked;
            copyRightButton.Click += _keywordHandler.OnCopyRightClicked;

            buttonBlogOpen.Click += ButtonBlogOpen_Click;
            buttonBlogClean.Click += ButtonBlogClean_Click;
            buttonBlogRefresh.Click += ButtonBlogRefresh_Click;
            buttonBlogDownloadImages.Click += ButtonBlogDownloadImages_Click;
            blogUrlTextBox.KeyDown += BlogUrlTextBox_KeyDown;
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
