using System.Drawing;
using System.Windows.Forms;
using Monthly_Excel.UI;

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
            AppTheme.ApplyPage(this);

            var surface = AppTheme.CreateSurfacePanel();
            var tableLayoutPanel = new TableLayoutPanel
            {
                ColumnCount = 2,
                Dock = DockStyle.Fill,
                RowCount = 6,
                BackColor = AppTheme.SurfaceBackground
            };
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
            tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            LabelUpload = new Label { Dock = DockStyle.Fill, Text = "엑셀 업로드", TextAlign = ContentAlignment.MiddleLeft };
            ButtonUpload = new Button { Dock = DockStyle.Fill, Text = "파일 선택", Margin = new Padding(8, 2, 0, 2) };
            LabelDownload = new Label { Dock = DockStyle.Fill, Text = "엑셀 다운로드", TextAlign = ContentAlignment.MiddleLeft };
            ButtonDownload = new Button { Dock = DockStyle.Fill, Text = "다운로드", Margin = new Padding(8, 2, 0, 2) };
            LabelTemplate = new Label { Dock = DockStyle.Fill, Text = "양식 다운로드", TextAlign = ContentAlignment.MiddleLeft };
            ButtonTemplateDownload = new Button { Dock = DockStyle.Fill, Text = "양식 받기", Margin = new Padding(8, 2, 0, 2) };
            LabelStatus = new Label { Dock = DockStyle.Fill, Padding = new Padding(4, 0, 0, 0), Text = "상태: 대기 중", TextAlign = ContentAlignment.MiddleLeft };
            ProgressBar = new ProgressBar { Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 0), Style = ProgressBarStyle.Continuous };

            AppTheme.StyleSectionLabel(LabelUpload);
            AppTheme.StyleSectionLabel(LabelDownload);
            AppTheme.StyleSectionLabel(LabelTemplate);
            AppTheme.StylePrimaryButton(ButtonUpload);
            AppTheme.StylePrimaryButton(ButtonDownload);
            AppTheme.StyleSecondaryButton(ButtonTemplateDownload);
            AppTheme.StyleStatusLabel(LabelStatus);
            AppTheme.StyleProgressBar(ProgressBar);

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

            surface.Controls.Add(tableLayoutPanel);
            Controls.Add(surface);
        }
    }
}
