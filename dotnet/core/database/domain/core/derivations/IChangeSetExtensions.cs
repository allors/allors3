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
        public static bool IsCreated(this IChangeSet @this, Object derivable) => @this.Created.Contains(derivable);

        public static bool HasChangedRole(this IChangeSet @this, Object derivable, IRoleType roleType)
        {
            @this.RoleTypesByAssociation.TryGetValue(derivable, out var changedRoleTypes);
            return changedRoleTypes?.Contains(roleType) ?? false;
        }

        public static bool HasChangedRoles(this IChangeSet @this, Object derivable, params IRoleType[] roleTypes)
        {
            @this.RoleTypesByAssociation.TryGetValue(derivable, out var changedRoleTypes);
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
            System.Func<IRoleType, bool> check = relationKind switch
            {
                RelationKind.Regular => (roleType) => !roleType.RelationType.IsDerived,
                RelationKind.Derived => (roleType) => roleType.RelationType.IsDerived,
                _ => (_) => true,
            };

            @this.RoleTypesByAssociation.TryGetValue(derivable, out var changedRoleTypes);
            return changedRoleTypes?.Any(roleType => check(roleType)) == true;
        }
    }
}
