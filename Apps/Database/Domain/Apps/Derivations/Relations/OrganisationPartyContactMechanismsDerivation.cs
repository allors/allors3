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

    public class OrganisationPartyContactMechanismsDerivation : DomainDerivation
    {
        public OrganisationPartyContactMechanismsDerivation(M m) : base(m, new Guid("BB960F7C-2B67-4B4D-967A-84B50F55BE6E")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.Organisation.PartyContactMechanisms),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Organisation>())
            {
                var partyContactMechanisms = @this.PartyContactMechanisms.ToArray();
                foreach (OrganisationContactRelationship organisationContactRelationship in @this.OrganisationContactRelationshipsWhereOrganisation)
                {
                    foreach (var partyContactMechanism in partyContactMechanisms)
                    {
                        var person = organisationContactRelationship.Contact;

                        person.RemoveCurrentOrganisationContactMechanism(partyContactMechanism.ContactMechanism);

                        if (partyContactMechanism.FromDate <= person.Session().Now() &&
                            (!partyContactMechanism.ExistThroughDate ||
                             partyContactMechanism.ThroughDate >= person.Session().Now()))
                        {
                            person.AddCurrentOrganisationContactMechanism(partyContactMechanism.ContactMechanism);
                        }
                    }
                }
            }
        }
    }
}
