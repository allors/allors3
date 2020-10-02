// <copyright file="TreeNodeExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Data
{
    using System;
    using Meta;

    public static class MetaPopulationExtensions
    {
        public static IAssociationType FindAssociationType(this IMetaPopulation @this, Guid? id) => id != null ? ((IRelationType)@this.Find(id.Value)).AssociationType : null;

        public static IRoleType FindRoleType(this IMetaPopulation @this, Guid? id) => id != null ? ((IRelationType)@this.Find(id.Value)).RoleType : null;
    }
}
