// <copyright file="LocalDatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using System.Linq;
    using Database.Security;
    using Meta;

    public class LocalDatabaseObject
    {
        private readonly long[] deniedPermissions;
        private readonly LocalAccessControl[] accessControls;

        internal LocalDatabaseObject(LocalDatabase database, long identity, IClass @class)
        {
            this.Database = database;
            this.Identity = identity;
            this.Class = @class;
            this.Version = 0;
        }

        internal LocalDatabaseObject(LocalDatabase database, long identity, IClass @class, long version, Dictionary<IRoleType, object> roleByRoleType, long[] deniedPermissions, LocalAccessControl[] accessControls)
        {
            this.Database = database;
            this.Identity = identity;
            this.Class = @class;
            this.Version = version;
            this.RoleByRoleType = roleByRoleType;
            this.deniedPermissions = deniedPermissions;
            this.accessControls = accessControls;
        }

        internal LocalDatabase Database { get; }

        public IClass Class { get; }

        public long Identity { get; }

        public long Version { get; }

        public Dictionary<IRoleType, object> RoleByRoleType { get; }

        public object GetRole(IRoleType roleType)
        {
            if (this.RoleByRoleType == null)
            {
                return null;
            }

            _ = this.RoleByRoleType.TryGetValue(roleType, out var role);
            return role;
        }

        internal bool IsPermitted(long permission) =>
            this.deniedPermissions?.Contains(permission) == false &&
            this.accessControls?.Any(v => v.PermissionIds.Any(w => w == permission)) != false;
    }
}
