using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using Monthly_Excel.Models;

namespace Monthly_Excel.Processors
{
    internal static class CrawlWorkbookWriter
    {
        public static void WriteResults(XLWorkbook workbook, IReadOnlyList<string> urls, IReadOnlyList<string> keywords, IReadOnlyCollection<CrawlResult> results)
        {
            var worksheet = workbook.Worksheets.Add("크롤링결과");

            WriteSourceArea(worksheet, urls, keywords);
            WriteDetailArea(worksheet, results);
            WriteSummaryArea(worksheet, results);
            ApplySheetStyle(worksheet);
        }

        public static void WriteTemplate(XLWorkbook workbook)
        {
            var worksheet = workbook.Worksheets.Add("크롤링결과");

            worksheet.Cell(1, 1).Value = "사용 방법";
            worksheet.Cell(2, 1).Value = "B열부터 카페 글 URL과 키워드를 입력한 뒤 크롤링을 실행하세요.";

            WriteHeaders(worksheet);

            string[] summaryHeaders = { "순위", "작성일", "글제목", "조회수", "댓글수", "키워드", "모바일노출", "링크" };
            for (int index = 0; index < summaryHeaders.Length; index++)
            {
                worksheet.Cell(11, index + 1).Value = summaryHeaders[index];
            }

            worksheet.Range("A1:H2").Style.Font.FontColor = XLColor.FromHtml("#666666");
            worksheet.Range("A11:H11").Style.Font.Bold = true;
            worksheet.Range("A11:H11").Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
            worksheet.Range("A4:H9").Style.Font.Bold = true;

            ApplySheetStyle(worksheet);
        }

        private static void WriteSourceArea(IXLWorksheet worksheet, IReadOnlyList<string> urls, IReadOnlyList<string> keywords)
        {
            WriteHeaders(worksheet);

            for (int index = 0; index < urls.Count; index++)
            {
                string url = urls[index]?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(url))
                {
                    continue;
                }

                int column = index + 2;
                string keyword = index < keywords.Count ? keywords[index]?.Trim() ?? string.Empty : string.Empty;

                var linkCell = worksheet.Cell(4, column);
                linkCell.Value = url;
                linkCell.SetHyperlink(new XLHyperlink(url));
                linkCell.Style.Font.Underline = XLFontUnderlineValues.Single;
                linkCell.Style.Font.FontColor = XLColor.Blue;

                worksheet.Cell(5, column).Value = keyword;
            }
        }

        private static void WriteDetailArea(IXLWorksheet worksheet, IReadOnlyCollection<CrawlResult> results)
        {
            foreach (var result in results)
            {
                worksheet.Cell(6, result.ColumnIndex).Value = result.Title;
                worksheet.Cell(7, result.ColumnIndex).Value = result.Views;

                if (result.WrittenDate.HasValue)
                {
                    worksheet.Cell(8, result.ColumnIndex).Value = result.WrittenDate.Value;
                    worksheet.Cell(8, result.ColumnIndex).Style.DateFormat.Format = "MM\"월\"dd\"일\"";
                }
                else
                {
                    worksheet.Cell(8, result.ColumnIndex).Value = "[작성일 파싱 실패]";
                }

                worksheet.Cell(9, result.ColumnIndex).Value = result.Comments;
            }
        }

        private static void WriteSummaryArea(IXLWorksheet worksheet, IReadOnlyCollection<CrawlResult> results)
        {
            string[] summaryHeaders = { "순위", "작성일", "글제목", "조회수", "댓글수", "키워드", "모바일노출", "링크" };
            for (int index = 0; index < summaryHeaders.Length; index++)
            {
                worksheet.Cell(11, index + 1).Value = summaryHeaders[index];
            }

            int row = 12;
            foreach (var result in results.OrderByDescending(item => item.Views).ThenBy(item => item.Url, StringComparer.OrdinalIgnoreCase))
            {
                worksheet.Cell(row, 1).Value = row - 11;

                if (result.WrittenDate.HasValue)
                {
                    worksheet.Cell(row, 2).Value = result.WrittenDate.Value;
                    worksheet.Cell(row, 2).Style.DateFormat.Format = "MM\"월\"dd\"일\"";
                }

                worksheet.Cell(row, 3).Value = result.Title;
                worksheet.Cell(row, 4).Value = result.Views;
                worksheet.Cell(row, 5).Value = result.Comments;

                if (!string.IsNullOrWhiteSpace(result.RawKeyword))
                {
                    var (leftKeywords, rightKeywords) = KeywordProcessor.ProcessKeywords(result.RawKeyword);
                    worksheet.Cell(row, 6).Value = string.Join(Environment.NewLine, leftKeywords);
                    worksheet.Cell(row, 7).Value = string.Join(Environment.NewLine, rightKeywords);
                }

                var linkCell = worksheet.Cell(row, 8);
                linkCell.Value = result.Url;
                linkCell.SetHyperlink(new XLHyperlink(result.Url));
                linkCell.Style.Font.Underline = XLFontUnderlineValues.Single;
                linkCell.Style.Font.FontColor = XLColor.Blue;

                row++;
            }
        }

        private static void WriteHeaders(IXLWorksheet worksheet)
        {
            worksheet.Cell(4, 1).Value = "링크";
            worksheet.Cell(5, 1).Value = "키워드";
            worksheet.Cell(6, 1).Value = "글제목";
            worksheet.Cell(7, 1).Value = "조회수";
            worksheet.Cell(8, 1).Value = "작성일";
            worksheet.Cell(9, 1).Value = "댓글수";
        }

        private static void ApplySheetStyle(IXLWorksheet worksheet)
        {
            worksheet.Columns().Width = 11.75;
            worksheet.Rows().Height = 20.25;
        }
    }
}
