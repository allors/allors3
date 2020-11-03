// <copyright file="SalesInvoiceItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
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

        public decimal PriceAdjustmentAsPercentage => Math.Round((this.TotalSurcharge - this.TotalDiscount) / this.TotalBasePrice * 100, 2);

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
            this.SalesInvoiceItemState.Equals(new SalesInvoiceItemStates(this.Strategy.Session).ReadyForPosting);

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

        public void AppsWriteOff()
        {
            this.SalesInvoiceItemState = new SalesInvoiceItemStates(this.Strategy.Session).WrittenOff;
            this.DerivationTrigger = Guid.NewGuid();
        }

        public void CancelFromInvoice()
        {
            this.SalesInvoiceItemState = new SalesInvoiceItemStates(this.Strategy.Session).CancelledByInvoice;
            this.DerivationTrigger = Guid.NewGuid();
        }

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

        public void AppsIsSubTotalItem(SalesInvoiceItemIsSubTotalItem method)
        {
            if (!method.Result.HasValue)
            {
                method.Result = this.InvoiceItemType.Equals(new InvoiceItemTypes(this.Strategy.Session).ProductItem)
                    || this.InvoiceItemType.Equals(new InvoiceItemTypes(this.Strategy.Session).PartItem);
            }
        }

        public void Sync(Invoice invoice) => this.SyncedInvoice = invoice;
    }
}
