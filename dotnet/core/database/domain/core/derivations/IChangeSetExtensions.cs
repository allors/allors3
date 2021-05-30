// <copyright file="IChangeSetExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>


namespace Allors.Database.Domain.Derivations
{
    using Meta;
    using System.Linq;
    using Object = Domain.Object;

    public static partial class IChangeSetExtensions
    {

        public static bool IsCreated(this IChangeSet @this, Object derivable) => @this.Created.Contains(derivable.Strategy);

        public static bool HasChangedRole(this IChangeSet @this, Object derivable, IRoleType roleType)
        {
            _ = @this.RoleTypesByAssociation.TryGetValue(derivable.Id, out var changedRoleTypes);
            return changedRoleTypes?.Contains(roleType) ?? false;
        }

        public static bool HasChangedRoles(this IChangeSet @this, Object derivable, params IRoleType[] roleTypes)
        {
            _ = @this.RoleTypesByAssociation.TryGetValue(derivable.Id, out var changedRoleTypes);
            if (changedRoleTypes != null)
            {
                if (roleTypes.Length == 0 || roleTypes.Any(roleType => changedRoleTypes.Contains(roleType)))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool HasChangedRoles(this IChangeSet @this, Object derivable, RelationKind relationKind)
        {
            System.Func<IRoleType, bool> check;
            switch (relationKind)
            {
                case RelationKind.Regular:
                    check = (roleType) => !roleType.RelationType.IsDerived && !roleType.RelationType.IsSynced;
                    break;

                case RelationKind.Derived:
                    check = (roleType) => roleType.RelationType.IsDerived;
                    break;

                case RelationKind.Synced:
                    check = (roleType) => roleType.RelationType.IsSynced;
                    break;

                default:
                    check = (roleType) => true;
                    break;
            }

            _ = @this.RoleTypesByAssociation.TryGetValue(derivable.Id, out var changedRoleTypes);
            if (changedRoleTypes != null)
            {
                if (changedRoleTypes.Any(roleType => check(roleType)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
