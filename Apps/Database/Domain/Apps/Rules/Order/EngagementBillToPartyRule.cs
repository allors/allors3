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

    public class EngagementBillToPartyRule : Rule
    {
        public EngagementBillToPartyRule(MetaPopulation m) : base(m, new Guid("65720881-aa46-4585-834b-41fb8c82a02e")) =>
            this.Patterns = new[]
            {
                new RolePattern(m.Engagement, m.Engagement.BillToParty),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Engagement>())
            {
                if (!@this.ExistBillToContactMechanism && @this.ExistBillToParty)
                {
                    @this.BillToContactMechanism = @this.BillToParty.BillingAddress;
                }
            }
        }
    }
}
