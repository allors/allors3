// <copyright file="DatabaseRecord.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class DatabaseRecord : Adapters.DatabaseRecord
    {
        private readonly AccessControl[] accessControls;
        private readonly long[] deniedPermissions;

        private readonly Dictionary<IRoleType, object> roleByRoleType;

        internal DatabaseRecord(IClass @class, long id)
            : base(@class, id, 0)
        {
        }

        internal DatabaseRecord(IClass @class, long id, long version, Dictionary<IRoleType, object> roleByRoleType,
            long[] deniedPermissions, AccessControl[] accessControls)
            : base(@class, id, version)
        {
            this.roleByRoleType = roleByRoleType;
            this.deniedPermissions = deniedPermissions;
            this.accessControls = accessControls;
        }

        public override object GetRole(IRoleType roleType)
        {
            if (this.roleByRoleType == null)
            {
                return null;
            }

            _ = this.roleByRoleType.TryGetValue(roleType, out var role);
            return role;
        }

        internal bool IsPermitted(long permission) =>
            this.deniedPermissions?.Contains(permission) == false &&
            this.accessControls?.Any(v => v.PermissionIds.Any(w => w == permission)) != false;
    }
}
