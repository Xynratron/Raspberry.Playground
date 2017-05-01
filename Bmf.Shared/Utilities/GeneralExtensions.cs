using System;
using System.Collections.Generic;
using System.Linq;

namespace Bmf.Shared.Utilities.Linq
{
    public static class GeneralExtensions
    {
        public static IEnumerable<T> Distinct<T>(this IEnumerable<T> list, Func<T, object> keyExtractor)
        {
            return list.Distinct(new KeyEqualityComparer<T>(keyExtractor));
        }

        public static bool Contains<T>(this IEnumerable<T> list, T item, Func<T, object> keyExtractor)
        {
            return list.Contains(item, new KeyEqualityComparer<T>(keyExtractor));
        }

        public static IEnumerable<T> Except<T>(this IEnumerable<T> list, IEnumerable<T> except,
            Func<T, object> keyExtractor)
        {
            return list.Except(except, new KeyEqualityComparer<T>(keyExtractor));
        }

        public static IEnumerable<T> Intersect<T>(this IEnumerable<T> list, IEnumerable<T> toIntersect,
            Func<T, object> keyExtractor)
        {
            return list.Intersect(toIntersect, new KeyEqualityComparer<T>(keyExtractor));
        }

        public static IOrderedEnumerable<T> OrderBy<T, TKey>(this IEnumerable<T> list, Func<T, TKey> keySelector,
            Func<TKey, TKey, int> comparer)
        {
            return list.OrderBy(keySelector, new Comparer<TKey>(comparer));
        }

        public static IOrderedEnumerable<T> OrderByDescending<T, TKey>(this IEnumerable<T> list,
            Func<T, TKey> keySelector,
            Func<TKey, TKey, int> comparer)
        {
            return list.OrderByDescending(keySelector, new Comparer<TKey>(comparer));
        }

        public static bool SequenceEqual<T>(this IEnumerable<T> list, IEnumerable<T> sequenceToEqual,
            Func<T, object> keyExtractor)
        {
            return list.SequenceEqual(sequenceToEqual, new KeyEqualityComparer<T>(keyExtractor));
        }

        public static IEnumerable<TSource> Union<TSource>(this IEnumerable<TSource> first,
            IEnumerable<TSource> second,
            Func<TSource, object> keyExtractor)
        {
            return first.Union(second, new KeyEqualityComparer<TSource>(keyExtractor));
        }

        public static IEnumerable<T> ToEnumerable<T>(this T o)
        {
            return new List<T> { o };
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static string FormatIt(this string val, params object[] args)
        {
            return string.Format(val, args);
        }
    }
}