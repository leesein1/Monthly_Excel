using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;

namespace Monthly_Excel.Processors
{
    public static class ImageConverterProcessor
    {
        private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".tiff", ".tif", ".webp", ".svg", ".heic"
        };

        public static string[] GetImageFiles(string folderPath)
        {
            return Directory.GetFiles(folderPath)
                .Where(filePath => SupportedExtensions.Contains(Path.GetExtension(filePath)))
                .OrderBy(Path.GetFileName, StringComparer.CurrentCultureIgnoreCase)
                .ToArray();
        }

        public static ImageConversionResult ConvertImages(string sourceFolderPath, IReadOnlyList<string> sourceFilePaths, string target)
        {
            var normalizedTarget = target.ToLowerInvariant();
            if (normalizedTarget is "svg" or "heic")
            {
                return ImageConversionResult.Fail($"{normalizedTarget.ToUpperInvariant()} 변환은 현재 지원되지 않습니다.");
            }

            var targetExtension = GetTargetExtension(normalizedTarget);
            var outputFolderPath = CreateOutputFolder(sourceFolderPath, normalizedTarget);
            var convertedCount = 0;
            var skippedCount = 0;

            foreach (var filePath in sourceFilePaths)
            {
                var ext = Path.GetExtension(filePath).ToLowerInvariant();
                if (ext == targetExtension)
                {
                    skippedCount++;
                    continue;
                }

                if (ext is ".svg" or ".heic")
                {
                    skippedCount++;
                    continue;
                }

                try
                {
                    var outputFileName = Path.ChangeExtension(Path.GetFileName(filePath), targetExtension);
                    var destinationPath = Path.Combine(outputFolderPath, outputFileName);
                    ConvertImage(filePath, destinationPath, normalizedTarget);
                    convertedCount++;
                }
                catch (Exception ex)
                {
                    return ImageConversionResult.Fail(
                        $"변환 오류: {Path.GetFileName(filePath)} - {ex.Message}",
                        outputFolderPath,
                        convertedCount,
                        skippedCount);
                }
            }

            var completionMessage = $"변환 완료: {convertedCount}개 변환, 저장 폴더: {Path.GetFileName(outputFolderPath)}";
            return ImageConversionResult.Success(completionMessage, outputFolderPath, convertedCount, skippedCount);
        }

        private static string GetTargetExtension(string target)
        {
            return target switch
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
    }

    public sealed class ImageConversionResult
    {
        private ImageConversionResult(bool succeeded, string message, string? outputFolderPath, int convertedCount, int skippedCount)
        {
            Succeeded = succeeded;
            Message = message;
            OutputFolderPath = outputFolderPath;
            ConvertedCount = convertedCount;
            SkippedCount = skippedCount;
        }

        public bool Succeeded { get; }
        public string Message { get; }
        public string? OutputFolderPath { get; }
        public int ConvertedCount { get; }
        public int SkippedCount { get; }

        public static ImageConversionResult Success(string message, string outputFolderPath, int convertedCount, int skippedCount)
        {
            return new ImageConversionResult(true, message, outputFolderPath, convertedCount, skippedCount);
        }

        public static ImageConversionResult Fail(string message, string? outputFolderPath = null, int convertedCount = 0, int skippedCount = 0)
        {
            return new ImageConversionResult(false, message, outputFolderPath, convertedCount, skippedCount);
        }
    }
}
