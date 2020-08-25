// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class PartyContactMechanismCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdPartyContactMechanism = changeSet.Created.Select(session.Instantiate).OfType<PartyContactMechanism>();

                foreach (var partyContactMechanism in createdPartyContactMechanism)
                {
                    if (partyContactMechanism.ExistUseAsDefault && partyContactMechanism.UseAsDefault)
                    {
                        validation.AssertExists(partyContactMechanism, M.PartyContactMechanism.ContactPurposes);
                    }

                    if (partyContactMechanism.UseAsDefault && partyContactMechanism.ExistPartyWherePartyContactMechanism && partyContactMechanism.ExistContactPurposes)
                    {
                        foreach (var contactMechanismPurpose in partyContactMechanism.ContactPurposes)
                        {
                            var partyContactMechanisms = partyContactMechanism.PartyWherePartyContactMechanism.PartyContactMechanisms;
                            partyContactMechanisms.Filter.AddContains(M.PartyContactMechanism.ContactPurposes, (IObject)contactMechanismPurpose);

                            foreach (PartyContactMechanism partyContactMechanismFromOrganisationWherePartyContactMechanism in partyContactMechanisms)
                            {
                                if (!partyContactMechanismFromOrganisationWherePartyContactMechanism.Equals(partyContactMechanism))
                                {
                                    partyContactMechanismFromOrganisationWherePartyContactMechanism.UseAsDefault = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void PartyContactMechanismRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("b6d04c19-d647-43b9-9bd1-f8c3f47a5b32")] = new PartyContactMechanismCreationDerivation();
        }
    }
}
