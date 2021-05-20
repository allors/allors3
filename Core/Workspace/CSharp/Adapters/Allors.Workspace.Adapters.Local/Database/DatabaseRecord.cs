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
        private readonly Database database;
        private readonly AccessControl[] accessControls;
        private readonly object deniedPermissionNumbers;

        private readonly Dictionary<IRoleType, object> roleByRoleType;

        internal DatabaseRecord(Database database, IClass @class, long id)
            : base(@class, id, 0) =>
            this.database = database;

        internal DatabaseRecord(Database database, IClass @class, long id, long version, Dictionary<IRoleType, object> roleByRoleType, object deniedPermissionNumbers, AccessControl[] accessControls)
            : base(@class, id, version)
        {
            this.database = database;
            this.roleByRoleType = roleByRoleType;
            this.deniedPermissionNumbers = deniedPermissionNumbers;
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

        public override bool IsPermitted(long permission)
        {
            if (this.accessControls == null)
            {
                return false;
            }

            return !this.database.Numbers.Contains(this.deniedPermissionNumbers, permission) && this.accessControls.Any(v => v.PermissionIds.Any(w => w == permission));
        }
    }
}
