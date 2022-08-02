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

    public class GrantEffectivePermissionsRule : Rule
    {
        public GrantEffectivePermissionsRule(MetaPopulation m) : base(m, new Guid("1F897B84-EF92-4E94-8877-3501D56D426B")) =>
            this.Patterns = new Pattern[]
            {
                m.Grant.RolePattern(v=>v.Role),
                m.Role.RolePattern(v=>v.Permissions, v=>v.GrantsWhereRole),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var grant in matches.Cast<Grant>())
            {
                grant.EffectivePermissions = (grant.Role?.Permissions.ToArray());
            }
        }
    }
}
