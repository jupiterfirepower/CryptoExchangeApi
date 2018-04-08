using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace Common.Contracts
{
    public static class LinqExtentions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return source.Where(element => seenKeys.Add(keySelector(element)));
        }

        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            var enumerable = source as T[] ?? source.ToArray();
            while (enumerable.Any())
            {
                yield return enumerable.Take(chunksize);
                source = enumerable.Skip(chunksize);
            }
        }

        public static IEnumerable<IEnumerable<T>> ChunkTrivialBetter<T>(this IEnumerable<T> source, int chunksize)
        {
            var pos = 0;
            while (source.Skip(pos).Any())
            {
                yield return source.Skip(pos).Take(chunksize);
                pos += chunksize;
            }
        }

        public static string[] ToChunks(this string message, int bufferLimit)
        {
            return Regex.Split(message, @"(?<=\G.{" + bufferLimit + "})");
        }
        public static HashSet<T> ToHashSet<T>(IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }
        public static HashSet<T> AddRange<T>(this HashSet<T> set, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                set.Add(item);
            }
            return set;
        }
        public static HashSet<T> Remove<T>(this HashSet<T> set, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                set.Remove(item);
            }
            return set;
        }
        public static IEnumerable<T> Map<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null || action == null)
                throw new ArgumentNullException();
            var enumerable = source as IList<T> ?? source.ToList();
            foreach (T element in enumerable)
                action(element);
            return enumerable;
        }

        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int size)
        {
            T[] array = null;
            int count = 0;
            foreach (T item in source)
            {
                if (array == null)
                {
                    array = new T[size];
                }
                array[count] = item;
                count++;
                if (count == size)
                {
                    yield return new ReadOnlyCollection<T>(array);
                    array = null;
                    count = 0;
                }
            }
            if (array != null)
            {
                Array.Resize(ref array, count);
                yield return new ReadOnlyCollection<T>(array);
            }
        }
    }
}
