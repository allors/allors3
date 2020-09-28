// <copyright file="PermissionCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class PermissionCacheEntry
    {
        public Guid ClassId { get; }

        public IReadOnlyDictionary<Guid, long> RoleReadPermissionIdByRelationTypeId { get; }

        public IReadOnlyDictionary<Guid, long> RoleWritePermissionIdByRelationTypeId { get; }

        public IReadOnlyDictionary<Guid, long> AssociationReadPermissionIdByRelationTypeId { get; }

        public IReadOnlyDictionary<Guid, long> MethodExecutePermissionIdByMethodTypeId { get; }

        public PermissionCacheEntry(IGrouping<Guid, Permission> permissionsByClassId)
        {
            this.ClassId = permissionsByClassId.Key;
            this.RoleReadPermissionIdByRelationTypeId = permissionsByClassId.OfType<RoleReadPermission>().ToDictionary(v => v.RelationTypePointer, v => v.Id);
            this.RoleWritePermissionIdByRelationTypeId = permissionsByClassId.OfType<RoleWritePermission>().ToDictionary(v => v.RelationTypePointer, v => v.Id);
            this.AssociationReadPermissionIdByRelationTypeId = permissionsByClassId.OfType<AssociationReadPermission>().ToDictionary(v => v.RelationTypePointer, v => v.Id);
            this.MethodExecutePermissionIdByMethodTypeId = permissionsByClassId.OfType<MethodExecutePermission>().ToDictionary(v => v.MethodTypePointer, v => v.Id);
        }
    }
}
