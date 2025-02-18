// <copyright file="SurchargeComponentDerivation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class SurchargeComponentRule : Rule
    {
        public SurchargeComponentRule(MetaPopulation m) : base(m, new Guid("1C8B75D1-3288-4DB7-987E-7A64A3225891")) =>
            this.Patterns = new Pattern[]
            {
                m.SurchargeComponent.RolePattern(v => v.Price),
                m.SurchargeComponent.RolePattern(v => v.Percentage),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SurchargeComponent>())
            {
                validation.AssertExistsAtMostOne(@this, this.M.SurchargeComponent.Price, this.M.SurchargeComponent.Percentage);
            }
        }
    }
}
