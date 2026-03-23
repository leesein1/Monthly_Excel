using System.Drawing;
using System.Windows.Forms;

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
            var keywordLayout = new TableLayoutPanel
            {
                ColumnCount = 2,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                RowCount = 5,
            };
            keywordLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            keywordLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            keywordLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            keywordLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            keywordLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            keywordLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            keywordLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));

            InputKeywordBox = new TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
            ConvertButton = new Button { Dock = DockStyle.Fill, Text = "Convert" };
            LabelLeft = new Label { Dock = DockStyle.Fill, Text = "키워드 1", TextAlign = ContentAlignment.MiddleLeft };
            LabelRight = new Label { Dock = DockStyle.Fill, Text = "키워드 2", TextAlign = ContentAlignment.MiddleLeft };
            LeftListBox = new ListBox { Dock = DockStyle.Fill }; 
            RightListBox = new ListBox { Dock = DockStyle.Fill };
            CopyLeftButton = new Button { Dock = DockStyle.Right, Text = "복사" };
            CopyRightButton = new Button { Dock = DockStyle.Right, Text = "복사" };

            keywordLayout.Controls.Add(InputKeywordBox, 0, 0);
            keywordLayout.Controls.Add(ConvertButton, 1, 0);
            keywordLayout.Controls.Add(LabelLeft, 0, 1);
            keywordLayout.Controls.Add(LabelRight, 1, 1);
            keywordLayout.Controls.Add(LeftListBox, 0, 2);
            keywordLayout.Controls.Add(RightListBox, 1, 2);
            keywordLayout.Controls.Add(CopyLeftButton, 0, 3);
            keywordLayout.Controls.Add(CopyRightButton, 1, 3);

            Controls.Add(keywordLayout);
        }
    }
}
