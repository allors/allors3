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
    using Resources;

    public class PaymentApplicationDerivation : DomainDerivation
    {
        public PaymentApplicationDerivation(M m) : base(m, new Guid("D3D3B1B9-4619-4720-8E73-04419896B3AE")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(m.PaymentApplication.Class)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var paymentApplication in matches.Cast<PaymentApplication>())
            {
                validation.AssertExistsAtMostOne(paymentApplication, this.M.PaymentApplication.Invoice, this.M.PaymentApplication.InvoiceItem, this.M.PaymentApplication.BillingAccount);
                validation.AssertAtLeastOne(paymentApplication, this.M.PaymentApplication.Invoice, this.M.PaymentApplication.InvoiceItem, this.M.PaymentApplication.BillingAccount);

                if (paymentApplication.ExistPaymentWherePaymentApplication && paymentApplication.AmountApplied > paymentApplication.PaymentWherePaymentApplication.Amount)
                {
                    validation.AddError($"{paymentApplication} {this.M.PaymentApplication.AmountApplied} {ErrorMessages.PaymentApplicationNotLargerThanPaymentAmount}");
                }

                var totalInvoiceAmountPaid = paymentApplication.Invoice?.PaymentApplicationsWhereInvoice.Sum(v => v.AmountApplied);
                if (paymentApplication.Invoice is SalesInvoice salesInvoice)
                {
                    totalInvoiceAmountPaid += salesInvoice.AdvancePayment;
                }

                if (paymentApplication.ExistInvoice && totalInvoiceAmountPaid > paymentApplication.Invoice.TotalIncVat)
                {
                    validation.AddError($"{paymentApplication} {this.M.PaymentApplication.AmountApplied} {ErrorMessages.PaymentApplicationNotLargerThanInvoiceAmount}");
                }
            }
        }
    }
}
