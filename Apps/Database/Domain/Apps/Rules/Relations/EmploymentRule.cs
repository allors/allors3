// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class EmploymentRule : Rule
    {
        public EmploymentRule(MetaPopulation m) : base(m, new Guid("F0587A19-E7CF-40FF-B715-5A6021525326")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Employment, m.Employment.Employee),
                new RolePattern(m.Employment, m.Employment.Employer),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Employment>())
            {
                @this.Parties = new Party[] { @this.Employee, @this.Employer };
            }
        }
    }
}
