// <copyright file="IndexStore.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System.Collections.Generic;
    using System.Threading;
    using Meta;

    /// <summary>
    /// Manages secondary indexes on role types for O(1) query lookups.
    /// Indexes are opt-in per role type via Database.CreateIndex().
    /// Thread-safe for concurrent read/write access.
    /// </summary>
    internal sealed class IndexStore
    {
        // Unit role indexes: roleType -> (value -> objectIds)
        private readonly Dictionary<IRoleType, Dictionary<object, HashSet<long>>> unitRoleIndexes;

        // Composite role indexes: roleType -> (targetObjectId -> sourceObjectIds)
        private readonly Dictionary<IRoleType, Dictionary<long, HashSet<long>>> compositeRoleIndexes;

        // Lock for thread-safe access to indexes
        private readonly ReaderWriterLockSlim rwLock;

        internal IndexStore()
        {
            this.unitRoleIndexes = new Dictionary<IRoleType, Dictionary<object, HashSet<long>>>();
            this.compositeRoleIndexes = new Dictionary<IRoleType, Dictionary<long, HashSet<long>>>();
            this.rwLock = new ReaderWriterLockSlim();
        }

        /// <summary>
        /// Creates an index for the specified unit role type.
        /// </summary>
        internal void CreateUnitRoleIndex(IRoleType roleType)
        {
            this.rwLock.EnterWriteLock();
            try
            {
                if (roleType.ObjectType is IUnit && !this.unitRoleIndexes.ContainsKey(roleType))
                {
                    this.unitRoleIndexes[roleType] = new Dictionary<object, HashSet<long>>();
                }
            }
            finally
            {
                this.rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Creates an index for the specified composite role type.
        /// </summary>
        internal void CreateCompositeRoleIndex(IRoleType roleType)
        {
            this.rwLock.EnterWriteLock();
            try
            {
                if (roleType.ObjectType is IComposite && !this.compositeRoleIndexes.ContainsKey(roleType))
                {
                    this.compositeRoleIndexes[roleType] = new Dictionary<long, HashSet<long>>();
                }
            }
            finally
            {
                this.rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Checks if a unit role index exists for the specified role type.
        /// </summary>
        internal bool HasUnitRoleIndex(IRoleType roleType)
        {
            this.rwLock.EnterReadLock();
            try
            {
                return this.unitRoleIndexes.ContainsKey(roleType);
            }
            finally
            {
                this.rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Checks if a composite role index exists for the specified role type.
        /// </summary>
        internal bool HasCompositeRoleIndex(IRoleType roleType)
        {
            this.rwLock.EnterReadLock();
            try
            {
                return this.compositeRoleIndexes.ContainsKey(roleType);
            }
            finally
            {
                this.rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets object IDs that have the specified unit role value.
        /// Returns null if no index exists for this role type.
        /// </summary>
        internal HashSet<long> GetObjectIdsByUnitRoleValue(IRoleType roleType, object value)
        {
            this.rwLock.EnterReadLock();
            try
            {
                if (this.unitRoleIndexes.TryGetValue(roleType, out var index))
                {
                    // Normalize the value for consistent lookups
                    var normalizedValue = roleType.Normalize(value);
                    if (normalizedValue != null && index.TryGetValue(normalizedValue, out var objectIds))
                    {
                        // Return a copy to avoid concurrent modification issues
                        return new HashSet<long>(objectIds);
                    }

                    return new HashSet<long>();
                }

                return null; // No index exists
            }
            finally
            {
                this.rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Gets object IDs that reference the specified target object ID via the composite role.
        /// Returns null if no index exists for this role type.
        /// </summary>
        internal HashSet<long> GetObjectIdsByCompositeRole(IRoleType roleType, long targetObjectId)
        {
            this.rwLock.EnterReadLock();
            try
            {
                if (this.compositeRoleIndexes.TryGetValue(roleType, out var index))
                {
                    if (index.TryGetValue(targetObjectId, out var objectIds))
                    {
                        // Return a copy to avoid concurrent modification issues
                        return new HashSet<long>(objectIds);
                    }

                    return new HashSet<long>();
                }

                return null; // No index exists
            }
            finally
            {
                this.rwLock.ExitReadLock();
            }
        }

        /// <summary>
        /// Updates indexes when an object's unit role value changes.
        /// </summary>
        internal void UpdateUnitRoleIndex(IRoleType roleType, long objectId, object oldValue, object newValue)
        {
            this.rwLock.EnterWriteLock();
            try
            {
                if (!this.unitRoleIndexes.TryGetValue(roleType, out var index))
                {
                    return;
                }

                // Remove from old value's index
                if (oldValue != null)
                {
                    var normalizedOldValue = roleType.Normalize(oldValue);
                    if (normalizedOldValue != null && index.TryGetValue(normalizedOldValue, out var oldSet))
                    {
                        oldSet.Remove(objectId);
                        if (oldSet.Count == 0)
                        {
                            index.Remove(normalizedOldValue);
                        }
                    }
                }

                // Add to new value's index
                if (newValue != null)
                {
                    var normalizedNewValue = roleType.Normalize(newValue);
                    if (normalizedNewValue != null)
                    {
                        if (!index.TryGetValue(normalizedNewValue, out var newSet))
                        {
                            newSet = new HashSet<long>();
                            index[normalizedNewValue] = newSet;
                        }

                        newSet.Add(objectId);
                    }
                }
            }
            finally
            {
                this.rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Updates indexes when an object's composite role changes.
        /// </summary>
        internal void UpdateCompositeRoleIndex(IRoleType roleType, long objectId, long? oldTargetId, long? newTargetId)
        {
            this.rwLock.EnterWriteLock();
            try
            {
                if (!this.compositeRoleIndexes.TryGetValue(roleType, out var index))
                {
                    return;
                }

                // Remove from old target's index
                if (oldTargetId.HasValue)
                {
                    if (index.TryGetValue(oldTargetId.Value, out var oldSet))
                    {
                        oldSet.Remove(objectId);
                        if (oldSet.Count == 0)
                        {
                            index.Remove(oldTargetId.Value);
                        }
                    }
                }

                // Add to new target's index
                if (newTargetId.HasValue)
                {
                    if (!index.TryGetValue(newTargetId.Value, out var newSet))
                    {
                        newSet = new HashSet<long>();
                        index[newTargetId.Value] = newSet;
                    }

                    newSet.Add(objectId);
                }
            }
            finally
            {
                this.rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes all index entries for the specified object.
        /// Called when an object is deleted.
        /// </summary>
        internal void RemoveObjectFromIndexes(long objectId, CommittedObject obj)
        {
            this.rwLock.EnterWriteLock();
            try
            {
                // Remove from unit role indexes
                foreach (var kvp in this.unitRoleIndexes)
                {
                    var roleType = kvp.Key;
                    var index = kvp.Value;

                    if (obj.UnitRoleByRoleType.TryGetValue(roleType, out var value) && value != null)
                    {
                        var normalizedValue = roleType.Normalize(value);
                        if (normalizedValue != null && index.TryGetValue(normalizedValue, out var set))
                        {
                            set.Remove(objectId);
                            if (set.Count == 0)
                            {
                                index.Remove(normalizedValue);
                            }
                        }
                    }
                }

                // Remove from composite role indexes
                foreach (var kvp in this.compositeRoleIndexes)
                {
                    var roleType = kvp.Key;
                    var index = kvp.Value;

                    if (obj.CompositeRoleByRoleType.TryGetValue(roleType, out var targetId))
                    {
                        if (index.TryGetValue(targetId, out var set))
                        {
                            set.Remove(objectId);
                            if (set.Count == 0)
                            {
                                index.Remove(targetId);
                            }
                        }
                    }
                }
            }
            finally
            {
                this.rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Rebuilds all indexes from the committed store.
        /// Called after database load or when indexes are created on existing data.
        /// </summary>
        internal void RebuildIndexes(IEnumerable<CommittedObject> objects)
        {
            this.rwLock.EnterWriteLock();
            try
            {
                // Clear all indexes
                foreach (var index in this.unitRoleIndexes.Values)
                {
                    index.Clear();
                }

                foreach (var index in this.compositeRoleIndexes.Values)
                {
                    index.Clear();
                }

                // Rebuild from objects
                foreach (var obj in objects)
                {
                    var objectId = obj.ObjectId;

                    // Index unit roles
                    foreach (var kvp in this.unitRoleIndexes)
                    {
                        var roleType = kvp.Key;
                        var index = kvp.Value;

                        if (obj.UnitRoleByRoleType.TryGetValue(roleType, out var value) && value != null)
                        {
                            var normalizedValue = roleType.Normalize(value);
                            if (normalizedValue != null)
                            {
                                if (!index.TryGetValue(normalizedValue, out var set))
                                {
                                    set = new HashSet<long>();
                                    index[normalizedValue] = set;
                                }

                                set.Add(objectId);
                            }
                        }
                    }

                    // Index composite roles
                    foreach (var kvp in this.compositeRoleIndexes)
                    {
                        var roleType = kvp.Key;
                        var index = kvp.Value;

                        if (obj.CompositeRoleByRoleType.TryGetValue(roleType, out var targetId))
                        {
                            if (!index.TryGetValue(targetId, out var set))
                            {
                                set = new HashSet<long>();
                                index[targetId] = set;
                            }

                            set.Add(objectId);
                        }
                    }
                }
            }
            finally
            {
                this.rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Clears all indexes.
        /// </summary>
        internal void Clear()
        {
            this.rwLock.EnterWriteLock();
            try
            {
                foreach (var index in this.unitRoleIndexes.Values)
                {
                    index.Clear();
                }

                foreach (var index in this.compositeRoleIndexes.Values)
                {
                    index.Clear();
                }
            }
            finally
            {
                this.rwLock.ExitWriteLock();
            }
        }
    }
}
