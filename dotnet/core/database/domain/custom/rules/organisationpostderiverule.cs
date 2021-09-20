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

    public class OrganisationPostDeriveRule : Rule
    {
        public OrganisationPostDeriveRule(MetaPopulation m) : base(m, new Guid("755E60CF-1D5E-4D24-8FDE-396FF7C3030B")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern<Meta.Organisation>(m.Organisation, v => v.PostDeriveTrigger),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var organisation in matches.Cast<Organisation>())
            {
                organisation.PostDeriveTriggered = organisation.PostDeriveTrigger;
            }
        }
    }
}
