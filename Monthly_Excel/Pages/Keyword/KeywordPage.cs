using System.Drawing;
using System.Windows.Forms;
using Monthly_Excel.UI;

namespace Monthly_Excel.Pages.Keyword
{
    public class KeywordPage : UserControl
    {
        public TextBox InputKeywordBox { get; }
        public Button ConvertButton { get; }
        public Label LabelLeft { get; }
        public Label LabelRight { get; }
        public ListBox LeftListBox { get; }
        public ListBox RightListBox { get; }
        public Button CopyLeftButton { get; }
        public Button CopyRightButton { get; }

        public KeywordPage()
        {
            AppTheme.ApplyPage(this);

            var surface = AppTheme.CreateSurfacePanel();
            var keywordLayout = new TableLayoutPanel
            {
                ColumnCount = 2,
                Dock = DockStyle.Fill,
                RowCount = 5,
                BackColor = AppTheme.SurfaceBackground
            };
            keywordLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            keywordLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            keywordLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 74F));
            keywordLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
            keywordLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            keywordLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
            keywordLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 8F));

            InputKeywordBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Margin = new Padding(0, 0, 8, 8),
                PlaceholderText = "키워드를 줄 단위로 입력하세요."
            };
            ConvertButton = new Button { Dock = DockStyle.Fill, Text = "변환", Margin = new Padding(0, 0, 0, 8) };
            LabelLeft = new Label { Dock = DockStyle.Fill, Text = "키워드 1", TextAlign = ContentAlignment.MiddleLeft };
            LabelRight = new Label { Dock = DockStyle.Fill, Text = "키워드 2", TextAlign = ContentAlignment.MiddleLeft };
            LeftListBox = new ListBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 8, 8) };
            RightListBox = new ListBox { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 8) };
            CopyLeftButton = new Button { Dock = DockStyle.Fill, Text = "복사", Margin = new Padding(0, 0, 8, 0) };
            CopyRightButton = new Button { Dock = DockStyle.Fill, Text = "복사", Margin = new Padding(0) };

            AppTheme.StyleTextBox(InputKeywordBox);
            AppTheme.StylePrimaryButton(ConvertButton);
            AppTheme.StyleSectionLabel(LabelLeft);
            AppTheme.StyleSectionLabel(LabelRight);
            AppTheme.StyleListBox(LeftListBox);
            AppTheme.StyleListBox(RightListBox);
            AppTheme.StyleSecondaryButton(CopyLeftButton);
            AppTheme.StyleSecondaryButton(CopyRightButton);

            keywordLayout.Controls.Add(InputKeywordBox, 0, 0);
            keywordLayout.Controls.Add(ConvertButton, 1, 0);
            keywordLayout.Controls.Add(LabelLeft, 0, 1);
            keywordLayout.Controls.Add(LabelRight, 1, 1);
            keywordLayout.Controls.Add(LeftListBox, 0, 2);
            keywordLayout.Controls.Add(RightListBox, 1, 2);
            keywordLayout.Controls.Add(CopyLeftButton, 0, 3);
            keywordLayout.Controls.Add(CopyRightButton, 1, 3);

            surface.Controls.Add(keywordLayout);
            Controls.Add(surface);
        }
    }
}
