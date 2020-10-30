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
                new ChangedPattern(M.OrganisationContactRelationship.FromDate),
                new ChangedPattern(M.OrganisationContactRelationship.ThroughDate),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;

            foreach (var @this in matches.Cast<OrganisationContactRelationship>())
            {
                if (@this.Organisation?.ContactsUserGroup != null)
                {
                    if (!(@this.FromDate <= session.Now()
                        && (!@this.ExistThroughDate
                        || @this.ThroughDate >= session.Now())))
                    {
                        @this.Organisation.ContactsUserGroup
                            .RemoveMember(@this.Contact);
                    }
                    else
                    {
                        @this.Organisation.ContactsUserGroup
                            .AddMember(@this.Contact);
                    }
                }
            }
        }
    }
}
