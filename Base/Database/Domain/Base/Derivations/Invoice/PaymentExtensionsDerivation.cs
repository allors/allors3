// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;
    using Resources;

    public class PaymentDerivation : DomainDerivation
    {
        public PaymentDerivation(M m) : base(m, new Guid("4C7D0834-A7F2-4ED6-AC58-9B2DFD719ED9")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(M.Payment.Interface)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var paymentExtension in matches.Cast<Payment>())
            {
                decimal totalAmountApplied = 0;
                foreach (PaymentApplication paymentApplication in paymentExtension.PaymentApplications)
                {
                    totalAmountApplied += paymentApplication.AmountApplied;
                }

                if (paymentExtension.ExistAmount && totalAmountApplied > paymentExtension.Amount)
                {
                    cycle.Validation.AddError($"{paymentExtension} {M.Payment.Amount} {ErrorMessages.PaymentAmountIsToSmall}");
                }
            }
        }
    }
}
