// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class OrganisationPartyNameRule : Rule
    {
        public OrganisationPartyNameRule(MetaPopulation m) : base(m, new Guid("27c869fa-60ff-478e-abec-c42ff5ba606f")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Organisation, m.Organisation.Name),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<Organisation>())
            {
                transaction.Prefetch(@this.PrefetchPolicy);

                @this.PartyName = @this.Name;
            }
        }
    }
}
