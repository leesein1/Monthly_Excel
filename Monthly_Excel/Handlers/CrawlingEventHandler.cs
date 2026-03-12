using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using ClosedXML.Excel;
using Monthly_Excel.Processors;

namespace Monthly_Excel.Handlers
{
    public class CrawlingEventHandler
    {
        private readonly Label _statusLabel;
        private readonly ProgressBar _progressBar;
        private readonly List<string> _urlList = new();
        private readonly List<string> _keywordList = new();

        public CrawlingEventHandler(Label statusLabel, ProgressBar progressBar)
        {
            _statusLabel = statusLabel;
            _progressBar = progressBar;
        }

        public void OnUploadClicked(object? sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls",
                Title = "엑셀 파일 선택"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            SetStatus("엑셀 분석 중...");
            SetProgress(10);

            try
            {
                LoadExcel(dialog.FileName);
                SetProgress(100);
                SetStatus($"URL {_urlList.Count}개, 키워드 {_keywordList.Count}개 추출 완료");
            }
            catch (Exception exception)
            {
                SetStatus("엑셀 로딩 실패");
                MessageBox.Show(
                    $"엑셀 로딩 중 오류 발생: {exception.Message}",
                    "에러",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        public async void OnDownloadClicked(object? sender, EventArgs e)
        {
            if (_urlList.Count == 0)
            {
                MessageBox.Show(
                    "저장할 URL이 없습니다. 먼저 엑셀 업로드를 진행하세요.",
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

            await ExecuteDownloadAsync(dialog.FileName);
        }

        public void OnTemplateDownloadClicked(object? sender, EventArgs e)
        {
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

        private async Task ExecuteDownloadAsync(string savePath)
        {
            try
            {
                SetStatus("크롤링 및 저장 중...");
                SetProgress(20);

                await CrawlingProcessor.SaveUrlsWithCrawlInfoToExcel(savePath, _urlList, _keywordList);

                SetProgress(100);
                SetStatus("다운로드 완료");
                MessageBox.Show(
                    $"크롤링 및 저장 완료: {savePath}",
                    "완료",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception exception)
            {
                SetStatus("크롤링 실패");
                MessageBox.Show(
                    $"크롤링 중 오류 발생: {exception.Message}",
                    "에러",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
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
