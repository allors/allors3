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
        public class OrganisationContactRelationshipCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdOrganisationContactRelationships = changeSet.Created.Select(session.Instantiate).OfType<OrganisationContactRelationship>();

                foreach (var organisationContactRelationship in createdOrganisationContactRelationships)
                {
                    organisationContactRelationship.Parties = new Party[] { organisationContactRelationship.Contact, organisationContactRelationship.Organisation };
                }
            }
        }

        public class OrganisationContactRelationshipDateChangeDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                changeSet.AssociationsByRoleType.TryGetValue(M.OrganisationContactRelationship.FromDate.RoleType, out var changedOrganisationContactRelationship);
                var organisationContactRelationshipWhereFromDateChanged = changedOrganisationContactRelationship?.Select(session.Instantiate).OfType<OrganisationContactRelationship>();

                changeSet.AssociationsByRoleType.TryGetValue(M.OrganisationContactRelationship.ThroughDate.RoleType, out var changedOrganisationContactRelationship2);
                var organisationContactRelationshipWhereThroughDateDateChanged = changedOrganisationContactRelationship2?.Select(session.Instantiate).OfType<OrganisationContactRelationship>();

                ValidateDate(session, organisationContactRelationshipWhereFromDateChanged);
                ValidateDate(session, organisationContactRelationshipWhereThroughDateDateChanged);

                static void ValidateDate(ISession session, System.Collections.Generic.IEnumerable<OrganisationContactRelationship> organisationContactRelationshipWhereFromDateChanged)
                {
                    if (organisationContactRelationshipWhereFromDateChanged?.Any() == true)
                    {
                        foreach (var organisationContactRelationship in organisationContactRelationshipWhereFromDateChanged)
                        {
                            if (organisationContactRelationship.Organisation?.ContactsUserGroup != null)
                            {
                                if (!(organisationContactRelationship.FromDate <= session.Now()
                            && (!organisationContactRelationship.ExistThroughDate
                            || organisationContactRelationship.ThroughDate >= session.Now())))
                                {
                                    organisationContactRelationship.Organisation.ContactsUserGroup
                                        .RemoveMember(organisationContactRelationship.Contact);
                                }
                                else
                                {
                                    organisationContactRelationship.Organisation.ContactsUserGroup
                                        .AddMember(organisationContactRelationship.Contact);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void OrganisationContactRelationshipRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("82517e91-adb3-43f3-b6c8-13f8f0142e35")] = new OrganisationContactRelationshipCreationDerivation();
            @this.DomainDerivationById[new Guid("916296aa-d713-4881-8a21-8f7a45397bfe")] = new OrganisationContactRelationshipDateChangeDerivation();
        }
    }
}
