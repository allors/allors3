// <copyright file="IDomainChangeSet.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain.Derivations
{
    using Allors.Domain.Derivations.Errors;

    public static class IDomainValidationExtensions
    {
        public static void AssertAtLeastOne(this IDomainValidation @this, IObject association, params Meta.RoleType[] roleTypes)
        {
            foreach (var roleType in roleTypes)
            {
                if (association.Strategy.ExistRole(roleType.RelationType))
                {
                    return;
                }
            }

            @this.AddError($"AssertAtLeastOne: {DerivationRelation.Create(association, roleTypes)}");
        }
    }
}
