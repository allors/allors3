// <copyright file="Domain.cs" company="Allors bv">
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

    public class DiscountComponentRule : Rule
    {
        public DiscountComponentRule(MetaPopulation m) : base(m, new Guid("C395DB2E-C4A6-4974-BE35-EF2CC70D347D")) =>
            this.Patterns = new Pattern[]
            {
                m.DiscountComponent.RolePattern(v => v.Price),
                m.DiscountComponent.RolePattern(v => v.Percentage),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<DiscountComponent>())
            {
                validation.AssertExistsAtMostOne(@this, this.M.DiscountComponent.Price, this.M.DiscountComponent.Percentage);
            }
        }
    }
}
