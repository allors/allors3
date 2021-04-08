// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class PaymentApplicationRule : Rule
    {
        public PaymentApplicationRule(MetaPopulation m) : base(m, new Guid("eec38edc-ea02-4bf2-8b1e-32511a019987")) =>
            this.Patterns = new Pattern[]
            {
                m.PaymentApplication.RolePattern(v => v.AmountApplied),
                m.SalesInvoice.RolePattern(v => v.AdvancePayment, v => v.PaymentApplicationsWhereInvoice),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PaymentApplication>())
            {
                if (@this.ExistPaymentWherePaymentApplication && @this.AmountApplied > @this.PaymentWherePaymentApplication.Amount)
                {
                    validation.AddError($"{@this} {this.M.PaymentApplication.AmountApplied} {ErrorMessages.PaymentApplicationNotLargerThanPaymentAmount}");
                }

                var totalInvoiceAmountPaid = @this.Invoice?.PaymentApplicationsWhereInvoice.Sum(v => v.AmountApplied);
                if (@this.Invoice is SalesInvoice salesInvoice)
                {
                    totalInvoiceAmountPaid += salesInvoice.AdvancePayment;
                }

                if (@this.ExistInvoice && totalInvoiceAmountPaid > @this.Invoice.TotalIncVat)
                {
                    validation.AddError($"{@this} {this.M.PaymentApplication.AmountApplied} {ErrorMessages.PaymentApplicationNotLargerThanInvoiceAmount}");
                }
            }
        }
    }
}
