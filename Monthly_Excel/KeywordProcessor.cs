// KeywordProcessor.cs
using System.Collections.Generic;
using System.Linq;

namespace Monthly_Excel
{
    public static class KeywordProcessor
    {
        public static (List<string> left, List<string> right) ProcessKeywords(string rawInput)
        {
            var leftList = new List<string>();
            var rightList = new List<string>();

            var lines = rawInput
                .Replace("\"", "")
                .Split(new[] { "\r\n", "\n" }, System.StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            foreach (var line in lines)
            {
                if (line.Contains("모바일"))
                {
                    int idx = line.IndexOf("모바일");
                    if (idx >= 0)
                    {
                        string before = line.Substring(0, idx).Trim();
                        string after = line.Substring(idx).Trim();
                        if (!string.IsNullOrEmpty(before)) leftList.Add(before);
                        if (!string.IsNullOrEmpty(after)) rightList.Add(after);
                    }
                }
                else if (line.Contains("카페"))
                {
                    int cafeCount = line.Split(new[] { "카페" }, System.StringSplitOptions.None).Length - 1;

                    if (cafeCount == 1)
                    {
                        int idx = line.IndexOf("카페");
                        if (idx >= 0)
                        {
                            string before = line.Substring(0, idx).Trim();
                            string after = line.Substring(idx).Trim();
                            if (!string.IsNullOrEmpty(before)) leftList.Add(before);
                            if (!string.IsNullOrEmpty(after)) rightList.Add(after);
                        }
                    }
                    else if (cafeCount >= 2)
                    {
                        string[] segments = line.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                        foreach (var segment in segments)
                        {
                            string seg = segment.Trim();
                            int idx = seg.IndexOf("카페");
                            if (idx >= 0)
                            {
                                string before = seg.Substring(0, idx).Trim();
                                string after = seg.Substring(idx).Trim();
                                if (!string.IsNullOrEmpty(before)) leftList.Add(before);
                                if (!string.IsNullOrEmpty(after)) rightList.Add(after);
                            }
                        }
                    }
                }
            }

            return (leftList, rightList);
        }
    }
}
