using System;

namespace Monthly_Excel.Models
{
    public class CrawlResult
    {
        public string Url { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Views { get; set; }
        public DateTime? WrittenDate { get; set; }
        public int Comments { get; set; }
        public int ColumnIndex { get; set; }
        public string RawKeyword { get; set; } = string.Empty;
    }
}
