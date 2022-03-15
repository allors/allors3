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

    public class SecurityTokenPermissionsRule : Rule
    {
        public SecurityTokenPermissionsRule(MetaPopulation m) : base(m, new Guid("E46C7A4C-7F63-4A5C-AE53-E8B43660FC47")) =>
            this.Patterns = new Pattern[]
            {
                m.SecurityToken.RolePattern(v=>v.Grants),
                m.Grant.RolePattern(v=>v.EffectivePermissions, v=>v.SecurityTokensWhereGrant),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var securityToken in matches.Cast<SecurityToken>())
            {
                securityToken.Permissions = securityToken.Grants.SelectMany(v => v.EffectivePermissions).ToArray();
            }
        }
    }
}
