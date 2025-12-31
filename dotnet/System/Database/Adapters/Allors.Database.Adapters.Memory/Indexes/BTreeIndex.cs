// <copyright file="BTreeIndex.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.Indexes
{
    using System;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    /// <summary>
    /// B-Tree based index for efficient range queries.
    /// Supports operations like LessThan, GreaterThan, Between.
    /// Uses a sorted dictionary as a simple B-Tree approximation.
    /// </summary>
    /// <typeparam name="TKey">The type of the indexed values (must be comparable).</typeparam>
    internal sealed class BTreeIndex<TKey> where TKey : IComparable<TKey>
    {
        // Sorted index: value -> objectIds
        private readonly SortedDictionary<TKey, FrozenSet<long>> index;

        // Lock for thread safety
        private readonly object syncLock;

        internal BTreeIndex(IRoleType roleType)
        {
            this.RoleType = roleType;
            this.index = new SortedDictionary<TKey, FrozenSet<long>>();
            this.syncLock = new object();
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
                    return this.index.Count;
                }
            }
        }

        /// <summary>
        /// Adds an object ID for a value.
        /// </summary>
        internal void Add(TKey value, long objectId)
        {
            lock (this.syncLock)
            {
                if (this.index.TryGetValue(value, out var existing))
                {
                    var newSet = existing.ToHashSet();
                    newSet.Add(objectId);
                    this.index[value] = newSet.ToFrozenSet();
                }
                else
                {
                    this.index[value] = new[] { objectId }.ToFrozenSet();
                }
            }
        }

        /// <summary>
        /// Removes an object ID for a value.
        /// </summary>
        internal void Remove(TKey value, long objectId)
        {
            lock (this.syncLock)
            {
                if (this.index.TryGetValue(value, out var existing))
                {
                    if (existing.Count == 1)
                    {
                        this.index.Remove(value);
                    }
                    else
                    {
                        var newSet = existing.ToHashSet();
                        newSet.Remove(objectId);
                        this.index[value] = newSet.ToFrozenSet();
                    }
                }
            }
        }

        /// <summary>
        /// Gets all object IDs with the exact value.
        /// </summary>
        internal IReadOnlySet<long> GetEquals(TKey value)
        {
            lock (this.syncLock)
            {
                return this.index.TryGetValue(value, out var result) ? result : FrozenSet<long>.Empty;
            }
        }

        /// <summary>
        /// Gets all object IDs with values less than the specified value.
        /// </summary>
        internal IEnumerable<long> GetLessThan(TKey value)
        {
            lock (this.syncLock)
            {
                foreach (var kvp in this.index)
                {
                    if (kvp.Key.CompareTo(value) >= 0)
                    {
                        break;
                    }

                    foreach (var objectId in kvp.Value)
                    {
                        yield return objectId;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all object IDs with values less than or equal to the specified value.
        /// </summary>
        internal IEnumerable<long> GetLessThanOrEquals(TKey value)
        {
            lock (this.syncLock)
            {
                foreach (var kvp in this.index)
                {
                    if (kvp.Key.CompareTo(value) > 0)
                    {
                        break;
                    }

                    foreach (var objectId in kvp.Value)
                    {
                        yield return objectId;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all object IDs with values greater than the specified value.
        /// </summary>
        internal IEnumerable<long> GetGreaterThan(TKey value)
        {
            lock (this.syncLock)
            {
                var started = false;
                foreach (var kvp in this.index)
                {
                    if (!started)
                    {
                        if (kvp.Key.CompareTo(value) > 0)
                        {
                            started = true;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    foreach (var objectId in kvp.Value)
                    {
                        yield return objectId;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all object IDs with values greater than or equal to the specified value.
        /// </summary>
        internal IEnumerable<long> GetGreaterThanOrEquals(TKey value)
        {
            lock (this.syncLock)
            {
                var started = false;
                foreach (var kvp in this.index)
                {
                    if (!started)
                    {
                        if (kvp.Key.CompareTo(value) >= 0)
                        {
                            started = true;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    foreach (var objectId in kvp.Value)
                    {
                        yield return objectId;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all object IDs with values between the specified range (inclusive).
        /// </summary>
        internal IEnumerable<long> GetBetween(TKey minValue, TKey maxValue)
        {
            lock (this.syncLock)
            {
                foreach (var kvp in this.index)
                {
                    if (kvp.Key.CompareTo(minValue) < 0)
                    {
                        continue;
                    }

                    if (kvp.Key.CompareTo(maxValue) > 0)
                    {
                        break;
                    }

                    foreach (var objectId in kvp.Value)
                    {
                        yield return objectId;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all object IDs with values between the specified range (exclusive).
        /// </summary>
        internal IEnumerable<long> GetBetweenExclusive(TKey minValue, TKey maxValue)
        {
            lock (this.syncLock)
            {
                foreach (var kvp in this.index)
                {
                    if (kvp.Key.CompareTo(minValue) <= 0)
                    {
                        continue;
                    }

                    if (kvp.Key.CompareTo(maxValue) >= 0)
                    {
                        break;
                    }

                    foreach (var objectId in kvp.Value)
                    {
                        yield return objectId;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the minimum value in the index.
        /// </summary>
        internal TKey GetMinValue()
        {
            lock (this.syncLock)
            {
                return this.index.Count > 0 ? this.index.Keys.First() : default;
            }
        }

        /// <summary>
        /// Gets the maximum value in the index.
        /// </summary>
        internal TKey GetMaxValue()
        {
            lock (this.syncLock)
            {
                return this.index.Count > 0 ? this.index.Keys.Last() : default;
            }
        }

        /// <summary>
        /// Clears all entries from the index.
        /// </summary>
        internal void Clear()
        {
            lock (this.syncLock)
            {
                this.index.Clear();
            }
        }
    }
}
