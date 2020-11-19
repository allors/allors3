// <copyright file="DatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Direct.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Object
    {
        internal Object(Guid @class, long id, long version, IReadOnlyDictionary<Guid, object> roleByRelationTypeId, AccessControl[] accessControls, Permission[] deniedPermissions)
        {
            this.Class = @class;
            this.Id = id;
            this.Version = version;
            this.RoleByRelationTypeId = roleByRelationTypeId;
            this.AccessControls = accessControls;
            this.DeniedPermissions = deniedPermissions;
        }

        public Guid Class { get; }

        public long Id { get; }

        public long Version { get; }

        public IReadOnlyDictionary<Guid, object> RoleByRelationTypeId { get; }

        public AccessControl[] AccessControls { get; }

        public Permission[] DeniedPermissions { get; }

        public bool IsPermitted(Permission permission) =>
            permission != null &&
            !this.DeniedPermissions.Contains(permission) &&
            this.AccessControls.Any(v => v.PermissionIds.Any(w => w == permission.Id));
    }
}
