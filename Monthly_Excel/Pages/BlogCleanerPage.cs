using System.Drawing;
using System.Windows.Forms;

namespace Monthly_Excel.Pages
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
            var blogLayout = new TableLayoutPanel
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                RowCount = 3,
            };
            blogLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            blogLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
            blogLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            blogLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var blogTopPanel = new Panel { Dock = DockStyle.Fill };
            BlogUrlTextBox = new TextBox { Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right, PlaceholderText = "네이버 블로그 URL 입력", Location = new Point(3, 9), Width = 168 };
            ButtonBlogOpen = new Button { Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(176, 8), Size = new Size(48, 25), Text = "열기" };
            ButtonBlogClean = new Button { Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(229, 8), Size = new Size(48, 25), Text = "정리" };
            ButtonBlogRefresh = new Button { Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(282, 8), Size = new Size(90, 25), Text = "새로고침" };
            ButtonBlogDownloadImages = new Button { Anchor = AnchorStyles.Top | AnchorStyles.Right, Location = new Point(380, 8), Size = new Size(90, 25), Text = "이미지 다운" };
            blogTopPanel.Controls.Add(BlogUrlTextBox);
            blogTopPanel.Controls.Add(ButtonBlogOpen);
            blogTopPanel.Controls.Add(ButtonBlogClean);
            blogTopPanel.Controls.Add(ButtonBlogRefresh);
            blogTopPanel.Controls.Add(ButtonBlogDownloadImages);

            LabelBlogStatus = new Label { Dock = DockStyle.Fill, Padding = new Padding(4, 0, 0, 0), Text = "상태: 대기 중", TextAlign = ContentAlignment.MiddleLeft };
            BlogWebView = new Microsoft.Web.WebView2.WinForms.WebView2 { Dock = DockStyle.Fill, Source = new System.Uri("https://blog.naver.com", System.UriKind.Absolute), ZoomFactor = 1D };

            blogLayout.Controls.Add(blogTopPanel, 0, 0);
            blogLayout.Controls.Add(LabelBlogStatus, 0, 1);
            blogLayout.Controls.Add(BlogWebView, 0, 2);

            Controls.Add(blogLayout);
        }
    }
}
