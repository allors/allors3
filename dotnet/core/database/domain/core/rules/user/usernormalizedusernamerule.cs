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

    public class UserNormalizedUserNameRule : Rule
    {
        public UserNormalizedUserNameRule(MetaPopulation m) : base(m, new Guid("FD6F30D8-FF50-44FA-8863-343D2B08783B")) =>
            this.Patterns = new Pattern[]
            {
                m.User.RolePattern(v=>v.UserName),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<User>())
            {
                @this.NormalizedUserName = Users.Normalize(@this.UserName);
            }
        }
    }
}
