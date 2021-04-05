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

    public class EngagementRule : Rule
    {
        public EngagementRule(MetaPopulation m) : base(m, new Guid("D2E8DC2A-BB26-4E85-A3DF-6D379A3CD0F0")) =>
            this.Patterns = new[]
            {
                new RolePattern(m.Engagement, m.Engagement.BillToParty),
                new RolePattern(m.Engagement, m.Engagement.PlacingParty),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Engagement>())
            {
                if (!@this.ExistBillToContactMechanism && @this.ExistBillToParty)
                {
                    @this.BillToContactMechanism = @this.BillToParty.BillingAddress;
                }

                if (!@this.ExistPlacingContactMechanism && @this.ExistPlacingParty)
                {
                    @this.PlacingContactMechanism = @this.PlacingParty.OrderAddress;
                }
            }
        }
    }
}
