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

    public class PersonTimeSheetWorkerRule : Rule
    {
        // TODO: Martien
        // TODO: Koen
        // Is time sensitive and should be in scheduler
        public PersonTimeSheetWorkerRule(MetaPopulation m) : base(m, new Guid("34705b9b-4634-4cbb-b9a0-a2cbec9f5cd9")) =>
            this.Patterns = new Pattern[]
            {
                m.Person.AssociationPattern(v => v.OrganisationContactRelationshipsWhereContact),
                m.OrganisationContactRelationship.RolePattern(v => v.FromDate, v => v.Contact),
                m.OrganisationContactRelationship.RolePattern(v => v.ThroughDate, v => v.Contact),
                m.Employment.RolePattern(v => v.FromDate, v => v.Employee),
                m.Employment.RolePattern(v => v.ThroughDate, v => v.Employee),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Person>())
            {
                var now = @this.Transaction().Now();

                @this.Strategy.Transaction.Prefetch(@this.PrefetchPolicy);

                if (!@this.ExistTimeSheetWhereWorker && (@this.AppsIsActiveEmployee(now) || @this.CurrentOrganisationContactRelationships.Any()))
                {
                    new TimeSheetBuilder(@this.Strategy.Transaction).WithWorker(@this).Build();
                }
            }
        }
    }
}
