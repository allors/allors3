// <copyright file="IPersistenceProvider.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using System.Threading.Tasks;

    /// <summary>
    /// Keeps the user's view beyond the memory cache, under the <see cref="CacheKey"/> of the
    /// user: the objects, grants, revocations and permissions in the shape the wire delivered
    /// them, so that restoring them replays the connection's own sync code path. A connection
    /// with a provider loads, per pull, the objects the pull advertises that its cache lacks and
    /// accepts each only at the version, grant ids and revocation ids the pull advertises; the
    /// grants and revocations likewise at their advertised version; what the server then sends
    /// is stored before the pull returns. The platform ships providers in a wave of their own.
    /// </summary>
    public interface IPersistenceProvider
    {
        /// <summary>
        /// The entries with the given ids that the provider holds under the key; an id it does
        /// not hold is left out.
        /// </summary>
        Task<CacheEntries> LoadAsync(CacheKey key, CacheEntryIds ids);

        /// <summary>
        /// Keeps the entries under the key, each replacing the entry with its id.
        /// </summary>
        Task StoreAsync(CacheKey key, CacheEntries entries);

        /// <summary>
        /// Forgets the objects with the given ids under the key.
        /// </summary>
        Task RemoveAsync(CacheKey key, long[] objectIds);

        /// <summary>
        /// Forgets everything under the key; for when the user signs out.
        /// </summary>
        Task ClearAsync(CacheKey key);
    }
}
