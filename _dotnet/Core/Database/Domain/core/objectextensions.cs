// <copyright file="ObjectExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Database.Data;
    using Meta;

    public static partial class ObjectExtensions
    {
        public static bool IsCloneable(this IRoleType roleType) => !roleType.RelationType.IsDerived && (roleType.ObjectType.IsUnit || roleType.AssociationType.IsMany);

        public static bool IsMergeable(this IRoleType roleType) => !roleType.RelationType.IsDerived;

        public static void Merge<T>(this T @this, IObject inTo) where T : IObject
        {
            foreach (var associationType in @this.Strategy.Class.DatabaseAssociationTypes.Where(v => v.RoleType.ObjectType == @this.Strategy.Class))
            {
                var roleType = associationType.RoleType;
                var fromRole = @this.Strategy.GetRole(roleType);

                if (roleType.IsOne)
                {
                    var inToRole = inTo.Strategy.GetRole(roleType);
                    if (inToRole == null && fromRole != null)
                    {
                        inTo.Strategy.SetRole(roleType, fromRole);
                    }
                }
                else
                {
                    foreach (var role in @this.Strategy.GetCompositesRole<IObject>(roleType))
                    {
                        inTo.Strategy.AddCompositesRole(roleType, role);
                    }
                }
            }

            foreach (var roleType in @this.Strategy.Class.DatabaseRoleTypes.Where(v => v.IsMergeable()))
            {
                var fromRole = @this.Strategy.GetRole(roleType);

                if (roleType.IsOne)
                {
                    var inToRole = inTo.Strategy.GetRole(roleType);
                    if (inToRole == null && fromRole != null)
                    {
                        inTo.Strategy.SetRole(roleType, fromRole);
                    }
                }
                else
                {
                    foreach (var role in @this.Strategy.GetCompositesRole<IObject>(roleType))
                    {
                        inTo.Strategy.AddCompositesRole(roleType, role);
                    }
                }
            }
        }

        public static T Clone<T>(this T @this) where T : IObject
        {
            var clone = (T)DefaultObjectBuilder.Build(@this.Strategy.Transaction, @this.Strategy.Class);

            foreach (var roleType in @this.Strategy.Class.DatabaseRoleTypes.Where(v => v.IsCloneable()))
            {
                var role = @this.Strategy.GetRole(roleType);
                clone.Strategy.SetRole(roleType, role);
            }

            return clone;
        }

        public static T Clone<T>(this T @this, IEnumerable<Node> deepClone) where T : IObject => @this.Clone(deepClone?.ToArray());

        public static T Clone<T>(this T @this, params Node[] deepClone) where T : IObject
        {
            if (deepClone == null || deepClone.Length == 0)
            {
                return @this.Clone();
            }

            var strategy = @this.Strategy;

            var clone = (T)DefaultObjectBuilder.Build(strategy.Transaction, strategy.Class);

            foreach (var roleType in strategy.Class.DatabaseRoleTypes.Where(v => v.IsCloneable() && !deepClone.Any(w => w.PropertyType.Equals(v))))
            {
                var role = strategy.GetRole(roleType);
                clone.Strategy.SetRole(roleType, role);
            }

            foreach (var node in deepClone)
            {
                var roleType = (IRoleType)node.PropertyType;
                if (roleType.IsOne)
                {
                    var role = strategy.GetCompositeRole(roleType);
                    if (role != null)
                    {
                        clone.Strategy.SetCompositeRole(roleType, role.Clone(node.Nodes));
                    }
                }
                else
                {
                    foreach (var role in strategy.GetCompositesRole<IObject>(roleType))
                    {
                        clone.Strategy.AddCompositesRole(roleType, role.Clone(node.Nodes));
                    }
                }
            }

            return clone;
        }
    }
}
