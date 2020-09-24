// <copyright file="TreeNodeExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Data
{
    using Meta;
    using Protocol.Data;

    public static class IPropertyTypeExtensions
    {
        public static PropertyType Save(this IPropertyType @this) => @this switch
        {
            IAssociationType associationType => new PropertyType
            {
                Kind = PropertyKind.Role,
                RelationType = associationType.RelationType.Id,
            },
            IRoleType roleType => new PropertyType
            {
                Kind = PropertyKind.Role,
                RelationType = roleType.RelationType.Id,
            },
            _ => null
        };
    }
}
