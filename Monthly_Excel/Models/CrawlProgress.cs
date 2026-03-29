namespace Monthly_Excel.Models
{
    public readonly record struct CrawlProgress(int Completed, int Total, string Url);
}
