using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Monthly_Excel.Models;

namespace Monthly_Excel.Processors
{
    public static class CrawlingProcessor
    {
        public static async Task SaveUrlsWithCrawlInfoToExcel(string savePath, IReadOnlyList<string> urlList, IReadOnlyList<string> keywordList)
        {
            if (urlList == null || urlList.Count == 0)
            {
                throw new ArgumentException("크롤링할 URL이 없습니다.", nameof(urlList));
            }

            var crawlTargets = urlList
                .Select((url, index) => new
                {
                    Url = url?.Trim() ?? string.Empty,
                    Keyword = index < keywordList.Count ? keywordList[index]?.Trim() ?? string.Empty : string.Empty,
                    ColumnIndex = index + 2
                })
                .Where(item => !string.IsNullOrWhiteSpace(item.Url))
                .Select(item => new CrawlTarget
                {
                    Url = item.Url,
                    Keyword = item.Keyword,
                    ColumnIndex = item.ColumnIndex
                })
                .ToList();

            if (crawlTargets.Count == 0)
            {
                throw new ArgumentException("유효한 URL이 없습니다.", nameof(urlList));
            }

            var results = await CrawlAllAsync(crawlTargets);

            using var workbook = new XLWorkbook();
            CrawlWorkbookWriter.WriteResults(
                workbook,
                crawlTargets.Select(item => item.Url).ToList(),
                crawlTargets.Select(item => item.Keyword).ToList(),
                results
            );
            workbook.SaveAs(savePath);
        }

        public static void SaveCrawlingTemplate(string path)
        {
            try
            {
                using var workbook = new XLWorkbook();
                CrawlWorkbookWriter.WriteTemplate(workbook);
                workbook.SaveAs(path);
            }
            catch (IOException exception)
            {
                throw new IOException($"파일 저장 중 오류 발생(파일이 열려 있을 수 있음): {exception.Message}", exception);
            }
            catch (Exception exception)
            {
                throw new Exception($"템플릿 저장 실패: {exception.Message}", exception);
            }
        }

        private static async Task<IReadOnlyCollection<CrawlResult>> CrawlAllAsync(IReadOnlyList<CrawlTarget> crawlTargets)
        {
            int workerCount = crawlTargets.Count < 3 ? 1 : 3;
            var partitions = MakeContiguousPartitions(crawlTargets, workerCount);
            var results = new ConcurrentBag<CrawlResult>();

            var tasks = partitions.Select(async partition =>
            {
                using var crawler = new CafeArticleCrawler();

                foreach (var target in partition)
                {
                    var result = await crawler.CrawlAsync(target.Url, target.Keyword, target.ColumnIndex);
                    results.Add(result);
                }
            });

            await Task.WhenAll(tasks);
            return results.ToList();
        }

        private static IReadOnlyList<IReadOnlyList<T>> MakeContiguousPartitions<T>(IReadOnlyList<T> source, int partitionCount)
        {
            var partitions = new List<IReadOnlyList<T>>();
            if (source.Count == 0 || partitionCount <= 0)
            {
                return partitions;
            }

            int count = Math.Min(partitionCount, source.Count);
            int quotient = source.Count / count;
            int remainder = source.Count % count;
            int startIndex = 0;

            for (int index = 0; index < count; index++)
            {
                int size = quotient + (index < remainder ? 1 : 0);
                partitions.Add(source.Skip(startIndex).Take(size).ToList());
                startIndex += size;
            }

            return partitions;
        }
    }
}
