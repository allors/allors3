// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class OrganisationContactRelationshipDateDerivation : DomainDerivation
    {
        public OrganisationContactRelationshipDateDerivation(M m) : base(m, new Guid("A00B983C-6766-406F-B137-19430890547A")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedConcreteRolePattern(this.M.OrganisationContactRelationship.FromDate),
                new ChangedConcreteRolePattern(this.M.OrganisationContactRelationship.ThroughDate),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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
