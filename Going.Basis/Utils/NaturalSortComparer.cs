using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Going.Basis.Utils
{
    public partial class NaturalSortComparer : IComparer<string>
    {
        private readonly bool _ascending;
        private readonly bool _ignoreCase;
        private readonly CultureInfo _culture;

        [GeneratedRegex(@"(\d+)|(\D+)", RegexOptions.Compiled)]
        private static partial Regex TokenRegex();

        public NaturalSortComparer(bool ascending = true, bool ignoreCase = true, CultureInfo? culture = null)
        {
            _ascending = ascending;
            _ignoreCase = ignoreCase;
            _culture = culture ?? CultureInfo.CurrentCulture;
        }

        public int Compare(string? x, string? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            if (x == y) return 0;

            var xTokens = Tokenize(x);
            var yTokens = Tokenize(y);

            int result = CompareTokens(xTokens, yTokens);
            return _ascending ? result : -result;
        }

        private List<Token> Tokenize(string text)
        {
            var tokens = new List<Token>();
            var matches = TokenRegex().Matches(text);

            foreach (Match match in matches)
            {
                if (string.IsNullOrEmpty(match.Value))
                    continue;

                if (char.IsDigit(match.Value[0]))
                {
                    if (long.TryParse(match.Value, out long numValue))
                    {
                        tokens.Add(new Token(true, numValue, match.Value));
                    }
                    else
                    {
                        tokens.Add(new Token(false, 0, match.Value));
                    }
                }
                else
                {
                    tokens.Add(new Token(false, 0, match.Value));
                }
            }

            return tokens;
        }

        private int CompareTokens(List<Token> xTokens, List<Token> yTokens)
        {
            int minLength = Math.Min(xTokens.Count, yTokens.Count);

            for (int i = 0; i < minLength; i++)
            {
                var xToken = xTokens[i];
                var yToken = yTokens[i];

                if (xToken.IsNumeric && yToken.IsNumeric)
                {
                    int numCompare = xToken.NumericValue.CompareTo(yToken.NumericValue);
                    if (numCompare != 0)
                        return numCompare;

                    int strCompare = CompareStrings(xToken.StringValue, yToken.StringValue);
                    if (strCompare != 0)
                        return strCompare;
                }
                else if (xToken.IsNumeric)
                {
                    return -1;
                }
                else if (yToken.IsNumeric)
                {
                    return 1;
                }
                else
                {
                    int strCompare = CompareStrings(xToken.StringValue, yToken.StringValue);
                    if (strCompare != 0)
                        return strCompare;
                }
            }

            return xTokens.Count.CompareTo(yTokens.Count);
        }

        private int CompareStrings(string x, string y)
        {
            return string.Compare(x, y, _ignoreCase, _culture);
        }

        private readonly record struct Token(bool IsNumeric, long NumericValue, string StringValue);
    }

    public static class NaturalSortExtensions
    {
        public static IOrderedEnumerable<TSource> OrderByNatural<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, string?> keySelector,
            bool ascending = true,
            bool ignoreCase = true)
        {
            var comparer = new NaturalSortComparer(ascending, ignoreCase);
            return ascending
                ? source.OrderBy(keySelector, comparer)
                : source.OrderByDescending(keySelector, comparer);
        }

        public static IOrderedEnumerable<TSource> OrderByNaturalDescending<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, string?> keySelector,
            bool ignoreCase = true)
        {
            return source.OrderByNatural(keySelector, ascending: false, ignoreCase);
        }

        public static void SortNatural<T>(this List<T> list, Func<T, string?> keySelector, bool ascending = true)
        {
            var comparer = new NaturalSortComparer(ascending);
            list.Sort((x, y) =>
            {
                var xKey = keySelector(x);
                var yKey = keySelector(y);
                return comparer.Compare(xKey, yKey);
            });
        }
    }
}

