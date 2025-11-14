using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ClosedXML.Excel;

namespace Monthly_Excel
{
    public class CrawlingEventHandler
    {
        private readonly Label statusLabel;
        private readonly ProgressBar progressBar;
        private readonly List<string> urlList = new();
        private readonly List<string> keywordList = new();

        public CrawlingEventHandler(Label statusLabel, ProgressBar progressBar)
        {
            this.statusLabel = statusLabel;
            this.progressBar = progressBar;
        }

        // 엑셀 업로드 버튼 클릭 시
        public void OnUploadClicked(object sender, EventArgs e)
        {
            using OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls",
                Title = "엑셀 파일 선택"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                string filePath = ofd.FileName;
                statusLabel.Text = "엑셀 분석 중...";
                progressBar.Value = 0;

                try
                {
                    urlList.Clear();
                    keywordList.Clear();

                    using var workbook = new XLWorkbook(filePath);
                    var worksheet = workbook.Worksheet(1);

                    int rowLink = 4; // 링크 위치
                    int rowKeyword = 5; // 키워드/모바일 원본 위치
                    int col = 2; // B = 2

                    while (true)
                    {
                        string urlValue = worksheet.Cell(rowLink, col).GetString().Trim();
                        string keywordValue = worksheet.Cell(rowKeyword, col).GetString().Trim();

                        if (string.IsNullOrEmpty(urlValue)) break;
                        if (!urlValue.StartsWith("https", StringComparison.OrdinalIgnoreCase)) break;

                        urlList.Add(urlValue);
                        keywordList.Add(keywordValue); // 같이 저장

                        col++;
                    }

                    progressBar.Value = 100;
                    statusLabel.Text = $"URL {urlList.Count}개 + 키워드 {keywordList.Count}개 추출 완료";

                    MessageBox.Show(
                        "URL:\n" + string.Join(Environment.NewLine, urlList) +
                        "\n\nKeyword:\n" + string.Join(Environment.NewLine, keywordList),
                        "추출 완료",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
                catch (Exception ex)
                {
                    MessageBox.Show("엑셀 로딩 중 오류 발생: " + ex.Message,
                                    "에러",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    statusLabel.Text = "엑셀 로딩 실패";
                }
            }
        }

        // 엑셀 다운로드 버튼 클릭 시
        public async void OnDownloadClicked(object sender, EventArgs e)
        {
            if (urlList.Count == 0)
            {
                MessageBox.Show("저장할 URL이 없습니다. 먼저 엑셀 업로드를 진행하세요.",
                                "알림", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel 파일 (*.xlsx)|*.xlsx",
                Title = "엑셀 파일로 저장"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string savePath = sfd.FileName;
                statusLabel.Text = "크롤링 및 저장 중...";
                progressBar.Value = 10;

                try
                {
                    // urlList + keywordList 함께 전달
                    await CrawlingProcessor.SaveUrlsWithCrawlInfoToExcel(savePath, urlList, keywordList);

                    progressBar.Value = 100;
                    statusLabel.Text = "다운로드 완료!";
                    MessageBox.Show($"크롤링 및 저장 완료: {savePath}", "완료", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("크롤링 중 오류 발생: " + ex.Message,
                                    "에러",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                    statusLabel.Text = "크롤링 실패";
                }
            }
        }

        // 양식 다운로드 버튼 클릭 시: 팝업만 띄우기
        public void OnTemplateDownloadClicked(object sender, EventArgs e)
        {

            using var sfd = new SaveFileDialog
            {
                Title = "템플릿 저장",
                Filter = "Excel 파일 (*.xlsx)|*.xlsx",
                FileName = "크롤링_양식.xlsx",
                OverwritePrompt = true // 같은 이름 있으면 덮어쓰기 확인창
            };

            if (sfd.ShowDialog() != DialogResult.OK)
                return; // 사용자가 취소

            try
            {
                // 경로 + 파일명
                string path = sfd.FileName;

                // 저장 (네가 만든 정적 메서드)
                CrawlingProcessor.SaveCrawlingTemplate(path);

                MessageBox.Show("템플릿을 저장했습니다.", "완료",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("저장 중 오류: " + ex.Message, "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
