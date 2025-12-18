// <copyright file="CommittedStore.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System.Collections.Generic;
    using System.Threading;
    using Meta;

    /// <summary>
    /// Thread-safe storage for committed database state.
    /// This is the source of truth for all committed data.
    /// </summary>
    internal sealed class CommittedStore
    {
        private readonly ReaderWriterLockSlim rwLock;
        private readonly Dictionary<long, CommittedObject> objectById;
        private readonly Dictionary<IObjectType, HashSet<long>> objectIdsByType;
        private long currentId;

        internal CommittedStore()
        {
            this.rwLock = new ReaderWriterLockSlim();
            this.objectById = new Dictionary<long, CommittedObject>();
            this.objectIdsByType = new Dictionary<IObjectType, HashSet<long>>();
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
            this.rwLock.EnterWriteLock();
            try
            {
                this.objectById.Clear();
                this.objectIdsByType.Clear();
                Interlocked.Exchange(ref this.currentId, 0);
            }
            finally
            {
                this.rwLock.ExitWriteLock();
            }
        }

        internal CommittedObject GetSnapshot(long objectId)
        {
            this.rwLock.EnterReadLock();
            try
            {
                return this.objectById.TryGetValue(objectId, out var obj) ? obj.Clone() : null;
            }
            finally
            {
                this.rwLock.ExitReadLock();
            }
        }

        internal bool ObjectExists(long objectId)
        {
            this.rwLock.EnterReadLock();
            try
            {
                return this.objectById.ContainsKey(objectId);
            }
            finally
            {
                this.rwLock.ExitReadLock();
            }
        }

        internal void Commit(TransactionChanges changes)
        {
            this.rwLock.EnterWriteLock();
            try
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

                // Apply deletes
                foreach (var deletedId in changes.DeletedObjectIds)
                {
                    if (this.objectById.TryGetValue(deletedId, out var deleted))
                    {
                        this.objectById.Remove(deletedId);
                        if (this.objectIdsByType.TryGetValue(deleted.ObjectType, out var typeSet))
                        {
                            typeSet.Remove(deletedId);
                        }
                    }
                }

                // Apply new objects
                foreach (var newObj in changes.NewObjects)
                {
                    this.objectById[newObj.ObjectId] = newObj;
                    if (!this.objectIdsByType.TryGetValue(newObj.ObjectType, out var typeSet))
                    {
                        typeSet = new HashSet<long>();
                        this.objectIdsByType[newObj.ObjectType] = typeSet;
                    }

                    typeSet.Add(newObj.ObjectId);
                }

                // Apply modifications
                foreach (var modified in changes.ModifiedObjects)
                {
                    this.objectById[modified.ObjectId] = modified;
                }
            }
            finally
            {
                this.rwLock.ExitWriteLock();
            }
        }

        internal HashSet<long> GetObjectIdsForType(IObjectType type)
        {
            this.rwLock.EnterReadLock();
            try
            {
                if (this.objectIdsByType.TryGetValue(type, out var ids))
                {
                    return new HashSet<long>(ids);
                }

                return new HashSet<long>();
            }
            finally
            {
                this.rwLock.ExitReadLock();
            }
        }

        internal IEnumerable<CommittedObject> GetAllObjects()
        {
            this.rwLock.EnterReadLock();
            try
            {
                return new List<CommittedObject>(this.objectById.Values);
            }
            finally
            {
                this.rwLock.ExitReadLock();
            }
        }
    }
}
