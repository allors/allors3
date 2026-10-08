// <copyright file="MemoryPersistenceProvider.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Workspace.Connection;

    /// <summary>
    /// A persistence provider in memory, for the tests: entries by id per key, and the calls
    /// it received. The platform's providers, on a file, a database or IndexedDB, come in a
    /// wave of their own.
    /// </summary>
    public sealed class MemoryPersistenceProvider : IPersistenceProvider
    {
        private readonly Dictionary<CacheKey, Store> storeByKey = new Dictionary<CacheKey, Store>();

        public int LoadCount { get; private set; }

        public int StoreCount { get; private set; }

        public int RemoveCount { get; private set; }

        public int ClearCount { get; private set; }

        public IEnumerable<CacheKey> Keys => this.storeByKey.Keys;

        public IReadOnlyDictionary<long, SyncResponseObject> Objects(CacheKey key) => this.StoreFor(key).Objects;

        public IReadOnlyDictionary<long, AccessResponseGrant> Grants(CacheKey key) => this.StoreFor(key).Grants;

        public IReadOnlyDictionary<long, AccessResponseRevocation> Revocations(CacheKey key) => this.StoreFor(key).Revocations;

        public IReadOnlyDictionary<long, PermissionResponsePermission> Permissions(CacheKey key) => this.StoreFor(key).Permissions;

        public Task<CacheEntries> LoadAsync(CacheKey key, CacheEntryIds ids)
        {
            this.LoadCount++;

            var store = this.StoreFor(key);
            return Task.FromResult(new CacheEntries
            {
                Objects = Pick(store.Objects, ids.Objects),
                Grants = Pick(store.Grants, ids.Grants),
                Revocations = Pick(store.Revocations, ids.Revocations),
                Permissions = Pick(store.Permissions, ids.Permissions),
            });
        }

        public Task StoreAsync(CacheKey key, CacheEntries entries)
        {
            this.StoreCount++;

            var store = this.StoreFor(key);
            foreach (var @object in entries.Objects ?? Enumerable.Empty<SyncResponseObject>())
            {
                store.Objects[@object.i] = @object;
            }

            foreach (var grant in entries.Grants ?? Enumerable.Empty<AccessResponseGrant>())
            {
                store.Grants[grant.i] = grant;
            }

            foreach (var revocation in entries.Revocations ?? Enumerable.Empty<AccessResponseRevocation>())
            {
                store.Revocations[revocation.i] = revocation;
            }

            foreach (var permission in entries.Permissions ?? Enumerable.Empty<PermissionResponsePermission>())
            {
                store.Permissions[permission.i] = permission;
            }

            return Task.CompletedTask;
        }

        public Task RemoveAsync(CacheKey key, long[] objectIds)
        {
            this.RemoveCount++;

            var store = this.StoreFor(key);
            foreach (var id in objectIds ?? System.Array.Empty<long>())
            {
                store.Objects.Remove(id);
            }

            return Task.CompletedTask;
        }

        public Task ClearAsync(CacheKey key)
        {
            this.ClearCount++;
            this.storeByKey.Remove(key);
            return Task.CompletedTask;
        }

        private static T[] Pick<T>(Dictionary<long, T> byId, long[] ids) =>
            ids == null ? null : ids.Where(byId.ContainsKey).Select(id => byId[id]).ToArray();

        private Store StoreFor(CacheKey key)
        {
            if (!this.storeByKey.TryGetValue(key, out var store))
            {
                store = new Store();
                this.storeByKey[key] = store;
            }

            return store;
        }

        private sealed class Store
        {
            public Dictionary<long, SyncResponseObject> Objects { get; } = new Dictionary<long, SyncResponseObject>();

            public Dictionary<long, AccessResponseGrant> Grants { get; } = new Dictionary<long, AccessResponseGrant>();

            public Dictionary<long, AccessResponseRevocation> Revocations { get; } = new Dictionary<long, AccessResponseRevocation>();

            public Dictionary<long, PermissionResponsePermission> Permissions { get; } = new Dictionary<long, PermissionResponsePermission>();
        }
    }
}
