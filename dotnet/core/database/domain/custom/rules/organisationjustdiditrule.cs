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
    using Derivations.Rules;
    using Meta;

    public class OrganisationJustDidItRule : Rule
    {
        public OrganisationJustDidItRule(MetaPopulation m) : base(m, new Guid("69C87CD7-52DE-45ED-8709-898A3A701A71")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern<Meta.Organisation>(m.Organisation, v => v.JustDidIt),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var organisation in matches.Cast<Organisation>())
            {
                organisation.JustDidItDerived = organisation.JustDidIt;
            }
        }
    }
}
