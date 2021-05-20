// <copyright file="DatabaseRecord.cs" company="Allors bvba">
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
        private readonly AccessControl[] accessControls;
        private readonly long[] deniedPermissions;

        private readonly Dictionary<IRoleType, object> roleByRoleType;

        internal DatabaseRecord(long id, IClass @class)
        {
            this.Id = id;
            this.Class = @class;
            this.Version = 0;
        }

        internal DatabaseRecord(long id, IClass @class, long version, Dictionary<IRoleType, object> roleByRoleType,
            long[] deniedPermissions, AccessControl[] accessControls)
        {
            this.Id = id;
            this.Class = @class;
            this.Version = version;
            this.roleByRoleType = roleByRoleType;
            this.deniedPermissions = deniedPermissions;
            this.accessControls = accessControls;
        }

        public IClass Class { get; }

        public long Id { get; }

        public long Version { get; }

        public object GetRole(IRoleType roleType)
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
