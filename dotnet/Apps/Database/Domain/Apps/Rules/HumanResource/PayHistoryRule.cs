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

    public class PayHistoryRule : Rule
    {
        public PayHistoryRule(MetaPopulation m) : base(m, new Guid("73e0bcd1-958e-451c-abf4-0b759d1ede4d")) =>
            this.Patterns = new Pattern[]
            {
                m.PayHistory.RolePattern(v => v.SalaryStep),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PayHistory>())
            {
                validation.AssertAtLeastOne(@this, @this.M.PayHistory.Amount, @this.M.PayHistory.SalaryStep);
                validation.AssertExistsAtMostOne(@this, @this.M.PayHistory.Amount, @this.M.PayHistory.SalaryStep);
            }
        }
    }
}
