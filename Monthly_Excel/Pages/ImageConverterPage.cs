using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Monthly_Excel.Pages
{
    public class ImageConverterPage : UserControl
    {
        private readonly HashSet<string> _supportedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".tif", ".webp", ".svg", ".heic"
        };
        private const string SelectAllText = "전체 선택";
        private const string ClearSelectionText = "선택 해제";
        private const int PreviewMaxDimension = 1600;
        private readonly System.Windows.Forms.Timer _previewTimer;
        private int _previewRequestId;
        private readonly Label _availableHeaderLabel;
        private readonly Label _selectedHeaderLabel;
        private readonly Label _previewHintLabel;

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
            }

            base.Dispose(disposing);
        }

        public ImageConverterPage()
        {
            _previewTimer = new System.Windows.Forms.Timer { Interval = 90 };
            _previewTimer.Tick += PreviewTimer_Tick;

            var mainLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4, Padding = new Padding(12) };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));

            var topBar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4 };
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
            topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 108F));

            FolderTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 6, 8, 6),
                PlaceholderText = "이미지 폴더 경로를 입력하거나 선택하세요."
            };
            ChooseFolderButton = new Button { Text = "폴더 선택", Dock = DockStyle.Fill, Margin = new Padding(0, 6, 8, 6) };
            RefreshButton = new Button { Text = "새로고침", Dock = DockStyle.Fill, Margin = new Padding(0, 6, 8, 6) };
            SelectAllButton = new Button { Text = SelectAllText, Dock = DockStyle.Fill, Margin = new Padding(0, 6, 0, 6) };
            topBar.Controls.Add(FolderTextBox, 0, 0);
            topBar.Controls.Add(ChooseFolderButton, 1, 0);
            topBar.Controls.Add(RefreshButton, 2, 0);
            topBar.Controls.Add(SelectAllButton, 3, 0);

            FolderPathDisplay = new Label
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.DarkBlue,
                Text = "선택된 폴더: 없음",
                Padding = new Padding(4, 0, 0, 0)
            };

            var middleSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                FixedPanel = FixedPanel.None
            };
            middleSplit.HandleCreated += (_, _) => BeginInvoke(new Action(() => AdjustSplitterLayout(middleSplit)));
            middleSplit.Resize += (_, _) => AdjustSplitterLayout(middleSplit);

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
            middleSplit.Panel1.Controls.Add(leftLayout);

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
            middleSplit.Panel2.Controls.Add(previewLayout);

            var bottomPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2, Padding = new Padding(0) };
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86F));
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140F));
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112F));
            bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            bottomPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
            bottomPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));

            var extLabel = new Label { Text = "변환 형식", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill };
            ExtensionComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 4, 8, 4) };
            ExtensionComboBox.Items.AddRange(new object[] { "jpeg", "png", "bmp", "gif", "tiff", "webp", "svg", "heic" });
            ExtensionComboBox.SelectedIndex = 0;
            ConvertButton = new Button
            {
                Text = "변환 실행",
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 4, 8, 4),
                BackColor = Color.FromArgb(75, 135, 220),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            StatusLabel = new Label { Dock = DockStyle.Fill, Text = "상태: 대기", TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(4, 0, 0, 0) };

            bottomPanel.Controls.Add(extLabel, 0, 0);
            bottomPanel.Controls.Add(ExtensionComboBox, 1, 0);
            bottomPanel.Controls.Add(ConvertButton, 2, 0);
            bottomPanel.SetColumnSpan(StatusLabel, 4);
            bottomPanel.Controls.Add(StatusLabel, 0, 1);

            Controls.Add(mainLayout);
            mainLayout.Controls.Add(topBar, 0, 0);
            mainLayout.Controls.Add(FolderPathDisplay, 0, 1);
            mainLayout.Controls.Add(middleSplit, 0, 2);
            mainLayout.Controls.Add(bottomPanel, 0, 3);

            Dock = DockStyle.Fill;
            MinimumSize = new Size(860, 560);

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

            Load += (_, _) => AdjustSplitterLayout(middleSplit);
        }

        private void FolderTextBox_TextChanged(object? sender, EventArgs e)
        {
            UpdateSelectionUi();
        }

        private void FolderTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;

            e.SuppressKeyPress = true;
            LoadImages();
        }

        private void ChooseFolderButton_Click(object? sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "이미지가 들어 있는 폴더를 선택하세요.",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            FolderTextBox.Text = dialog.SelectedPath;
            LoadImages();
        }

        private void RefreshButton_Click(object? sender, EventArgs e)
        {
            LoadImages();
        }

        private void ImageListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (ImageListBox.SelectedIndex >= 0)
                SelectedImageListBox.ClearSelected();

            UpdateSelectionUi();
            QueuePreview();
        }

        private void SelectedImageListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (SelectedImageListBox.SelectedIndex >= 0)
                ImageListBox.ClearSelected();

            UpdateSelectionUi();
            QueuePreview();
        }

        private void ImageListBox_DoubleClick(object? sender, EventArgs e)
        {
            MoveSelectedItem(ImageListBox, SelectedImageListBox);
        }

        private void SelectedImageListBox_DoubleClick(object? sender, EventArgs e)
        {
            MoveSelectedItem(SelectedImageListBox, ImageListBox);
        }

        private void SelectAllButton_Click(object? sender, EventArgs e)
        {
            var totalCount = ImageListBox.Items.Count + SelectedImageListBox.Items.Count;
            if (totalCount == 0)
                return;

            if (SelectedImageListBox.Items.Count == totalCount)
            {
                MoveAllItems(SelectedImageListBox, ImageListBox);
                UpdateSelectionUi();
                SetStatus("전체 선택 해제");
                return;
            }

            MoveAllItems(ImageListBox, SelectedImageListBox);

            UpdateSelectionUi();
            SetStatus("전체 선택");
        }

        private void ConvertButton_Click(object? sender, EventArgs e)
        {
            var folderPath = ResolveFolderPath();
            if (folderPath is null)
            {
                SetStatus("폴더 경로가 없습니다.");
                return;
            }

            var images = GetImageFiles(folderPath);
            if (!images.Any())
            {
                SetStatus("이미지 파일이 없습니다.");
                return;
            }

            var target = ExtensionComboBox.SelectedItem?.ToString()?.ToLower() ?? "jpeg";
            var targetExt = target switch
            {
                "jpeg" => ".jpg",
                "png" => ".png",
                "bmp" => ".bmp",
                "gif" => ".gif",
                "webp" => ".webp",
                "tiff" => ".tiff",
                "svg" => ".svg",
                "heic" => ".heic",
                _ => ".jpg",
            };

            var selectedItems = SelectedImageListBox.Items.Cast<string>().ToArray();
            var toConvert = selectedItems.Length > 0
                ? selectedItems.Select(name => Path.Combine(folderPath, name)).ToArray()
                : images;
            var outputFolderPath = CreateOutputFolder(folderPath, target);
            var convertedCount = 0;

            foreach (var filePath in toConvert)
            {
                try
                {
                    var ext = Path.GetExtension(filePath).ToLower();
                    if (ext == targetExt)
                    {
                        continue;
                    }

                    if (target is "svg" or "heic")
                    {
                        SetStatus($"{target.ToUpper()} 변환은 현재 지원되지 않습니다.");
                        return;
                    }

                    if (ext is ".svg" or ".heic")
                    {
                        SetStatus($"{Path.GetFileName(filePath)}는 변환할 수 없는 형식입니다.");
                        continue;
                    }

                    var outputFileName = Path.ChangeExtension(Path.GetFileName(filePath), targetExt);
                    var destPath = Path.Combine(outputFolderPath, outputFileName);
                    ConvertImage(filePath, destPath, target);
                    convertedCount++;
                }
                catch (Exception ex)
                {
                    SetStatus($"변환 오류: {Path.GetFileName(filePath)} - {ex.Message}");
                    return;
                }
            }

            var completionMessage = $"변환 완료: {convertedCount}개 변환, 저장 폴더: {Path.GetFileName(outputFolderPath)}";
            SetStatus(completionMessage);
            MessageBox.Show(
                completionMessage,
                "변환 완료",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            LoadImages();
        }

        private void LoadImages(string? selectedFileName = null)
        {
            ImageListBox.BeginUpdate();
            SelectedImageListBox.BeginUpdate();
            ImageListBox.Items.Clear();
            SelectedImageListBox.Items.Clear();
            ClearPreview();

            var folderPath = ResolveFolderPath();
            if (folderPath is null)
            {
                ImageListBox.EndUpdate();
                SelectedImageListBox.EndUpdate();
                UpdateSelectionUi();
                QueuePreview();
                SetStatus("유효한 폴더를 선택하세요.");
                return;
            }

            if (!string.Equals(FolderTextBox.Text, folderPath, StringComparison.OrdinalIgnoreCase))
                FolderTextBox.Text = folderPath;

            var files = GetImageFiles(folderPath);
            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                if (string.IsNullOrWhiteSpace(fileName))
                    continue;

                ImageListBox.Items.Add(fileName);
            }

            if (!string.IsNullOrWhiteSpace(selectedFileName))
            {
                var selectedIndex = ImageListBox.FindStringExact(selectedFileName);
                if (selectedIndex >= 0)
                {
                    ImageListBox.SelectedIndex = selectedIndex;
                    ImageListBox.TopIndex = selectedIndex;
                    ImageListBox.Focus();
                }
            }

            ImageListBox.EndUpdate();
            SelectedImageListBox.EndUpdate();
            UpdateSelectionUi();
            QueuePreview();
            SetStatus($"이미지 발견: {ImageListBox.Items.Count}개");
        }

        private string[] GetImageFiles(string folder)
        {
            return Directory.GetFiles(folder)
                .Where(f => _supportedExtensions.Contains(Path.GetExtension(f).ToLower()))
                .OrderBy(Path.GetFileName, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();
        }

        private async void PreviewTimer_Tick(object? sender, EventArgs e)
        {
            _previewTimer.Stop();
            var requestId = ++_previewRequestId;
            var previewTarget = GetCurrentPreviewPath();
            if (previewTarget is null)
            {
                ClearPreview();
                _previewHintLabel.Text = "이미지를 선택하면 미리보기가 표시됩니다.";
                _previewHintLabel.Visible = true;
                return;
            }

            var (path, fileName, ext) = previewTarget.Value;
            if (ext == ".svg" || ext == ".heic")
            {
                ClearPreview();
                _previewHintLabel.Text = "이 형식은 미리보기를 지원하지 않습니다.";
                _previewHintLabel.Visible = true;
                SetStatus("미리보기 지원하지 않는 형식입니다.");
                return;
            }

            if (!File.Exists(path))
            {
                SetStatus("이미지를 찾을 수 없습니다.");
                ClearPreview();
                _previewHintLabel.Text = "이미지를 찾을 수 없습니다.";
                _previewHintLabel.Visible = true;
                return;
            }

            _previewHintLabel.Text = "미리보기 불러오는 중...";
            _previewHintLabel.Visible = true;

            try
            {
                var previewBitmap = await Task.Run(() => LoadPreviewBitmap(path));
                if (IsDisposed || requestId != _previewRequestId)
                {
                    previewBitmap?.Dispose();
                    return;
                }

                ClearPreview();
                PreviewBox.Image = previewBitmap;
                _previewHintLabel.Visible = previewBitmap is null;
                if (previewBitmap is null)
                {
                    _previewHintLabel.Text = "미리보기를 만들 수 없습니다.";
                    SetStatus("미리보기 오류: 지원되지 않거나 손상된 이미지입니다.");
                    return;
                }

                SetStatus($"미리보기: {fileName}");
            }
            catch (OutOfMemoryException)
            {
                ClearPreview();
                _previewHintLabel.Text = "이미지가 너무 크거나 손상되었습니다.";
                _previewHintLabel.Visible = true;
                SetStatus("미리보기 오류: 이미지가 너무 크거나 형식이 올바르지 않습니다.");
            }
            catch (Exception ex)
            {
                ClearPreview();
                _previewHintLabel.Text = "미리보기를 불러오지 못했습니다.";
                _previewHintLabel.Visible = true;
                SetStatus($"미리보기 오류: {ex.Message}");
            }
        }

        private void SetStatus(string text)
        {
            StatusLabel.Text = $"상태: {text}";
        }

        private void UpdateSelectionUi()
        {
            var totalCount = ImageListBox.Items.Count + SelectedImageListBox.Items.Count;
            var selectedCount = SelectedImageListBox.Items.Count;

            var folderPath = ResolveFolderPath();
            FolderPathDisplay.Text = folderPath is not null
                ? $"선택된 폴더: {folderPath}   |   선택: {selectedCount}/{totalCount}"
                : $"선택된 폴더: {FolderTextBox.Text}";

            _availableHeaderLabel.Text = $"이미지 목록 ({ImageListBox.Items.Count})";
            _selectedHeaderLabel.Text = $"선택된 이미지 ({SelectedImageListBox.Items.Count})";
            SelectAllButton.Text = selectedCount > 0 ? ClearSelectionText : SelectAllText;
        }

        private void ClearPreview()
        {
            if (PreviewBox.Image is null)
                return;

            var image = PreviewBox.Image;
            PreviewBox.Image = null;
            image.Dispose();
        }

        private string? ResolveFolderPath()
        {
            var input = FolderTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input))
                return null;

            if (Directory.Exists(input))
                return input;

            if (!File.Exists(input))
                return null;

            return Path.GetDirectoryName(input);
        }

        private void MoveSelectedItem(ListBox source, ListBox target)
        {
            if (source.SelectedItem is not string fileName)
                return;

            source.BeginUpdate();
            target.BeginUpdate();

            InsertSorted(target, fileName);
            source.Items.Remove(fileName);

            if (target.Items.Count > 0)
            {
                var selectedIndex = target.FindStringExact(fileName);
                if (selectedIndex >= 0)
                    target.SelectedIndex = selectedIndex;
            }
            else if (source.Items.Count > 0)
            {
                source.SelectedIndex = Math.Min(source.Items.Count - 1, Math.Max(0, source.SelectedIndex));
            }

            source.EndUpdate();
            target.EndUpdate();
            UpdateSelectionUi();
            QueuePreview();
        }

        private static void MoveAllItems(ListBox source, ListBox target)
        {
            if (source.Items.Count == 0)
                return;

            source.BeginUpdate();
            target.BeginUpdate();

            var items = source.Items.Cast<string>()
                .OrderBy(item => item, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();

            foreach (var item in items)
            {
                if (target.FindStringExact(item) < 0)
                    InsertSorted(target, item);
            }

            source.Items.Clear();

            source.ClearSelected();
            target.ClearSelected();

            source.EndUpdate();
            target.EndUpdate();
        }

        private static void InsertSorted(ListBox listBox, string item)
        {
            var insertIndex = 0;
            while (insertIndex < listBox.Items.Count &&
                   StringComparer.CurrentCultureIgnoreCase.Compare(listBox.Items[insertIndex]?.ToString(), item) < 0)
            {
                insertIndex++;
            }

            listBox.Items.Insert(insertIndex, item);
        }

        private static Bitmap CreatePreviewBitmap(Image image)
        {
            var scale = Math.Min(
                1d,
                Math.Min(
                    (double)PreviewMaxDimension / image.Width,
                    (double)PreviewMaxDimension / image.Height));

            var width = Math.Max(1, (int)Math.Round(image.Width * scale));
            var height = Math.Max(1, (int)Math.Round(image.Height * scale));
            var preview = new Bitmap(width, height);

            using var graphics = Graphics.FromImage(preview);
            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            graphics.DrawImage(image, 0, 0, width, height);
            return preview;
        }

        private void QueuePreview()
        {
            _previewTimer.Stop();
            _previewTimer.Start();
        }

        private (string Path, string FileName, string Extension)? GetCurrentPreviewPath()
        {
            var fileName = ImageListBox.SelectedItem as string ?? SelectedImageListBox.SelectedItem as string;
            if (fileName is null)
                return null;

            var folderPath = ResolveFolderPath();
            if (folderPath is null)
                return null;

            var path = Path.Combine(folderPath, fileName);
            return (path, fileName, Path.GetExtension(path).ToLowerInvariant());
        }

        private static Bitmap? LoadPreviewBitmap(string path)
        {
            using var image = SixLabors.ImageSharp.Image.Load(path);
            image.Mutate(context => context.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new SixLabors.ImageSharp.Size(PreviewMaxDimension, PreviewMaxDimension)
            }));

            using var ms = new MemoryStream();
            image.Save(ms, new PngEncoder());
            ms.Position = 0;
            using var tempBitmap = new Bitmap(ms);
            return new Bitmap(tempBitmap);
        }

        private static string CreateOutputFolder(string sourceFolderPath, string targetExtension)
        {
            var folderName = $"converted_{targetExtension}_{DateTime.Now:yyyyMMdd_HHmmss}";
            var candidate = Path.Combine(sourceFolderPath, folderName);
            var suffix = 1;

            while (Directory.Exists(candidate))
            {
                candidate = Path.Combine(sourceFolderPath, $"{folderName}_{suffix++}");
            }

            Directory.CreateDirectory(candidate);
            return candidate;
        }

        private static void ConvertImage(string sourcePath, string destinationPath, string target)
        {
            using var image = SixLabors.ImageSharp.Image.Load(sourcePath);
            using var outputStream = File.Create(destinationPath);
            image.Save(outputStream, GetEncoder(target));
        }

        private static IImageEncoder GetEncoder(string target)
        {
            return target switch
            {
                "png" => new PngEncoder(),
                "bmp" => new BmpEncoder(),
                "gif" => new GifEncoder(),
                "tiff" => new TiffEncoder(),
                "webp" => new WebpEncoder(),
                _ => new JpegEncoder(),
            };
        }

        private static void AdjustSplitterLayout(SplitContainer splitContainer)
        {
            var availableWidth = splitContainer.ClientSize.Width;
            if (availableWidth <= 0)
                return;

            const int desiredPanel1MinSize = 240;
            const int desiredPanel2MinSize = 320;
            var minimumRequiredWidth = desiredPanel1MinSize + desiredPanel2MinSize + splitContainer.SplitterWidth;

            if (availableWidth < minimumRequiredWidth)
            {
                splitContainer.Panel1MinSize = 0;
                splitContainer.Panel2MinSize = 0;
                return;
            }

            splitContainer.Panel1MinSize = desiredPanel1MinSize;
            splitContainer.Panel2MinSize = desiredPanel2MinSize;

            var minimumDistance = splitContainer.Panel1MinSize;
            var maximumDistance = availableWidth - splitContainer.Panel2MinSize - splitContainer.SplitterWidth;

            var preferredDistance = Math.Max(320, availableWidth / 3);
            splitContainer.SplitterDistance = Math.Min(Math.Max(preferredDistance, minimumDistance), maximumDistance);
        }
    }
}
