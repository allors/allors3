// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public class OrganisationContactRelationshipDateDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("C7C44D1F-11F1-4A48-8385-491089090F44");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new ChangedRolePattern(M.OrganisationContactRelationship.FromDate.RoleType),
            new ChangedRolePattern(M.OrganisationContactRelationship.ThroughDate.RoleType),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;

            foreach (var organisationContactRelationship in matches.Cast<OrganisationContactRelationship>())
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
