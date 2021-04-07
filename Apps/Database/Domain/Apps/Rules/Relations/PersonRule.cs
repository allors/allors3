// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Meta;
    using Database.Derivations;

    public class PersonRule : Rule
    {
        public PersonRule(MetaPopulation m) : base(m, new Guid("BC3969F4-4050-47A1-B80C-8F23879E3B10")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Person, m.Person.DerivationTrigger),
                new RolePattern(m.OrganisationContactRelationship, m.OrganisationContactRelationship.FromDate) {Steps = new IPropertyType[]{ m.OrganisationContactRelationship.Contact } },
                new RolePattern(m.OrganisationContactRelationship, m.OrganisationContactRelationship.ThroughDate) {Steps = new IPropertyType[]{ m.OrganisationContactRelationship.Contact } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Person>())
            {
                @this.Strategy.Transaction.Prefetch(@this.PrefetchPolicy);

                @this.DeriveRelationships();
            }
        }
    }
}
