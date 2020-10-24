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

    public class SalesInvoiceStateDerivation : DomainDerivation
    {
        public SalesInvoiceStateDerivation(M m) : base(m, new Guid("c273de35-fd1c-4353-8354-d3640ba5dff8")) =>
            this.Patterns = new Pattern[]
        {
            // Do not listen to changes in SalesInvoiceItem.TotalIncVat
            // This is used here when salesInvoiceItemState has passed the ReadyForPosting state and by then invoice item changes are not allowed.

            new CreatedPattern(this.M.SalesInvoice.Class),
            new ChangedRolePattern(this.M.SalesInvoice.SalesInvoiceItems),
            new ChangedRolePattern(this.M.SalesInvoice.AdvancePayment),
            new ChangedRolePattern(this.M.SalesInvoiceItem.DerivationTrigger) { Steps =  new IPropertyType[] {m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem} },
            new ChangedRolePattern(this.M.SalesInvoiceItem.AmountPaid) { Steps =  new IPropertyType[] {m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem} },
            new ChangedRolePattern(this.M.PaymentApplication.AmountApplied) { Steps =  new IPropertyType[] {m.PaymentApplication.Invoice} },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var salesInvoice in matches.Cast<SalesInvoice>())
            {
                var salesInvoiceItemStates = new SalesInvoiceItemStates(session);
                var salesInvoiceStates = new SalesInvoiceStates(session);

                if (!salesInvoice.ExistSalesInvoiceState)
                {
                    salesInvoice.SalesInvoiceState = new SalesInvoiceStates(session).ReadyForPosting;
                }

                var validInvoiceItems = salesInvoice.SalesInvoiceItems.Where(v => v.IsValid).ToArray();
                salesInvoice.ValidInvoiceItems = validInvoiceItems;

                var amountPaid = salesInvoice.AdvancePayment;
                amountPaid += salesInvoice.PaymentApplicationsWhereInvoice.Sum(v => v.AmountApplied);

                //// Perhaps payments are recorded at the item level.
                if (amountPaid == 0)
                {
                    amountPaid = salesInvoice.InvoiceItems.Sum(v => v.AmountPaid);
                }

                if (salesInvoice.AmountPaid != amountPaid)
                {
                    salesInvoice.AmountPaid = amountPaid;
                }

                foreach (var invoiceItem in validInvoiceItems)
                {
                    if (!invoiceItem.SalesInvoiceItemState.Equals(salesInvoiceItemStates.ReadyForPosting))
                    {
                        if (invoiceItem.AmountPaid == 0)
                        {
                            // If receipts are not matched at invoice level
                            // if only advancedPayment is received do not set to partially paid
                            // this would disable the invoice for editing and adding new items
                            if (salesInvoice.AmountPaid - salesInvoice.AdvancePayment > 0)
                            {
                                if (salesInvoice.AmountPaid >= salesInvoice.TotalIncVat) // TotalIncVat is immutable
                                {
                                    invoiceItem.SalesInvoiceItemState = salesInvoiceItemStates.Paid;
                                }
                                else
                                {
                                    invoiceItem.SalesInvoiceItemState = salesInvoiceItemStates.PartiallyPaid;
                                }
                            }
                            else
                            {
                                invoiceItem.SalesInvoiceItemState = salesInvoiceItemStates.NotPaid;
                            }
                        }
                        else if (invoiceItem.ExistAmountPaid
                                    && invoiceItem.AmountPaid > 0
                                    && invoiceItem.AmountPaid >= invoiceItem.TotalIncVat)  // TotalIncVat is immutable
                        {
                            invoiceItem.SalesInvoiceItemState = salesInvoiceItemStates.Paid;
                        }
                        else
                        {
                            invoiceItem.SalesInvoiceItemState = salesInvoiceItemStates.PartiallyPaid;
                        }
                    }
                }

                if (validInvoiceItems.Any() && !salesInvoice.SalesInvoiceState.Equals(salesInvoiceStates.ReadyForPosting))
                {
                    if (validInvoiceItems.All(v => v.SalesInvoiceItemState.IsPaid))
                    {
                        salesInvoice.SalesInvoiceState = salesInvoiceStates.Paid;
                    }
                    else if (validInvoiceItems.All(v => v.SalesInvoiceItemState.IsNotPaid))
                    {
                        salesInvoice.SalesInvoiceState = salesInvoiceStates.NotPaid;
                    }
                    else
                    {
                        salesInvoice.SalesInvoiceState = salesInvoiceStates.PartiallyPaid;
                    }
                }
            }
        }
    }
}
