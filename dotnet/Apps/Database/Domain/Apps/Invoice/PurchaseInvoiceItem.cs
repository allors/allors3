// <copyright file="PurchaseInvoiceItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class PurchaseInvoiceItem
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.PurchaseInvoiceItem, this.M.PurchaseInvoiceItem.PurchaseInvoiceItemState),
        };

        public bool IsValid => !(this.PurchaseInvoiceItemState.IsCancelledByInvoice || this.PurchaseInvoiceItemState.IsRejected);

        public decimal PriceAdjustment => this.TotalSurcharge - this.TotalDiscount;

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistPurchaseInvoiceItemState)
            {
                this.PurchaseInvoiceItemState = new PurchaseInvoiceItemStates(this.Strategy.Transaction).Created;
            }

            if (this.ExistPart && !this.ExistInvoiceItemType)
            {
                this.InvoiceItemType = new InvoiceItemTypes(this.Strategy.Transaction).PartItem;
            }

            this.DerivationTrigger = Guid.NewGuid();
        }

        public void CancelFromInvoice() => this.PurchaseInvoiceItemState = new PurchaseInvoiceItemStates(this.Strategy.Transaction).CancelledByinvoice;

        public void AppsDelete(DeletableDelete method)
        {
            foreach (var deletable in this.AllVersions)
            {
                deletable.Strategy.Delete();
            }

            if (this.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceState.IsCreated
                || this.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceState.IsRevising)
            {
                this.PurchaseInvoiceWherePurchaseInvoiceItem.RemovePurchaseInvoiceItem(this);
                foreach (var orderItemBilling in this.OrderItemBillingsWhereInvoiceItem)
                {
                    orderItemBilling.OrderItem.DerivationTrigger = Guid.NewGuid();
                    orderItemBilling.CascadingDelete();
                }
            }
        }

        public void AppsReject(PurchaseInvoiceItemReject method)
        {
            this.PurchaseInvoiceItemState = new PurchaseInvoiceItemStates(this.Strategy.Transaction).Rejected;
            method.StopPropagation = true;
        }

        public void AppsRevise(PurchaseInvoiceItemRevise method)
        {
            this.PurchaseInvoiceItemState = new PurchaseInvoiceItemStates(this.Strategy.Transaction).Revising;
            method.StopPropagation = true;
        }

        public void AppsFinishRevising(PurchaseInvoiceItemFinishRevising method)
        {
            this.PurchaseInvoiceItemState = new PurchaseInvoiceItemStates(this.Strategy.Transaction).Created;
            method.StopPropagation = true;
        }
    }
}
