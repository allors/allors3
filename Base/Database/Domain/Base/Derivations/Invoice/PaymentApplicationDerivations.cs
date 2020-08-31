// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class PaymentApplicationCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdPaymentApplications = changeSet.Created.Select(v=>v.GetObject()).OfType<PaymentApplication>();

                foreach(var paymentApplication in createdPaymentApplications)
                {
                    validation.AssertExistsAtMostOne(paymentApplication, M.PaymentApplication.Invoice, M.PaymentApplication.InvoiceItem);

                    if (paymentApplication.ExistPaymentWherePaymentApplication && paymentApplication.AmountApplied > paymentApplication.PaymentWherePaymentApplication.Amount)
                    {
                        validation.AddError($"{paymentApplication} {M.PaymentApplication.AmountApplied} {ErrorMessages.PaymentApplicationNotLargerThanPaymentAmount}");
                    }

                    var totalInvoiceAmountPaid = paymentApplication.Invoice?.PaymentApplicationsWhereInvoice.Sum(v => v.AmountApplied);
                    if (paymentApplication.Invoice is SalesInvoice salesInvoice)
                    {
                        totalInvoiceAmountPaid += salesInvoice.AdvancePayment;
                    }

                    if (paymentApplication.ExistInvoice && totalInvoiceAmountPaid > paymentApplication.Invoice.TotalIncVat)
                    {
                        validation.AddError($"{paymentApplication} {M.PaymentApplication.AmountApplied} {ErrorMessages.PaymentApplicationNotLargerThanInvoiceAmount}");
                    }

                    var totalInvoiceItemAmountPaid = paymentApplication.InvoiceItem?.PaymentApplicationsWhereInvoiceItem.Sum(v => v.AmountApplied);
                    if (paymentApplication.ExistInvoiceItem && totalInvoiceItemAmountPaid > paymentApplication.InvoiceItem.TotalIncVat)
                    {
                        validation.AddError($"{paymentApplication} {M.PaymentApplication.AmountApplied} {ErrorMessages.PaymentApplicationNotLargerThanInvoiceItemAmount}");
                    }
                }
            }
        }

        public static void PaymentApplicationRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("0262c2e8-d4e5-4d0b-8524-7c6929c71eba")] = new PaymentApplicationCreationDerivation();
        }
    }
}
