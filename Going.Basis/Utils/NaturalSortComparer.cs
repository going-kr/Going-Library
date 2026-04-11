using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Going.Basis.Utils
{
    /// <summary>
    /// 자연어 정렬 비교기. 문자열 내의 숫자를 수치로 비교하여 "file1", "file2", "file10" 순으로 정렬한다.
    /// </summary>
    public partial class NaturalSortComparer : IComparer<string>
    {
        private readonly bool _ascending;
        private readonly bool _ignoreCase;
        private readonly CultureInfo _culture;

        [GeneratedRegex(@"(\d+)|(\D+)", RegexOptions.Compiled)]
        private static partial Regex TokenRegex();

        /// <summary>자연어 정렬 비교기를 생성한다.</summary>
        /// <param name="ascending">오름차순 여부 (기본값: true)</param>
        /// <param name="ignoreCase">대소문자 무시 여부 (기본값: true)</param>
        /// <param name="culture">문자열 비교에 사용할 CultureInfo (기본값: 현재 문화권)</param>
        public NaturalSortComparer(bool ascending = true, bool ignoreCase = true, CultureInfo? culture = null)
        {
            _ascending = ascending;
            _ignoreCase = ignoreCase;
            _culture = culture ?? CultureInfo.CurrentCulture;
        }

        /// <summary>두 문자열을 자연어 순서로 비교한다.</summary>
        /// <param name="x">비교할 첫 번째 문자열</param>
        /// <param name="y">비교할 두 번째 문자열</param>
        /// <returns>x가 y보다 앞이면 음수, 같으면 0, 뒤이면 양수</returns>
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

    /// <summary>자연어 정렬을 위한 LINQ 확장 메서드 모음</summary>
    public static class NaturalSortExtensions
    {
        /// <summary>시퀀스를 자연어 순서로 오름차순 정렬한다.</summary>
        /// <typeparam name="TSource">시퀀스 요소 타입</typeparam>
        /// <param name="source">정렬할 시퀀스</param>
        /// <param name="keySelector">정렬 키를 추출하는 함수</param>
        /// <param name="ascending">오름차순 여부 (기본값: true)</param>
        /// <param name="ignoreCase">대소문자 무시 여부 (기본값: true)</param>
        /// <returns>자연어 순서로 정렬된 시퀀스</returns>
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

        /// <summary>시퀀스를 자연어 순서로 내림차순 정렬한다.</summary>
        /// <typeparam name="TSource">시퀀스 요소 타입</typeparam>
        /// <param name="source">정렬할 시퀀스</param>
        /// <param name="keySelector">정렬 키를 추출하는 함수</param>
        /// <param name="ignoreCase">대소문자 무시 여부 (기본값: true)</param>
        /// <returns>자연어 역순으로 정렬된 시퀀스</returns>
        public static IOrderedEnumerable<TSource> OrderByNaturalDescending<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, string?> keySelector,
            bool ignoreCase = true)
        {
            return source.OrderByNatural(keySelector, ascending: false, ignoreCase);
        }

        /// <summary>리스트를 자연어 순서로 제자리 정렬한다.</summary>
        /// <typeparam name="T">리스트 요소 타입</typeparam>
        /// <param name="list">정렬할 리스트</param>
        /// <param name="keySelector">정렬 키를 추출하는 함수</param>
        /// <param name="ascending">오름차순 여부 (기본값: true)</param>
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

