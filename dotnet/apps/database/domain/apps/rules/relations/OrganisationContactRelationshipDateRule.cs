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

    public class OrganisationContactRelationshipDateRule : Rule
    {
        public OrganisationContactRelationshipDateRule(MetaPopulation m) : base(m, new Guid("A00B983C-6766-406F-B137-19430890547A")) =>
            this.Patterns = new Pattern[]
            {
                m.OrganisationContactRelationship.RolePattern(v => v.Contact),
                m.OrganisationContactRelationship.RolePattern(v => v.FromDate),
                m.OrganisationContactRelationship.RolePattern(v => v.ThroughDate),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<OrganisationContactRelationship>())
            {
                if (@this.Organisation?.ContactsUserGroup != null)
                {
                    if (!(@this.FromDate <= transaction.Now()
                        && (!@this.ExistThroughDate
                        || @this.ThroughDate >= transaction.Now())))
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
