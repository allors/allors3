// <copyright file="SalesOrderItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class SalesOrderItem
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.SalesOrderItem, this.M.SalesOrderItem.SalesOrderItemState),
            new TransitionalConfiguration(this.M.SalesOrderItem, this.M.SalesOrderItem.SalesOrderItemShipmentState),
            new TransitionalConfiguration(this.M.SalesOrderItem, this.M.SalesOrderItem.SalesOrderItemInvoiceState),
            new TransitionalConfiguration(this.M.SalesOrderItem, this.M.SalesOrderItem.SalesOrderItemPaymentState),
        };

        public bool IsValid => !(this.SalesOrderItemState.IsCancelled || this.SalesOrderItemState.IsRejected);

        public bool WasValid => this.ExistLastObjectStates && !(this.LastSalesOrderItemState.IsCancelled || this.LastSalesOrderItemState.IsRejected);

        internal bool IsDeletable =>
            (this.SalesOrderItemState.Equals(new SalesOrderItemStates(this.Strategy.Transaction).Provisional)
                || this.SalesOrderItemState.Equals(new SalesOrderItemStates(this.Strategy.Transaction).ReadyForPosting)
                || this.SalesOrderItemState.Equals(new SalesOrderItemStates(this.Strategy.Transaction).Cancelled)
                || this.SalesOrderItemState.Equals(new SalesOrderItemStates(this.Strategy.Transaction).Rejected))
            && !this.ExistOrderItemBillingsWhereOrderItem
            && !this.ExistOrderShipmentsWhereOrderItem
            && !this.ExistOrderRequirementCommitmentsWhereOrderItem
            && !this.ExistWorkEffortsWhereOrderItemFulfillment;

        public Part Part
        {
            get
            {
                if (this.ExistProduct)
                {
                    var nonUnifiedGood = this.Product as NonUnifiedGood;
                    var unifiedGood = this.Product as UnifiedGood;
                    return unifiedGood ?? nonUnifiedGood?.Part;
                }

                return null;
            }
        }

        public void AppsDelegateAccess(DelegatedAccessControlledObjectDelegateAccess method)
        {
            if (method.SecurityTokens == null)
            {
                method.SecurityTokens = this.SyncedOrder?.SecurityTokens.ToArray();
            }

            if (method.DeniedPermissions == null)
            {
                method.DeniedPermissions = this.SyncedOrder?.DeniedPermissions.ToArray();
            }
        }

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistSalesOrderItemState)
            {
                this.SalesOrderItemState = new SalesOrderItemStates(this.Strategy.Transaction).Provisional;
            }

            if (this.ExistProduct && !this.ExistInvoiceItemType)
            {
                this.InvoiceItemType = new InvoiceItemTypes(this.Strategy.Transaction).ProductItem;
            }

            if (!this.ExistSalesOrderItemShipmentState)
            {
                this.SalesOrderItemShipmentState = new SalesOrderItemShipmentStates(this.Strategy.Transaction).NotShipped;
            }

            if (!this.ExistSalesOrderItemInvoiceState)
            {
                this.SalesOrderItemInvoiceState = new SalesOrderItemInvoiceStates(this.Strategy.Transaction).NotInvoiced;
            }

            if (!this.ExistSalesOrderItemPaymentState)
            {
                this.SalesOrderItemPaymentState = new SalesOrderItemPaymentStates(this.Strategy.Transaction).NotPaid;
            }
        }

        public void AppsOnInit(ObjectOnInit method)
        {
            if (this.ExistProduct && !this.ExistInvoiceItemType)
            {
                this.InvoiceItemType = new InvoiceItemTypes(this.Strategy.Transaction).ProductItem;
            }
        }

        public void AppsDelete(SalesOrderItemDelete method)
        {
            foreach (SalesTerm salesTerm in this.SalesTerms)
            {
                salesTerm.Delete();
            }

            if (this.ExistSerialisedItem)
            {
                this.SerialisedItem.DerivationTrigger = Guid.NewGuid();
            }
        }

        public void AppsCancel(OrderItemCancel method) => this.SalesOrderItemState = new SalesOrderItemStates(this.Strategy.Transaction).Cancelled;

        public void AppsReject(OrderItemReject method) => this.SalesOrderItemState = new SalesOrderItemStates(this.Strategy.Transaction).Rejected;

        public void AppsApprove(OrderItemApprove method) => this.SalesOrderItemState = new SalesOrderItemStates(this.Strategy.Transaction).ReadyForPosting;

        public void AppsReopen(OrderItemReopen method) => this.SalesOrderItemState = this.PreviousSalesOrderItemState;

        public void Sync(Order order) => this.SyncedOrder = order;
    }
}
