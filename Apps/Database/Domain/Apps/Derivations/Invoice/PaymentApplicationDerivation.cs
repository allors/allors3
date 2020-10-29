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
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.PaymentApplication.Class),
                new ChangedPattern(this.M.SalesInvoice.AdvancePayment) { Steps =  new IPropertyType[] {m.SalesInvoice.PaymentApplicationsWhereInvoice} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PaymentApplication>())
            {
                validation.AssertExistsAtMostOne(@this, this.M.PaymentApplication.Invoice, this.M.PaymentApplication.InvoiceItem, this.M.PaymentApplication.BillingAccount);
                validation.AssertAtLeastOne(@this, this.M.PaymentApplication.Invoice, this.M.PaymentApplication.InvoiceItem, this.M.PaymentApplication.BillingAccount);

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
