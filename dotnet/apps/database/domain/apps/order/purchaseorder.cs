// <copyright file="PurchaseOrder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class PurchaseOrder
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.PurchaseOrder, this.M.PurchaseOrder.PurchaseOrderState),
            new TransitionalConfiguration(this.M.PurchaseOrder, this.M.PurchaseOrder.PurchaseOrderShipmentState),
            new TransitionalConfiguration(this.M.PurchaseOrder, this.M.PurchaseOrder.PurchaseOrderPaymentState),
        };

        private bool NeedsApprovalLevel1
        {
            get
            {
                if (this.ExistTakenViaSupplier && this.ExistOrderedBy)
                {
                    var supplierRelationship = this.TakenViaSupplier.SupplierRelationshipsWhereSupplier.FirstOrDefault(v => v.InternalOrganisation.Equals(this.OrderedBy));
                    if (supplierRelationship != null
                        && supplierRelationship.NeedsApproval
                        && supplierRelationship.ApprovalThresholdLevel1.HasValue
                        && this.TotalExVat >= supplierRelationship.ApprovalThresholdLevel1.Value)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private bool NeedsApprovalLevel2
        {
            get
            {
                if (this.ExistTakenViaSupplier && this.ExistOrderedBy)
                {
                    var supplierRelationship = this.TakenViaSupplier.SupplierRelationshipsWhereSupplier.FirstOrDefault(v => v.InternalOrganisation.Equals(this.OrderedBy));
                    if (supplierRelationship != null
                        && supplierRelationship.NeedsApproval
                        && supplierRelationship.ApprovalThresholdLevel2.HasValue
                        && this.TotalExVat >= supplierRelationship.ApprovalThresholdLevel2.Value)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public bool CanInvoice
        {
            get
            {
                if (this.PurchaseOrderState.IsSent || this.PurchaseOrderState.IsCompleted)
                {
                    foreach (PurchaseOrderItem purchaseOrderItem in this.ValidOrderItems)
                    {
                        if (!purchaseOrderItem.ExistOrderItemBillingsWhereOrderItem)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public bool CanRevise
        {
            get
            {
                if ((this.PurchaseOrderState.IsInProcess || this.PurchaseOrderState.IsSent || this.PurchaseOrderState.IsCompleted)
                    && (this.PurchaseOrderShipmentState.IsNotReceived || this.PurchaseOrderShipmentState.IsNa)
                    && !this.ExistPurchaseInvoicesWherePurchaseOrder)
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsReceivable
        {
            get
            {
                if (this.PurchaseOrderState.IsSent
                    && this.ValidOrderItems.Any(v => ((PurchaseOrderItem)v).IsReceivable))
                {
                    return true;
                }

                return false;
            }
        }

        public bool IsDeletable =>
            (this.PurchaseOrderState.Equals(new PurchaseOrderStates(this.Strategy.Transaction).Created)
                || this.PurchaseOrderState.Equals(new PurchaseOrderStates(this.Strategy.Transaction).Cancelled)
                || this.PurchaseOrderState.Equals(new PurchaseOrderStates(this.Strategy.Transaction).Rejected))
            && !this.ExistPurchaseInvoicesWherePurchaseOrder
            && !this.ExistSerialisedItemsWherePurchaseOrder
            && !this.ExistWorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrder
            && this.PurchaseOrderItems.All(v => v.IsDeletable);

        public void AppsDelete(PurchaseOrderDelete method)
        {
            if (this.IsDeletable)
            {
                foreach (var orderAdjustment in this.OrderAdjustments)
                {
                    orderAdjustment.Delete();
                }

                foreach (var item in this.PurchaseOrderItems)
                {
                    item.Delete();
                }
            }
        }

        public void AppsOnInit(ObjectOnInit method)
        {
            this.PurchaseOrderState ??= new PurchaseOrderStates(this.Strategy.Transaction).Created;
            this.PurchaseOrderShipmentState ??= new PurchaseOrderShipmentStates(this.Strategy.Transaction).NotReceived;
            this.PurchaseOrderPaymentState ??= new PurchaseOrderPaymentStates(this.Strategy.Transaction).NotPaid;

            if (!this.ExistOrderDate)
            {
                this.OrderDate = this.Transaction().Now();
            }

            if (!this.ExistEntryDate)
            {
                this.EntryDate = this.Transaction().Now();
            }

            if (!this.ExistOrderedBy)
            {
                var internalOrganisations = new Organisations(this.Strategy.Transaction).InternalOrganisations();
                if (internalOrganisations.Count() == 1)
                {
                    this.OrderedBy = internalOrganisations.First();
                }
            }

            this.DerivedCurrency = this.OrderedBy?.PreferredCurrency;
        }

        public void AppsPrint(PrintablePrint method)
    {
            var singleton = this.Strategy.Transaction.GetSingleton();
            var logo = this.OrderedBy?.ExistLogoImage == true ?
                this.OrderedBy.LogoImage.MediaContent.Data :
                singleton.LogoImage.MediaContent.Data;

            var images = new Dictionary<string, byte[]>
            {
                { "Logo", logo },
            };

            if (this.ExistOrderNumber)
            {
                var transaction = this.Strategy.Transaction;
                var barcodeGenerator = transaction.Database.Services.Get<IBarcodeGenerator>();
                var barcode = barcodeGenerator.Generate(this.OrderNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                images.Add("Barcode", barcode);
            }

            var model = new Print.PurchaseOrderModel.Model(this);
            this.RenderPrintDocument(this.OrderedBy?.PurchaseOrderTemplate, model, images);

            this.PrintDocument.Media.InFileName = $"{this.OrderNumber}.odt";

            method.StopPropagation = true;
        }

        public void AppsCancel(OrderCancel method)
        {
            this.PurchaseOrderState = new PurchaseOrderStates(this.Strategy.Transaction).Cancelled;
            method.StopPropagation = true;
        }

        public void AppsSetReadyForProcessing(PurchaseOrderSetReadyForProcessing method)
        {
            this.PurchaseOrderState = this.NeedsApprovalLevel1
                ? new PurchaseOrderStates(this.Strategy.Transaction).AwaitingApprovalLevel1
                : this.PurchaseOrderState = this.NeedsApprovalLevel2
                ? new PurchaseOrderStates(this.Strategy.Transaction).AwaitingApprovalLevel2
                : new PurchaseOrderStates(this.Strategy.Transaction).InProcess;

            method.StopPropagation = true;
        }

        public void AppsReject(OrderReject method)
        {
            this.PurchaseOrderState = new PurchaseOrderStates(this.Strategy.Transaction).Rejected;
            foreach (PurchaseOrderItem purchaseOrderItem in this.ValidOrderItems)
            {
                purchaseOrderItem.Reject();
            }

            method.StopPropagation = true;
        }

        public void AppsHold(OrderHold method)
        {
            this.PurchaseOrderState = new PurchaseOrderStates(this.Strategy.Transaction).OnHold;
            method.StopPropagation = true;
        }

        public void AppsApprove(OrderApprove method)
        {
            this.PurchaseOrderState = this.NeedsApprovalLevel2 && this.PurchaseOrderState.IsAwaitingApprovalLevel1 ? new PurchaseOrderStates(this.Strategy.Transaction).AwaitingApprovalLevel2 : new PurchaseOrderStates(this.Strategy.Transaction).InProcess;

            var openTasks = this.TasksWhereWorkItem.Where(v => !v.ExistDateClosed).ToArray();

            if (openTasks.OfType<PurchaseOrderApprovalLevel1>().Any())
            {
                openTasks.First().DateClosed = this.Transaction().Now();
            }

            if (openTasks.OfType<PurchaseOrderApprovalLevel2>().Any())
            {
                openTasks.First().DateClosed = this.Transaction().Now();
            }

            method.StopPropagation = true;
        }

        public void AppsRevise(PurchaseOrderRevise method)
        {
            this.PurchaseOrderState = new PurchaseOrderStates(this.Strategy.Transaction).Created;
            method.StopPropagation = true;
        }

        public void AppsReopen(OrderReopen method)
        {
            this.PurchaseOrderState = this.PreviousPurchaseOrderState;
            method.StopPropagation = true;
        }

        public void AppsContinue(OrderContinue method)
        {
            this.PurchaseOrderState = this.PreviousPurchaseOrderState;
            method.StopPropagation = true;
        }

        public void AppsSend(PurchaseOrderSend method)
        {
            this.PurchaseOrderState = new PurchaseOrderStates(this.Strategy.Transaction).Sent;
            method.StopPropagation = true;
        }

        public void AppsQuickReceive(PurchaseOrderQuickReceive method)
        {
            var transaction = this.Transaction();

            if (this.ValidOrderItems.Any(v => ((PurchaseOrderItem)v).IsReceivable))
            {
                var shipment = new PurchaseShipmentBuilder(transaction)
                    .WithShipmentMethod(new ShipmentMethods(transaction).Ground)
                    .WithShipToParty(this.OrderedBy)
                    .WithShipToAddress(this.DerivedShipToAddress)
                    .WithShipFromParty(this.TakenViaSupplier)
                    .WithShipToFacility(this.StoredInFacility)
                    .Build();

                foreach (PurchaseOrderItem orderItem in this.ValidOrderItems)
                {
                    ShipmentItem shipmentItem = null;
                    if (orderItem.PurchaseOrderItemShipmentState.IsNotReceived && orderItem.ExistPart)
                    {
                        shipmentItem = new ShipmentItemBuilder(transaction)
                            .WithPart(orderItem.Part)
                            .WithStoredInFacility(orderItem.StoredInFacility)
                            .WithQuantity(orderItem.QuantityOrdered)
                            .WithUnitPurchasePrice(orderItem.UnitPrice)
                            .WithContentsDescription($"{orderItem.QuantityOrdered} * {orderItem.Part.Name}")
                            .Build();

                        shipment.AddShipmentItem(shipmentItem);

                        new OrderShipmentBuilder(transaction)
                            .WithOrderItem(orderItem)
                            .WithShipmentItem(shipmentItem)
                            .WithQuantity(orderItem.QuantityOrdered)
                            .Build();

                        if (orderItem.Part.InventoryItemKind.IsSerialised)
                        {
                            var serialisedItem = orderItem.SerialisedItem;
                            if (!orderItem.ExistSerialisedItem)
                            {
                                serialisedItem = new SerialisedItemBuilder(transaction)
                                    .WithSerialNumber(orderItem.SerialNumber)
                                    .Build();

                                orderItem.Part.AddSerialisedItem(serialisedItem);
                            }

                            shipmentItem.SerialisedItem = serialisedItem;

                            // HACK: DerivedRoles (WIP)
                            serialisedItem.AcquiredDate = orderItem.PurchaseOrderWherePurchaseOrderItem.OrderDate;

                            if (serialisedItem.ExistAcquiredDate && serialisedItem.ExistAcquisitionYear)
                            {
                                serialisedItem.RemoveAcquisitionYear();
                            }

                            if (this.OrderedBy.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(this.Transaction()).PurchaseshipmentReceive))
                            {
                                serialisedItem.Buyer = this.OrderedBy;
                                serialisedItem.OwnedBy = this.OrderedBy;
                                serialisedItem.Ownership = new Ownerships(this.Transaction()).Own;
                            }
                        }
                    }
                }

                if (shipment.ShipToParty is InternalOrganisation internalOrganisation && internalOrganisation.IsAutomaticallyReceived)
                {
                    shipment.Receive();
                }
            }

            method.StopPropagation = true;
        }

        public void AppsInvoice(PurchaseOrderInvoice method)
        {
            if (this.CanInvoice)
            {
                var purchaseInvoice = new PurchaseInvoiceBuilder(this.Strategy.Transaction)
                    .WithBilledFrom(this.TakenViaSupplier)
                    .WithAssignedBilledFromContactMechanism(this.DerivedTakenViaContactMechanism)
                    .WithBilledFromContactPerson(this.TakenViaContactPerson)
                    .WithBilledTo(this.OrderedBy)
                    .WithBilledToContactPerson(this.BillToContactPerson)
                    .WithDescription(this.Description)
                    .WithInvoiceDate(this.Transaction().Now())
                    .WithAssignedVatRegime(this.DerivedVatRegime)
                    .WithAssignedIrpfRegime(this.DerivedIrpfRegime)
                    .WithCustomerReference(this.CustomerReference)
                    .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Transaction()).PurchaseInvoice)
                    .Build();

                foreach (var orderAdjustment in this.OrderAdjustments)
                {
                    OrderAdjustment newAdjustment = null;
                    if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
                    {
                        newAdjustment = new DiscountAdjustmentBuilder(this.Transaction()).Build();
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                    {
                        newAdjustment = new SurchargeAdjustmentBuilder(this.Transaction()).Build();
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                    {
                        newAdjustment = new FeeBuilder(this.Transaction()).Build();
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                    {
                        newAdjustment = new ShippingAndHandlingChargeBuilder(this.Transaction()).Build();
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                    {
                        newAdjustment = new MiscellaneousChargeBuilder(this.Transaction()).Build();
                    }

                    newAdjustment.Amount ??= orderAdjustment.Amount;
                    newAdjustment.Percentage ??= orderAdjustment.Percentage;
                    purchaseInvoice.AddOrderAdjustment(newAdjustment);
                }

                foreach (PurchaseOrderItem orderItem in this.ValidOrderItems)
                {
                    if (orderItem.CanInvoice)
                    {
                        var invoiceItem = new PurchaseInvoiceItemBuilder(this.Strategy.Transaction)
                            .WithAssignedUnitPrice(orderItem.UnitPrice)
                            .WithInvoiceItemType(orderItem.InvoiceItemType)
                            .WithPart(orderItem.Part)
                            .WithQuantity(orderItem.QuantityOrdered)
                            .WithAssignedVatRegime(orderItem.AssignedVatRegime)
                            .WithAssignedIrpfRegime(orderItem.AssignedIrpfRegime)
                            .WithDescription(orderItem.Description)
                            .WithInternalComment(orderItem.InternalComment)
                            .WithMessage(orderItem.Message)
                            .Build();

                        purchaseInvoice.AddPurchaseInvoiceItem(invoiceItem);

                        new OrderItemBillingBuilder(this.Strategy.Transaction)
                            .WithQuantity(orderItem.QuantityOrdered)
                            .WithAmount(orderItem.TotalBasePrice)
                            .WithOrderItem(orderItem)
                            .WithInvoiceItem(invoiceItem)
                            .Build();
                    }
                }

                this.DerivationTrigger = new Guid();
            }

            method.StopPropagation = true;
        }
    }
}
