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
    using Derivations.Rules;

    public class PartyContactMechanismRule : Rule
    {
        public PartyContactMechanismRule(MetaPopulation m) : base(m, new Guid("7C4E6217-8D71-4544-B8E4-8B2C51F6A5C1")) =>
            this.Patterns = new Pattern[]
            {
                m.PartyContactMechanism.RolePattern(v => v.ContactPurposes),
                m.PartyContactMechanism.RolePattern(v => v.UseAsDefault),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PartyContactMechanism>())
            {
                if (@this.UseAsDefault)
                {
                    cycle.Validation.AssertExists(@this, this.M.PartyContactMechanism.ContactPurposes);
                }

                if (@this.UseAsDefault && @this.ExistParty && @this.ExistContactPurposes)
                {
                    foreach (var contactMechanismPurpose in @this.ContactPurposes)
                    {
                        foreach (var partyContactMechanismFromOrganisationWherePartyContactMechanism in @this.Party.PartyContactMechanismsWhereParty.Where(v => v.ContactPurposes.Contains(contactMechanismPurpose)))
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
