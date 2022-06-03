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

    public class GrantSecurityCacheRule : Rule
    {
        public GrantSecurityCacheRule(MetaPopulation m) : base(m, new Guid("3E12D31C-913E-4CE9-A38D-3C3C0D056DA4")) =>
            this.Patterns = new Pattern[]
            {
                m.Grant.RolePattern(v=>v.EffectivePermissions),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var grant in matches.Cast<Grant>())
            {
                grant.SecurityCacheId = Guid.NewGuid();
            }
        }
    }
}
