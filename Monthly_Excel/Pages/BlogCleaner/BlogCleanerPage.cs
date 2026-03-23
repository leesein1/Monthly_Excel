using System.Drawing;
using System.Windows.Forms;
using Monthly_Excel.UI;

namespace Monthly_Excel.Pages.BlogCleaner
{
    public class BlogCleanerPage : UserControl
    {
        public TextBox BlogUrlTextBox { get; }
        public Button ButtonBlogOpen { get; }
        public Button ButtonBlogClean { get; }
        public Button ButtonBlogRefresh { get; }
        public Button ButtonBlogDownloadImages { get; }
        public Label LabelBlogStatus { get; }
        public Microsoft.Web.WebView2.WinForms.WebView2 BlogWebView { get; }

        public BlogCleanerPage()
        {
            AppTheme.ApplyPage(this);

            var surface = AppTheme.CreateSurfacePanel();
            var blogLayout = new TableLayoutPanel
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                RowCount = 3,
                BackColor = AppTheme.SurfaceBackground
            };
            blogLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            blogLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            blogLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            blogLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var blogTopLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0, 0, 0, 8)
            };
            blogTopLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            blogTopLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            blogTopLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));

            BlogUrlTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8),
                PlaceholderText = "네이버 블로그 URL 입력"
            };

            var buttonLayout = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0)
            };

            ButtonBlogOpen = new Button { Width = 82, Margin = new Padding(0, 0, 8, 0), Text = "열기" };
            ButtonBlogClean = new Button { Width = 82, Margin = new Padding(0, 0, 8, 0), Text = "정리" };
            ButtonBlogRefresh = new Button { Width = 92, Margin = new Padding(0, 0, 8, 0), Text = "새로고침" };
            ButtonBlogDownloadImages = new Button { Width = 112, Margin = new Padding(0), Text = "이미지 다운" };

            AppTheme.StyleTextBox(BlogUrlTextBox);
            AppTheme.StylePrimaryButton(ButtonBlogOpen);
            AppTheme.StyleSecondaryButton(ButtonBlogClean);
            AppTheme.StyleSecondaryButton(ButtonBlogRefresh);
            AppTheme.StyleSecondaryButton(ButtonBlogDownloadImages);

            buttonLayout.Controls.Add(ButtonBlogOpen);
            buttonLayout.Controls.Add(ButtonBlogClean);
            buttonLayout.Controls.Add(ButtonBlogRefresh);
            buttonLayout.Controls.Add(ButtonBlogDownloadImages);

            blogTopLayout.Controls.Add(BlogUrlTextBox, 0, 0);
            blogTopLayout.Controls.Add(buttonLayout, 0, 1);

            LabelBlogStatus = new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(4, 0, 0, 0),
                Text = "상태: 대기 중",
                TextAlign = ContentAlignment.MiddleLeft
            };
            AppTheme.StyleStatusLabel(LabelBlogStatus);

            BlogWebView = new Microsoft.Web.WebView2.WinForms.WebView2
            {
                Dock = DockStyle.Fill,
                Source = new System.Uri("https://blog.naver.com", System.UriKind.Absolute),
                ZoomFactor = 1D
            };

            blogLayout.Controls.Add(blogTopLayout, 0, 0);
            blogLayout.Controls.Add(LabelBlogStatus, 0, 1);
            blogLayout.Controls.Add(BlogWebView, 0, 2);

            surface.Controls.Add(blogLayout);
            Controls.Add(surface);
        }
    }
}
