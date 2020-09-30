// <copyright file="PropertyTypeExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Data
{
    using Meta;

    public static class PropertyTypeExtensions
    {
        public static IPropertyType Load(this PropertyType @this, ISession session)
        {
            var relationType = (IRelationType)session.Database.ObjectFactory.MetaPopulation.Find(@this.RelationType.Value);

            return @this.Kind switch
            {
                PropertyKind.Association => relationType.AssociationType,
                PropertyKind.Role => relationType.RoleType,
            };
        }
    }
}
