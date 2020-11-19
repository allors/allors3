// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class PurchaseOrderItemDerivation : DomainDerivation
    {
        public PurchaseOrderItemDerivation(M m) : base(m, new Guid("A59A2EFC-AF5C-4F95-9212-4FD4B0306957")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.PurchaseOrderItem.PurchaseOrderItemState),
                new ChangedPattern(m.PurchaseOrderItem.IsReceivable),
                new ChangedPattern(m.PurchaseOrderItem.AssignedVatRegime),
                new ChangedPattern(m.PurchaseOrderItem.AssignedIrpfRegime),
                new ChangedPattern(m.PurchaseOrder.DerivedVatRegime) { Steps = new IPropertyType[] {m.PurchaseOrder.PurchaseOrderItems} },
                new ChangedPattern(m.PurchaseOrder.DerivedIrpfRegime ) { Steps = new IPropertyType[] {m.PurchaseOrder.PurchaseOrderItems} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<PurchaseOrderItem>())
            {
                if (!@this.ExistDerivationTrigger)
                {
                    @this.DerivationTrigger = Guid.NewGuid();
                }

                if (!@this.ExistStoredInFacility && @this.PurchaseOrderWherePurchaseOrderItem.ExistStoredInFacility)
                {
                    @this.StoredInFacility = @this.PurchaseOrderWherePurchaseOrderItem.StoredInFacility;
                }

                if (!@this.ExistPurchaseOrderItemShipmentState)
                {
                    if (@this.IsReceivable)
                    {
                        @this.PurchaseOrderItemShipmentState = new PurchaseOrderItemShipmentStates(@this.Strategy.Session).NotReceived;
                    }
                    else
                    {
                        @this.PurchaseOrderItemShipmentState = new PurchaseOrderItemShipmentStates(@this.Strategy.Session).Na;
                    }
                }

                // States
                var states = new PurchaseOrderItemStates(@this.Session());

                var purchaseOrderState = @this.PurchaseOrderWherePurchaseOrderItem.PurchaseOrderState;
                //if (purchaseOrderState.IsCreated
                //    && !@this.PurchaseOrderItemState.IsCancelled
                //    && !@this.PurchaseOrderItemState.IsRejected)
                //{
                //    @this.PurchaseOrderItemState = states.Created;
                //}

                if (purchaseOrderState.IsInProcess &&
                    (@this.PurchaseOrderItemState.IsCreated || @this.PurchaseOrderItemState.IsOnHold))
                {
                    @this.PurchaseOrderItemState = states.InProcess;
                }

                if (purchaseOrderState.IsOnHold && @this.PurchaseOrderItemState.IsInProcess)
                {
                    @this.PurchaseOrderItemState = states.OnHold;
                }

                if (purchaseOrderState.IsSent && @this.PurchaseOrderItemState.IsInProcess)
                {
                    @this.PurchaseOrderItemState = states.Sent;
                }

                if (@this.IsValid && purchaseOrderState.IsFinished)
                {
                    @this.PurchaseOrderItemState = states.Finished;
                }

                if (@this.IsValid && purchaseOrderState.IsCancelled)
                {
                    @this.PurchaseOrderItemState = states.Cancelled;
                }

                if (@this.IsValid && purchaseOrderState.IsRejected)
                {
                    @this.PurchaseOrderItemState = states.Rejected;
                }

                if (@this.IsValid)
                {
                    if (@this.AssignedDeliveryDate.HasValue)
                    {
                        @this.DeliveryDate = @this.AssignedDeliveryDate.Value;
                    }
                    else if (@this.PurchaseOrderWherePurchaseOrderItem.DeliveryDate.HasValue)
                    {
                        @this.DeliveryDate = @this.PurchaseOrderWherePurchaseOrderItem.DeliveryDate.Value;
                    }

                    @this.UnitBasePrice = 0;
                    @this.UnitDiscount = 0;
                    @this.UnitSurcharge = 0;

                    if (@this.AssignedUnitPrice.HasValue)
                    {
                        @this.UnitBasePrice = @this.AssignedUnitPrice.Value;
                        @this.UnitPrice = @this.AssignedUnitPrice.Value;
                    }
                    else
                    {
                        var order = @this.PurchaseOrderWherePurchaseOrderItem;
                        @this.UnitBasePrice = new SupplierOfferings(@this.Strategy.Session).PurchasePrice(order.TakenViaSupplier, order.OrderDate, @this.Part);
                    }

                    @this.DerivedVatRegime = @this.AssignedVatRegime ?? @this.PurchaseOrderWherePurchaseOrderItem.DerivedVatRegime;
                    @this.VatRate = @this.DerivedVatRegime?.VatRate;

                    @this.DerivedIrpfRegime = @this.AssignedIrpfRegime ?? @this.PurchaseOrderWherePurchaseOrderItem.DerivedIrpfRegime;
                    @this.IrpfRate = @this.DerivedIrpfRegime?.IrpfRate;

                    @this.TotalBasePrice = @this.UnitBasePrice * @this.QuantityOrdered;
                    @this.TotalDiscount = @this.UnitDiscount * @this.QuantityOrdered;
                    @this.TotalSurcharge = @this.UnitSurcharge * @this.QuantityOrdered;
                    @this.UnitPrice = @this.UnitBasePrice - @this.UnitDiscount + @this.UnitSurcharge;

                    @this.UnitVat = @this.ExistVatRate ? @this.UnitPrice * @this.VatRate.Rate / 100 : 0;
                    @this.UnitIrpf = @this.ExistIrpfRate ? @this.UnitPrice * @this.IrpfRate.Rate / 100 : 0;
                    @this.TotalVat = @this.UnitVat * @this.QuantityOrdered;
                    @this.TotalExVat = @this.UnitPrice * @this.QuantityOrdered;
                    @this.TotalIrpf = @this.UnitIrpf * @this.QuantityOrdered;
                    @this.TotalIncVat = @this.TotalExVat + @this.TotalVat;
                    @this.GrandTotal = @this.TotalIncVat - @this.TotalIrpf;
                }

                if (@this.ExistPart && @this.Part.InventoryItemKind.IsSerialised)
                {
                    validation.AssertAtLeastOne(@this, this.M.PurchaseOrderItem.SerialisedItem, this.M.PurchaseOrderItem.SerialNumber);
                    validation.AssertExistsAtMostOne(@this, this.M.PurchaseOrderItem.SerialisedItem, this.M.PurchaseOrderItem.SerialNumber);

                    if (@this.QuantityOrdered != 1)
                    {
                        validation.AddError($"{@this} {this.M.PurchaseOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                    }
                }

                if (!@this.ExistPart && @this.QuantityOrdered != 1)
                {
                    validation.AddError($"{@this} {this.M.PurchaseOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                }

                if (@this.ExistPart && @this.Part.InventoryItemKind.IsNonSerialised && @this.QuantityOrdered == 0)
                {
                    validation.AddError($"{@this} {this.M.PurchaseOrderItem.QuantityOrdered} {ErrorMessages.InvalidQuantity}");
                }

                var purchaseOrderItemShipmentStates = new PurchaseOrderItemShipmentStates(session);
                var purchaseOrderItemPaymentStates = new PurchaseOrderItemPaymentStates(session);
                var purchaseOrderItemStates = new PurchaseOrderItemStates(session);

                if (@this.IsValid)
                {
                    // ShipmentState
                    if (@this.IsReceivable)
                    {
                        var quantityReceived = 0M;
                        foreach (ShipmentReceipt shipmentReceipt in @this.ShipmentReceiptsWhereOrderItem)
                        {
                            quantityReceived += shipmentReceipt.QuantityAccepted;
                        }

                        @this.QuantityReceived = quantityReceived;
                    }

                    if (!@this.IsReceivable)
                    {
                        @this.PurchaseOrderItemShipmentState = new PurchaseOrderItemShipmentStates(@this.Strategy.Session).Na;
                    }
                    else
                    {
                        if (@this.QuantityReceived == 0 && @this.IsReceivable)
                        {
                            @this.PurchaseOrderItemShipmentState = new PurchaseOrderItemShipmentStates(@this.Strategy.Session).NotReceived;
                        }
                        else
                        {
                            @this.PurchaseOrderItemShipmentState = @this.QuantityReceived < @this.QuantityOrdered ?
                                purchaseOrderItemShipmentStates.PartiallyReceived :
                                purchaseOrderItemShipmentStates.Received;
                        }
                    }

                    // PaymentState
                    var orderBilling = @this.OrderItemBillingsWhereOrderItem.Select(v => v.InvoiceItem).OfType<PurchaseInvoiceItem>().ToArray();

                    if (orderBilling.Any())
                    {
                        if (orderBilling.All(v => v.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceState.IsPaid))
                        {
                            @this.PurchaseOrderItemPaymentState = purchaseOrderItemPaymentStates.Paid;
                        }
                        else if (orderBilling.All(v => !v.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceState.IsPaid))
                        {
                            @this.PurchaseOrderItemPaymentState = purchaseOrderItemPaymentStates.NotPaid;
                        }
                        else
                        {
                            @this.PurchaseOrderItemPaymentState = purchaseOrderItemPaymentStates.PartiallyPaid;
                        }
                    }

                    // PurchaseOrderItem States
                    if (@this.PurchaseOrderItemState.IsInProcess
                        && (@this.PurchaseOrderItemShipmentState.IsReceived || @this.PurchaseOrderItemShipmentState.IsNa))
                    {
                        @this.PurchaseOrderItemState = purchaseOrderItemStates.Completed;
                    }

                    if (@this.PurchaseOrderItemState.IsCompleted && @this.PurchaseOrderItemPaymentState.IsPaid)
                    {
                        @this.PurchaseOrderItemState = purchaseOrderItemStates.Finished;
                    }
                }

                if (@this.PurchaseOrderItemState.Equals(states.InProcess) ||
                    @this.PurchaseOrderItemState.Equals(states.Cancelled) ||
                    @this.PurchaseOrderItemState.Equals(states.Rejected))
                {
                    NonSerialisedInventoryItem inventoryItem = null;

                    if (@this.ExistPart)
                    {
                        var inventoryItems = @this.Part.InventoryItemsWherePart;
                        inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, @this.PurchaseOrderWherePurchaseOrderItem.StoredInFacility);
                        inventoryItem = inventoryItems.First as NonSerialisedInventoryItem;
                    }

                    if (@this.PurchaseOrderItemState.Equals(new PurchaseOrderItemStates(@this.Strategy.Session).InProcess))
                    {
                        if (!@this.ExistPreviousQuantity || !@this.QuantityOrdered.Equals(@this.PreviousQuantity))
                        {
                            // TODO: Remove OnDerive
                            //inventoryItem?.OnDerive(x => x.WithDerivation(derivation));
                        }
                    }

                    if (@this.PurchaseOrderItemState.Equals(new PurchaseOrderItemStates(@this.Strategy.Session).Cancelled) ||
                        @this.PurchaseOrderItemState.Equals(new PurchaseOrderItemStates(@this.Strategy.Session).Rejected))
                    {
                        // TODO: Remove OnDerive
                        //inventoryItem?.OnDerive(x => x.WithDerivation(derivation));
                    }
                }

                if (@this.IsValid && !@this.ExistOrderItemBillingsWhereOrderItem)
                {
                    @this.CanInvoice = true;
                }
                else
                {
                    @this.CanInvoice = false;
                }

                if (!@this.PurchaseOrderItemShipmentState.IsNotReceived && !@this.PurchaseOrderItemShipmentState.IsNa)
                {
                    var deniablePermissionByOperandType = new Dictionary<OperandType, Permission>();

                    foreach (Permission permission in @this.Session().Extent<Permission>())
                    {
                        if (permission.ClassPointer == @this.Strategy.Class.Id
                            && (permission.Operation == Operations.Write || permission.Operation == Operations.Execute))
                        {
                            deniablePermissionByOperandType.Add(permission.OperandType, permission);
                        }
                    }

                    foreach (var keyValuePair in deniablePermissionByOperandType)
                    {
                        @this.AddDeniedPermission(keyValuePair.Value);
                    }
                }
            }
        }
    }
}
