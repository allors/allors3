// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class UserNormalizedUserEmailRule : Rule
    {
        public UserNormalizedUserEmailRule(MetaPopulation m) : base(m, new Guid("904187C3-773E-47BC-A2EA-EF45ECA78FD2")) =>
            this.Patterns = new Pattern[]
            {
                m.User.RolePattern(v=>v.UserEmail),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<User>())
            {
                @this.NormalizedUserEmail = Users.Normalize(@this.UserEmail);
            }
        }
    }
}
