using System.Drawing;
using System.Windows.Forms;

namespace Monthly_Excel.Pages.Crawling
{
    public class CrawlingPage : UserControl
    {
        public Label LabelUpload { get; }
        public Button ButtonUpload { get; }
        public Label LabelDownload { get; }
        public Button ButtonDownload { get; }
        public Label LabelTemplate { get; }
        public Button ButtonTemplateDownload { get; }
        public Label LabelStatus { get; }
        public ProgressBar ProgressBar { get; }

        public CrawlingPage()
        {
            var tableLayoutPanel = new TableLayoutPanel
            {
                ColumnCount = 2,
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                RowCount = 6,
            };
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            LabelUpload = new Label { Dock = DockStyle.Fill, Text = "📅 엑셀 업로드", TextAlign = ContentAlignment.MiddleLeft };
            ButtonUpload = new Button { Dock = DockStyle.Fill, Text = "파일 선택" };
            LabelDownload = new Label { Dock = DockStyle.Fill, Text = "📄 엑셀 다운로드", TextAlign = ContentAlignment.MiddleLeft };
            ButtonDownload = new Button { Dock = DockStyle.Fill, Text = "다운로드" };
            LabelTemplate = new Label { Dock = DockStyle.Fill, Text = "📘 양식 다운로드", TextAlign = ContentAlignment.MiddleLeft };
            ButtonTemplateDownload = new Button { Dock = DockStyle.Fill, Text = "양식 받기" };
            LabelStatus = new Label { Dock = DockStyle.Fill, Padding = new Padding(5, 0, 0, 0), Text = "상태: 대기 중", TextAlign = ContentAlignment.MiddleLeft };
            ProgressBar = new ProgressBar { Dock = DockStyle.Fill, Style = ProgressBarStyle.Continuous };

            tableLayoutPanel.Controls.Add(LabelUpload, 0, 0);
            tableLayoutPanel.Controls.Add(ButtonUpload, 1, 0);
            tableLayoutPanel.Controls.Add(LabelDownload, 0, 1);
            tableLayoutPanel.Controls.Add(ButtonDownload, 1, 1);
            tableLayoutPanel.Controls.Add(LabelTemplate, 0, 2);
            tableLayoutPanel.Controls.Add(ButtonTemplateDownload, 1, 2);
            tableLayoutPanel.Controls.Add(LabelStatus, 0, 3);
            tableLayoutPanel.SetColumnSpan(LabelStatus, 2);
            tableLayoutPanel.Controls.Add(ProgressBar, 0, 4);
            tableLayoutPanel.SetColumnSpan(ProgressBar, 2);

            Controls.Add(tableLayoutPanel);
        }
    }
}
