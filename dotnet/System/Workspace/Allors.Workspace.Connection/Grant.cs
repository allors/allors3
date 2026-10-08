// <copyright file="Grant.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using Ranges;

    /// <summary>
    /// A grant of the user: the permissions it holds, at the version the server sent.
    /// </summary>
    public sealed class Grant
    {
        public Grant(long id, long version, IRange<long> permissionIds)
        {
            this.Id = id;
            this.Version = version;
            this.PermissionIds = permissionIds;
        }

        public long Id { get; }

        public long Version { get; }

        public IRange<long> PermissionIds { get; }
    }
}
