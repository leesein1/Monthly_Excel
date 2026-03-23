using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Monthly_Excel.Processors;

namespace Monthly_Excel.Pages.ImageConverter
{
    public partial class ImageConverterPage
    {
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

            var target = ExtensionComboBox.SelectedItem?.ToString()?.ToLowerInvariant() ?? "jpeg";
            var selectedItems = SelectedImageListBox.Items.Cast<string>().ToArray();
            var toConvert = selectedItems.Length > 0
                ? selectedItems.Select(name => Path.Combine(folderPath, name)).ToArray()
                : images;
            var result = ImageConverterProcessor.ConvertImages(folderPath, toConvert, target);
            SetStatus(result.Message);

            if (!result.Succeeded)
            {
                return;
            }

            MessageBox.Show(
                result.Message,
                "변환 완료",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            LoadImages();
        }
    }
}
