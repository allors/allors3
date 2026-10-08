// <copyright file="CacheEntries.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;

    /// <summary>
    /// Entries of a user's view in the shape the wire delivers them: what a
    /// <see cref="IPersistenceProvider"/> stores and loads. A kind that has no entries is null.
    /// </summary>
    public sealed class CacheEntries
    {
        public static readonly CacheEntries Empty = new CacheEntries();

        public SyncResponseObject[] Objects { get; set; }

        public AccessResponseGrant[] Grants { get; set; }

        public AccessResponseRevocation[] Revocations { get; set; }

        public PermissionResponsePermission[] Permissions { get; set; }

        public bool IsEmpty =>
            !(this.Objects?.Length > 0) &&
            !(this.Grants?.Length > 0) &&
            !(this.Revocations?.Length > 0) &&
            !(this.Permissions?.Length > 0);
    }

    /// <summary>
    /// The ids of the entries a connection asks a <see cref="IPersistenceProvider"/> for, by
    /// kind. A kind that is not asked for is null.
    /// </summary>
    public sealed class CacheEntryIds
    {
        public long[] Objects { get; set; }

        public long[] Grants { get; set; }

        public long[] Revocations { get; set; }

        public long[] Permissions { get; set; }

        public bool IsEmpty =>
            !(this.Objects?.Length > 0) &&
            !(this.Grants?.Length > 0) &&
            !(this.Revocations?.Length > 0) &&
            !(this.Permissions?.Length > 0);
    }
}
