// <copyright file="CommittedStore.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Frozen;
using System.Linq;
using System.Threading;
using Concurrency;
using Meta;

/// <summary>
/// Thread-safe storage for committed database state using SlotSnapshot.
/// Uses ConcurrentDictionary for lock-free reads and lock striping for parallel commits.
/// Transactions affecting disjoint sets of objects can commit in parallel.
/// </summary>
internal sealed class CommittedStore
{
    // Lock-free primary storage for object snapshots
    private readonly ConcurrentDictionary<long, SlotSnapshot> snapshotById;

    // Type index with its own lock (modified less frequently)
    private readonly ConcurrentDictionary<IObjectType, ConcurrentDictionary<long, byte>> objectIdsByType;

    // Lock striping for parallel commits - transactions touching different stripes can commit in parallel
    private readonly LockStripe lockStripe;

    // Global lock for operations that need exclusive access (Reset, etc.)
    private readonly Lock globalLock;

    private long currentId;

    internal CommittedStore()
    {
        this.snapshotById = new ConcurrentDictionary<long, SlotSnapshot>();
        this.objectIdsByType = new ConcurrentDictionary<IObjectType, ConcurrentDictionary<long, byte>>();
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
                this.snapshotById.Clear();
                this.objectIdsByType.Clear();
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
    /// Returns default (IsValid=false) if object doesn't exist.
    /// </summary>
    internal SlotSnapshot GetSnapshot(long objectId)
    {
        // Lock-free read from ConcurrentDictionary
        // SlotSnapshot is immutable - no clone needed
        return this.snapshotById.TryGetValue(objectId, out var snapshot) ? snapshot : default;
    }

    /// <summary>
    /// Checks if an object exists. Lock-free read operation.
    /// </summary>
    internal bool ObjectExists(long objectId)
    {
        // Lock-free read from ConcurrentDictionary
        return this.snapshotById.ContainsKey(objectId);
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
            foreach (var modified in changes.ModifiedSnapshots)
            {
                if (this.snapshotById.TryGetValue(modified.Snapshot.ObjectId, out var current))
                {
                    if (current.Version != modified.OriginalVersion)
                    {
                        throw new ConcurrencyException(modified.Snapshot.ObjectId, current.Version, modified.OriginalVersion);
                    }
                }
            }

            // Apply deletes
            foreach (var deletedId in changes.DeletedObjectIds)
            {
                if (this.snapshotById.TryRemove(deletedId, out var deleted))
                {
                    if (this.objectIdsByType.TryGetValue(deleted.ObjectType, out var typeSet))
                    {
                        typeSet.TryRemove(deletedId, out _);
                    }
                }
            }

            // Apply new objects
            foreach (var newSnapshot in changes.NewSnapshots)
            {
                this.snapshotById[newSnapshot.ObjectId] = newSnapshot;

                var typeSet = this.objectIdsByType.GetOrAdd(
                    newSnapshot.ObjectType,
                    _ => new ConcurrentDictionary<long, byte>());
                typeSet[newSnapshot.ObjectId] = 0;
            }

            // Apply modifications
            foreach (var modified in changes.ModifiedSnapshots)
            {
                this.snapshotById[modified.Snapshot.ObjectId] = modified.Snapshot;
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

        // Add new object IDs and their referenced objects
        foreach (var newSnapshot in changes.NewSnapshots)
        {
            affectedIds.Add(newSnapshot.ObjectId);
            this.AddReferencedObjectIds(newSnapshot, affectedIds);
        }

        // Add modified object IDs and their referenced objects
        foreach (var modified in changes.ModifiedSnapshots)
        {
            affectedIds.Add(modified.Snapshot.ObjectId);
            this.AddReferencedObjectIds(modified.Snapshot, affectedIds);

            // Include previously referenced objects (from old version)
            if (this.snapshotById.TryGetValue(modified.Snapshot.ObjectId, out var oldSnapshot))
            {
                this.AddReferencedObjectIds(oldSnapshot, affectedIds);
            }
        }

        // Add deleted object IDs and their referenced objects
        foreach (var deletedId in changes.DeletedObjectIds)
        {
            affectedIds.Add(deletedId);

            // Include objects that referenced the deleted object
            if (this.snapshotById.TryGetValue(deletedId, out var deleted))
            {
                this.AddReferencedObjectIds(deleted, affectedIds);
            }
        }

        return affectedIds;
    }

    /// <summary>
    /// Adds object IDs referenced by a snapshot to the set.
    /// </summary>
    private void AddReferencedObjectIds(SlotSnapshot snapshot, HashSet<long> affectedIds)
    {
        // Add composite role targets
        if (snapshot.CompositeRoles != null)
        {
            foreach (var targetId in snapshot.CompositeRoles)
            {
                if (targetId != 0)
                {
                    affectedIds.Add(targetId);
                }
            }
        }

        // Add composites role targets (many-to-many)
        if (snapshot.CompositesRoles != null)
        {
            foreach (var targetIds in snapshot.CompositesRoles)
            {
                if (targetIds != null)
                {
                    foreach (var targetId in targetIds)
                    {
                        affectedIds.Add(targetId);
                    }
                }
            }
        }

        // Add association sources
        if (snapshot.CompositeAssociations != null)
        {
            foreach (var sourceId in snapshot.CompositeAssociations)
            {
                if (sourceId != 0)
                {
                    affectedIds.Add(sourceId);
                }
            }
        }

        // Add associations sources (many-to-many)
        if (snapshot.CompositesAssociations != null)
        {
            foreach (var sourceIds in snapshot.CompositesAssociations)
            {
                if (sourceIds != null)
                {
                    foreach (var sourceId in sourceIds)
                    {
                        affectedIds.Add(sourceId);
                    }
                }
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
    /// Gets all object IDs. Returns a snapshot copy.
    /// </summary>
    internal IEnumerable<long> GetAllObjectIds()
    {
        // Lock-free read - ToList creates a snapshot
        return this.snapshotById.Keys.ToList();
    }

    /// <summary>
    /// Gets all snapshots. Returns a snapshot copy.
    /// </summary>
    internal IEnumerable<SlotSnapshot> GetAllSnapshots()
    {
        // Lock-free read - ToList creates a snapshot
        return this.snapshotById.Values.ToList();
    }
}
