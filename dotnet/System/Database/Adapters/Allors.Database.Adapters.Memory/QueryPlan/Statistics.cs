// <copyright file="Statistics.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory.QueryPlan
{
    using System.Collections.Concurrent;
    using System.Threading;
    using Meta;

    /// <summary>
    /// Collects and maintains statistics for query optimization.
    /// Statistics are updated incrementally during normal operations
    /// and used by the query planner for cost estimation.
    /// </summary>
    internal sealed class Statistics
    {
        // Type statistics: type -> count of objects
        private readonly ConcurrentDictionary<IObjectType, long> typeCounts;

        // Role statistics: roleType -> count of objects with this role set
        private readonly ConcurrentDictionary<IRoleType, long> roleCounts;

        // Distinct value counts: roleType -> count of distinct values
        private readonly ConcurrentDictionary<IRoleType, long> distinctValueCounts;

        // Value counts: (roleType, value) -> count of objects with this value
        private readonly ConcurrentDictionary<(IRoleType, object), long> valueCounts;

        // Total object count
        private long totalObjectCount;

        internal Statistics()
        {
            this.typeCounts = new ConcurrentDictionary<IObjectType, long>();
            this.roleCounts = new ConcurrentDictionary<IRoleType, long>();
            this.distinctValueCounts = new ConcurrentDictionary<IRoleType, long>();
            this.valueCounts = new ConcurrentDictionary<(IRoleType, object), long>();
            this.totalObjectCount = 0;
        }

        /// <summary>
        /// Gets the total number of objects.
        /// </summary>
        internal long TotalObjectCount => Interlocked.Read(ref this.totalObjectCount);

        /// <summary>
        /// Gets the count of objects of a specific type.
        /// </summary>
        internal long GetTypeCount(IComposite objectType)
        {
            long total = 0;
            foreach (var cls in objectType.Classes)
            {
                if (this.typeCounts.TryGetValue(cls, out var count))
                {
                    total += count;
                }
            }

            return total > 0 ? total : 100; // Default estimate if no stats
        }

        /// <summary>
        /// Gets the count of objects with a specific role set.
        /// </summary>
        internal long GetRoleCount(IRoleType roleType)
        {
            return this.roleCounts.TryGetValue(roleType, out var count) ? count : 100;
        }

        /// <summary>
        /// Gets the count of distinct values for a role.
        /// </summary>
        internal long GetDistinctValueCount(IRoleType roleType)
        {
            return this.distinctValueCounts.TryGetValue(roleType, out var count) ? count : 10;
        }

        /// <summary>
        /// Gets the count of objects with a specific value for a role.
        /// Returns -1 if not tracked.
        /// </summary>
        internal long GetValueCount(IRoleType roleType, object value)
        {
            return this.valueCounts.TryGetValue((roleType, value), out var count) ? count : -1;
        }

        /// <summary>
        /// Records that an object of a type was created.
        /// </summary>
        internal void RecordObjectCreated(IClass objectType)
        {
            Interlocked.Increment(ref this.totalObjectCount);
            this.typeCounts.AddOrUpdate(objectType, 1, (_, c) => c + 1);
        }

        /// <summary>
        /// Records that an object of a type was deleted.
        /// </summary>
        internal void RecordObjectDeleted(IClass objectType)
        {
            Interlocked.Decrement(ref this.totalObjectCount);
            this.typeCounts.AddOrUpdate(objectType, 0, (_, c) => c > 0 ? c - 1 : 0);
        }

        /// <summary>
        /// Records that a role value was set.
        /// </summary>
        internal void RecordRoleSet(IRoleType roleType, object oldValue, object newValue)
        {
            // Update value counts
            if (oldValue != null)
            {
                this.valueCounts.AddOrUpdate((roleType, oldValue), 0, (_, c) => c > 0 ? c - 1 : 0);
            }

            if (newValue != null)
            {
                this.valueCounts.AddOrUpdate((roleType, newValue), 1, (_, c) => c + 1);
            }

            // Update role count for first set / last clear
            if (oldValue == null && newValue != null)
            {
                this.roleCounts.AddOrUpdate(roleType, 1, (_, c) => c + 1);
            }
            else if (oldValue != null && newValue == null)
            {
                this.roleCounts.AddOrUpdate(roleType, 0, (_, c) => c > 0 ? c - 1 : 0);
            }
        }

        /// <summary>
        /// Records a distinct value observation for a role.
        /// Called during index updates to track distinct value counts.
        /// </summary>
        internal void RecordDistinctValue(IRoleType roleType, bool added)
        {
            if (added)
            {
                this.distinctValueCounts.AddOrUpdate(roleType, 1, (_, c) => c + 1);
            }
            else
            {
                this.distinctValueCounts.AddOrUpdate(roleType, 0, (_, c) => c > 0 ? c - 1 : 0);
            }
        }

        /// <summary>
        /// Resets all statistics.
        /// </summary>
        internal void Reset()
        {
            this.typeCounts.Clear();
            this.roleCounts.Clear();
            this.distinctValueCounts.Clear();
            this.valueCounts.Clear();
            Interlocked.Exchange(ref this.totalObjectCount, 0);
        }

        /// <summary>
        /// Gets a summary of statistics for debugging/diagnostics.
        /// </summary>
        internal StatisticsSummary GetSummary()
        {
            return new StatisticsSummary
            {
                TotalObjects = this.TotalObjectCount,
                TypeCount = this.typeCounts.Count,
                RoleCount = this.roleCounts.Count,
                TrackedValueCount = this.valueCounts.Count
            };
        }
    }

    /// <summary>
    /// Summary of statistics for diagnostics.
    /// </summary>
    internal sealed class StatisticsSummary
    {
        internal long TotalObjects { get; init; }

        internal int TypeCount { get; init; }

        internal int RoleCount { get; init; }

        internal int TrackedValueCount { get; init; }
    }
}
