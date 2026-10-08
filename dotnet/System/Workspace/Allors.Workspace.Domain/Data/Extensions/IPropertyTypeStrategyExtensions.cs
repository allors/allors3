// <copyright file="IPropertyTypeStrategyExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Data
{
    using System;
    using System.Linq;
    using Allors.Workspace.Meta;

    public static class IPropertyTypeStrategyExtensions
    {
        /// <summary>
        /// The role or association of the strategy for this property type: an object, or the
        /// objects, of the given type when one is given.
        /// </summary>
        public static object Get<T>(this T @this, IStrategy strategy, IComposite ofType = null) where T : IPropertyType
        {
            if (@this is IRoleType roleType)
            {
                if (roleType.IsOne)
                {
                    var association = strategy.GetCompositeRole<IObject>(roleType);

                    if (ofType == null || association == null)
                    {
                        return association;
                    }

                    return !ofType.IsAssignableFrom(association.Strategy.Class) ? null : association;
                }
                else
                {
                    var association = strategy.GetCompositesRole<IObject>(roleType);

                    if (ofType == null || association == null)
                    {
                        return association;
                    }

                    return association.Where(v => ofType.IsAssignableFrom(v.Strategy.Class));
                }
            }

            if (@this is IAssociationType associationType)
            {
                if (associationType.IsOne)
                {
                    var association = strategy.GetCompositeAssociation<IObject>(associationType);

                    if (ofType == null || association == null)
                    {
                        return association;
                    }

                    return !ofType.IsAssignableFrom(association.Strategy.Class) ? null : association;
                }
                else
                {
                    var association = strategy.GetCompositesAssociation<IObject>(associationType);

                    if (ofType == null || association == null)
                    {
                        return association;
                    }

                    return association.Where(v => ofType.IsAssignableFrom(v.Strategy.Class));
                }
            }

            throw new ArgumentException("Get only supports RoleType or AssociationType");
        }
    }
}
