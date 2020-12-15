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
            (this.SalesOrderItemState.Equals(new SalesOrderItemStates(this.Strategy.Session).Provisional)
                || this.SalesOrderItemState.Equals(new SalesOrderItemStates(this.Strategy.Session).ReadyForPosting)
                || this.SalesOrderItemState.Equals(new SalesOrderItemStates(this.Strategy.Session).Cancelled)
                || this.SalesOrderItemState.Equals(new SalesOrderItemStates(this.Strategy.Session).Rejected))
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
                this.SalesOrderItemState = new SalesOrderItemStates(this.Strategy.Session).Provisional;
            }

            if (this.ExistProduct && !this.ExistInvoiceItemType)
            {
                this.InvoiceItemType = new InvoiceItemTypes(this.Strategy.Session).ProductItem;
            }

            if (!this.ExistSalesOrderItemShipmentState)
            {
                this.SalesOrderItemShipmentState = new SalesOrderItemShipmentStates(this.Strategy.Session).NotShipped;
            }

            if (!this.ExistSalesOrderItemInvoiceState)
            {
                this.SalesOrderItemInvoiceState = new SalesOrderItemInvoiceStates(this.Strategy.Session).NotInvoiced;
            }

            if (!this.ExistSalesOrderItemPaymentState)
            {
                this.SalesOrderItemPaymentState = new SalesOrderItemPaymentStates(this.Strategy.Session).NotPaid;
            }

            this.DerivationTrigger = Guid.NewGuid();
        }

        public void AppsOnInit(ObjectOnInit method)
        {
            if (this.ExistProduct && !this.ExistInvoiceItemType)
            {
                this.InvoiceItemType = new InvoiceItemTypes(this.Strategy.Session).ProductItem;
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

        public void AppsCancel(OrderItemCancel method)
        {
            this.SalesOrderItemState = new SalesOrderItemStates(this.Strategy.Session).Cancelled;
            this.DerivationTrigger = Guid.NewGuid();
        }

        public void AppsReject(OrderItemReject method)
        {
            this.SalesOrderItemState = new SalesOrderItemStates(this.Strategy.Session).Rejected;
            this.DerivationTrigger = Guid.NewGuid();
        }

        public void AppsApprove(OrderItemApprove method) => this.SalesOrderItemState = new SalesOrderItemStates(this.Strategy.Session).ReadyForPosting;

        public void AppsReopen(OrderItemReopen method) => this.SalesOrderItemState = this.PreviousSalesOrderItemState;

        public void Sync(Order order) => this.SyncedOrder = order;
    }
}
