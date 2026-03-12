using System;
using System.Collections.Generic;
using System.Linq;

namespace Monthly_Excel.Processors
{
    public static class KeywordProcessor
    {
        public static (List<string> left, List<string> right) ProcessKeywords(string rawInput)
        {
            var leftKeywords = new List<string>();
            var rightKeywords = new List<string>();

            if (string.IsNullOrWhiteSpace(rawInput))
            {
                return (leftKeywords, rightKeywords);
            }

            var lines = rawInput
                .Replace("\"", string.Empty)
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line));

            foreach (var line in lines)
            {
                if (line.Contains("모바일", StringComparison.Ordinal))
                {
                    AddSplitPair(line, "모바일", leftKeywords, rightKeywords);
                    continue;
                }

                if (!line.Contains("카페", StringComparison.Ordinal))
                {
                    continue;
                }

                int cafeCount = line.Split(new[] { "카페" }, StringSplitOptions.None).Length - 1;
                if (cafeCount == 1)
                {
                    AddSplitPair(line, "카페", leftKeywords, rightKeywords);
                    continue;
                }

                foreach (var segment in line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                {
                    AddSplitPair(segment, "카페", leftKeywords, rightKeywords);
                }
            }

            return (leftKeywords, rightKeywords);
        }

        private static void AddSplitPair(string source, string separator, ICollection<string> leftKeywords, ICollection<string> rightKeywords)
        {
            int index = source.IndexOf(separator, StringComparison.Ordinal);
            if (index < 0)
            {
                return;
            }

            string left = source[..index].Trim();
            string right = source[index..].Trim();

            if (!string.IsNullOrEmpty(left))
            {
                leftKeywords.Add(left);
            }

            if (!string.IsNullOrEmpty(right))
            {
                rightKeywords.Add(right);
            }
        }
    }
}
