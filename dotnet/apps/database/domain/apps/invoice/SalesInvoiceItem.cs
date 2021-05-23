// <copyright file="SalesInvoiceItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class SalesInvoiceItem
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.SalesInvoiceItem, this.M.SalesInvoiceItem.SalesInvoiceItemState),
        };

        public bool IsValid => !(this.SalesInvoiceItemState.IsCancelledByInvoice || this.SalesInvoiceItemState.IsWrittenOff);

        public decimal PriceAdjustment => this.TotalSurcharge - this.TotalDiscount;

        public decimal PriceAdjustmentAsPercentage => Rounder.RoundDecimal((this.TotalSurcharge - this.TotalDiscount) / this.TotalBasePrice * 100, 2);

        public Part DerivedPart
        {
            get
            {
                if (this.ExistPart)
                {
                    return this.Part;
                }
                else
                {
                    if (this.ExistProduct)
                    {
                        var nonUnifiedGood = this.Product as NonUnifiedGood;
                        var unifiedGood = this.Product as UnifiedGood;
                        return unifiedGood ?? nonUnifiedGood?.Part;
                    }
                }

                return null;
            }
        }

        internal bool IsDeletable =>
            this.SalesInvoiceItemState.Equals(new SalesInvoiceItemStates(this.Strategy.Transaction).ReadyForPosting);

        public void AppsDelegateAccess(DelegatedAccessControlledObjectDelegateAccess method)
        {
            if (method.SecurityTokens == null)
            {
                method.SecurityTokens = this.SyncedInvoice?.SecurityTokens.ToArray();
            }

            if (method.DeniedPermissions == null)
            {
                method.DeniedPermissions = this.SyncedInvoice?.DeniedPermissions.ToArray();
            }
        }

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistSalesInvoiceItemState)
            {
                this.SalesInvoiceItemState = new SalesInvoiceItemStates(this.Strategy.Transaction).ReadyForPosting;
            }

            if (this.ExistProduct && !this.ExistInvoiceItemType)
            {
                this.InvoiceItemType = new InvoiceItemTypes(this.Strategy.Transaction).ProductItem;
            }
        }

        public void AppsOnInit(ObjectOnInit method)
        {
            if (this.ExistProduct && !this.ExistInvoiceItemType)
            {
                this.InvoiceItemType = new InvoiceItemTypes(this.Strategy.Transaction).ProductItem;
            }
        }

        public void AppsWriteOff() => this.SalesInvoiceItemState = new SalesInvoiceItemStates(this.Strategy.Transaction).WrittenOff;

        public void CancelFromInvoice() => this.SalesInvoiceItemState = new SalesInvoiceItemStates(this.Strategy.Transaction).CancelledByInvoice;

        public void AppsDelete(DeletableDelete method)
        {
            foreach (SalesTerm salesTerm in this.SalesTerms)
            {
                salesTerm.Delete();
            }

            foreach (InvoiceVatRateItem invoiceVatRateItem in this.InvoiceVatRateItems)
            {
                invoiceVatRateItem.Delete();
            }

            foreach (WorkEffortBilling billing in this.WorkEffortBillingsWhereInvoiceItem)
            {
                billing.WorkEffort.DerivationTrigger = Guid.NewGuid();
                billing.Delete();
            }

            foreach (OrderItemBilling billing in this.OrderItemBillingsWhereInvoiceItem)
            {
                billing.OrderItem.DerivationTrigger = Guid.NewGuid();
                billing.Delete();
            }

            foreach (ShipmentItemBilling billing in this.ShipmentItemBillingsWhereInvoiceItem)
            {
                billing.ShipmentItem.DerivationTrigger = Guid.NewGuid();
                billing.Delete();
            }

            foreach (TimeEntryBilling billing in this.TimeEntryBillingsWhereInvoiceItem)
            {
                billing.TimeEntry.WorkEffort.DerivationTrigger = Guid.NewGuid();
                billing.Delete();
            }

            foreach (ServiceEntryBilling billing in this.ServiceEntryBillingsWhereInvoiceItem)
            {
                billing.ServiceEntry.DerivationTrigger = Guid.NewGuid();
                billing.Delete();
            }
        }

        public void Sync(Invoice invoice) => this.SyncedInvoice = invoice;
    }
}
