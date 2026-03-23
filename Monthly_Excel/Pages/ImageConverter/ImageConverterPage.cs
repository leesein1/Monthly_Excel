using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Monthly_Excel.Pages.ImageConverter
{
    public partial class ImageConverterPage : UserControl
    {
        private const string SelectAllText = "전체 선택";
        private const string ClearSelectionText = "선택 해제";
        private const int PreviewMaxDimension = 800;
        private const int PreviewCacheCapacity = 12;
        private readonly System.Windows.Forms.Timer _previewTimer;
        private int _previewRequestId;
        private readonly Label _availableHeaderLabel;
        private readonly Label _selectedHeaderLabel;
        private readonly Label _previewHintLabel;
        private readonly TableLayoutPanel _mainLayout;
        private readonly SplitContainer _middleSplit;
        private readonly Dictionary<string, Bitmap> _previewCache = new(StringComparer.OrdinalIgnoreCase);
        private readonly LinkedList<string> _previewCacheOrder = new();

        public TextBox FolderTextBox { get; }
        public Button ChooseFolderButton { get; }
        public Button RefreshButton { get; }
        public Button SelectAllButton { get; }
        public ListBox ImageListBox { get; }
        public ListBox SelectedImageListBox { get; }
        public PictureBox PreviewBox { get; }
        public ComboBox ExtensionComboBox { get; }
        public Button ConvertButton { get; }
        public Label StatusLabel { get; }
        public Label FolderPathDisplay { get; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _previewTimer.Dispose();
                ClearPreview();
                ClearPreviewCache();
            }

            base.Dispose(disposing);
        }

        public ImageConverterPage()
        {
            _previewTimer = new System.Windows.Forms.Timer { Interval = 180 };
            _previewTimer.Tick += PreviewTimer_Tick;

            _mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(12) };
            _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var topBar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            topBar.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            topBar.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            FolderTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 8, 8),
                PlaceholderText = "이미지 폴더 경로를 입력하거나 선택하세요."
            };
            ChooseFolderButton = new Button { Text = "폴더 선택", AutoSize = true, MinimumSize = new Size(92, 30), Margin = new Padding(0, 0, 0, 8) };
            RefreshButton = new Button { Text = "새로고침", AutoSize = true, MinimumSize = new Size(92, 30), Margin = new Padding(0, 0, 8, 0) };
            SelectAllButton = new Button { Text = SelectAllText, AutoSize = true, MinimumSize = new Size(104, 30), Margin = new Padding(0) };

            var topActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0)
            };
            topActions.Controls.Add(RefreshButton);
            topActions.Controls.Add(SelectAllButton);

            topBar.Controls.Add(FolderTextBox, 0, 0);
            topBar.Controls.Add(ChooseFolderButton, 1, 0);
            topBar.Controls.Add(topActions, 0, 1);
            topBar.SetColumnSpan(topActions, 2);

            FolderPathDisplay = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.DarkBlue,
                Text = "선택된 폴더: 없음",
                Padding = new Padding(4, 0, 0, 0),
                Margin = new Padding(0, 0, 0, 8)
            };

            _middleSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                FixedPanel = FixedPanel.None
            };
            _middleSplit.HandleCreated += (_, _) => BeginInvoke(new Action(UpdateResponsiveLayout));
            _middleSplit.Resize += (_, _) => AdjustSplitterLayout();

            var leftLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
            leftLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            leftLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));

            _availableHeaderLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "이미지 목록",
                Font = new Font(Font, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(2, 0, 0, 0)
            };

            var leftPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 8, 0) };
            ImageListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                SelectionMode = SelectionMode.One,
                IntegralHeight = false,
                HorizontalScrollbar = true,
                BorderStyle = BorderStyle.FixedSingle
            };
            leftPanel.Controls.Add(ImageListBox);

            _selectedHeaderLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "선택된 이미지",
                Font = new Font(Font, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(2, 0, 0, 0)
            };

            var selectedPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 0, 8, 0) };
            SelectedImageListBox = new ListBox
            {
                Dock = DockStyle.Fill,
                SelectionMode = SelectionMode.One,
                IntegralHeight = false,
                HorizontalScrollbar = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(247, 251, 255)
            };
            selectedPanel.Controls.Add(SelectedImageListBox);

            leftLayout.Controls.Add(_availableHeaderLabel, 0, 0);
            leftLayout.Controls.Add(leftPanel, 0, 1);
            leftLayout.Controls.Add(_selectedHeaderLabel, 0, 2);
            leftLayout.Controls.Add(selectedPanel, 0, 3);
            _middleSplit.Panel1.Controls.Add(leftLayout);

            var previewLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            previewLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            previewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            previewLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            var previewHeader = new Label
            {
                Dock = DockStyle.Fill,
                Text = "미리보기",
                Font = new Font(Font, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(2, 0, 0, 0)
            };

            PreviewBox = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(34, 34, 34),
                BorderStyle = BorderStyle.FixedSingle
            };
            _previewHintLabel = new Label
            {
                Dock = DockStyle.Fill,
                Text = "이미지를 선택하면 미리보기가 표시됩니다.",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gainsboro,
                BackColor = Color.Transparent
            };
            PreviewBox.Controls.Add(_previewHintLabel);
            previewLayout.Controls.Add(previewHeader, 0, 0);
            previewLayout.Controls.Add(PreviewBox, 0, 1);
            _middleSplit.Panel2.Controls.Add(previewLayout);

            var bottomPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = new Padding(0, 8, 0, 0)
            };
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            bottomPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            bottomPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var extensionLabel = new Label
            {
                Text = "변환 형식",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0, 7, 8, 0)
            };
            ExtensionComboBox = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140, Margin = new Padding(0, 2, 8, 0) };
            ExtensionComboBox.Items.AddRange(new object[] { "jpeg", "png", "bmp", "gif", "tiff", "webp", "svg", "heic" });
            ExtensionComboBox.SelectedIndex = 0;
            ConvertButton = new Button
            {
                Text = "변환 실행",
                AutoSize = true,
                MinimumSize = new Size(112, 30),
                Margin = new Padding(0, 0, 0, 0),
                BackColor = Color.FromArgb(75, 135, 220),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            StatusLabel = new Label { Dock = DockStyle.Fill, Text = "상태: 대기", TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(4, 6, 0, 0), AutoSize = true };

            var convertActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Margin = new Padding(0)
            };
            convertActions.Controls.Add(extensionLabel);
            convertActions.Controls.Add(ExtensionComboBox);
            convertActions.Controls.Add(ConvertButton);

            bottomPanel.Controls.Add(convertActions, 0, 0);
            bottomPanel.Controls.Add(StatusLabel, 0, 1);

            Controls.Add(_mainLayout);
            _mainLayout.Controls.Add(topBar, 0, 0);
            _mainLayout.Controls.Add(FolderPathDisplay, 0, 1);
            _mainLayout.Controls.Add(_middleSplit, 0, 2);
            _mainLayout.Controls.Add(bottomPanel, 0, 3);

            Dock = DockStyle.Fill;

            ChooseFolderButton.Click += ChooseFolderButton_Click;
            RefreshButton.Click += RefreshButton_Click;
            FolderTextBox.TextChanged += FolderTextBox_TextChanged;
            FolderTextBox.KeyDown += FolderTextBox_KeyDown;
            SelectAllButton.Click += SelectAllButton_Click;
            ImageListBox.SelectedIndexChanged += ImageListBox_SelectedIndexChanged;
            ImageListBox.DoubleClick += ImageListBox_DoubleClick;
            SelectedImageListBox.SelectedIndexChanged += SelectedImageListBox_SelectedIndexChanged;
            SelectedImageListBox.DoubleClick += SelectedImageListBox_DoubleClick;
            ConvertButton.Click += ConvertButton_Click;

            Load += (_, _) => UpdateResponsiveLayout();
            Resize += (_, _) => UpdateResponsiveLayout();
        }

    }
}
