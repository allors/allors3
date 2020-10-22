// <copyright file="IDomainChangeSet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain.Derivations
{
    using Allors.Meta;

    public static class IDomainValidationExtensions
    {
        public static void AssertExists(this IDomainValidation @this, IObject association, RoleType roleType)
        {
            if (!association.Strategy.ExistRole(roleType))
            {
                @this.AddError($"AssertExists: {DerivationRelation.Create(association, roleType)}");
            }
        }

        public static void AssertNotExists(this IDomainValidation @this, IObject association, RoleType roleType)
        {
            if (association.Strategy.ExistRole(roleType))
            {
                @this.AddError($"AssertNotExists: {DerivationRelation.Create(association, roleType)}");
            }
        }

        public static void AssertNonEmptyString(this IDomainValidation @this, IObject association, RoleType roleType)
        {
            if (association.Strategy.ExistRole(roleType))
            {
                if (association.Strategy.GetUnitRole(roleType).Equals(string.Empty))
                {
                    @this.AddError($"AssertNonEmptyString: {DerivationRelation.Create(association, roleType)}");
                }
            }
        }

        public static void AssertExistsNonEmptyString(this IDomainValidation @this, IObject association, RoleType roleType)
        {
            AssertExists(@this, association, roleType);
            AssertNonEmptyString(@this, association, roleType);
        }

        public static void AssertIsUnique(this IDomainValidation @this, IObject association, RoleType roleType, IChangeSet changeSet)
        {
            if (changeSet.RoleTypesByAssociation.TryGetValue(association.Id, out var roleTypes))
            {
                if (roleTypes.Contains(roleType))
                {
                    var objectType = roleType.AssociationType.ObjectType;
                    var role = association.Strategy.GetRole(roleType);

                    if (role != null)
                    {
                        var session = association.Strategy.Session;
                        var extent = session.Extent(objectType);
                        extent.Filter.AddEquals(roleType, role);
                        if (extent.Count != 1)
                        {
                            @this.AddError($"AssertIsUnique: {DerivationRelation.Create(association, roleType)}");
                        }
                    }
                }
            }
        }

        public static void AssertAtLeastOne(this IDomainValidation @this, IObject association, params RoleType[] roleTypes)
        {
            foreach (var roleType in roleTypes)
            {
                if (association.Strategy.ExistRole(roleType))
                {
                    return;
                }
            }

            @this.AddError($"AssertAtLeastOne: {DerivationRelation.Create(association, roleTypes)}");
        }

        public static void AssertExistsAtMostOne(this IDomainValidation @this, IObject association, params RoleType[] roleTypes)
        {
            var count = 0;
            foreach (var roleType in roleTypes)
            {
                if (association.Strategy.ExistRole(roleType))
                {
                    ++count;
                }
            }

            if (count > 1)
            {
                @this.AddError($"AssertExistsAtMostOne: {DerivationRelation.Create(association, roleTypes)}");
            }
        }

        public static void AssertAreEqual(this IDomainValidation @this, IObject association, RoleType roleType, RoleType otherRoleType)
        {
            var value = association.Strategy.GetRole(roleType);
            var otherValue = association.Strategy.GetRole(otherRoleType);

            bool equal;
            if (value == null)
            {
                equal = otherValue == null;
            }
            else
            {
                equal = value.Equals(otherValue);
            }

            if (!equal)
            {
                @this.AddError($"AssertAreEqual: {DerivationRelation.Create(association, roleType, otherRoleType)}");
            }
        }

        public static void AssertExists(this IDomainValidation @this, IObject role, AssociationType associationType)
        {
            if (!role.Strategy.ExistAssociation(associationType))
            {
                @this.AddError($"AssertExists: {DerivationRelation.Create(role, associationType)}");
            }
        }
    }
}
