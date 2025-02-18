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
    using Meta;
    using Derivations.Rules;

    public class UserGroupInMembersRule : Rule
    {
        public UserGroupInMembersRule(MetaPopulation m) : base(m, new Guid("5f8a26c5-e6fd-4d7f-b51f-a81258a1d25e")) =>
            this.Patterns = new Pattern[]
            {
                m.UserGroup.RolePattern(v => v.InMembers),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<UserGroup>())
            {
                foreach(User user in @this.InMembers)
                {
                    @this.AddMember(user);
                }

                @this.RemoveInMembers();
            }
        }
    }
}
