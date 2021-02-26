// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;

    public class PartyContactMechanismDerivation : DomainDerivation
    {
        public PartyContactMechanismDerivation(M m) : base(m, new Guid("7C4E6217-8D71-4544-B8E4-8B2C51F6A5C1")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.PartyContactMechanism.ContactPurposes),
                new AssociationPattern(m.PartyContactMechanism.UseAsDefault),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PartyContactMechanism>())
            {
                if (@this.UseAsDefault)
                {
                    cycle.Validation.AssertExists(@this, this.M.PartyContactMechanism.ContactPurposes);
                }

                if (@this.UseAsDefault && @this.ExistPartyWherePartyContactMechanism && @this.ExistContactPurposes)
                {
                    foreach (var contactMechanismPurpose in @this.ContactPurposes)
                    {
                        var partyContactMechanisms = @this.PartyWherePartyContactMechanism.PartyContactMechanisms;
                        partyContactMechanisms.Filter.AddContains(this.M.PartyContactMechanism.ContactPurposes, (IObject)contactMechanismPurpose);

                        foreach (PartyContactMechanism partyContactMechanismFromOrganisationWherePartyContactMechanism in partyContactMechanisms)
                        {
                            if (!partyContactMechanismFromOrganisationWherePartyContactMechanism.Equals(@this))
                            {
                                partyContactMechanismFromOrganisationWherePartyContactMechanism.UseAsDefault = false;
                            }
                        }
                    }
                }
            }
        }
    }
}
