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

                foreach(var organisationContactRelationship in createdOrganisationContactRelationships)
                {
                    organisationContactRelationship.Parties = new Party[] { organisationContactRelationship.Contact, organisationContactRelationship.Organisation };
                }
                
            }
        }

        public static void OrganisationContactRelationshipRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("82517e91-adb3-43f3-b6c8-13f8f0142e35")] = new OrganisationContactRelationshipCreationDerivation();
        }
    }
}
