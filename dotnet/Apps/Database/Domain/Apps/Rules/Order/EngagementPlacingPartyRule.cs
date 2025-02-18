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

    public class EngagementPlacingPartyRule : Rule
    {
        public EngagementPlacingPartyRule(MetaPopulation m) : base(m, new Guid("D2E8DC2A-BB26-4E85-A3DF-6D379A3CD0F0")) =>
            this.Patterns = new[]
            {
                m.Engagement.RolePattern(v => v.PlacingParty),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Engagement>())
            {
                if (!@this.ExistPlacingContactMechanism && @this.ExistPlacingParty)
                {
                    @this.PlacingContactMechanism = @this.PlacingParty.OrderAddress;
                }
            }
        }
    }
}
