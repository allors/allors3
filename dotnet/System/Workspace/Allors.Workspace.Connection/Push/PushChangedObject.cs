// <copyright file="PushChangedObject.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    /// <summary>
    /// A changed database object to push: its id, the version the changes were made from, which
    /// the server compares with its own, and the changed roles.
    /// </summary>
    public sealed class PushChangedObject
    {
        public PushChangedObject(long id, long version, RoleChange[] roles)
        {
            this.Id = id;
            this.Version = version;
            this.Roles = roles;
        }

        public long Id { get; }

        public long Version { get; }

        public RoleChange[] Roles { get; }
    }
}
