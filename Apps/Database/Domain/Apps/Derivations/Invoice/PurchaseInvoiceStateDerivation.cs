// <copyright file="PurchaseInvoiceStateDerivation.cs" company="Allors bvba">
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

    public class PurchaseInvoiceStateDerivation : DomainDerivation
    {
        public PurchaseInvoiceStateDerivation(M m) : base(m, new Guid("efdca6b3-b895-4a60-90a2-32f54120126b")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.PurchaseInvoice.BilledFrom),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>())
            {
                var purchaseInvoiceStates = new PurchaseInvoiceStates(@this.Strategy.Session);
                var purchaseInvoiceItemStates = new PurchaseInvoiceItemStates(@this.Strategy.Session);

                foreach (PurchaseInvoiceItem invoiceItem in @this.ValidInvoiceItems)
                {
                    if (invoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceState.IsCreated)
                    {
                        invoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItemStates.Created;
                    }
                    else if (invoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceState.IsAwaitingApproval)
                    {
                        invoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItemStates.AwaitingApproval;
                    }
                    else if (invoiceItem.AmountPaid == 0)
                    {
                        invoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItemStates.NotPaid;
                    }
                    else if (invoiceItem.ExistAmountPaid && invoiceItem.AmountPaid >= invoiceItem.TotalIncVat)
                    {
                        invoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItemStates.Paid;
                    }
                    else
                    {
                        invoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItemStates.PartiallyPaid;
                    }
                }

                if (@this.ValidInvoiceItems.Any())
                {
                    if (!@this.PurchaseInvoiceState.IsCreated && !@this.PurchaseInvoiceState.IsAwaitingApproval)
                    {
                        if (@this.PurchaseInvoiceItems.All(v => v.PurchaseInvoiceItemState.IsPaid))
                        {
                            @this.PurchaseInvoiceState = purchaseInvoiceStates.Paid;
                        }
                        else if (@this.PurchaseInvoiceItems.All(v => v.PurchaseInvoiceItemState.IsNotPaid))
                        {
                            @this.PurchaseInvoiceState = purchaseInvoiceStates.NotPaid;
                        }
                        else
                        {
                            @this.PurchaseInvoiceState = purchaseInvoiceStates.PartiallyPaid;
                        }
                    }
                }

                // If disbursements are not matched at invoice level
                if (!@this.PurchaseInvoiceState.IsRevising
                    && @this.AmountPaid != 0)
                {
                    if (@this.AmountPaid >= decimal.Round(@this.TotalIncVat, 2))
                    {
                        @this.PurchaseInvoiceState = purchaseInvoiceStates.Paid;
                    }
                    else
                    {
                        @this.PurchaseInvoiceState = purchaseInvoiceStates.PartiallyPaid;
                    }

                    foreach (PurchaseInvoiceItem invoiceItem in @this.ValidInvoiceItems)
                    {
                        if (@this.AmountPaid >= decimal.Round(@this.TotalIncVat, 2))
                        {
                            invoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItemStates.Paid;
                        }
                        else
                        {
                            invoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItemStates.PartiallyPaid;
                        }
                    }
                }
            }
        }
    }
}
