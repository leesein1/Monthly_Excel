using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;

namespace Monthly_Excel.Pages.ImageConverter
{
    public partial class ImageConverterPage
    {
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
                var previewBitmap = TryGetCachedPreview(path);
                previewBitmap ??= await Task.Run(() => LoadPreviewBitmap(path));
                if (IsDisposed || requestId != _previewRequestId)
                {
                    previewBitmap?.Dispose();
                    return;
                }

                if (previewBitmap is not null)
                {
                    RememberPreview(path, previewBitmap);
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

        private void ClearPreview()
        {
            if (PreviewBox.Image is null)
            {
                return;
            }

            var image = PreviewBox.Image;
            PreviewBox.Image = null;
            image.Dispose();
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
            {
                return null;
            }

            var folderPath = ResolveFolderPath();
            if (folderPath is null)
            {
                return null;
            }

            var path = Path.Combine(folderPath, fileName);
            return (path, fileName, Path.GetExtension(path).ToLowerInvariant());
        }

        private Bitmap? TryGetCachedPreview(string path)
        {
            if (!_previewCache.TryGetValue(path, out var cachedBitmap))
            {
                return null;
            }

            TouchPreviewCache(path);
            return new Bitmap(cachedBitmap);
        }

        private void RememberPreview(string path, Bitmap previewBitmap)
        {
            if (_previewCache.ContainsKey(path))
            {
                TouchPreviewCache(path);
                return;
            }

            _previewCache[path] = new Bitmap(previewBitmap);
            _previewCacheOrder.AddFirst(path);

            while (_previewCacheOrder.Count > PreviewCacheCapacity)
            {
                var leastRecentNode = _previewCacheOrder.Last;
                if (leastRecentNode is null)
                {
                    break;
                }

                _previewCacheOrder.RemoveLast();
                if (_previewCache.Remove(leastRecentNode.Value, out var leastRecentBitmap))
                {
                    leastRecentBitmap.Dispose();
                }
            }
        }

        private void TouchPreviewCache(string path)
        {
            var node = _previewCacheOrder.Find(path);
            if (node is null)
            {
                return;
            }

            _previewCacheOrder.Remove(node);
            _previewCacheOrder.AddFirst(node);
        }

        private void ClearPreviewCache()
        {
            foreach (var cachedBitmap in _previewCache.Values)
            {
                cachedBitmap.Dispose();
            }

            _previewCache.Clear();
            _previewCacheOrder.Clear();
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
    }
}
