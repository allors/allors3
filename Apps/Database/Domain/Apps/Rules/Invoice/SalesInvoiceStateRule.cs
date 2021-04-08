// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class SalesInvoiceStateRule : Rule
    {
        public SalesInvoiceStateRule(MetaPopulation m) : base(m, new Guid("c273de35-fd1c-4353-8354-d3640ba5dff8")) =>
            this.Patterns = new Pattern[]
        {
            m.PaymentApplication.RolePattern(v => v.AmountApplied, v => v.Invoice, m.SalesInvoice),
            m.SalesInvoice.RolePattern(v => v.SalesInvoiceItems),
            m.SalesInvoice.RolePattern(v => v.AdvancePayment),
            m.SalesInvoice.RolePattern(v => v.DerivationTrigger),
            m.SalesInvoiceItem.RolePattern(v => v.SalesInvoiceItemState, v => v.SalesInvoiceWhereSalesInvoiceItem),
            m.SalesInvoiceItem.RolePattern(v => v.GrandTotal, v => v.SalesInvoiceWhereSalesInvoiceItem),
            m.SalesInvoiceItem.RolePattern(v => v.DerivationTrigger, v => v.SalesInvoiceWhereSalesInvoiceItem),
            m.SalesInvoiceItem.RolePattern(v => v.AmountPaid, v => v.SalesInvoiceWhereSalesInvoiceItem),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                var salesInvoiceItemStates = new SalesInvoiceItemStates(transaction);
                var salesInvoiceStates = new SalesInvoiceStates(transaction);

                if (!@this.ExistSalesInvoiceState)
                {
                    @this.SalesInvoiceState = new SalesInvoiceStates(transaction).ReadyForPosting;
                }

                var validInvoiceItems = @this.SalesInvoiceItems.Where(v => v.IsValid).ToArray();
                @this.ValidInvoiceItems = validInvoiceItems;

                var amountPaid = @this.AdvancePayment;
                amountPaid += @this.PaymentApplicationsWhereInvoice.Sum(v => v.AmountApplied);

                //// Perhaps payments are recorded at the item level.
                if (amountPaid == 0)
                {
                    amountPaid = @this.InvoiceItems.Sum(v => v.AmountPaid);
                }

                if (@this.AmountPaid != amountPaid)
                {
                    @this.AmountPaid = amountPaid;
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
                            if (@this.AmountPaid - @this.AdvancePayment > 0)
                            {
                                if (@this.AmountPaid >= @this.GrandTotal) // GrandTotal is immutable
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
                                    && invoiceItem.AmountPaid >= invoiceItem.GrandTotal)  // GrandTotal is immutable
                        {
                            invoiceItem.SalesInvoiceItemState = salesInvoiceItemStates.Paid;
                        }
                        else
                        {
                            invoiceItem.SalesInvoiceItemState = salesInvoiceItemStates.PartiallyPaid;
                        }
                    }
                }

                if (validInvoiceItems.Any() && !@this.SalesInvoiceState.Equals(salesInvoiceStates.ReadyForPosting))
                {
                    if (validInvoiceItems.All(v => v.SalesInvoiceItemState.IsPaid))
                    {
                        @this.SalesInvoiceState = salesInvoiceStates.Paid;
                    }
                    else if (validInvoiceItems.All(v => v.SalesInvoiceItemState.IsNotPaid))
                    {
                        @this.SalesInvoiceState = salesInvoiceStates.NotPaid;
                    }
                    else
                    {
                        @this.SalesInvoiceState = salesInvoiceStates.PartiallyPaid;
                    }
                }
            }
        }
    }
}
