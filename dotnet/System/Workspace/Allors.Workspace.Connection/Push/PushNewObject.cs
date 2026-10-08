// <copyright file="PushNewObject.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using Meta;

    /// <summary>
    /// A new object to push: its workspace id, its class and the roles it was given.
    /// </summary>
    public sealed class PushNewObject
    {
        public PushNewObject(long workspaceId, IClass @class, RoleChange[] roles)
        {
            this.WorkspaceId = workspaceId;
            this.Class = @class;
            this.Roles = roles;
        }

        public long WorkspaceId { get; }

        public IClass Class { get; }

        public RoleChange[] Roles { get; }
    }
}
