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

    public class OrganisationEmployementRule : Rule
    {
        public OrganisationEmployementRule(MetaPopulation m) : base(m, new Guid("4B144553-5EED-4B52-BFB3-FACE609C6341")) =>
            this.Patterns = new Pattern[]
            {
                m.Employment.RolePattern(v => v.FromDate, v => v.Employer, m.Organisation),
                m.Employment.RolePattern(v => v.ThroughDate, v => v.Employer, m.Organisation),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<Organisation>())
            {
                var now = @this.Transaction().Now();

                var employments = @this.EmploymentsWhereEmployer.ToArray();

                @this.ActiveEmployments = employments
                    .Where(v => v.FromDate <= now && (!v.ExistThroughDate || v.ThroughDate >= now))
                    .ToArray();

                @this.InactiveEmployments = employments
                    .Except(@this.ActiveEmployments)
                    .ToArray();

                @this.ActiveEmployees = @this.ActiveEmployments
                    .Select(v => v.Employee)
                    .ToArray();
            }
        }
    }
}
