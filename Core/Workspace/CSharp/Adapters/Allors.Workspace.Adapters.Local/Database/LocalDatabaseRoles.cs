// <copyright file="LocalDatabaseRoles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using Meta;

    public class LocalDatabaseRoles
    {
        public IClass Class { get; internal set; }

        public long Id { get; internal set; }

        public long Version { get; internal set; }

        public Dictionary<IRoleType, object> RoleByRoleType { get; internal set; }

        public object GetRole(IRoleType roleType)
        {
            this.RoleByRoleType.TryGetValue(roleType, out var role);
            return role;
        }
    }
}
