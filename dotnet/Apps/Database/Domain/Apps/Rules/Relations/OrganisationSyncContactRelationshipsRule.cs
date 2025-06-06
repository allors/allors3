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

    public class OrganisationSyncContactRelationshipsRule : Rule
    {
        public OrganisationSyncContactRelationshipsRule(MetaPopulation m) : base(m, new Guid("9a950f76-9a7a-447b-b208-b01adc09b64d")) =>
            this.Patterns = new Pattern[]
            {
                m.Organisation.AssociationPattern(v => v.OrganisationContactRelationshipsWhereOrganisation),
                m.OrganisationContactRelationship.RolePattern(v => v.FromDate, v => v.Organisation),
                m.OrganisationContactRelationship.RolePattern(v => v.ThroughDate, v => v.Organisation),
                m.Party.AssociationPattern(v => v.PartyContactMechanismsWhereParty, m.Organisation),
                m.PartyContactMechanism.RolePattern(v => v.FromDate, v => v.Party.ObjectType, m.Organisation),
                m.PartyContactMechanism.RolePattern(v => v.ThroughDate, v => v.Party.ObjectType, m.Organisation),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<Organisation>())
            {
                transaction.Prefetch(@this.PrefetchPolicy, @this);

                var partyContactMechanisms = @this.PartyContactMechanismsWhereParty?.ToArray();
                foreach (var organisationContactRelationship in @this.OrganisationContactRelationshipsWhereOrganisation)
                {
                    organisationContactRelationship.Contact?.Sync(partyContactMechanisms);
                }
            }
        }
    }
}
