﻿// Needed for NET40

#pragma warning disable CC0031 // Check for null before calling a delegate
#pragma warning disable RCS1224 // Make method an extension method.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Theraot.Collections
{
    [DebuggerNonUserCode]
    public static partial class Extensions
    {
        public static T[] AddFirst<T>(this T[] array, T item)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            // Copyright (c) Microsoft. All rights reserved.
            // Licensed under the MIT license. See LICENSE file in the project root for full license information.
            var res = new T[array.Length + 1];
            res[0] = item;
            array.CopyTo(res, 1);
            return res;
        }

        public static ReadOnlyCollectionEx<T> AddFirst<T>(this ReadOnlyCollection<T> list, T item)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            // Copyright (c) Microsoft. All rights reserved.
            // Licensed under the MIT license. See LICENSE file in the project root for full license information.
            var res = new T[list.Count + 1];
            res[0] = item;
            list.CopyTo(res, 1);
            return ReadOnlyCollectionEx.Create(res);
        }

        public static T[] AddLast<T>(this T[] array, T item)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            // Copyright (c) Microsoft. All rights reserved.
            // Licensed under the MIT license. See LICENSE file in the project root for full license information.
            var res = new T[array.Length + 1];
            array.CopyTo(res, 0);
            res[array.Length] = item;
            return res;
        }

        public static ReadOnlyCollectionEx<T> AddLast<T>(this ReadOnlyCollection<T> list, T item)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            var res = new T[list.Count + 1];
            list.CopyTo(res, 0);
            res[list.Count] = item;
            return ReadOnlyCollectionEx.Create(res);
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static void CanCopyTo(int count, Array array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (count > array.Length)
            {
                throw new ArgumentException("The array can not contain the number of elements.", nameof(array));
            }
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static void CanCopyTo(int count, Array array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Non-negative number is required.");
            }

            if (count > array.Length - arrayIndex)
            {
                throw new ArgumentException("The array can not contain the number of elements.", nameof(array));
            }
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static void CanCopyTo<T>(int count, T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (count > array.Length)
            {
                throw new ArgumentException("The array can not contain the number of elements.", nameof(array));
            }
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static void CanCopyTo<T>(int count, T[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Non-negative number is required.");
            }

            if (count > array.Length - arrayIndex)
            {
                throw new ArgumentException("The array can not contain the number of elements.", nameof(array));
            }
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static void CanCopyTo<T>(T[] array, int arrayIndex, int countLimit)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "Non-negative number is required.");
            }

            if (countLimit < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(countLimit), "Non-negative number is required.");
            }

            if (countLimit > array.Length - arrayIndex)
            {
                throw new ArgumentException("The array can not contain the number of elements.", nameof(array));
            }
        }

        public static void Consume<T>(this IEnumerable<T> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            foreach (var _ in source)
            {
                // Empty
            }
        }

        public static bool ContainsAny<T>(this IEnumerable<T> source, IEnumerable<T> items)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
            var localCollection = AsICollection(source);
            return items.Any(item => localCollection.Contains(item, comparer));
        }

        public static List<TOutput> ConvertFiltered<T, TOutput>(this IEnumerable<T> source, Func<T, TOutput> converter, Predicate<T> filter)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            return (from item in source where filter(item) select converter(item)).ToList();
        }

        public static IEnumerable<TOutput> ConvertProgressive<T, TOutput>(this IEnumerable<T> source, Func<T, TOutput> converter)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            return ConvertProgressiveExtracted();

            IEnumerable<TOutput> ConvertProgressiveExtracted()
            {
                foreach (var item in source)
                {
                    yield return converter(item);
                }
            }
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static void CopyTo<T>(this IEnumerable<T> source, T[] array)
        {
            CopyTo(source, array, 0);
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static void CopyTo<T>(this IEnumerable<T> source, int sourceIndex, T[] array)
        {
            CopyTo(source.Skip(sourceIndex), array, 0);
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static void CopyTo<T>(this IEnumerable<T> source, T[] array, int arrayIndex)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            try
            {
                var index = arrayIndex;
                foreach (var item in source)
                {
                    array[index] = item;
                    index++;
                }
            }
            catch (IndexOutOfRangeException exception)
            {
                throw new ArgumentException(exception.Message, nameof(array));
            }
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static void CopyTo<T>(this IEnumerable<T> source, int sourceIndex, T[] array, int arrayIndex)
        {
            CopyTo(source.Skip(sourceIndex), array, arrayIndex);
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static void CopyTo<T>(this IEnumerable<T> source, T[] array, int arrayIndex, int countLimit)
        {
            CopyTo(source.Take(countLimit), array, arrayIndex);
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static void CopyTo<T>(this IEnumerable<T> source, int sourceIndex, T[] array, int arrayIndex, int countLimit)
        {
            CopyTo(source.Skip(sourceIndex).Take(countLimit), array, arrayIndex);
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static void DeprecatedCopyTo<T>(this IEnumerable<T> source, Array array)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            var index = 0;
            foreach (var item in source)
            {
                array.SetValue(item, index++);
            }
        }

        [MethodImpl(MethodImplOptionsEx.AggressiveInlining)]
        public static void DeprecatedCopyTo<T>(this IEnumerable<T> source, Array array, int index)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            foreach (var item in source)
            {
                array.SetValue(item, index++);
            }
        }

        public static bool Dequeue<T>(this Queue<T> source, T item, IEqualityComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            if (!comparer.Equals(item, source.Peek()))
            {
                return false;
            }

            source.Dequeue();
            return true;
        }

        public static int ExceptWith<T>(this ICollection<T> source, IEnumerable<T> other)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            var count = 0;
            foreach (var item in other)
            {
                while (source.Remove(item))
                {
                    count++;
                }
            }

            return count;
        }

        public static IEnumerable<T> ExceptWithEnumerable<T>(this ICollection<T> source, IEnumerable<T> other)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return ExceptWithEnumerableExtracted();

            IEnumerable<T> ExceptWithEnumerableExtracted()
            {
                foreach (var item in other)
                {
                    while (source.Remove(item))
                    {
                        yield return item;
                    }
                }
            }
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return FlattenExtracted();

            IEnumerable<T> FlattenExtracted()
            {
                foreach (var key in source)
                {
                    foreach (var item in key)
                    {
                        yield return item;
                    }
                }
            }
        }

        public static int IndexOf<T>(this IEnumerable<T> source, T item)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var currentIndex = 0;
            var comparer = EqualityComparer<T>.Default;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (comparer.Equals(enumerator.Current, item))
                    {
                        return currentIndex;
                    }

                    currentIndex++;
                }

                return -1;
            }
        }

        public static int IndexOf<T>(this IEnumerable<T> source, T item, IEqualityComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var currentIndex = 0;
            comparer = comparer ?? EqualityComparer<T>.Default;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (comparer.Equals(enumerator.Current, item))
                    {
                        return currentIndex;
                    }

                    currentIndex++;
                }

                return -1;
            }
        }

        public static int IntersectWith<T>(this ICollection<T> source, IEnumerable<T> other)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            var otherAsCollection = AsICollection(other);
            return source.RemoveWhere(input => !otherAsCollection.Contains(input));
        }

        public static int IntersectWith<T>(this ICollection<T> source, IEnumerable<T> other, IEqualityComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            comparer = comparer ?? EqualityComparer<T>.Default;
            var otherAsCollection = AsICollection(other);
            return source.RemoveWhere(input => !otherAsCollection.Contains(input, comparer));
        }

        public static bool IsProperSubsetOf<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return IsSubsetOf(source, other, true);
        }

        public static bool IsProperSupersetOf<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return IsSupersetOf(source, other, true);
        }

        public static bool IsSubsetOf<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return IsSubsetOf(source, other, false);
        }

        public static bool IsSupersetOf<T>(this IEnumerable<T> source, IEnumerable<T> other)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            return IsSupersetOf(source, other, false);
        }

        public static bool ListEquals<T>(this IList<T> first, IList<T> second)
        {
            if (ReferenceEquals(first, second))
            {
                return true;
            }

            var count = first.Count;
            if (count != second.Count)
            {
                return false;
            }

            var cmp = EqualityComparer<T>.Default;
            for (var i = 0; i != count; ++i)
            {
                if (!cmp.Equals(first[i], second[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static void Move<T>(this IList<T> list, int oldIndex, int newIndex)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            var item = list[oldIndex];
            list.RemoveAt(oldIndex);
            if (newIndex > oldIndex)
            {
                newIndex--;
            }

            list.Insert(newIndex, item);
        }

        public static bool Overlaps<T>(this IEnumerable<T> source, IEnumerable<T> items)
        {
            return ContainsAny(source, items);
        }

        public static bool Pop<T>(this Stack<T> source, T item, IEqualityComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            if (!comparer.Equals(item, source.Peek()))
            {
                return false;
            }

            source.Pop();
            return true;
        }

        public static bool ReadOnlyListEquals<T>(this IReadOnlyList<T> first, IReadOnlyList<T> second)
        {
            if (ReferenceEquals(first, second))
            {
                return true;
            }

            var count = first.Count;
            if (count != second.Count)
            {
                return false;
            }

            var cmp = EqualityComparer<T>.Default;
            for (var i = 0; i != count; ++i)
            {
                if (!cmp.Equals(first[i], second[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool Remove<T>(this ICollection<T> source, T item, IEqualityComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (comparer == null)
            {
                comparer = EqualityComparer<T>.Default;
            }

            return source.RemoveWhereEnumerable(input => comparer.Equals(input, item)).Any();
        }

        public static T[] RemoveFirst<T>(this T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            // Copyright (c) Microsoft. All rights reserved.
            // Licensed under the MIT license. See LICENSE file in the project root for full license information.
            var result = new T[array.Length - 1];
            Array.Copy(array, 1, result, 0, result.Length);
            return result;
        }

        public static T[] RemoveLast<T>(this T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            // Copyright (c) Microsoft. All rights reserved.
            // Licensed under the MIT license. See LICENSE file in the project root for full license information.
            var result = new T[array.Length - 1];
            Array.Copy(array, 0, result, 0, result.Length);
            return result;
        }

        public static int RemoveWhere<T>(this ICollection<T> source, Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return RemoveWhere(source, items => items.Where(predicate));
        }

        public static int RemoveWhere<T>(this ICollection<T> source, Func<IEnumerable<T>, IEnumerable<T>> converter)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            return ExceptWith
            (
                source,
                new List<T>(converter.Invoke(source))
            );
        }

        public static IEnumerable<T> RemoveWhereEnumerable<T>(this ICollection<T> source, Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return RemoveWhereEnumerable(source, items => items.Where(predicate));
        }

        public static IEnumerable<T> RemoveWhereEnumerable<T>(this ICollection<T> source, Func<IEnumerable<T>, IEnumerable<T>> converter)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            return ExceptWithEnumerable
            (
                source,
                new List<T>(converter.Invoke(source))
            );
        }

        public static bool SetEquals<T>(this ICollection<T> source, IEnumerable<T> other)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            var thatAsCollection = AsICollection(other);
            return thatAsCollection.All(source.Contains) && source.All(input => thatAsCollection.Contains(input));
        }

        public static void Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (indexA < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(indexA), "Non-negative number is required.");
            }

            if (indexB < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(indexB), "Non-negative number is required.");
            }

            var listCount = list.Count;
            if (indexA >= listCount || indexB >= listCount)
            {
                throw new ArgumentException("The list does not contain the number of elements.", nameof(list));
            }

            if (indexA != indexB)
            {
                SwapExtracted(list, indexA, indexB);
            }
        }

        public static int SymmetricExceptWith<T>(this ICollection<T> source, IEnumerable<T> other)
        {
            return source.AddRange(other.Distinct().Where(input => !source.Remove(input)));
        }

        public static bool TryTake<T>(this Stack<T> stack, out T item)
        {
            if (stack == null)
            {
                throw new ArgumentNullException(nameof(stack));
            }

            try
            {
                item = stack.Pop();
                return true;
            }
            catch (InvalidOperationException)
            {
                item = default;
                return false;
            }
        }

        public static int UnionWith<T>(this ICollection<T> source, IEnumerable<T> other)
        {
            return source.AddRange(other.Where(input => !source.Contains(input)));
        }

        private static bool IsSubsetOf<T>(this IEnumerable<T> source, IEnumerable<T> other, bool proper)
        {
            var @this = AsDistinctICollection(source);
            var that = AsDistinctICollection(other);
            var elementCount = 0;
            var matchCount = 0;
            foreach (var item in that)
            {
                elementCount++;
                if (@this.Contains(item))
                {
                    matchCount++;
                }
            }

            if (proper)
            {
                return matchCount == @this.Count && elementCount > @this.Count;
            }

            return matchCount == @this.Count;
        }

        private static bool IsSupersetOf<T>(this IEnumerable<T> source, IEnumerable<T> other, bool proper)
        {
            var @this = AsDistinctICollection(source);
            var that = AsDistinctICollection(other);
            var elementCount = 0;
            foreach (var item in that)
            {
                elementCount++;
                if (!@this.Contains(item))
                {
                    return false;
                }
            }

            if (proper)
            {
                return elementCount < @this.Count;
            }

            return true;
        }

        private static void SwapExtracted<T>(IList<T> list, int indexA, int indexB)
        {
            var itemA = list[indexA];
            var itemB = list[indexB];
            list[indexA] = itemB;
            list[indexB] = itemA;
        }
    }

    public static partial class Extensions
    {
        public static List<TOutput> ConvertAll<T, TOutput>(this IEnumerable<T> source, Func<T, TOutput> converter)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            return source.Select(converter).ToList();
        }

        public static TList ConvertAll<T, TOutput, TList>(this IEnumerable<T> source, Func<T, TOutput> converter)
            where TList : ICollection<TOutput>, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }

            var result = new TList();
            foreach (var item in source)
            {
                result.Add(converter(item));
            }

            return result;
        }

        public static int CountContiguousItems<T>(this IEnumerable<T> source, T item)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var result = 0;
            var equalityComparer = EqualityComparer<T>.Default;
            foreach (var value in source)
            {
                if (equalityComparer.Equals(value, item))
                {
                    result++;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        public static int CountContiguousItemsWhere<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            var result = 0;
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    result++;
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        public static int CountItems<T>(this IEnumerable<T> source, T item)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var equalityComparer = EqualityComparer<T>.Default;
            return source.Count(value => equalityComparer.Equals(value, item));
        }

        public static int CountItemsWhere<T>(this IEnumerable<T> source, Func<T, bool> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return source.Count(predicate);
        }
    }

    public static partial class Extensions
    {
#if NET35
        public static bool Contains<T>(this IEnumerable<T> source, IEnumerable<T> items)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            var localComparer = EqualityComparer<T>.Default;
            var localCollection = AsICollection(source);
            return items.All(item => localCollection.Contains(item, localComparer));
        }

        public static bool Contains<T>(this IEnumerable<T> source, IEnumerable<T> items, IEqualityComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            var localComparer = comparer ?? EqualityComparer<T>.Default;
            var localCollection = AsICollection(source);
            return items.All(item => localCollection.Contains(item, localComparer));
        }

#endif

#if TARGETS_NET || LESSTHAN_NETCOREAPP20 || TARGETS_NETSTANDARD

        public static bool TryDequeue<T>(this Queue<T> source, out T item)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            try
            {
                item = source.Dequeue();
                return true;
            }
            catch (InvalidOperationException)
            {
                item = default;
                return false;
            }
        }

        public static bool TryPeek<T>(this Stack<T> source, out T item)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            try
            {
                item = source.Peek();
                return true;
            }
            catch (InvalidOperationException)
            {
                item = default;
                return false;
            }
        }

        public static bool TryPeek<T>(this Queue<T> source, out T item)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            try
            {
                item = source.Peek();
                return true;
            }
            catch (InvalidOperationException)
            {
                item = default;
                return false;
            }
        }

        public static bool TryPop<T>(this Stack<T> source, out T item)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            try
            {
                item = source.Pop();
                return true;
            }
            catch (InvalidOperationException)
            {
                item = default;
                return false;
            }
        }
#endif
    }

#if FAT
    public static partial class Extensions
    {
        public static void Add<T>(this Stack<T> stack, T item)
        {
            if (stack == null)
            {
                throw new ArgumentNullException(nameof(stack));
            }
            stack.Push(item);
        }

        public static void Add<T>(this Queue<T> queue, T item)
        {
            if (queue == null)
            {
                throw new ArgumentNullException(nameof(queue));
            }
            queue.Enqueue(item);
        }

        public static IEnumerable<T> Clone<T>(this IEnumerable<T> target)
        {
            return new List<T>(target);
        }

        public static bool ContainsAny<T>(this IEnumerable<T> source, IEnumerable<T> items, IEqualityComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            comparer = comparer ?? EqualityComparer<T>.Default;
            var localCollection = AsICollection(source);
            return items.Any(item => localCollection.Contains(item, comparer));
        }

        public static List<TOutput> ConvertFiltered<T, TOutput>(this IEnumerable<T> source, Func<T, TOutput> converter, Func<T, int, bool> filter)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            var index = 0;
            var result = new List<TOutput>();
            foreach (var item in source)
            {
                if (filter(item, index))
                {
                    result.Add(converter(item));
                }
                index++;
            }
            return result;
        }

        public static TList ConvertFiltered<T, TOutput, TList>(this IEnumerable<T> source, Func<T, TOutput> converter, Predicate<T> filter)
            where TList : ICollection<TOutput>, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            var result = new TList();
            foreach (var item in source)
            {
                if (filter(item))
                {
                    result.Add(converter(item));
                }
            }
            return result;
        }

        public static TList ConvertFiltered<T, TOutput, TList>(this IEnumerable<T> source, Func<T, TOutput> converter, Func<T, int, bool> filter)
            where TList : ICollection<TOutput>, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            var index = 0;
            var result = new TList();
            foreach (var item in source)
            {
                if (filter(item, index))
                {
                    result.Add(converter(item));
                }
                index++;
            }
            return result;
        }

        public static List<KeyValuePair<int, TOutput>> ConvertIndexed<T, TOutput>(this IEnumerable<T> source, Func<T, TOutput> converter, Predicate<T> filter)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            var index = 0;
            var result = new List<KeyValuePair<int, TOutput>>();
            foreach (var item in source)
            {
                if (filter(item))
                {
                    result.Add(new KeyValuePair<int, TOutput>(index, converter(item)));
                }
                index++;
            }
            return result;
        }

        public static List<KeyValuePair<int, TOutput>> ConvertIndexed<T, TOutput>(this IEnumerable<T> source, Func<T, TOutput> converter, Func<T, int, bool> filter)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            var index = 0;
            var result = new List<KeyValuePair<int, TOutput>>();
            foreach (var item in source)
            {
                if (filter(item, index))
                {
                    result.Add(new KeyValuePair<int, TOutput>(index, converter(item)));
                }
                index++;
            }
            return result;
        }

        public static TList ConvertIndexed<T, TOutput, TList>(this IEnumerable<T> source, Func<T, TOutput> converter, Predicate<T> filter)
            where TList : ICollection<KeyValuePair<int, TOutput>>, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            var index = 0;
            var result = new TList();
            foreach (var item in source)
            {
                if (filter(item))
                {
                    result.Add(new KeyValuePair<int, TOutput>(index, converter(item)));
                }
                index++;
            }
            return result;
        }

        public static TList ConvertIndexed<T, TOutput, TList>(this IEnumerable<T> source, Func<T, TOutput> converter, Func<T, int, bool> filter)
            where TList : ICollection<KeyValuePair<int, TOutput>>, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            var index = 0;
            var result = new TList();
            foreach (var item in source)
            {
                if (filter(item, index))
                {
                    result.Add(new KeyValuePair<int, TOutput>(index, converter(item)));
                }
                index++;
            }
            return result;
        }

        public static IEnumerable<TOutput> ConvertProgressiveFiltered<T, TOutput>(this IEnumerable<T> source, Func<T, TOutput> converter, Predicate<T> filter)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            return ConvertProgressiveFilteredExtracted();

            IEnumerable<TOutput> ConvertProgressiveFilteredExtracted()
            {
                foreach (var item in source)
                {
                    if (filter(item))
                    {
                        yield return converter(item);
                    }
                }
            }
        }

        public static IEnumerable<TOutput> ConvertProgressiveFiltered<T, TOutput>(this IEnumerable<T> source, Func<T, TOutput> converter, Func<T, int, bool> filter)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            return ConvertProgressiveFilteredIterator();
            IEnumerable<TOutput> ConvertProgressiveFilteredIterator()
            {
                var index = 0;
                foreach (var item in source)
                {
                    if (filter(item, index))
                    {
                        yield return converter(item);
                    }
                    index++;
                }
            }
        }

        public static IEnumerable<KeyValuePair<int, TOutput>> ConvertProgressiveIndexed<T, TOutput>(this IEnumerable<T> source, Func<T, TOutput> converter, Predicate<T> filter)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            return ConvertProgressiveIndexedExtracted();

            IEnumerable<KeyValuePair<int, TOutput>> ConvertProgressiveIndexedExtracted()
            {
                var index = 0;
                foreach (var item in source)
                {
                    if (filter(item))
                    {
                        yield return new KeyValuePair<int, TOutput>(index, converter(item));
                    }
                    index++;
                }
            }
        }

        public static IEnumerable<KeyValuePair<int, TOutput>> ConvertProgressiveIndexed<T, TOutput>(this IEnumerable<T> source, Func<T, TOutput> converter, Func<T, int, bool> filter)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            return ConvertProgressiveIndexedExtracted();

            IEnumerable<KeyValuePair<int, TOutput>> ConvertProgressiveIndexedExtracted()
            {
                var index = 0;
                foreach (var item in source)
                {
                    if (filter(item, index))
                    {
                        yield return new KeyValuePair<int, TOutput>(index, converter(item));
                    }
                    index++;
                }
            }
        }

        public static T[] Copy<T>(this T[] array)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            // Copyright (c) Microsoft. All rights reserved.
            // Licensed under the MIT license. See LICENSE file in the project root for full license information.
            var copy = new T[array.Length];
            Array.Copy(array, copy, array.Length);
            return copy;
        }

        public static bool Exists<T>(this IEnumerable<T> source, T value)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
            return source.Any(local => comparer.Equals(local, value));
        }

        public static bool Exists<T>(this IEnumerable<T> source, T value, IEqualityComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            comparer = comparer ?? EqualityComparer<T>.Default;
            return source.Any(local => comparer.Equals(local, value));
        }

        public static bool Exists<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return source.Any(item => predicate(item));
        }

        public static T Find<T>(this IEnumerable<T> source, int index, int count, Predicate<T> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            var currentIndex = 0;
            var limit = index + count;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (currentIndex == limit)
                    {
                        break;
                    }
                    if (predicate(enumerator.Current))
                    {
                        return enumerator.Current;
                    }
                    currentIndex++;
                }
                return default;
            }
        }

        public static T Find<T>(this IEnumerable<T> source, int index, Predicate<T> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            var currentIndex = 0;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (predicate(enumerator.Current))
                    {
                        return enumerator.Current;
                    }
                    currentIndex++;
                }
                return default;
            }
        }

        public static T Find<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (predicate(enumerator.Current))
                    {
                        return enumerator.Current;
                    }
                }
                return default;
            }
        }

        public static int FindIndex<T>(this IEnumerable<T> source, int index, int count, Predicate<T> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            var currentIndex = 0;
            var limit = index + count;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (currentIndex == limit)
                    {
                        break;
                    }
                    if (predicate(enumerator.Current))
                    {
                        return currentIndex;
                    }
                    currentIndex++;
                }
                return -1;
            }
        }

        public static int FindIndex<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            var currentIndex = 0;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (predicate(enumerator.Current))
                    {
                        return currentIndex;
                    }
                    currentIndex++;
                }
                return -1;
            }
        }

        public static int FindIndex<T>(this IEnumerable<T> source, int index, Predicate<T> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            var currentIndex = 0;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (predicate(enumerator.Current))
                    {
                        return currentIndex;
                    }
                    currentIndex++;
                }
                return -1;
            }
        }

        public static T FindLast<T>(this IEnumerable<T> source, int index, int count, Predicate<T> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            var currentIndex = 0;
            var limit = index + count;
            var result = default(T);
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (currentIndex == limit)
                    {
                        break;
                    }
                    if (predicate(enumerator.Current))
                    {
                        result = enumerator.Current;
                    }
                    currentIndex++;
                }
                return result;
            }
        }

        public static T FindLast<T>(this IEnumerable<T> source, int index, Predicate<T> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            var currentIndex = 0;
            var result = default(T);
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (predicate(enumerator.Current))
                    {
                        result = enumerator.Current;
                    }
                    currentIndex++;
                }
                return result;
            }
        }

        public static T FindLast<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            var result = default(T);
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (predicate(enumerator.Current))
                    {
                        result = enumerator.Current;
                    }
                }
                return result;
            }
        }

        public static int FindLastIndex<T>(this IEnumerable<T> source, int index, int count, Predicate<T> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            var currentIndex = 0;
            var limit = index + count;
            var result = -1;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (currentIndex == limit)
                    {
                        break;
                    }
                    if (predicate(enumerator.Current))
                    {
                        result = currentIndex;
                    }
                    currentIndex++;
                }
                return result;
            }
        }

        public static int FindLastIndex<T>(this IEnumerable<T> source, int index, Predicate<T> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            var currentIndex = 0;
            var result = -1;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (predicate(enumerator.Current))
                    {
                        result = currentIndex;
                    }
                    currentIndex++;
                }
                return result;
            }
        }

        public static int FindLastIndex<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            var currentIndex = 0;
            var result = -1;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (predicate(enumerator.Current))
                    {
                        result = currentIndex;
                    }
                    currentIndex++;
                }
                return result;
            }
        }

        public static List<T> FindWhere<T>(this IEnumerable<T> source, Predicate<T> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return source.Where(item => predicate(item)).ToList();
        }

        public static TList FindWhere<T, TList>(this IEnumerable<T> source, Predicate<T> predicate)
            where TList : ICollection<T>, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            var result = new TList();
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    result.Add(item);
                }
            }
            return result;
        }

        public static void For<T>(this IEnumerable<T> source, Action<int, T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            var index = 0;
            foreach (var item in source)
            {
                action.Invoke(index, item);
                index++;
            }
        }

        public static void For<T>(this IEnumerable<T> source, Action<int, T> action, Predicate<T> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            var index = 0;
            foreach (var item in source)
            {
                if (predicate.Invoke(item))
                {
                    action.Invoke(index, item);
                }
                index++;
            }
        }

        public static void For<T>(this IEnumerable<T> source, Action<int, T> action, Func<T, int, bool> filter)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            var index = 0;
            foreach (var item in source)
            {
                if (filter.Invoke(item, index))
                {
                    action.Invoke(index, item);
                }
                index++;
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            foreach (var item in source)
            {
                action.Invoke(item);
            }
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action, Predicate<T> predicate)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            foreach (var item in source)
            {
                if (predicate.Invoke(item))
                {
                    action.Invoke(item);
                }
            }
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue newValue)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            dictionary.Add(key, newValue);
            return newValue;
        }

        public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> create)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }
            if (dictionary.TryGetValue(key, out var value))
            {
                return value;
            }
            var newValue = create != null ? create() : default;
            dictionary.Add(key, newValue);
            return newValue;
        }

        public static int IndexOf<T>(this IEnumerable<T> source, T item, int index, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var currentIndex = 0;
            var limit = index + count;
            var comparer = EqualityComparer<T>.Default;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (currentIndex == limit)
                    {
                        break;
                    }
                    if (comparer.Equals(enumerator.Current, item))
                    {
                        return currentIndex;
                    }
                    currentIndex++;
                }
                return -1;
            }
        }

        public static int IndexOf<T>(this IEnumerable<T> source, T item, int index, int count, IEqualityComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var currentIndex = 0;
            var limit = index + count;
            comparer = comparer ?? EqualityComparer<T>.Default;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (currentIndex == limit)
                    {
                        break;
                    }
                    if (comparer.Equals(enumerator.Current, item))
                    {
                        return currentIndex;
                    }
                    currentIndex++;
                }
                return -1;
            }
        }

        public static int IndexOf<T>(this IEnumerable<T> source, T item, int index)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var currentIndex = 0;
            var comparer = EqualityComparer<T>.Default;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (comparer.Equals(enumerator.Current, item))
                    {
                        return currentIndex;
                    }
                    currentIndex++;
                }
                return -1;
            }
        }

        public static int IndexOf<T>(this IEnumerable<T> source, T item, int index, IEqualityComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var currentIndex = 0;
            comparer = comparer ?? EqualityComparer<T>.Default;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (comparer.Equals(enumerator.Current, item))
                    {
                        return currentIndex;
                    }
                    currentIndex++;
                }
                return -1;
            }
        }

        public static IEnumerable<T> InterleaveMany<T>(this IEnumerable<IEnumerable<T>> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var enumerators = source.Select(x => x.GetEnumerator()).ToList();
            return InterleaveManyExtracted();

            IEnumerable<T> InterleaveManyExtracted()
            {
                try
                {
                    while (enumerators.All(enumerator => enumerator.MoveNext()))
                    {
                        foreach (var enumerator in enumerators)
                        {
                            yield return enumerator.Current;
                        }
                    }
                }
                finally
                {
                    foreach (var enumerator in enumerators)
                    {
                        enumerator.Dispose();
                    }
                }
            }
        }

        public static IEnumerable<T> IntersectWithEnumerable<T>(this ICollection<T> source, IEnumerable<T> other)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }
            var otherAsCollection = AsICollection(other);
            return source.RemoveWhereEnumerable(input => !otherAsCollection.Contains(input));
        }

        public static IEnumerable<T> IntersectWithEnumerable<T>(this ICollection<T> source, IEnumerable<T> other, IEqualityComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }
            comparer = comparer ?? EqualityComparer<T>.Default;
            var otherAsCollection = AsICollection(other);
            return source.RemoveWhereEnumerable(input => !otherAsCollection.Contains(input, comparer));
        }

        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return !source.Any();
        }

        public static int LastIndexOf<T>(this IEnumerable<T> source, T item, int index, IEqualityComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var currentIndex = 0;
            comparer = comparer ?? EqualityComparer<T>.Default;
            var result = -1;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (comparer.Equals(enumerator.Current, item))
                    {
                        result = currentIndex;
                    }
                    currentIndex++;
                }
                return result;
            }
        }

        public static int LastIndexOf<T>(this IEnumerable<T> source, T item, int index)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var currentIndex = 0;
            var comparer = EqualityComparer<T>.Default;
            var result = -1;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (comparer.Equals(enumerator.Current, item))
                    {
                        result = currentIndex;
                    }
                    currentIndex++;
                }
                return result;
            }
        }

        public static int LastIndexOf<T>(this IEnumerable<T> source, T item)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var currentIndex = 0;
            var comparer = EqualityComparer<T>.Default;
            var result = -1;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (comparer.Equals(enumerator.Current, item))
                    {
                        result = currentIndex;
                    }
                    currentIndex++;
                }
                return result;
            }
        }

        public static int LastIndexOf<T>(this IEnumerable<T> source, T item, IEqualityComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var currentIndex = 0;
            comparer = comparer ?? EqualityComparer<T>.Default;
            var result = -1;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (comparer.Equals(enumerator.Current, item))
                    {
                        result = currentIndex;
                    }
                    currentIndex++;
                }
                return result;
            }
        }

        public static int LastIndexOf<T>(this IEnumerable<T> source, T item, int index, int count, IEqualityComparer<T> comparer)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var currentIndex = 0;
            var limit = index + count;
            comparer = comparer ?? EqualityComparer<T>.Default;
            var result = -1;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (currentIndex == limit)
                    {
                        break;
                    }
                    if (comparer.Equals(enumerator.Current, item))
                    {
                        result = currentIndex;
                    }
                    currentIndex++;
                }
                return result;
            }
        }

        public static int LastIndexOf<T>(this IEnumerable<T> source, T item, int index, int count)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var currentIndex = 0;
            var limit = index + count;
            var comparer = EqualityComparer<T>.Default;
            var result = -1;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (currentIndex == limit)
                    {
                        break;
                    }
                    if (comparer.Equals(enumerator.Current, item))
                    {
                        result = currentIndex;
                    }
                    currentIndex++;
                }
                return result;
            }
        }

        public static bool ListEquals<T>(this ICollection<T> first, ICollection<T> second)
        {
            if (first == null)
            {
                throw new ArgumentNullException(nameof(first));
            }
            if (second == null)
            {
                throw new ArgumentNullException(nameof(second));
            }
            // Copyright (c) Microsoft. All rights reserved.
            // Licensed under the MIT license. See LICENSE file in the project root for full license information.
            if (first.Count != second.Count)
            {
                return false;
            }
            var cmp = EqualityComparer<T>.Default;
            var f = first.GetEnumerator();
            var s = second.GetEnumerator();
            try
            {
                while (f.MoveNext())
                {
                    s.MoveNext();

                    if (!cmp.Equals(f.Current, s.Current))
                    {
                        return false;
                    }
                }
                return true;
            }
            finally
            {
                f.Dispose();
                s.Dispose();
            }
        }

        public static TOutput[] Map<TInput, TOutput>(this ICollection<TInput> source, Func<TInput, TOutput> select)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (select == null)
            {
                throw new ArgumentNullException(nameof(select));
            }
            // Copyright (c) Microsoft. All rights reserved.
            // Licensed under the MIT license. See LICENSE file in the project root for full license information.
            var count = source.Count;
            var result = new TOutput[count];
            count = 0;
            foreach (var t in source)
            {
                result[count++] = select(t);
            }
            return result;
        }

        public static IEnumerable<TPackage> Pack<T, TPackage>(this IEnumerable<T> source, int size)
            where TPackage : ICollection<T>, new()
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return PackExtracted<T, TPackage>(source, size);
        }

        public static IEnumerable<T[]> Pack<T>(this IEnumerable<T> source, int size)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return PackExtracted();

            IEnumerable<T[]> PackExtracted()
            {
                var index = 0;
                var currentPackage = new T[size];
                foreach (var item in source)
                {
                    currentPackage[index] = item;
                    index++;
                    if (index != size)
                    {
                        continue;
                    }

                    yield return currentPackage;
                    currentPackage = new T[size];
                    index = 0;
                }
                if (index > 0)
                {
                    Array.Resize(ref currentPackage, index);
                    yield return currentPackage;
                }
            }
        }

        public static void Reverse<T>(this IList<T> list, int index, int count)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Non-negative number is required.");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Non-negative number is required.");
            }
            var listCount = list.Count;
            if (count > listCount - index)
            {
                throw new ArgumentException("The list does not contain the number of elements.", nameof(list));
            }
            var end = index + count;
            for (; index < end; index++, end++)
            {
                SwapExtracted(list, index, end);
            }
        }

        public static void Sort<T>(this IList<T> list, int index, int count, IComparer<T> comparer)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }
            comparer = comparer ?? Comparer<T>.Default;
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Non-negative number is required.");
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Non-negative number is required.");
            }
            var listCount = list.Count;
            if (count > listCount - index)
            {
                throw new ArgumentException("The list does not contain the number of elements.", nameof(list));
            }
            SortExtracted(list, index, count + index, comparer);
        }

        public static IEnumerable<T> SymmetricExceptWithEnumerable<T>(this ICollection<T> source, IEnumerable<T> other)
        {
            bool Predicate(T input) => !source.Remove(input);
            return source.AddRangeEnumerable(other.Distinct().Where(Predicate));
        }

        public static bool TryFind<T>(this IEnumerable<T> source, int index, int count, Predicate<T> predicate, out T found)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var currentIndex = 0;
            var limit = index + count;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (currentIndex == limit)
                    {
                        break;
                    }
                    if (predicate(enumerator.Current))
                    {
                        found = enumerator.Current;
                        return true;
                    }
                    currentIndex++;
                }
                found = default;
                return false;
            }
        }

        public static bool TryFind<T>(this IEnumerable<T> source, int index, Predicate<T> predicate, out T found)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var currentIndex = 0;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (predicate(enumerator.Current))
                    {
                        found = enumerator.Current;
                        return true;
                    }
                    currentIndex++;
                }
                found = default;
                return false;
            }
        }

        public static bool TryFind<T>(this IEnumerable<T> source, Predicate<T> predicate, out T found)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (!predicate(enumerator.Current))
                    {
                        continue;
                    }

                    found = enumerator.Current;
                    return true;
                }
                found = default;
                return false;
            }
        }

        public static bool TryFindLast<T>(this IEnumerable<T> source, int index, int count, Predicate<T> predicate, out T foundItem)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var currentIndex = 0;
            var limit = index + count;
            foundItem = default;
            var found = false;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (currentIndex == limit)
                    {
                        break;
                    }
                    if (predicate(enumerator.Current))
                    {
                        foundItem = enumerator.Current;
                        found = true;
                    }
                    currentIndex++;
                }
                return found;
            }
        }

        public static bool TryFindLast<T>(this IEnumerable<T> source, int index, Predicate<T> predicate, out T foundItem)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            var currentIndex = 0;
            foundItem = default;
            var found = false;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                    {
                        break;
                    }
                }
                while (enumerator.MoveNext())
                {
                    if (predicate(enumerator.Current))
                    {
                        foundItem = enumerator.Current;
                        found = true;
                    }
                    currentIndex++;
                }
                return found;
            }
        }

        public static bool TryFindLast<T>(this IEnumerable<T> source, Predicate<T> predicate, out T foundItem)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            foundItem = default;
            var found = false;
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if (!predicate(enumerator.Current))
                    {
                        continue;
                    }

                    foundItem = enumerator.Current;
                    found = true;
                }
                return found;
            }
        }

        public static bool TryTake<T>(this Queue<T> queue, out T item)
        {
            if (queue == null)
            {
                throw new ArgumentNullException(nameof(queue));
            }
            try
            {
                item = queue.Dequeue();
                return true;
            }
            catch (InvalidOperationException)
            {
                item = default;
                return false;
            }
        }

        public static IEnumerable<T> UnionWithEnumerable<T>(this ICollection<T> source, IEnumerable<T> other)
        {
            return source.AddRangeEnumerable(other.Where(input => !source.Contains(input)));
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, int, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return WhereExtracted(source, predicate);
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, bool> predicate, Action whereNot)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return whereNot == null ? WhereExtracted(source, predicate) : WhereExtracted(source, predicate, whereNot);
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, bool> predicate, Action<T> whereNot)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return whereNot == null ? WhereExtracted(source, predicate) : WhereExtracted(source, predicate, whereNot);
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, int, bool> predicate, Action whereNot)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return whereNot == null ? WhereExtracted(source, predicate) : WhereExtracted(source, predicate, whereNot);
        }

        public static IEnumerable<T> Where<T>(this IEnumerable<T> source, Func<T, int, bool> predicate, Action<T> whereNot)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            return whereNot == null ? WhereExtracted(source, predicate) : WhereExtracted(source, predicate, whereNot);
        }

        public static IEnumerable<TResult> ZipMany<T, TResult>(this IEnumerable<IEnumerable<T>> source, Func<IEnumerable<T>, TResult> func)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (func == null)
            {
                throw new ArgumentNullException(nameof(func));
            }
            return ZipManyExtracted();

            IEnumerable<TResult> ZipManyExtracted()
            {
                var enumerators = source.Select(x => x.GetEnumerator()).ToList();
                try
                {
                    while (enumerators.All(enumerator => enumerator.MoveNext()))
                    {
                        yield return func(enumerators.Select(enumerator => enumerator.Current));
                    }
                }
                finally
                {
                    foreach (var enumerator in enumerators)
                    {
                        enumerator.Dispose();
                    }
                }
            }
        }

        private static IEnumerable<TPackage> PackExtracted<T, TPackage>(IEnumerable<T> source, int size)
            where TPackage : ICollection<T>, new()
        {
            var count = 0;
            var currentPackage = new TPackage();
            foreach (var item in source)
            {
                currentPackage.Add(item);
                count++;
                if (count != size)
                {
                    continue;
                }

                yield return currentPackage;
                currentPackage = new TPackage();
                count = 0;
            }
            if (count > 0)
            {
                yield return currentPackage;
            }
        }

        private static void SortExtracted<T>(IList<T> list, int indexStart, int indexEnd, IComparer<T> comparer)
        {
            var low = indexStart;
            var high = indexEnd;
            var pivot = list[low + ((high - low) / 2)];
            while (low <= high)
            {
                while (low < indexEnd && comparer.Compare(list[low], pivot) < 0)
                {
                    low++;
                }
                while (high > indexStart && comparer.Compare(pivot, list[high]) < 0)
                {
                    high--;
                }
                if (low == high)
                {
                    low++;
                    high--;
                }
                else if (low < high)
                {
                    SwapExtracted(list, low, high);
                    low++;
                    high--;
                }
            }
            if (indexStart < high)
            {
                SortExtracted(list, indexStart, high, comparer);
            }
            if (low < indexEnd)
            {
                // ReSharper disable once TailRecursiveCall
                SortExtracted(list, low, indexEnd, comparer);
            }
        }

        private static IEnumerable<T> WhereExtracted<T>(IEnumerable<T> source, Func<T, int, bool> predicate)
        {
            var index = 0;
            foreach (var item in source)
            {
                if (predicate(item, index))
                {
                    yield return item;
                }
                index++;
            }
        }

        private static IEnumerable<T> WhereExtracted<T>(IEnumerable<T> source, Func<T, bool> predicate, Action whereNot)
        {
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    yield return item;
                }
                else
                {
                    whereNot();
                }
            }
        }

        private static IEnumerable<T> WhereExtracted<T>(IEnumerable<T> source, Func<T, bool> predicate)
        {
            return source.Where(predicate);
        }

        private static IEnumerable<T> WhereExtracted<T>(IEnumerable<T> source, Func<T, int, bool> predicate, Action whereNot)
        {
            var index = 0;
            foreach (var item in source)
            {
                if (predicate(item, index))
                {
                    yield return item;
                }
                else
                {
                    whereNot();
                }
                index++;
            }
        }

        private static IEnumerable<T> WhereExtracted<T>(IEnumerable<T> source, Func<T, bool> predicate, Action<T> whereNot)
        {
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    yield return item;
                }
                else
                {
                    whereNot(item);
                }
            }
        }

        private static IEnumerable<T> WhereExtracted<T>(IEnumerable<T> source, Func<T, int, bool> predicate, Action<T> whereNot)
        {
            var index = 0;
            foreach (var item in source)
            {
                if (predicate(item, index))
                {
                    yield return item;
                }
                else
                {
                    whereNot(item);
                }
                index++;
            }
        }
    }

#endif
}