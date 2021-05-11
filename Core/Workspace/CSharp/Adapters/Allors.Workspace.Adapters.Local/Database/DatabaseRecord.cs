// <copyright file="LocalDatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class DatabaseRecord : IRecord
    {
        private readonly long[] deniedPermissions;
        private readonly AccessControl[] accessControls;

        internal DatabaseRecord(DatabaseAdapter databaseAdapter, long identity, IClass @class)
        {
            this.DatabaseAdapter = databaseAdapter;
            this.Identity = identity;
            this.Class = @class;
            this.Version = 0;
        }

        internal DatabaseRecord(DatabaseAdapter databaseAdapter, long identity, IClass @class, long version, Dictionary<IRoleType, object> roleByRoleType, long[] deniedPermissions, AccessControl[] accessControls)
        {
            this.DatabaseAdapter = databaseAdapter;
            this.Identity = identity;
            this.Class = @class;
            this.Version = version;
            this.RoleByRoleType = roleByRoleType;
            this.deniedPermissions = deniedPermissions;
            this.accessControls = accessControls;
        }

        internal DatabaseAdapter DatabaseAdapter { get; }

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
