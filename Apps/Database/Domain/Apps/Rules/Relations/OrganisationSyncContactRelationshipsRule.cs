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

    public class OrganisationSyncContactRelationshipsRule : Rule
    {
        public OrganisationSyncContactRelationshipsRule(MetaPopulation m) : base(m, new Guid("9a950f76-9a7a-447b-b208-b01adc09b64d")) =>
            this.Patterns = new Pattern[]
            {
                m.Organisation.RolePattern(v => v.PartyContactMechanisms),
                m.Organisation.AssociationPattern(v => v.OrganisationContactRelationshipsWhereOrganisation),
                m.OrganisationContactRelationship.RolePattern(v => v.FromDate, v => v.Organisation),
                m.OrganisationContactRelationship.RolePattern(v => v.ThroughDate, v => v.Organisation),
                m.Organisation.RolePattern(v => v.PartyContactMechanisms),
                m.PartyContactMechanism.RolePattern(v => v.FromDate, v => v.PartyWherePartyContactMechanism, m.Organisation),
                m.PartyContactMechanism.RolePattern(v => v.ThroughDate, v => v.PartyWherePartyContactMechanism, m.Organisation),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<Organisation>())
            {
                transaction.Prefetch(@this.PrefetchPolicy);

                var partyContactMechanisms = @this.PartyContactMechanisms?.ToArray();
                foreach (OrganisationContactRelationship organisationContactRelationship in @this.OrganisationContactRelationshipsWhereOrganisation)
                {
                    organisationContactRelationship.Contact?.Sync(partyContactMechanisms);
                }
            }
        }
    }
}
