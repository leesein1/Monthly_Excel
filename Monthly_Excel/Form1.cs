using System;
using System.Drawing;
using System.Windows.Forms;
using Monthly_Excel.Handlers;

namespace Monthly_Excel
{
    public partial class Form1 : Form
    {
        private readonly Size _defaultFormSize = new Size(532, 328);
        private readonly Size _defaultMinSize = new Size(516, 289);

        private readonly Size _blogFormSize = new Size(1400, 900);
        private readonly Size _blogMinSize = new Size(1200, 800);

        private BlogCleanerHandler? _blogCleanerHandler;

        public Form1()
        {
            InitializeComponent();
            BindEvents();
        }

        private void BindEvents()
        {
            Load += Form1_Load;
            FormClosing += Form1_FormClosing;
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;

            buttonBlogOpen.Click += ButtonBlogOpen_Click;
            buttonBlogClean.Click += ButtonBlogClean_Click;
            buttonBlogRefresh.Click += ButtonBlogRefresh_Click;
            buttonBlogDownloadImages.Click += ButtonBlogDownloadImages_Click;
        }

        private async void Form1_Load(object? sender, EventArgs e)
        {
            ResizeForCurrentTab();

            _blogCleanerHandler = new BlogCleanerHandler(
                blogUrlTextBox,
                labelBlogStatus,
                blogWebView
            );

            await _blogCleanerHandler.InitializeAsync();
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            _blogCleanerHandler?.Dispose();
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
            if (_blogCleanerHandler == null)
                return;

            await _blogCleanerHandler.OpenAsync();
        }

        private async void ButtonBlogClean_Click(object? sender, EventArgs e)
        {
            if (_blogCleanerHandler == null)
                return;

            await _blogCleanerHandler.CleanAsync();
        }

        private async void ButtonBlogRefresh_Click(object? sender, EventArgs e)
        {
            if (_blogCleanerHandler == null)
                return;

            await _blogCleanerHandler.RefreshAsync();
        }

        private async void ButtonBlogDownloadImages_Click(object? sender, EventArgs e)
        {
            if (_blogCleanerHandler == null)
                return;

            await _blogCleanerHandler.SaveImagesAsync();
        }
    }
}