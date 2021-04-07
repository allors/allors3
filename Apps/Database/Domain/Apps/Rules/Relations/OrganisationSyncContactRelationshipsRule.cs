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
                new RolePattern(m.Organisation, m.Organisation.PartyContactMechanisms),
                new AssociationPattern(m.OrganisationContactRelationship.Organisation),
                new RolePattern(m.OrganisationContactRelationship, m.OrganisationContactRelationship.FromDate) {Steps = new IPropertyType[]{ m.OrganisationContactRelationship.Organisation} },
                new RolePattern(m.OrganisationContactRelationship, m.OrganisationContactRelationship.ThroughDate) {Steps = new IPropertyType[]{ m.OrganisationContactRelationship.Organisation} },
                new RolePattern(m.Organisation, m.Organisation.PartyContactMechanisms),
                new RolePattern(m.PartyContactMechanism, m.PartyContactMechanism.FromDate) {Steps = new IPropertyType[]{ m.PartyContactMechanism.PartyWherePartyContactMechanism }, OfType = m.Organisation },
                new RolePattern(m.PartyContactMechanism, m.PartyContactMechanism.ThroughDate) {Steps = new IPropertyType[]{ m.PartyContactMechanism.PartyWherePartyContactMechanism }, OfType = m.Organisation },
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
