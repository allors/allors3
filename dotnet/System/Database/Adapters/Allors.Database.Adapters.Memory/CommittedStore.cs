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
    using Concurrency;
    using Meta;

    /// <summary>
    /// Thread-safe storage for committed database state.
    /// Uses ConcurrentDictionary for lock-free reads and lock striping for parallel commits.
    /// Transactions affecting disjoint sets of objects can commit in parallel.
    /// </summary>
    internal sealed class CommittedStore
    {
        // Lock-free primary storage for objects
        private readonly ConcurrentDictionary<long, CommittedObject> objectById;

        // Type index with its own lock (modified less frequently)
        private readonly ConcurrentDictionary<IObjectType, ConcurrentDictionary<long, byte>> objectIdsByType;

        private readonly IndexStore indexStore;

        // Lock striping for parallel commits - transactions touching different stripes can commit in parallel
        private readonly LockStripe lockStripe;

        // Global lock for operations that need exclusive access (Reset, etc.)
        private readonly Lock globalLock;

        private long currentId;

        internal CommittedStore(IndexStore indexStore)
        {
            this.objectById = new ConcurrentDictionary<long, CommittedObject>();
            this.objectIdsByType = new ConcurrentDictionary<IObjectType, ConcurrentDictionary<long, byte>>();
            this.indexStore = indexStore;
            this.lockStripe = new LockStripe();
            this.globalLock = new Lock();
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
            // Use global lock for exclusive access during reset
            this.globalLock.Enter();
            try
            {
                // Also acquire all stripe locks to ensure no commits are in progress
                var allStripes = new int[this.lockStripe.StripeCount];
                for (var i = 0; i < allStripes.Length; i++)
                {
                    allStripes[i] = i;
                }

                using (this.lockStripe.AcquireLocks(allStripes))
                {
                    this.objectById.Clear();
                    this.objectIdsByType.Clear();
                    this.indexStore.Clear();
                    Interlocked.Exchange(ref this.currentId, 0);
                }
            }
            finally
            {
                this.globalLock.Exit();
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
        /// Uses lock striping to allow parallel commits for non-conflicting transactions.
        /// </summary>
        internal void Commit(TransactionChanges changes)
        {
            // Collect all affected object IDs
            var affectedObjectIds = this.CollectAffectedObjectIds(changes);

            // If no objects affected, nothing to do
            if (affectedObjectIds.Count == 0)
            {
                return;
            }

            // Get sorted stripe indices for deadlock-free lock acquisition
            var stripeIndices = this.lockStripe.GetSortedStripeIndices(affectedObjectIds);

            // Acquire only the necessary stripe locks
            using (this.lockStripe.AcquireLocks(stripeIndices))
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

        /// <summary>
        /// Collects all object IDs that will be affected by a commit.
        /// This includes new objects, modified objects, deleted objects,
        /// and any objects referenced in relationships.
        /// </summary>
        private HashSet<long> CollectAffectedObjectIds(TransactionChanges changes)
        {
            var affectedIds = new HashSet<long>();

            // Add new object IDs
            foreach (var newObj in changes.NewObjects)
            {
                affectedIds.Add(newObj.ObjectId);

                // Also include referenced objects (composite roles and associations)
                this.AddReferencedObjectIds(newObj, affectedIds);
            }

            // Add modified object IDs
            foreach (var modified in changes.ModifiedObjects)
            {
                affectedIds.Add(modified.ObjectId);

                // Also include referenced objects
                this.AddReferencedObjectIds(modified, affectedIds);

                // Include previously referenced objects (from old version)
                if (this.objectById.TryGetValue(modified.ObjectId, out var oldObj))
                {
                    this.AddReferencedObjectIds(oldObj, affectedIds);
                }
            }

            // Add deleted object IDs
            foreach (var deletedId in changes.DeletedObjectIds)
            {
                affectedIds.Add(deletedId);

                // Include objects that referenced the deleted object
                if (this.objectById.TryGetValue(deletedId, out var deleted))
                {
                    this.AddReferencedObjectIds(deleted, affectedIds);
                }
            }

            return affectedIds;
        }

        /// <summary>
        /// Adds object IDs referenced by a committed object to the set.
        /// </summary>
        private void AddReferencedObjectIds(CommittedObject obj, HashSet<long> affectedIds)
        {
            // Add composite role targets
            foreach (var targetId in obj.CompositeRoleByRoleType.Values)
            {
                affectedIds.Add(targetId);
            }

            // Add composites role targets (many-to-many)
            foreach (var targetIds in obj.CompositesRoleByRoleType.Values)
            {
                foreach (var targetId in targetIds)
                {
                    affectedIds.Add(targetId);
                }
            }

            // Add association sources
            foreach (var sourceId in obj.CompositeAssociationByAssociationType.Values)
            {
                affectedIds.Add(sourceId);
            }

            // Add associations sources (many-to-many)
            foreach (var sourceIds in obj.CompositesAssociationByAssociationType.Values)
            {
                foreach (var sourceId in sourceIds)
                {
                    affectedIds.Add(sourceId);
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
