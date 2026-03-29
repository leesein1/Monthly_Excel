using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using Monthly_Excel.Models;
using Monthly_Excel.Processors;

namespace Monthly_Excel.Handlers
{
    public class CrawlingEventHandler
    {
        private readonly Label _statusLabel;
        private readonly ProgressBar _progressBar;
        private readonly Button _uploadButton;
        private readonly Button _downloadButton;
        private readonly Button _templateDownloadButton;
        private readonly List<string> _urlList = new();
        private readonly List<string> _keywordList = new();

        private bool _isDownloading;
        private CancellationTokenSource? _downloadCts;

        public CrawlingEventHandler(
            Label statusLabel,
            ProgressBar progressBar,
            Button uploadButton,
            Button downloadButton,
            Button templateDownloadButton)
        {
            _statusLabel = statusLabel;
            _progressBar = progressBar;
            _uploadButton = uploadButton;
            _downloadButton = downloadButton;
            _templateDownloadButton = templateDownloadButton;
        }

        public void OnUploadClicked(object? sender, EventArgs e)
        {
            if (_isDownloading)
            {
                MessageBox.Show(
                    "크롤링 실행 중에는 업로드를 할 수 없습니다.",
                    "안내",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            using var dialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls",
                Title = "원본 파일 선택"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            SetStatus("원본 분석 중...");
            SetProgress(10);

            try
            {
                LoadExcel(dialog.FileName);
                SetProgress(100);
                SetStatus($"URL {_urlList.Count}개, 키워드 {_keywordList.Count}개 추출 완료");
            }
            catch (Exception exception)
            {
                SetStatus("원본 로딩 실패");
                MessageBox.Show(
                    $"원본 로딩 중 오류 발생: {exception.Message}",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        public async void OnDownloadClicked(object? sender, EventArgs e)
        {
            if (_isDownloading)
            {
                _downloadCts?.Cancel();
                SetStatus("취소 요청 중...");
                return;
            }

            if (_urlList.Count == 0)
            {
                MessageBox.Show(
                    "다운로드할 URL이 없습니다. 먼저 원본 업로드를 진행해주세요.",
                    "알림",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Filter = "Excel 파일 (*.xlsx)|*.xlsx",
                Title = "엑셀 파일로 저장",
                FileName = $"크롤링결과_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            _downloadCts = new CancellationTokenSource();
            await ExecuteDownloadAsync(dialog.FileName, _downloadCts.Token);
        }

        public void OnTemplateDownloadClicked(object? sender, EventArgs e)
        {
            if (_isDownloading)
            {
                MessageBox.Show(
                    "크롤링 실행 중에는 템플릿 저장을 할 수 없습니다.",
                    "안내",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            using var dialog = new SaveFileDialog
            {
                Title = "템플릿 저장",
                Filter = "Excel 파일 (*.xlsx)|*.xlsx",
                FileName = "크롤링_양식.xlsx",
                OverwritePrompt = true
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            try
            {
                CrawlingProcessor.SaveCrawlingTemplate(dialog.FileName);
                SetStatus("템플릿 저장 완료");
                MessageBox.Show("템플릿을 저장했습니다.", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception exception)
            {
                SetStatus("템플릿 저장 실패");
                MessageBox.Show(
                    $"저장 중 오류: {exception.Message}",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        public void CancelDownload()
        {
            _downloadCts?.Cancel();
        }

        private void LoadExcel(string filePath)
        {
            _urlList.Clear();
            _keywordList.Clear();

            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1);

            int rowLink = 4;
            int rowKeyword = 5;
            int column = 2;

            while (true)
            {
                string url = worksheet.Cell(rowLink, column).GetString().Trim();
                string keyword = worksheet.Cell(rowKeyword, column).GetString().Trim();

                if (string.IsNullOrWhiteSpace(url) ||
                    !url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                _urlList.Add(url);
                _keywordList.Add(keyword);
                column++;
            }
        }

        private async Task ExecuteDownloadAsync(string savePath, CancellationToken cancellationToken)
        {
            SetBusy(true);
            SetStatus("크롤링 및 저장 중...");
            SetProgress(0);

            var progress = new Progress<CrawlProgress>(update =>
            {
                int value = update.Total <= 0 ? 0 : (int)Math.Round((double)update.Completed / update.Total * 100);
                SetProgress(value);
                SetStatus($"크롤링 진행 중... ({update.Completed}/{update.Total})");
            });

            try
            {
                await CrawlingProcessor.SaveUrlsWithCrawlInfoToExcel(
                    savePath,
                    _urlList,
                    _keywordList,
                    progress,
                    cancellationToken,
                    maxConcurrency: 2);

                SetProgress(100);
                SetStatus("다운로드 완료");
                MessageBox.Show(
                    $"크롤링 결과 저장 완료: {savePath}",
                    "완료",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (OperationCanceledException)
            {
                SetStatus("작업이 취소되었습니다.");
                SetProgress(0);
            }
            catch (Exception exception)
            {
                SetStatus("크롤링 실패");
                MessageBox.Show(
                    $"크롤링 중 오류 발생: {exception.Message}",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            finally
            {
                _downloadCts?.Dispose();
                _downloadCts = null;
                SetBusy(false);
            }
        }

        private void SetBusy(bool isBusy)
        {
            _isDownloading = isBusy;
            _uploadButton.Enabled = !isBusy;
            _templateDownloadButton.Enabled = !isBusy;
            _downloadButton.Text = isBusy ? "취소" : "다운로드";
            _downloadButton.Enabled = true;
        }

        private void SetStatus(string message)
        {
            _statusLabel.Text = message;
        }

        private void SetProgress(int value)
        {
            _progressBar.Value = Math.Max(_progressBar.Minimum, Math.Min(_progressBar.Maximum, value));
        }
    }
}
