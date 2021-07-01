// <copyright file="PurchaseOrderItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;

    public partial class PurchaseOrderItem
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.PurchaseOrderItem, this.M.PurchaseOrderItem.PurchaseOrderItemState),
            new TransitionalConfiguration(this.M.PurchaseOrderItem, this.M.PurchaseOrderItem.PurchaseOrderItemShipmentState),
            new TransitionalConfiguration(this.M.PurchaseOrderItem, this.M.PurchaseOrderItem.PurchaseOrderItemPaymentState),
        };

        public bool IsValid => !(this.PurchaseOrderItemState.IsCancelled || this.PurchaseOrderItemState.IsRejected);

        internal bool IsDeletable =>
            (this.PurchaseOrderItemState.Equals(new PurchaseOrderItemStates(this.Strategy.Transaction).Created)
                || this.PurchaseOrderItemState.Equals(new PurchaseOrderItemStates(this.Strategy.Transaction).Cancelled)
                || this.PurchaseOrderItemState.Equals(new PurchaseOrderItemStates(this.Strategy.Transaction).Rejected))
            && !this.ExistOrderItemBillingsWhereOrderItem
            && !this.ExistOrderShipmentsWhereOrderItem
            && !this.ExistOrderRequirementCommitmentsWhereOrderItem
            && !this.ExistWorkEffortsWhereOrderItemFulfillment;

        //public string SupplierReference
        //{
        //    get
        //    {
        //        Extent<SupplierOffering> offerings = null;

        //        if (this.ExistPart)
        //        {
        //            offerings = this.Part.SupplierOfferingsWherePart;
        //        }

        //        if (offerings != null)
        //        {
        //            offerings.Filter.AddEquals(this.M.SupplierOffering.Supplier, this.PurchaseOrderWherePurchaseOrderItem.TakenViaSupplier);
        //            foreach (SupplierOffering offering in offerings)
        //            {
        //                if (offering.FromDate <= this.PurchaseOrderWherePurchaseOrderItem.OrderDate &&
        //                    (!offering.ExistThroughDate || offering.ThroughDate >= this.PurchaseOrderWherePurchaseOrderItem.OrderDate))
        //                {
        //                    return offering.SupplierProductId;
        //                }
        //            }
        //        }

        //        return string.Empty;
        //    }
        //}

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
            if (!this.ExistPurchaseOrderItemState)
            {
                this.PurchaseOrderItemState = new PurchaseOrderItemStates(this.Strategy.Transaction).Created;
            }

            if (!this.ExistPurchaseOrderItemPaymentState)
            {
                this.PurchaseOrderItemPaymentState = new PurchaseOrderItemPaymentStates(this.Strategy.Transaction).NotPaid;
            }

            if (this.ExistPart && !this.ExistInvoiceItemType)
            {
                this.InvoiceItemType = new InvoiceItemTypes(this.Strategy.Transaction).PartItem;
            }

            this.DerivationTrigger = Guid.NewGuid();
        }

        public void BaseOnInit(ObjectOnInit method)
        {
            if (!this.ExistStoredInFacility
                && this.ExistInvoiceItemType
                && (this.InvoiceItemType.IsPartItem || this.InvoiceItemType.IsProductItem))
            {
                this.StoredInFacility = this.PurchaseOrderWherePurchaseOrderItem?.StoredInFacility;

                if (!this.ExistStoredInFacility && this.PurchaseOrderWherePurchaseOrderItem?.OrderedBy?.StoresWhereInternalOrganisation.Count() == 1)
                {
                    this.StoredInFacility = this.PurchaseOrderWherePurchaseOrderItem.OrderedBy.StoresWhereInternalOrganisation.Single().DefaultFacility;
                }
            }
        }

        public void AppsDelete(PurchaseOrderItemDelete method)
        {
            if (this.ExistSerialisedItem)
            {
                this.SerialisedItem.DerivationTrigger = Guid.NewGuid();
            }
        }

        public void AppsApprove(OrderItemApprove method)
        {
            this.PurchaseOrderItemState = new PurchaseOrderItemStates(this.Strategy.Transaction).InProcess;
            method.StopPropagation = true;
        }

        public void AppsQuickReceive(PurchaseOrderItemQuickReceive method)
        {
            var transaction = this.Transaction();
            var order = this.PurchaseOrderWherePurchaseOrderItem;

            if (this.ExistPart)
            {
                var shipment = new PurchaseShipmentBuilder(transaction)
                    .WithShipmentMethod(new ShipmentMethods(transaction).Ground)
                    .WithShipToParty(order.OrderedBy)
                    .WithShipFromParty(order.TakenViaSupplier)
                    .WithShipToFacility(order.StoredInFacility)
                    .Build();

                var shipmentItem = new ShipmentItemBuilder(transaction)
                    .WithPart(this.Part)
                    .WithStoredInFacility(this.StoredInFacility)
                    .WithQuantity(this.QuantityOrdered)
                    .WithUnitPurchasePrice(this.UnitPrice)
                    .WithContentsDescription($"{this.QuantityOrdered} * {this.Part.Name}")
                    .Build();

                shipment.AddShipmentItem(shipmentItem);

                new OrderShipmentBuilder(transaction)
                    .WithOrderItem(this)
                    .WithShipmentItem(shipmentItem)
                    .WithQuantity(this.QuantityOrdered)
                    .Build();

                if (this.Part.InventoryItemKind.IsSerialised)
                {
                    var serialisedItem = this.SerialisedItem;
                    if (!this.ExistSerialisedItem)
                    {
                        serialisedItem = new SerialisedItemBuilder(transaction)
                            .WithSerialNumber(this.SerialNumber)
                            .Build();

                        this.Part.AddSerialisedItem(serialisedItem);
                    }

                    shipmentItem.SerialisedItem = serialisedItem;
                }

                if (shipment.ShipToParty is InternalOrganisation internalOrganisation)
                {
                    if (internalOrganisation.IsAutomaticallyReceived)
                    {
                        shipment.Receive();
                    }
                }
            }
            else
            {
                this.QuantityReceived = 1;
            }

            method.StopPropagation = true;
        }

        public void AppsCancel(OrderItemCancel method)
        {
            this.PurchaseOrderItemState = new PurchaseOrderItemStates(this.Strategy.Transaction).Cancelled;
            method.StopPropagation = true;
        }

        public void AppsReject(OrderItemReject method)
        {
            this.PurchaseOrderItemState = new PurchaseOrderItemStates(this.Strategy.Transaction).Rejected;
            method.StopPropagation = true;
        }

        public void AppsReopen(OrderItemReopen method)
        {
            this.PurchaseOrderItemState = this.PreviousPurchaseOrderItemState;
            method.StopPropagation = true;
        }

        public void Sync(Order order) => this.SyncedOrder = order;
    }
}
