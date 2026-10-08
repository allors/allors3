// <copyright file="MemoryCache.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using System;
    using System.Collections.Concurrent;
    using Meta;

    /// <summary>
    /// The cache in memory: concurrent dictionaries of immutable records, grants, revocations
    /// and permissions, so that the connections of one user may share it from their own
    /// threads. A set keeps the newest version of an object, a grant or a revocation, whichever
    /// connection delivers it first. It holds everything until <see cref="RemoveRecord"/> or
    /// <see cref="Clear"/>; an eviction policy builds on those two.
    /// </summary>
    public sealed class MemoryCache : ICache
    {
        private readonly ConcurrentDictionary<long, IRecord> recordById = new ConcurrentDictionary<long, IRecord>();
        private readonly ConcurrentDictionary<long, Grant> grantById = new ConcurrentDictionary<long, Grant>();
        private readonly ConcurrentDictionary<long, Revocation> revocationById = new ConcurrentDictionary<long, Revocation>();
        private readonly ConcurrentDictionary<long, Permission> permissionById = new ConcurrentDictionary<long, Permission>();
        private readonly ConcurrentDictionary<(IClass Class, IOperandType OperandType, Operations Operation), long> permissionIdByOperation = new ConcurrentDictionary<(IClass, IOperandType, Operations), long>();

        private readonly object keyLock = new object();
        private CacheKey key;

        public MemoryCache(string workspaceName, IMetaPopulation metaPopulation)
        {
            this.WorkspaceName = workspaceName ?? throw new ArgumentNullException(nameof(workspaceName));
            this.MetaPopulation = metaPopulation ?? throw new ArgumentNullException(nameof(metaPopulation));
        }

        public event EventHandler<RecordChangedEventArgs> RecordChanged;

        public string WorkspaceName { get; }

        public IMetaPopulation MetaPopulation { get; }

        public CacheKey Key
        {
            get
            {
                lock (this.keyLock)
                {
                    return this.key;
                }
            }
        }

        public void Bind(CacheKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            lock (this.keyLock)
            {
                if (this.key == null)
                {
                    this.key = key;
                    return;
                }

                if (!this.key.Equals(key))
                {
                    throw new InvalidOperationException(
                        $"The cache holds the view of user {this.key.UserId} of database '{this.key.DatabaseId}' and cannot serve user {key.UserId} of database '{key.DatabaseId}': a cache serves one user of one database. Give each user a cache of its own, or clear the cache when the user signs out.");
                }
            }
        }

        public IRecord GetRecord(long id)
        {
            this.recordById.TryGetValue(id, out var record);
            return record;
        }

        public bool SetRecord(IRecord record)
        {
            if (record == null)
            {
                throw new ArgumentNullException(nameof(record));
            }

            IRecord previous = null;
            var kept = false;

            // The update may run more than once under contention; the last run decides.
            this.recordById.AddOrUpdate(
                record.Id,
                _ =>
                {
                    previous = null;
                    kept = true;
                    return record;
                },
                (_, held) =>
                {
                    previous = held;
                    kept = record.Version >= held.Version;
                    return kept ? record : held;
                });

            if (kept && previous != null)
            {
                this.RecordChanged?.Invoke(this, new RecordChangedEventArgs(record));
            }

            return kept;
        }

        public void RemoveRecord(long id) => this.recordById.TryRemove(id, out _);

        public void Clear()
        {
            this.recordById.Clear();
            this.grantById.Clear();
            this.revocationById.Clear();
            this.permissionById.Clear();
            this.permissionIdByOperation.Clear();

            lock (this.keyLock)
            {
                this.key = null;
            }
        }

        public Grant GetGrant(long id)
        {
            this.grantById.TryGetValue(id, out var grant);
            return grant;
        }

        public void SetGrant(Grant grant)
        {
            if (grant == null)
            {
                throw new ArgumentNullException(nameof(grant));
            }

            this.grantById.AddOrUpdate(grant.Id, grant, (_, held) => grant.Version >= held.Version ? grant : held);
        }

        public Revocation GetRevocation(long id)
        {
            this.revocationById.TryGetValue(id, out var revocation);
            return revocation;
        }

        public void SetRevocation(Revocation revocation)
        {
            if (revocation == null)
            {
                throw new ArgumentNullException(nameof(revocation));
            }

            this.revocationById.AddOrUpdate(revocation.Id, revocation, (_, held) => revocation.Version >= held.Version ? revocation : held);
        }

        public bool HasPermission(long id) => this.permissionById.ContainsKey(id);

        public void SetPermission(Permission permission)
        {
            if (permission == null)
            {
                throw new ArgumentNullException(nameof(permission));
            }

            this.permissionById[permission.Id] = permission;
            this.permissionIdByOperation[(permission.Class, permission.OperandType, permission.Operation)] = permission.Id;
        }

        public long GetPermission(IClass @class, IOperandType operandType, Operations operation) =>
            this.permissionIdByOperation.TryGetValue((@class, operandType, operation), out var permissionId) ? permissionId : 0;
    }
}
