// <copyright file="Store.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory;

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Meta;

/// <summary>
/// Thread-safe storage for committed database state using SlotSnapshot.
/// Uses ConcurrentDictionary for lock-free reads.
/// All write operations (Commit, Reset) must be externally synchronized by the caller.
/// </summary>
internal sealed class Store
{
    private readonly ConcurrentDictionary<long, SlotSnapshot> snapshotById;
    private readonly ConcurrentDictionary<IObjectType, ConcurrentDictionary<long, byte>> objectIdsByType;

    private long currentId;

    internal Store()
    {
        this.snapshotById = new ConcurrentDictionary<long, SlotSnapshot>();
        this.objectIdsByType = new ConcurrentDictionary<IObjectType, ConcurrentDictionary<long, byte>>();
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

    /// <summary>
    /// Resets the store to empty state.
    /// Caller must hold exclusive lock.
    /// </summary>
    internal void Reset()
    {
        this.snapshotById.Clear();
        this.objectIdsByType.Clear();
        Interlocked.Exchange(ref this.currentId, 0);
    }

    /// <summary>
    /// Gets a snapshot of an object. Lock-free read operation.
    /// Returns default (IsValid=false) if object doesn't exist.
    /// </summary>
    internal SlotSnapshot GetSnapshot(long objectId)
    {
        return this.snapshotById.TryGetValue(objectId, out var snapshot) ? snapshot : default;
    }

    /// <summary>
    /// Checks if an object exists. Lock-free read operation.
    /// </summary>
    internal bool ObjectExists(long objectId)
    {
        return this.snapshotById.ContainsKey(objectId);
    }

    /// <summary>
    /// Commits transaction changes with optimistic concurrency.
    /// Caller must hold exclusive lock.
    /// </summary>
    internal void Commit(TransactionChanges changes)
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

    /// <summary>
    /// Gets all object IDs for a type. Returns a snapshot copy.
    /// </summary>
    internal HashSet<long> GetObjectIdsForType(IObjectType type)
    {
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
        return this.snapshotById.Keys.ToList();
    }

    /// <summary>
    /// Gets all snapshots. Returns a snapshot copy.
    /// </summary>
    internal IEnumerable<SlotSnapshot> GetAllSnapshots()
    {
        return this.snapshotById.Values.ToList();
    }
}
