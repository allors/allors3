// <copyright file="IDomainChangeSet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Derivations
{
    using Meta;
    using Database.Derivations;
    using System.Linq;

    public static class IDomainValidationExtensions
    {
        public static void AssertExists(this IDomainValidation @this, IObject association, IRoleType roleType)
        {
            if (!association.Strategy.ExistRole(roleType))
            {
                @this.AddError($"AssertExists: {DerivationRelation.Create(association, roleType)[0]}");
            }
        }

        public static void AssertNotExists(this IDomainValidation @this, IObject association, IRoleType roleType)
        {
            if (association.Strategy.ExistRole(roleType))
            {
                @this.AddError($"AssertNotExists: {DerivationRelation.Create(association, roleType)[0]}");
            }
        }

        public static void AssertNonEmptyString(this IDomainValidation @this, IObject association, IRoleType roleType)
        {
            if (association.Strategy.ExistRole(roleType))
            {
                if (association.Strategy.GetUnitRole(roleType).Equals(string.Empty))
                {
                    @this.AddError($"AssertNonEmptyString: {DerivationRelation.Create(association, roleType)[0]}");
                }
            }
        }

        public static void AssertExistsNonEmptyString(this IDomainValidation @this, IObject association, IRoleType roleType)
        {
            AssertExists(@this, association, roleType);
            AssertNonEmptyString(@this, association, roleType);
        }

        public static void AssertIsUnique(this IDomainValidation @this, IObject association, IRoleType roleType, IChangeSet changeSet)
        {
            if (changeSet.RoleTypesByAssociation.TryGetValue(association.Id, out var roleTypes))
            {
                if (roleTypes.Contains(roleType))
                {
                    var objectType = roleType.AssociationType.ObjectType;
                    var role = association.Strategy.GetRole(roleType);

                    if (role != null)
                    {
                        var transaction = association.Strategy.Transaction;
                        var extent = transaction.Extent(objectType);
                        _ = extent.Filter.AddEquals(roleType, role);
                        if (extent.Count != 1)
                        {
                            @this.AddError($"AssertIsUnique: {DerivationRelation.Create(association, roleType)[0]}");
                        }
                    }
                }
            }
        }

        public static void AssertAtLeastOne(this IDomainValidation @this, IObject association, params IRoleType[] roleTypes)
        {
            foreach (var roleType in roleTypes)
            {
                if (association.Strategy.ExistRole(roleType))
                {
                    return;
                }
            }

            @this.AddError($"AssertAtLeastOne: {string.Join("\n", DerivationRelation.Create(association, roleTypes).Select(v => v.ToString()))}");
        }

        public static void AssertExistsAtMostOne(this IDomainValidation @this, IObject association, params IRoleType[] roleTypes)
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
                @this.AddError($"AssertExistsAtMostOne: {string.Join("\n", DerivationRelation.Create(association, roleTypes).Select(v => v.ToString()))}");
            }
        }

        public static void AssertAreEqual(this IDomainValidation @this, IObject association, IRoleType roleType, IRoleType otherRoleType)
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
                @this.AddError($"AssertAreEqual: {string.Join("\n", DerivationRelation.Create(association, roleType, otherRoleType).Select(v => v.ToString()))}");
            }
        }

        public static void AssertExists(this IDomainValidation @this, IObject role, IAssociationType associationType)
        {
            if (!role.Strategy.ExistAssociation(associationType))
            {
                @this.AddError($"AssertExists: {DerivationRelation.Create(role, associationType)[0]}");
            }
        }
    }
}
