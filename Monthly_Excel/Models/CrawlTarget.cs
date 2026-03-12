namespace Monthly_Excel.Models
{
    internal sealed class CrawlTarget
    {
        public string Url { get; init; } = string.Empty;
        public string Keyword { get; init; } = string.Empty;
        public int ColumnIndex { get; init; }
    }
}
