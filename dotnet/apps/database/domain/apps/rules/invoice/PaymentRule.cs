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
    using Resources;

    public class PaymentRule : Rule
    {
        public PaymentRule(MetaPopulation m) : base(m, new Guid("4C7D0834-A7F2-4ED6-AC58-9B2DFD719ED9")) =>
            this.Patterns = new[]
            {
                m.Payment.RolePattern(v => v.PaymentApplications),
                m.Payment.RolePattern(v => v.Amount),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Payment>())
            {
                decimal totalAmountApplied = 0;
                foreach (var paymentApplication in @this.PaymentApplications)
                {
                    totalAmountApplied += paymentApplication.AmountApplied;
                }

                if (@this.ExistAmount && totalAmountApplied > @this.Amount)
                {
                    cycle.Validation.AddError(@this, this.M.Payment.Amount, ErrorMessages.PaymentAmountIsToSmall);
                }
            }
        }
    }
}
