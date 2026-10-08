// <copyright file="CacheKey.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using System;

    /// <summary>
    /// What a cached view of the database belongs to: the database, the user, the workspace
    /// name and the fingerprint of the meta population. A connection learns the first two from
    /// the server's first response; the other two are its own. A persistence provider keeps the
    /// entries of each key apart.
    /// </summary>
    public sealed class CacheKey : IEquatable<CacheKey>
    {
        public CacheKey(string databaseId, long userId, string workspaceName, string metaFingerprint)
        {
            this.DatabaseId = databaseId ?? throw new ArgumentNullException(nameof(databaseId));
            this.UserId = userId;
            this.WorkspaceName = workspaceName ?? throw new ArgumentNullException(nameof(workspaceName));
            this.MetaFingerprint = metaFingerprint ?? throw new ArgumentNullException(nameof(metaFingerprint));
        }

        public string DatabaseId { get; }

        public long UserId { get; }

        public string WorkspaceName { get; }

        public string MetaFingerprint { get; }

        public bool Equals(CacheKey other) =>
            other != null &&
            string.Equals(this.DatabaseId, other.DatabaseId, StringComparison.Ordinal) &&
            this.UserId == other.UserId &&
            string.Equals(this.WorkspaceName, other.WorkspaceName, StringComparison.Ordinal) &&
            string.Equals(this.MetaFingerprint, other.MetaFingerprint, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is CacheKey other && this.Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = StringComparer.Ordinal.GetHashCode(this.DatabaseId);
                hash = (hash * 397) ^ this.UserId.GetHashCode();
                hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(this.WorkspaceName);
                hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(this.MetaFingerprint);
                return hash;
            }
        }

        public override string ToString() => $"{this.DatabaseId}/{this.UserId}/{this.WorkspaceName}/{this.MetaFingerprint}";
    }
}
