// <copyright file="PermissionsCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    using Security;
    using Services;

    public class PermissionsCacheEntry : IPermissionsCacheEntry
    {
        public Guid ClassId { get; }

        public IReadOnlyDictionary<Guid, long> RoleReadPermissionIdByRelationTypeId { get; }

        public IReadOnlyDictionary<Guid, long> RoleWritePermissionIdByRelationTypeId { get; }

        public IReadOnlyDictionary<Guid, long> MethodExecutePermissionIdByMethodTypeId { get; }

        public PermissionsCacheEntry(IGrouping<Guid, Permission> permissionsByClassId)
        {
            this.ClassId = permissionsByClassId.Key;
            this.RoleReadPermissionIdByRelationTypeId = permissionsByClassId.OfType<ReadPermission>().ToDictionary(v => v.RelationTypePointer, v => v.Id);
            this.RoleWritePermissionIdByRelationTypeId = permissionsByClassId.OfType<WritePermission>().ToDictionary(v => v.RelationTypePointer, v => v.Id);
            this.MethodExecutePermissionIdByMethodTypeId = permissionsByClassId.OfType<ExecutePermission>().ToDictionary(v => v.MethodTypePointer, v => v.Id);
        }

        public PermissionsCacheEntry(IGrouping<Guid, IPermission> permissionsByClassId)
        {
            this.ClassId = permissionsByClassId.Key;
            this.RoleReadPermissionIdByRelationTypeId = permissionsByClassId.OfType<ReadPermission>().ToDictionary(v => v.RelationTypePointer, v => v.Id);
            this.RoleWritePermissionIdByRelationTypeId = permissionsByClassId.OfType<WritePermission>().ToDictionary(v => v.RelationTypePointer, v => v.Id);
            this.MethodExecutePermissionIdByMethodTypeId = permissionsByClassId.OfType<ExecutePermission>().ToDictionary(v => v.MethodTypePointer, v => v.Id);
        }
    }
}
