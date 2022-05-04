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
    using Meta;
    using Derivations.Rules;

    public class UserGroupOutMembersRuleRule : Rule
    {
        public UserGroupOutMembersRuleRule(MetaPopulation m) : base(m, new Guid("59aa339d-af58-416f-8765-48779166ac68")) =>
            this.Patterns = new Pattern[]
            {
                m.UserGroup.RolePattern(v => v.OutMembers),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<UserGroup>())
            {
                foreach(User user in @this.OutMembers)
                {
                    @this.RemoveMember(user);
                }

                @this.RemoveOutMembers();
            }
        }
    }
}
