// <copyright file="PurchaseInvoiceItemStateDerivation.cs" company="Allors bvba">
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

    public class PurchaseInvoiceItemStateDerivation : DomainDerivation
    {
        public PurchaseInvoiceItemStateDerivation(M m) : base(m, new Guid("17686122-4e0d-4a4f-ad5c-6b3c77b969c4")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.PurchaseInvoiceItem.AmountPaid),
                new ChangedPattern(m.PurchaseInvoiceItem.TotalIncVat),
                new ChangedPattern(m.PurchaseInvoice.PurchaseInvoiceState) { Steps =  new IPropertyType[] {m.PurchaseInvoice.PurchaseInvoiceItems} },
                new ChangedPattern(m.PurchaseInvoice.AmountPaid) { Steps =  new IPropertyType[] {m.PurchaseInvoice.PurchaseInvoiceItems} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoiceItem>())
            {
                var purchaseInvoiceItemStates = new PurchaseInvoiceItemStates(@this.Strategy.Session);

                if (@this.ExistPurchaseInvoiceWherePurchaseInvoiceItem
                    && @this.PurchaseInvoiceWherePurchaseInvoiceItem.ExistPurchaseInvoiceState
                    && @this.IsValid)
                {
                    var nextState = @this.PurchaseInvoiceItemState;

                    if (@this.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceState.IsCreated)
                    {
                        nextState = purchaseInvoiceItemStates.Created;
                    }
                    else if (@this.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceState.IsRevising)
                    {
                        nextState = purchaseInvoiceItemStates.Revising;
                    }
                    else if (@this.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceState.IsAwaitingApproval)
                    {
                        nextState = purchaseInvoiceItemStates.AwaitingApproval;
                    }
                    else if (@this.AmountPaid == 0)
                    {
                        nextState = purchaseInvoiceItemStates.NotPaid;
                    }
                    else if (@this.ExistAmountPaid && @this.AmountPaid >= @this.TotalIncVat)
                    {
                        nextState = purchaseInvoiceItemStates.Paid;
                    }
                    else
                    {
                        nextState = purchaseInvoiceItemStates.PartiallyPaid;
                    }

                    // If disbursements are not matched at invoice item level
                    var invoiceAmountPaid = @this.PurchaseInvoiceWherePurchaseInvoiceItem.AmountPaid;

                    if (!@this.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceState.IsRevising
                        && @this.AmountPaid == 0
                        && invoiceAmountPaid != 0)
                    {
                        if (@this.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceState.IsPaid)
                        {
                            nextState = purchaseInvoiceItemStates.Paid;
                        }
                        else
                        {
                            nextState = purchaseInvoiceItemStates.PartiallyPaid;
                        }
                    }

                    if (@this.PurchaseInvoiceItemState != nextState)
                    {
                        @this.PurchaseInvoiceItemState = nextState;
                    }
                }
            }
        }
    }
}
