// <copyright file="ChangedRoles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the IDomainDerivation type.</summary>

namespace Allors.Domain.Derivations.Validating
{
    using System;
    using System.Collections.Generic;
    using Meta;

    public class Matcher
    {
        private readonly IDictionary<IStrategy, ISet<IPropertyType>> propertyTypesByStrategies;

        public Matcher() => this.propertyTypesByStrategies = new Dictionary<IStrategy, ISet<IPropertyType>>();

        public void Add(IStrategy strategy, IPropertyType propertyType)
        {
            if (!this.propertyTypesByStrategies.TryGetValue(strategy, out var propertyTypes))
            {
                propertyTypes = new HashSet<IPropertyType>();
                this.propertyTypesByStrategies.Add(strategy, propertyTypes);
            }

            if (propertyType is RoleClass roleClass && roleClass.ExistRoleInterface)
            {
                propertyTypes.Add(roleClass.RoleInterface);
            }
            else
            {
                propertyTypes.Add(propertyType);
            }
        }

        public void Match(IStrategy strategy, IPropertyType propertyType)
        {
            propertyType = propertyType is RoleClass roleClass && roleClass.ExistRoleInterface
                ? roleClass.RoleInterface
                : propertyType;

            if (!this.propertyTypesByStrategies.TryGetValue(strategy, out var propertyTypes) || !propertyTypes.Contains(propertyType))
            {
                throw new Exception($"Could not match [{strategy}].{propertyType}");
            }
        }
    }
}
