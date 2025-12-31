// <copyright file="BitmapIndex.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.Indexes
{
    using System;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using Meta;

    /// <summary>
    /// Bitmap index optimized for low-cardinality columns (boolean, enum, small sets of values).
    /// Uses bit manipulation for very fast set operations.
    /// Each distinct value has its own bitmap where each bit represents an object.
    /// </summary>
    internal sealed class BitmapIndex
    {
        // Value bitmaps: value -> bitmap (using ulong arrays for large sets)
        private readonly Dictionary<object, BitSet> bitmaps;

        // Object ID to position mapping
        private readonly Dictionary<long, int> objectIdToPosition;
        private readonly Dictionary<int, long> positionToObjectId;

        // Lock for thread safety
        private readonly object syncLock;

        private int nextPosition;

        internal BitmapIndex(IRoleType roleType)
        {
            this.RoleType = roleType;
            this.bitmaps = new Dictionary<object, BitSet>();
            this.objectIdToPosition = new Dictionary<long, int>();
            this.positionToObjectId = new Dictionary<int, long>();
            this.syncLock = new object();
            this.nextPosition = 0;
        }

        /// <summary>
        /// Gets the role type this index covers.
        /// </summary>
        internal IRoleType RoleType { get; }

        /// <summary>
        /// Gets the number of distinct values in the index.
        /// </summary>
        internal int DistinctValueCount
        {
            get
            {
                lock (this.syncLock)
                {
                    return this.bitmaps.Count;
                }
            }
        }

        /// <summary>
        /// Sets a value for an object ID (replaces any existing value).
        /// </summary>
        internal void Set(long objectId, object oldValue, object newValue)
        {
            lock (this.syncLock)
            {
                var position = this.GetOrCreatePosition(objectId);

                // Remove from old value's bitmap
                if (oldValue != null && this.bitmaps.TryGetValue(oldValue, out var oldBitmap))
                {
                    oldBitmap.Clear(position);
                    if (oldBitmap.IsEmpty)
                    {
                        this.bitmaps.Remove(oldValue);
                    }
                }

                // Add to new value's bitmap
                if (newValue != null)
                {
                    if (!this.bitmaps.TryGetValue(newValue, out var newBitmap))
                    {
                        newBitmap = new BitSet();
                        this.bitmaps[newValue] = newBitmap;
                    }

                    newBitmap.Set(position);
                }
            }
        }

        /// <summary>
        /// Removes an object from the index entirely.
        /// </summary>
        internal void Remove(long objectId, object value)
        {
            lock (this.syncLock)
            {
                if (!this.objectIdToPosition.TryGetValue(objectId, out var position))
                {
                    return;
                }

                if (value != null && this.bitmaps.TryGetValue(value, out var bitmap))
                {
                    bitmap.Clear(position);
                    if (bitmap.IsEmpty)
                    {
                        this.bitmaps.Remove(value);
                    }
                }

                // Note: we don't remove from position mappings to avoid renumbering
                // This is a tradeoff for simplicity vs. perfect space efficiency
            }
        }

        /// <summary>
        /// Gets all object IDs with the specified value.
        /// </summary>
        internal IReadOnlySet<long> GetEquals(object value)
        {
            lock (this.syncLock)
            {
                if (!this.bitmaps.TryGetValue(value, out var bitmap))
                {
                    return FrozenSet<long>.Empty;
                }

                return this.BitmapToObjectIds(bitmap);
            }
        }

        /// <summary>
        /// Gets all object IDs that have any of the specified values (OR operation).
        /// </summary>
        internal IReadOnlySet<long> GetAny(IEnumerable<object> values)
        {
            lock (this.syncLock)
            {
                BitSet result = null;
                foreach (var value in values)
                {
                    if (this.bitmaps.TryGetValue(value, out var bitmap))
                    {
                        if (result == null)
                        {
                            result = bitmap.Clone();
                        }
                        else
                        {
                            result.Or(bitmap);
                        }
                    }
                }

                return result != null ? this.BitmapToObjectIds(result) : FrozenSet<long>.Empty;
            }
        }

        /// <summary>
        /// Gets all object IDs that do NOT have the specified value.
        /// </summary>
        internal IEnumerable<long> GetNotEquals(object value)
        {
            lock (this.syncLock)
            {
                if (!this.bitmaps.TryGetValue(value, out var bitmap))
                {
                    // No objects have this value, so all objects match
                    return this.objectIdToPosition.Keys;
                }

                // Return all object IDs not in the bitmap
                return this.objectIdToPosition
                    .Where(kvp => !bitmap.Get(kvp.Value))
                    .Select(kvp => kvp.Key);
            }
        }

        /// <summary>
        /// Computes the intersection of object IDs with value1 AND value2.
        /// </summary>
        internal IReadOnlySet<long> GetIntersection(object value1, object value2)
        {
            lock (this.syncLock)
            {
                if (!this.bitmaps.TryGetValue(value1, out var bitmap1) ||
                    !this.bitmaps.TryGetValue(value2, out var bitmap2))
                {
                    return FrozenSet<long>.Empty;
                }

                var result = bitmap1.Clone();
                result.And(bitmap2);
                return this.BitmapToObjectIds(result);
            }
        }

        /// <summary>
        /// Clears all entries from the index.
        /// </summary>
        internal void Clear()
        {
            lock (this.syncLock)
            {
                this.bitmaps.Clear();
                this.objectIdToPosition.Clear();
                this.positionToObjectId.Clear();
                this.nextPosition = 0;
            }
        }

        private int GetOrCreatePosition(long objectId)
        {
            if (!this.objectIdToPosition.TryGetValue(objectId, out var position))
            {
                position = this.nextPosition++;
                this.objectIdToPosition[objectId] = position;
                this.positionToObjectId[position] = objectId;
            }

            return position;
        }

        private FrozenSet<long> BitmapToObjectIds(BitSet bitmap)
        {
            var result = new HashSet<long>();
            foreach (var position in bitmap.GetSetBits())
            {
                if (this.positionToObjectId.TryGetValue(position, out var objectId))
                {
                    result.Add(objectId);
                }
            }

            return result.ToFrozenSet();
        }
    }

    /// <summary>
    /// Simple bit set implementation using ulong arrays.
    /// Supports efficient bit manipulation operations.
    /// </summary>
    internal sealed class BitSet
    {
        private const int BitsPerWord = 64;
        private ulong[] words;

        internal BitSet()
        {
            this.words = new ulong[4]; // Start with space for 256 bits
        }

        internal bool IsEmpty
        {
            get
            {
                foreach (var word in this.words)
                {
                    if (word != 0)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        internal void Set(int position)
        {
            var wordIndex = position / BitsPerWord;
            var bitIndex = position % BitsPerWord;

            this.EnsureCapacity(wordIndex + 1);
            this.words[wordIndex] |= 1UL << bitIndex;
        }

        internal void Clear(int position)
        {
            var wordIndex = position / BitsPerWord;
            if (wordIndex >= this.words.Length)
            {
                return;
            }

            var bitIndex = position % BitsPerWord;
            this.words[wordIndex] &= ~(1UL << bitIndex);
        }

        internal bool Get(int position)
        {
            var wordIndex = position / BitsPerWord;
            if (wordIndex >= this.words.Length)
            {
                return false;
            }

            var bitIndex = position % BitsPerWord;
            return (this.words[wordIndex] & (1UL << bitIndex)) != 0;
        }

        internal void And(BitSet other)
        {
            var minLength = Math.Min(this.words.Length, other.words.Length);
            for (var i = 0; i < minLength; i++)
            {
                this.words[i] &= other.words[i];
            }

            // Clear remaining words (AND with 0)
            for (var i = minLength; i < this.words.Length; i++)
            {
                this.words[i] = 0;
            }
        }

        internal void Or(BitSet other)
        {
            if (other.words.Length > this.words.Length)
            {
                this.EnsureCapacity(other.words.Length);
            }

            for (var i = 0; i < other.words.Length; i++)
            {
                this.words[i] |= other.words[i];
            }
        }

        internal BitSet Clone()
        {
            var clone = new BitSet
            {
                words = new ulong[this.words.Length]
            };
            Array.Copy(this.words, clone.words, this.words.Length);
            return clone;
        }

        internal IEnumerable<int> GetSetBits()
        {
            for (var wordIndex = 0; wordIndex < this.words.Length; wordIndex++)
            {
                var word = this.words[wordIndex];
                if (word == 0)
                {
                    continue;
                }

                // Use BitOperations for efficient bit scanning
                var basePosition = wordIndex * BitsPerWord;
                while (word != 0)
                {
                    var trailingZeros = BitOperations.TrailingZeroCount(word);
                    yield return basePosition + trailingZeros;
                    word &= word - 1; // Clear lowest set bit
                }
            }
        }

        private void EnsureCapacity(int wordCount)
        {
            if (wordCount <= this.words.Length)
            {
                return;
            }

            var newSize = Math.Max(wordCount, this.words.Length * 2);
            var newWords = new ulong[newSize];
            Array.Copy(this.words, newWords, this.words.Length);
            this.words = newWords;
        }
    }
}
