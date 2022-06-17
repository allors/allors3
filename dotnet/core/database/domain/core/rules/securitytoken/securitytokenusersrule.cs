// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Derivations.Rules;
    using Meta;

    public class SecurityTokenUsersRule : Rule
    {
        public SecurityTokenUsersRule(MetaPopulation m) : base(m, new Guid("BE41A6CF-0B98-473C-8705-0345E3B390E0")) =>
            this.Patterns = new Pattern[]
            {
                m.SecurityToken.RolePattern(v=>v.Grants),
                m.Grant.RolePattern(v=>v.EffectiveUsers, v => v.SecurityTokensWhereGrant),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var securityToken in matches.Cast<SecurityToken>())
            {
                securityToken.DeriveSecurityTokenUsersRule(validation);
            }
        }
    }

    public static class SecurityTokenUsersRuleExtensions
    {
        public static void DeriveSecurityTokenUsersRule(this SecurityToken @this, IValidation validation) => @this.Users = @this.Grants.SelectMany(v => v.EffectiveUsers);
    }
}
