// <copyright file="UserEntraIdentityRule.cs" company="Allors bv">
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

    // An Entra identity is the pair of a tenant id and an object id, and it belongs to one user.
    // The rule refuses half a pair, and a pair that another user in the view of the transaction
    // already has.
    //
    // This is not a guarantee of the store. The view of a transaction does not hold what a
    // transaction that runs at the same time creates, and the store has no unique index: two
    // servers can each create a user for the first sign-in of one person.
    // Users.FindByEntraIdentity then finds the oldest of them.
    public class UserEntraIdentityRule : Rule
    {
        public UserEntraIdentityRule(MetaPopulation m) : base(m, new Guid("c3497da1-319d-414d-8852-d40df73adedb")) =>
            this.Patterns = new Pattern[]
            {
                m.User.RolePattern(v => v.EntraTenantId),
                m.User.RolePattern(v => v.EntraObjectId),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var m = this.M;

            foreach (var user in matches.Cast<User>())
            {
                if (user.ExistEntraTenantId != user.ExistEntraObjectId)
                {
                    cycle.Validation.AssertExists(user, m.User.EntraTenantId);
                    cycle.Validation.AssertExists(user, m.User.EntraObjectId);
                    continue;
                }

                cycle.Validation.AssertIsUnique(cycle.ChangeSet, user, m.User, m.User.EntraTenantId, m.User.EntraObjectId);
            }
        }
    }
}
