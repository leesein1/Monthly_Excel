using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Monthly_Excel.Processors;

namespace Monthly_Excel.Pages.ImageConverter
{
    public partial class ImageConverterPage
    {
        private void FolderTextBox_TextChanged(object? sender, EventArgs e)
        {
            UpdateSelectionUi();
        }

        private void FolderTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
            {
                return;
            }

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
            {
                return;
            }

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
            {
                SelectedImageListBox.ClearSelected();
            }

            UpdateSelectionUi();
            QueuePreview();
        }

        private void SelectedImageListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (SelectedImageListBox.SelectedIndex >= 0)
            {
                ImageListBox.ClearSelected();
            }

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
            {
                return;
            }

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
            {
                FolderTextBox.Text = folderPath;
            }

            var files = GetImageFiles(folderPath);
            foreach (var filePath in files)
            {
                var fileName = Path.GetFileName(filePath);
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    continue;
                }

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
            return ImageConverterProcessor.GetImageFiles(folder);
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

        private string? ResolveFolderPath()
        {
            var input = FolderTextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            if (Directory.Exists(input))
            {
                return input;
            }

            if (!File.Exists(input))
            {
                return null;
            }

            return Path.GetDirectoryName(input);
        }

        private void MoveSelectedItem(ListBox source, ListBox target)
        {
            if (source.SelectedItem is not string fileName)
            {
                return;
            }

            source.BeginUpdate();
            target.BeginUpdate();

            InsertSorted(target, fileName);
            source.Items.Remove(fileName);

            if (target.Items.Count > 0)
            {
                var selectedIndex = target.FindStringExact(fileName);
                if (selectedIndex >= 0)
                {
                    target.SelectedIndex = selectedIndex;
                }
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
            {
                return;
            }

            source.BeginUpdate();
            target.BeginUpdate();

            var items = source.Items.Cast<string>()
                .OrderBy(item => item, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();

            foreach (var item in items)
            {
                if (target.FindStringExact(item) < 0)
                {
                    InsertSorted(target, item);
                }
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
    }
}
