// <copyright file="ObjectExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;
    using Domain;
    using Meta;
    using Database;

    public static partial class ObjectExtensions
    {
        public static void CoreOnPostBuild(this Object @this, ObjectOnPostBuild method)
        {
            // TODO: Optimize
            foreach (var roleType in ((Class)@this.Strategy.Class).RoleTypes)
            {
                if (roleType.IsRequired)
                {
                    if (roleType.ObjectType is IUnit unit && !@this.Strategy.ExistRole(roleType))
                    {
                        switch (unit.UnitTag)
                        {
                            case UnitTags.Boolean:
                                @this.Strategy.SetUnitRole(roleType, false);
                                break;

                            case UnitTags.Decimal:
                                @this.Strategy.SetUnitRole(roleType, 0m);
                                break;

                            case UnitTags.Float:
                                @this.Strategy.SetUnitRole(roleType, 0d);
                                break;

                            case UnitTags.Integer:
                                @this.Strategy.SetUnitRole(roleType, 0);
                                break;

                            case UnitTags.Unique:
                                @this.Strategy.SetUnitRole(roleType, Guid.NewGuid());
                                break;

                            case UnitTags.DateTime:
                                @this.Strategy.SetUnitRole(roleType, @this.Strategy.Transaction.Now());
                                break;
                        }
                    }
                }
            }
        }


        public static T Clone<T>(this T @this, params IRoleType[] deepClone) where T : IObject
        {
            var strategy = @this.Strategy;
            var transaction = strategy.Transaction;
            var @class = strategy.Class;

            var clone = (T)DefaultObjectBuilder.Build(transaction, @class);

            foreach (var roleType in @class.DatabaseRoleTypes.Where(v => !(v.RelationType.IsDerived || v.RelationType.IsSynced) && !deepClone.Contains(v) && (v.ObjectType.IsUnit || v.AssociationType.IsMany)))
            {
                if (!clone.Strategy.ExistRole(roleType))
                {
                    var role = @this.Strategy.GetRole(roleType);
                    clone.Strategy.SetRole(roleType, role);
                }
            }

            foreach (var roleType in deepClone)
            {
                if (roleType.IsOne)
                {
                    var role = strategy.GetCompositeRole(roleType);
                    if (role != null)
                    {
                        clone.Strategy.SetCompositeRole(roleType, role.Clone());
                    }
                }
                else
                {
                    foreach (IObject role in strategy.GetCompositeRoles(roleType))
                    {
                        clone.Strategy.AddCompositeRole(roleType, role.Clone());
                    }
                }
            }

            return clone;
        }
    }
}
