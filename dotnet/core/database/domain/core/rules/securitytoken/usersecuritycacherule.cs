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

    public class UserSecurityCacheRule : Rule
    {
        public UserSecurityCacheRule(MetaPopulation m) : base(m, new Guid("302F5D78-3287-40DF-A371-357CAFC60647")) =>
            this.Patterns = new Pattern[]
            {
                m.User.AssociationPattern(v=>v.GrantsWhereEffectiveUser),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var user in matches.Cast<User>())
            {
                user.SecurityCacheId = Guid.NewGuid();
            }
        }
    }
}
