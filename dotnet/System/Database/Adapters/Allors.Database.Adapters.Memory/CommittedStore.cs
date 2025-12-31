// <copyright file="CommittedStore.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Meta;

    /// <summary>
    /// Thread-safe storage for committed database state.
    /// Uses ConcurrentDictionary for lock-free reads and a simple lock for atomic commits.
    /// </summary>
    internal sealed class CommittedStore
    {
        // Lock-free primary storage for objects
        private readonly ConcurrentDictionary<long, CommittedObject> objectById;

        // Type index with its own lock (modified less frequently)
        private readonly ConcurrentDictionary<IObjectType, ConcurrentDictionary<long, byte>> objectIdsByType;

        private readonly IndexStore indexStore;

        // Commit lock - only needed for version check + apply atomicity
        private readonly object commitLock;

        private long currentId;

        internal CommittedStore(IndexStore indexStore)
        {
            this.objectById = new ConcurrentDictionary<long, CommittedObject>();
            this.objectIdsByType = new ConcurrentDictionary<IObjectType, ConcurrentDictionary<long, byte>>();
            this.indexStore = indexStore;
            this.commitLock = new object();
            this.currentId = 0;
        }

        internal long NextId() => Interlocked.Increment(ref this.currentId);

        internal void UpdateCurrentId(long objectId)
        {
            long current;
            do
            {
                current = Interlocked.Read(ref this.currentId);
                if (objectId <= current)
                {
                    return;
                }
            }
            while (Interlocked.CompareExchange(ref this.currentId, objectId, current) != current);
        }

        internal void Reset()
        {
            lock (this.commitLock)
            {
                this.objectById.Clear();
                this.objectIdsByType.Clear();
                this.indexStore.Clear();
                Interlocked.Exchange(ref this.currentId, 0);
            }
        }

        /// <summary>
        /// Gets a snapshot of an object. Lock-free read operation.
        /// </summary>
        internal CommittedObject GetSnapshot(long objectId)
        {
            // Lock-free read from ConcurrentDictionary
            // No clone needed - CommittedObject is immutable after being stored.
            // Strategy uses an overlay pattern for local modifications.
            return this.objectById.TryGetValue(objectId, out var obj) ? obj : null;
        }

        /// <summary>
        /// Checks if an object exists. Lock-free read operation.
        /// </summary>
        internal bool ObjectExists(long objectId)
        {
            // Lock-free read from ConcurrentDictionary
            return this.objectById.ContainsKey(objectId);
        }

        /// <summary>
        /// Atomically commits transaction changes with optimistic concurrency.
        /// </summary>
        internal void Commit(TransactionChanges changes)
        {
            lock (this.commitLock)
            {
                // Version check and conflict detection for modified objects
                foreach (var modified in changes.ModifiedObjects)
                {
                    if (this.objectById.TryGetValue(modified.ObjectId, out var current))
                    {
                        if (current.Version != modified.OriginalVersion)
                        {
                            throw new ConcurrencyException(modified.ObjectId, current.Version, modified.OriginalVersion);
                        }
                    }
                }

                // Apply deletes and update indexes
                foreach (var deletedId in changes.DeletedObjectIds)
                {
                    if (this.objectById.TryRemove(deletedId, out var deleted))
                    {
                        // Remove from indexes before removing object
                        this.indexStore.RemoveObjectFromIndexes(deletedId, deleted);

                        if (this.objectIdsByType.TryGetValue(deleted.ObjectType, out var typeSet))
                        {
                            typeSet.TryRemove(deletedId, out _);
                        }
                    }
                }

                // Apply new objects and update indexes
                foreach (var newObj in changes.NewObjects)
                {
                    this.objectById[newObj.ObjectId] = newObj;

                    var typeSet = this.objectIdsByType.GetOrAdd(
                        newObj.ObjectType,
                        _ => new ConcurrentDictionary<long, byte>());
                    typeSet[newObj.ObjectId] = 0;

                    // Add to indexes (no old values since this is a new object)
                    this.UpdateIndexesForObject(newObj.ObjectId, null, newObj);
                }

                // Apply modifications and update indexes
                foreach (var modified in changes.ModifiedObjects)
                {
                    this.objectById.TryGetValue(modified.ObjectId, out var oldObj);
                    this.objectById[modified.ObjectId] = modified;

                    // Update indexes with old and new values
                    this.UpdateIndexesForObject(modified.ObjectId, oldObj, modified);
                }
            }
        }

        private void UpdateIndexesForObject(long objectId, CommittedObject oldObj, CommittedObject newObj)
        {
            // Update unit role indexes
            var oldUnitRoles = oldObj?.UnitRoleByRoleType ?? new Dictionary<IRoleType, object>();
            var newUnitRoles = newObj.UnitRoleByRoleType;

            // Find all unit role types that have changed
            var allUnitRoleTypes = new HashSet<IRoleType>(oldUnitRoles.Keys);
            foreach (var key in newUnitRoles.Keys)
            {
                allUnitRoleTypes.Add(key);
            }

            foreach (var roleType in allUnitRoleTypes)
            {
                oldUnitRoles.TryGetValue(roleType, out var oldValue);
                newUnitRoles.TryGetValue(roleType, out var newValue);

                if (!Equals(oldValue, newValue))
                {
                    this.indexStore.UpdateUnitRoleIndex(roleType, objectId, oldValue, newValue);
                }
            }

            // Update composite role indexes
            var oldCompositeRoles = oldObj?.CompositeRoleByRoleType ?? new Dictionary<IRoleType, long>();
            var newCompositeRoles = newObj.CompositeRoleByRoleType;

            var allCompositeRoleTypes = new HashSet<IRoleType>(oldCompositeRoles.Keys);
            foreach (var key in newCompositeRoles.Keys)
            {
                allCompositeRoleTypes.Add(key);
            }

            foreach (var roleType in allCompositeRoleTypes)
            {
                var hasOld = oldCompositeRoles.TryGetValue(roleType, out var oldTargetId);
                var hasNew = newCompositeRoles.TryGetValue(roleType, out var newTargetId);

                if (hasOld != hasNew || (hasOld && oldTargetId != newTargetId))
                {
                    this.indexStore.UpdateCompositeRoleIndex(
                        roleType,
                        objectId,
                        hasOld ? oldTargetId : null,
                        hasNew ? newTargetId : null);
                }
            }
        }

        /// <summary>
        /// Gets all object IDs for a type. Returns a snapshot copy.
        /// </summary>
        internal HashSet<long> GetObjectIdsForType(IObjectType type)
        {
            // Lock-free read from ConcurrentDictionary
            if (this.objectIdsByType.TryGetValue(type, out var typeSet))
            {
                return new HashSet<long>(typeSet.Keys);
            }

            return new HashSet<long>();
        }

        /// <summary>
        /// Gets all objects. Returns a snapshot copy.
        /// </summary>
        internal IEnumerable<CommittedObject> GetAllObjects()
        {
            // Lock-free read - ToList creates a snapshot
            return this.objectById.Values.ToList();
        }
    }
}
