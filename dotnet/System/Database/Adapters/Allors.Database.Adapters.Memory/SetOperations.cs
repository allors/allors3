// <copyright file="SetOperations.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System;
    using System.Buffers;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Provides efficient O(n+m) set operations on sorted arrays using merge algorithms.
    /// These operations are more cache-friendly than hash-based approaches for large sets.
    /// </summary>
    internal static class SetOperations
    {
        /// <summary>
        /// Computes the intersection of two sorted arrays.
        /// Returns a new sorted array containing elements present in both inputs.
        /// Time complexity: O(n+m), Space complexity: O(min(n,m))
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long[] IntersectSorted(ReadOnlySpan<long> left, ReadOnlySpan<long> right)
        {
            if (left.IsEmpty || right.IsEmpty)
            {
                return Array.Empty<long>();
            }

            // Result size is at most min(left, right)
            var maxResultSize = Math.Min(left.Length, right.Length);
            var result = ArrayPool<long>.Shared.Rent(maxResultSize);
            var count = 0;

            try
            {
                var i = 0;
                var j = 0;

                while (i < left.Length && j < right.Length)
                {
                    if (left[i] < right[j])
                    {
                        i++;
                    }
                    else if (left[i] > right[j])
                    {
                        j++;
                    }
                    else
                    {
                        result[count++] = left[i];
                        i++;
                        j++;
                    }
                }

                // Copy to correctly sized array
                var final = new long[count];
                result.AsSpan(0, count).CopyTo(final);
                return final;
            }
            finally
            {
                ArrayPool<long>.Shared.Return(result);
            }
        }

        /// <summary>
        /// Computes the union of two sorted arrays.
        /// Returns a new sorted array containing elements present in either input.
        /// Time complexity: O(n+m), Space complexity: O(n+m)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long[] UnionSorted(ReadOnlySpan<long> left, ReadOnlySpan<long> right)
        {
            if (left.IsEmpty)
            {
                return right.ToArray();
            }

            if (right.IsEmpty)
            {
                return left.ToArray();
            }

            // Result size is at most left + right
            var maxResultSize = left.Length + right.Length;
            var result = ArrayPool<long>.Shared.Rent(maxResultSize);
            var count = 0;

            try
            {
                var i = 0;
                var j = 0;

                while (i < left.Length && j < right.Length)
                {
                    if (left[i] < right[j])
                    {
                        result[count++] = left[i++];
                    }
                    else if (left[i] > right[j])
                    {
                        result[count++] = right[j++];
                    }
                    else
                    {
                        result[count++] = left[i++];
                        j++;
                    }
                }

                // Copy remaining elements
                while (i < left.Length)
                {
                    result[count++] = left[i++];
                }

                while (j < right.Length)
                {
                    result[count++] = right[j++];
                }

                // Copy to correctly sized array
                var final = new long[count];
                result.AsSpan(0, count).CopyTo(final);
                return final;
            }
            finally
            {
                ArrayPool<long>.Shared.Return(result);
            }
        }

        /// <summary>
        /// Computes the difference of two sorted arrays.
        /// Returns a new sorted array containing elements in left but not in right.
        /// Time complexity: O(n+m), Space complexity: O(n)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static long[] ExceptSorted(ReadOnlySpan<long> left, ReadOnlySpan<long> right)
        {
            if (left.IsEmpty)
            {
                return Array.Empty<long>();
            }

            if (right.IsEmpty)
            {
                return left.ToArray();
            }

            // Result size is at most left
            var result = ArrayPool<long>.Shared.Rent(left.Length);
            var count = 0;

            try
            {
                var i = 0;
                var j = 0;

                while (i < left.Length && j < right.Length)
                {
                    if (left[i] < right[j])
                    {
                        result[count++] = left[i++];
                    }
                    else if (left[i] > right[j])
                    {
                        j++;
                    }
                    else
                    {
                        i++;
                        j++;
                    }
                }

                // Copy remaining elements from left
                while (i < left.Length)
                {
                    result[count++] = left[i++];
                }

                // Copy to correctly sized array
                var final = new long[count];
                result.AsSpan(0, count).CopyTo(final);
                return final;
            }
            finally
            {
                ArrayPool<long>.Shared.Return(result);
            }
        }

        /// <summary>
        /// Converts a FrozenSet to a sorted array for use with merge operations.
        /// </summary>
        internal static long[] ToSortedArray(FrozenSet<long> set)
        {
            if (set == null || set.Count == 0)
            {
                return Array.Empty<long>();
            }

            var array = new long[set.Count];
            var i = 0;
            foreach (var item in set)
            {
                array[i++] = item;
            }

            Array.Sort(array);
            return array;
        }

        /// <summary>
        /// Converts a collection of object IDs to a sorted array.
        /// </summary>
        internal static long[] ToSortedArray(IEnumerable<long> objectIds)
        {
            if (objectIds == null)
            {
                return Array.Empty<long>();
            }

            long[] array;
            if (objectIds is ICollection<long> collection)
            {
                if (collection.Count == 0)
                {
                    return Array.Empty<long>();
                }

                array = new long[collection.Count];
                collection.CopyTo(array, 0);
            }
            else
            {
                var list = new List<long>(objectIds);
                if (list.Count == 0)
                {
                    return Array.Empty<long>();
                }

                array = list.ToArray();
            }

            Array.Sort(array);
            return array;
        }

        /// <summary>
        /// Checks if two sorted arrays have any elements in common.
        /// More efficient than computing full intersection when only existence matters.
        /// </summary>
        internal static bool HasIntersection(ReadOnlySpan<long> left, ReadOnlySpan<long> right)
        {
            if (left.IsEmpty || right.IsEmpty)
            {
                return false;
            }

            var i = 0;
            var j = 0;

            while (i < left.Length && j < right.Length)
            {
                if (left[i] < right[j])
                {
                    i++;
                }
                else if (left[i] > right[j])
                {
                    j++;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Counts the number of elements in the intersection of two sorted arrays.
        /// More efficient than computing full intersection when only count matters.
        /// </summary>
        internal static int IntersectionCount(ReadOnlySpan<long> left, ReadOnlySpan<long> right)
        {
            if (left.IsEmpty || right.IsEmpty)
            {
                return 0;
            }

            var count = 0;
            var i = 0;
            var j = 0;

            while (i < left.Length && j < right.Length)
            {
                if (left[i] < right[j])
                {
                    i++;
                }
                else if (left[i] > right[j])
                {
                    j++;
                }
                else
                {
                    count++;
                    i++;
                    j++;
                }
            }

            return count;
        }
    }
}
