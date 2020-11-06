// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public class PayHistoryDerivation : DomainDerivation
    {
        public PayHistoryDerivation(M m) : base(m, new Guid("73e0bcd1-958e-451c-abf4-0b759d1ede4d")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.PayHistory.SalaryStep),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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
