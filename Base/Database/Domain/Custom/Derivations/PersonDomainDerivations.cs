// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class PersonCreationDerivation : IDomainDerivation
        {
            public void Derive(IDomainChangeSet changeSet, IDomainValidation validation)
            {
                var createdPersons = changeSet.Created.OfType<Person>();

                foreach (var person in createdPersons)
                {
                    var now = person.Session().Now();

                    person.Strategy.Session.Prefetch(person.PrefetchPolicy);

                    if (person.ExistSalutation
                        && (person.Salutation.Equals(new Salutations(person.Session()).Mr)
                            || person.Salutation.Equals(new Salutations(person.Session()).Dr)))
                    {
                        person.Gender = new GenderTypes(person.Session()).Male;
                    }

                    if (person.ExistSalutation
                        && (person.Salutation.Equals(new Salutations(person.Session()).Mrs)
                            || person.Salutation.Equals(new Salutations(person.Session()).Ms)
                            || person.Salutation.Equals(new Salutations(person.Session()).Mme)))
                    {
                        person.Gender = new GenderTypes(person.Session()).Female;
                    }

                    person.PartyName = person.DerivePartyName();

                    person.VatRegime = new VatRegimes(person.Session()).PrivatePerson;

                    person.DeriveRelationships();

                    if (!person.ExistTimeSheetWhereWorker && (person.BaseIsActiveEmployee(now) || person.CurrentOrganisationContactRelationships.Count > 0))
                    {
                        new TimeSheetBuilder(person.Strategy.Session).WithWorker(person).Build();
                    }

                    var deletePermission = new Permissions(person.Strategy.Session).Get(person.Meta.ObjectType, person.Meta.Delete, Operations.Execute);
                    if (person.IsDeletable)
                    {
                        person.RemoveDeniedPermission(deletePermission);
                    }
                    else
                    {
                        person.AddDeniedPermission(deletePermission);
                    }
                }
            }
        }

        public class PersonSyncDerivation : IDomainDerivation
        {
            public void Derive(IDomainChangeSet changeSet, IDomainValidation validation)
            {
                changeSet.AssociationsByRoleType.TryGetValue(M.Organisation.PartyContactMechanisms.RoleType, out var changedOrganisations);
                var organisationWherePartyContactMechanismChanged = changedOrganisations?.OfType<Organisation>();

                if (organisationWherePartyContactMechanismChanged?.Any() == true)
                {
                    foreach (var organisation in organisationWherePartyContactMechanismChanged)
                    {
                        var partyContactMechanisms = organisation.PartyContactMechanisms.ToArray();
                        foreach (OrganisationContactRelationship organisationContactRelationship in organisation.OrganisationContactRelationshipsWhereOrganisation)
                        {
                            foreach (var partyContactMechanism in partyContactMechanisms)
                            {
                                var person = organisationContactRelationship.Contact;

                                ((PersonDerivedRoles)person).RemoveCurrentOrganisationContactMechanism(partyContactMechanism.ContactMechanism);

                                if (partyContactMechanism.FromDate <= person.Session().Now() &&
                                    (!partyContactMechanism.ExistThroughDate || partyContactMechanism.ThroughDate >= person.Session().Now()))
                                {
                                    ((PersonDerivedRoles)person).AddCurrentOrganisationContactMechanism(partyContactMechanism.ContactMechanism);
                                }
                            }
                        }
                    }
                }
            }
        }

        public class PersonPartyContactMechanismChangeDerivation : IDomainDerivation
        {
            public void Derive(IDomainChangeSet changeSet, IDomainValidation validation)
            {
                changeSet.AssociationsByRoleType.TryGetValue(M.PartyContactMechanism.FromDate.RoleType, out var changedPartyContactMechanisms);
                var partyContactMechansmWhereFromDateChanged = changedPartyContactMechanisms?.OfType<PartyContactMechanism>();

                if (partyContactMechansmWhereFromDateChanged?.Any() == true)
                {
                    foreach (var partyContactMechanism in partyContactMechansmWhereFromDateChanged)
                    {
                        var temp = partyContactMechanism.PartyWhereCurrentPartyContactMechanism;
                    }
                }
            }
        }

        public static void PersonRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("2EE5DF33-C797-426B-AB9F-19065513F4B8")] = new PersonCreationDerivation();
            @this.DomainDerivationById[new Guid("83EBBE6E-226D-4A25-815F-334CD25BB679")] = new PersonSyncDerivation();
            @this.DomainDerivationById[new Guid("B6FCF861-DF51-4AAA-8A73-254FD5DC2445")] = new PersonPartyContactMechanismChangeDerivation();
        }
    }
}
