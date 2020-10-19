// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class PurchaseOrderItemDerivation : DomainDerivation
    {
        public PurchaseOrderItemDerivation(M m) : base(m, new Guid("A59A2EFC-AF5C-4F95-9212-4FD4B0306957")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.PurchaseOrderItem.Class),
                new ChangedRolePattern(this.M.PurchaseOrderItem.PurchaseOrderItemState),
                // new ChangedRolePattern(M.PurchaseOrder.StoredInFacility) { Steps = new IPropertyType[] {M.PurchaseOrder.PurchaseOrderItems} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var purchaseOrderItem in matches.Cast<PurchaseOrderItem>())
            {
                if (!purchaseOrderItem.ExistDerivationTrigger)
                {
                    purchaseOrderItem.DerivationTrigger = Guid.NewGuid();
                }

                if (!purchaseOrderItem.ExistStoredInFacility && purchaseOrderItem.PurchaseOrderWherePurchaseOrderItem.ExistStoredInFacility)
                {
                    purchaseOrderItem.StoredInFacility = purchaseOrderItem.PurchaseOrderWherePurchaseOrderItem.StoredInFacility;
                }

                // TODO: Martien
                //purchaseOrderItem.DeriveIsReceivable();

                if (!purchaseOrderItem.ExistPurchaseOrderItemShipmentState)
                {
                    if (purchaseOrderItem.IsReceivable)
                    {
                        purchaseOrderItem.PurchaseOrderItemShipmentState = new PurchaseOrderItemShipmentStates(purchaseOrderItem.Strategy.Session).NotReceived;
                    }
                    else
                    {
                        purchaseOrderItem.PurchaseOrderItemShipmentState = new PurchaseOrderItemShipmentStates(purchaseOrderItem.Strategy.Session).Na;
                    }
                }

                // States
                var states = new PurchaseOrderItemStates(purchaseOrderItem.Session());

                var purchaseOrderState = purchaseOrderItem.PurchaseOrderWherePurchaseOrderItem.PurchaseOrderState;
                if (purchaseOrderState.IsCreated
                    && !purchaseOrderItem.PurchaseOrderItemState.IsCancelled
                    && !purchaseOrderItem.PurchaseOrderItemState.IsRejected)
                {
                    purchaseOrderItem.PurchaseOrderItemState = states.Created;
                }

                if (purchaseOrderState.IsInProcess &&
                    (purchaseOrderItem.PurchaseOrderItemState.IsCreated || purchaseOrderItem.PurchaseOrderItemState.IsOnHold))
                {
                    purchaseOrderItem.PurchaseOrderItemState = states.InProcess;
                }

                if (purchaseOrderState.IsOnHold && purchaseOrderItem.PurchaseOrderItemState.IsInProcess)
                {
                    purchaseOrderItem.PurchaseOrderItemState = states.OnHold;
                }

                if (purchaseOrderState.IsSent && purchaseOrderItem.PurchaseOrderItemState.IsInProcess)
                {
                    purchaseOrderItem.PurchaseOrderItemState = states.Sent;
                }

                if (purchaseOrderItem.IsValid && purchaseOrderState.IsFinished)
                {
                    purchaseOrderItem.PurchaseOrderItemState = states.Finished;
                }

                if (purchaseOrderItem.IsValid && purchaseOrderState.IsCancelled)
                {
                    purchaseOrderItem.PurchaseOrderItemState = states.Cancelled;
                }

                if (purchaseOrderItem.IsValid && purchaseOrderState.IsRejected)
                {
                    purchaseOrderItem.PurchaseOrderItemState = states.Rejected;
                }

                if (purchaseOrderItem.IsValid)
                {
                    if (purchaseOrderItem.AssignedDeliveryDate.HasValue)
                    {
                        purchaseOrderItem.DeliveryDate = purchaseOrderItem.AssignedDeliveryDate.Value;
                    }
                    else if (purchaseOrderItem.PurchaseOrderWherePurchaseOrderItem.DeliveryDate.HasValue)
                    {
                        purchaseOrderItem.DeliveryDate = purchaseOrderItem.PurchaseOrderWherePurchaseOrderItem.DeliveryDate.Value;
                    }

                    purchaseOrderItem.UnitBasePrice = 0;
                    purchaseOrderItem.UnitDiscount = 0;
                    purchaseOrderItem.UnitSurcharge = 0;

                    if (purchaseOrderItem.AssignedUnitPrice.HasValue)
                    {
                        purchaseOrderItem.UnitBasePrice = purchaseOrderItem.AssignedUnitPrice.Value;
                        purchaseOrderItem.UnitPrice = purchaseOrderItem.AssignedUnitPrice.Value;
                    }
                    else
                    {
                        var order = purchaseOrderItem.PurchaseOrderWherePurchaseOrderItem;
                        purchaseOrderItem.UnitBasePrice = new SupplierOfferings(purchaseOrderItem.Strategy.Session).PurchasePrice(order.TakenViaSupplier, order.OrderDate, purchaseOrderItem.Part);
                    }

                    purchaseOrderItem.VatRegime = purchaseOrderItem.AssignedVatRegime ?? purchaseOrderItem.PurchaseOrderWherePurchaseOrderItem.VatRegime;
                    purchaseOrderItem.VatRate = purchaseOrderItem.VatRegime?.VatRate;

                    purchaseOrderItem.IrpfRegime = purchaseOrderItem.AssignedIrpfRegime ?? purchaseOrderItem.PurchaseOrderWherePurchaseOrderItem.IrpfRegime;
                    purchaseOrderItem.IrpfRate = purchaseOrderItem.IrpfRegime?.IrpfRate;

                    purchaseOrderItem.TotalBasePrice = purchaseOrderItem.UnitBasePrice * purchaseOrderItem.QuantityOrdered;
                    purchaseOrderItem.TotalDiscount = purchaseOrderItem.UnitDiscount * purchaseOrderItem.QuantityOrdered;
                    purchaseOrderItem.TotalSurcharge = purchaseOrderItem.UnitSurcharge * purchaseOrderItem.QuantityOrdered;
                    purchaseOrderItem.UnitPrice = purchaseOrderItem.UnitBasePrice - purchaseOrderItem.UnitDiscount + purchaseOrderItem.UnitSurcharge;

                    purchaseOrderItem.UnitVat = purchaseOrderItem.ExistVatRate ? purchaseOrderItem.UnitPrice * purchaseOrderItem.VatRate.Rate / 100 : 0;
                    purchaseOrderItem.UnitIrpf = purchaseOrderItem.ExistIrpfRate ? purchaseOrderItem.UnitPrice * purchaseOrderItem.IrpfRate.Rate / 100 : 0;
                    purchaseOrderItem.TotalVat = purchaseOrderItem.UnitVat * purchaseOrderItem.QuantityOrdered;
                    purchaseOrderItem.TotalExVat = purchaseOrderItem.UnitPrice * purchaseOrderItem.QuantityOrdered;
                    purchaseOrderItem.TotalIrpf = purchaseOrderItem.UnitIrpf * purchaseOrderItem.QuantityOrdered;
                    purchaseOrderItem.TotalIncVat = purchaseOrderItem.TotalExVat + purchaseOrderItem.TotalVat;
                    purchaseOrderItem.GrandTotal = purchaseOrderItem.TotalIncVat - purchaseOrderItem.TotalIrpf;
                }

                if (purchaseOrderItem.ExistPart && purchaseOrderItem.Part.InventoryItemKind.IsSerialised)
                {
                    validation.AssertAtLeastOne(purchaseOrderItem, this.M.PurchaseOrderItem.SerialisedItem, this.M.PurchaseOrderItem.SerialNumber);
                    validation.AssertExistsAtMostOne(purchaseOrderItem, this.M.PurchaseOrderItem.SerialisedItem, this.M.PurchaseOrderItem.SerialNumber);

                    if (purchaseOrderItem.QuantityOrdered != 1)
                    {
                        validation.AddError($"{purchaseOrderItem} {this.M.PurchaseOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                    }
                }

                if (!purchaseOrderItem.ExistPart && purchaseOrderItem.QuantityOrdered != 1)
                {
                    validation.AddError($"{purchaseOrderItem} {this.M.PurchaseOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                }

                if (purchaseOrderItem.ExistPart && purchaseOrderItem.Part.InventoryItemKind.IsNonSerialised && purchaseOrderItem.QuantityOrdered == 0)
                {
                    validation.AddError($"{purchaseOrderItem} {this.M.PurchaseOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                }

                var purchaseOrderItemShipmentStates = new PurchaseOrderItemShipmentStates(session);
                var purchaseOrderItemPaymentStates = new PurchaseOrderItemPaymentStates(session);
                var purchaseOrderItemStates = new PurchaseOrderItemStates(session);

                if (purchaseOrderItem.IsValid)
                {
                    // ShipmentState
                    if (purchaseOrderItem.IsReceivable)
                    {
                        var quantityReceived = 0M;
                        foreach (ShipmentReceipt shipmentReceipt in purchaseOrderItem.ShipmentReceiptsWhereOrderItem)
                        {
                            quantityReceived += shipmentReceipt.QuantityAccepted;
                        }

                        purchaseOrderItem.QuantityReceived = quantityReceived;
                    }

                    if (!purchaseOrderItem.IsReceivable)
                    {
                        purchaseOrderItem.PurchaseOrderItemShipmentState = new PurchaseOrderItemShipmentStates(purchaseOrderItem.Strategy.Session).Na;
                    }
                    else
                    {
                        if (purchaseOrderItem.QuantityReceived == 0 && purchaseOrderItem.IsReceivable)
                        {
                            purchaseOrderItem.PurchaseOrderItemShipmentState = new PurchaseOrderItemShipmentStates(purchaseOrderItem.Strategy.Session).NotReceived;
                        }
                        else
                        {
                            purchaseOrderItem.PurchaseOrderItemShipmentState = purchaseOrderItem.QuantityReceived < purchaseOrderItem.QuantityOrdered ?
                                purchaseOrderItemShipmentStates.PartiallyReceived :
                                purchaseOrderItemShipmentStates.Received;
                        }
                    }

                    // PaymentState
                    var orderBilling = purchaseOrderItem.OrderItemBillingsWhereOrderItem.Select(v => v.InvoiceItem).OfType<PurchaseInvoiceItem>().ToArray();

                    if (orderBilling.Any())
                    {
                        if (orderBilling.All(v => v.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceState.IsPaid))
                        {
                            purchaseOrderItem.PurchaseOrderItemPaymentState = purchaseOrderItemPaymentStates.Paid;
                        }
                        else if (orderBilling.All(v => !v.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceState.IsPaid))
                        {
                            purchaseOrderItem.PurchaseOrderItemPaymentState = purchaseOrderItemPaymentStates.NotPaid;
                        }
                        else
                        {
                            purchaseOrderItem.PurchaseOrderItemPaymentState = purchaseOrderItemPaymentStates.PartiallyPaid;
                        }
                    }

                    // PurchaseOrderItem States
                    if (purchaseOrderItem.PurchaseOrderItemState.IsInProcess
                        && (purchaseOrderItem.PurchaseOrderItemShipmentState.IsReceived || purchaseOrderItem.PurchaseOrderItemShipmentState.IsNa))
                    {
                        purchaseOrderItem.PurchaseOrderItemState = purchaseOrderItemStates.Completed;
                    }

                    if (purchaseOrderItem.PurchaseOrderItemState.IsCompleted && purchaseOrderItem.PurchaseOrderItemPaymentState.IsPaid)
                    {
                        purchaseOrderItem.PurchaseOrderItemState = purchaseOrderItemStates.Finished;
                    }
                }

                if (purchaseOrderItem.PurchaseOrderItemState.Equals(states.InProcess) ||
                    purchaseOrderItem.PurchaseOrderItemState.Equals(states.Cancelled) ||
                    purchaseOrderItem.PurchaseOrderItemState.Equals(states.Rejected))
                {
                    NonSerialisedInventoryItem inventoryItem = null;

                    if (purchaseOrderItem.ExistPart)
                    {
                        var inventoryItems = purchaseOrderItem.Part.InventoryItemsWherePart;
                        inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, purchaseOrderItem.PurchaseOrderWherePurchaseOrderItem.StoredInFacility);
                        inventoryItem = inventoryItems.First as NonSerialisedInventoryItem;
                    }

                    if (purchaseOrderItem.PurchaseOrderItemState.Equals(new PurchaseOrderItemStates(purchaseOrderItem.Strategy.Session).InProcess))
                    {
                        if (!purchaseOrderItem.ExistPreviousQuantity || !purchaseOrderItem.QuantityOrdered.Equals(purchaseOrderItem.PreviousQuantity))
                        {
                            // TODO: Remove OnDerive
                            //inventoryItem?.OnDerive(x => x.WithDerivation(derivation));
                        }
                    }

                    if (purchaseOrderItem.PurchaseOrderItemState.Equals(new PurchaseOrderItemStates(purchaseOrderItem.Strategy.Session).Cancelled) ||
                        purchaseOrderItem.PurchaseOrderItemState.Equals(new PurchaseOrderItemStates(purchaseOrderItem.Strategy.Session).Rejected))
                    {
                        // TODO: Remove OnDerive
                        //inventoryItem?.OnDerive(x => x.WithDerivation(derivation));
                    }
                }

                if (purchaseOrderItem.IsValid && !purchaseOrderItem.ExistOrderItemBillingsWhereOrderItem)
                {
                    purchaseOrderItem.CanInvoice = true;
                }
                else
                {
                    purchaseOrderItem.CanInvoice = false;
                }

                var deletePermission = new Permissions(purchaseOrderItem.Strategy.Session).Get(purchaseOrderItem.Meta.ObjectType, purchaseOrderItem.Meta.Delete);
                if (purchaseOrderItem.IsDeletable)
                {
                    purchaseOrderItem.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    purchaseOrderItem.AddDeniedPermission(deletePermission);
                }

                if (!purchaseOrderItem.PurchaseOrderItemShipmentState.IsNotReceived && !purchaseOrderItem.PurchaseOrderItemShipmentState.IsNa)
                {
                    var deniablePermissionByOperandType = new Dictionary<OperandType, Permission>();

                    foreach (Permission permission in purchaseOrderItem.Session().Extent<Permission>())
                    {
                        if (permission.ClassPointer == purchaseOrderItem.Strategy.Class.Id
                            && (permission.Operation == Operations.Write || permission.Operation == Operations.Execute))
                        {
                            deniablePermissionByOperandType.Add(permission.OperandType, permission);
                        }
                    }

                    foreach (var keyValuePair in deniablePermissionByOperandType)
                    {
                        purchaseOrderItem.AddDeniedPermission(keyValuePair.Value);
                    }
                }
            }
        }
    }
}