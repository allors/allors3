// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using System.Text;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class PersonCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdPersons = changeSet.Created.Select(v=>v.GetObject()).OfType<Person>();

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

        static string DerivePartyName(this Person person)
        {
            var partyName = new StringBuilder();

            if (person.ExistFirstName)
            {
                partyName.Append(person.FirstName);
            }

            if (person.ExistMiddleName)
            {
                if (partyName.Length > 0)
                {
                    partyName.Append(" ");
                }

                partyName.Append(person.MiddleName);
            }

            if (person.ExistLastName)
            {
                if (partyName.Length > 0)
                {
                    partyName.Append(" ");
                }

                partyName.Append(person.LastName);
            }

            if (partyName.Length == 0)
            {
                partyName.Append($"[{person.UserName}]");
            }

            return partyName.ToString();
        }

        public class PersonSyncDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                changeSet.AssociationsByRoleType.TryGetValue(M.Organisation.PartyContactMechanisms.RoleType, out var changedOrganisations);
                var organisationWherePartyContactMechanismChanged = changedOrganisations?.Select(session.Instantiate).OfType<Organisation>();

                if (organisationWherePartyContactMechanismChanged?.Any() == true)
                {
                    foreach (var organisation in organisationWherePartyContactMechanismChanged)
                    {
                        SyncContactPartyContactMechanism(organisation);
                    }
                }
            }

            public static void SyncContactPartyContactMechanism(Organisation organisation)
            {
                var partyContactMechanisms = organisation.PartyContactMechanisms.ToArray();
                foreach (OrganisationContactRelationship organisationContactRelationship in organisation.OrganisationContactRelationshipsWhereOrganisation)
                {
                    foreach (var partyContactMechanism in partyContactMechanisms)
                    {
                        var person = organisationContactRelationship.Contact;

                        (person).RemoveCurrentOrganisationContactMechanism(partyContactMechanism.ContactMechanism);

                        if (partyContactMechanism.FromDate <= person.Session().Now() &&
                            (!partyContactMechanism.ExistThroughDate || partyContactMechanism.ThroughDate >= person.Session().Now()))
                        {
                            (person).AddCurrentOrganisationContactMechanism(partyContactMechanism.ContactMechanism);
                        }
                    }
                }
            }
        }

        public static void PersonRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("2EE5DF33-C797-426B-AB9F-19065513F4B8")] = new PersonCreationDerivation();
            @this.DomainDerivationById[new Guid("83EBBE6E-226D-4A25-815F-334CD25BB679")] = new PersonSyncDerivation();
        }
    }
}
