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

    public class PurchaseOrderDerivation : DomainDerivation
    {
        public PurchaseOrderDerivation(M m) : base(m, new Guid("C98B629B-12F8-4297-B6DA-FB0C36C56C39")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.PurchaseOrder.Class),
                new ChangedPattern(this.M.PurchaseOrder.PurchaseOrderState),
                new ChangedPattern(this.M.PurchaseOrder.PurchaseOrderItems),
                new ChangedPattern(this.M.PurchaseOrderItem.IsReceivable) { Steps = new IPropertyType[] { this.M.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem}},
                new ChangedPattern(this.M.PurchaseOrderItem.PurchaseOrderItemState)  { Steps = new IPropertyType[] { this.M.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem}},
                new ChangedPattern(this.M.InternalOrganisation.ActiveSuppliers) { Steps = new IPropertyType[] { this.M.InternalOrganisation.ActiveSuppliers, this.M.Organisation.PurchaseOrdersWhereOrderedBy}},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            bool NeedsApprovalLevel1(PurchaseOrder purchaseOrder)
            {
                if (purchaseOrder.ExistTakenViaSupplier && purchaseOrder.ExistOrderedBy)
                {
                    var supplierRelationship = ((Organisation)purchaseOrder.TakenViaSupplier).SupplierRelationshipsWhereSupplier.FirstOrDefault(v => v.InternalOrganisation.Equals(purchaseOrder.OrderedBy));
                    if (supplierRelationship != null
                        && supplierRelationship.NeedsApproval
                        && supplierRelationship.ApprovalThresholdLevel1.HasValue
                        && purchaseOrder.TotalExVat >= supplierRelationship.ApprovalThresholdLevel1.Value)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool NeedsApprovalLevel2(PurchaseOrder purchaseOrder)
            {
                if (purchaseOrder.ExistTakenViaSupplier && purchaseOrder.ExistOrderedBy)
                {
                    var supplierRelationship = ((Organisation)purchaseOrder.TakenViaSupplier).SupplierRelationshipsWhereSupplier.FirstOrDefault(v => v.InternalOrganisation.Equals(purchaseOrder.OrderedBy));
                    if (supplierRelationship != null
                        && supplierRelationship.NeedsApproval
                        && supplierRelationship.ApprovalThresholdLevel2.HasValue
                        && purchaseOrder.TotalExVat >= supplierRelationship.ApprovalThresholdLevel2.Value)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool CanInvoice(PurchaseOrder purchaseOrder)
            {
                if (purchaseOrder.PurchaseOrderState.IsSent || purchaseOrder.PurchaseOrderState.IsCompleted)
                {
                    foreach (PurchaseOrderItem purchaseOrderItem in purchaseOrder.ValidOrderItems)
                    {
                        if (!purchaseOrderItem.ExistOrderItemBillingsWhereOrderItem)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            bool CanRevise(PurchaseOrder purchaseOrder)
            {
                if ((purchaseOrder.PurchaseOrderState.IsInProcess || purchaseOrder.PurchaseOrderState.IsSent || purchaseOrder.PurchaseOrderState.IsCompleted)
                    && (purchaseOrder.PurchaseOrderShipmentState.IsNotReceived || purchaseOrder.PurchaseOrderShipmentState.IsNa))
                {
                    if (!purchaseOrder.ExistPurchaseInvoicesWherePurchaseOrder)
                    {
                        return true;
                    }
                }

                return false;
            }

            bool IsReceivable(PurchaseOrder purchaseOrder)
            {
                if (purchaseOrder.PurchaseOrderState.IsSent
                    && purchaseOrder.ValidOrderItems.Any(v => ((PurchaseOrderItem)v).IsReceivable))
                {
                    return true;
                }

                return false;
            }

            bool IsDeletable(PurchaseOrder purchaseOrder)
            {
                return (purchaseOrder.PurchaseOrderState.Equals(new PurchaseOrderStates(purchaseOrder.Strategy.Session).Created)
                 || purchaseOrder.PurchaseOrderState.Equals(new PurchaseOrderStates(purchaseOrder.Strategy.Session).Cancelled)
                 || purchaseOrder.PurchaseOrderState.Equals(new PurchaseOrderStates(purchaseOrder.Strategy.Session).Rejected))
             && !purchaseOrder.ExistPurchaseInvoicesWherePurchaseOrder
             && !purchaseOrder.ExistSerialisedItemsWherePurchaseOrder
             && !purchaseOrder.ExistWorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrder
             && purchaseOrder.PurchaseOrderItems.All(v => v.IsDeletable);
            }

            foreach (var @this in matches.Cast<PurchaseOrder>().Where(v => v.OrderedBy?.ActiveSuppliers.Any() == true))
            {
                var validation = cycle.Validation;

                if (!@this.ExistOrderNumber)
                {
                    @this.OrderNumber = @this.OrderedBy?.NextPurchaseOrderNumber(@this.OrderDate.Year);
                    @this.SortableOrderNumber = @this.Session().GetSingleton().SortableNumber(@this.OrderedBy?.PurchaseOrderNumberPrefix, @this.OrderNumber, @this.OrderDate.Year.ToString());
                }

                if (@this.TakenViaSupplier is Organisation supplier)
                {
                    if (!@this.OrderedBy.ActiveSuppliers.Contains(supplier))
                    {
                        validation.AddError($"{@this} {@this.Meta.TakenViaSupplier} {ErrorMessages.PartyIsNotASupplier}");
                    }
                }

                if (@this.TakenViaSubcontractor is Organisation subcontractor)
                {
                    if (!@this.OrderedBy.ActiveSubContractors.Contains(subcontractor))
                    {
                        validation.AddError($"{@this} {@this.Meta.TakenViaSubcontractor} {ErrorMessages.PartyIsNotASubcontractor}");
                    }
                }

                validation.AssertExistsAtMostOne(@this, @this.Meta.TakenViaSupplier, @this.Meta.TakenViaSubcontractor);
                validation.AssertAtLeastOne(@this, @this.Meta.TakenViaSupplier, @this.Meta.TakenViaSubcontractor);

                if (!@this.ExistShipToAddress)
                {
                    @this.ShipToAddress = @this.OrderedBy.ShippingAddress;
                }

                if (!@this.ExistBillToContactMechanism)
                {
                    @this.BillToContactMechanism = @this.OrderedBy.ExistBillingAddress ? @this.OrderedBy.BillingAddress : @this.OrderedBy.GeneralCorrespondence;
                }

                if (!@this.ExistTakenViaContactMechanism && @this.ExistTakenViaSupplier)
                {
                    @this.TakenViaContactMechanism = @this.TakenViaSupplier.OrderAddress;
                }

                @this.VatRegime ??= @this.TakenViaSupplier?.VatRegime;
                @this.IrpfRegime ??= @this.TakenViaSupplier?.IrpfRegime;

                @this.Locale = @this.Strategy.Session.GetSingleton().DefaultLocale;

                var validOrderItems = @this.PurchaseOrderItems.Where(v => v.IsValid).ToArray();
                @this.ValidOrderItems = validOrderItems;

                var purchaseOrderShipmentStates = new PurchaseOrderShipmentStates(@this.Strategy.Session);
                var purchaseOrderPaymentStates = new PurchaseOrderPaymentStates(@this.Strategy.Session);
                var purchaseOrderItemStates = new PurchaseOrderItemStates(cycle.Session);

                // PurchaseOrder Shipment State
                if (validOrderItems.Any())
                {
                    if (validOrderItems.Any(v => v.IsReceivable))
                    {
                        if (validOrderItems.Where(v => v.IsReceivable).All(v => v.PurchaseOrderItemShipmentState.IsReceived))
                        {
                            @this.PurchaseOrderShipmentState = purchaseOrderShipmentStates.Received;
                        }
                        else if (validOrderItems.Where(v => v.IsReceivable).All(v => v.PurchaseOrderItemShipmentState.IsNotReceived))
                        {
                            @this.PurchaseOrderShipmentState = purchaseOrderShipmentStates.NotReceived;
                        }
                        else
                        {
                            @this.PurchaseOrderShipmentState = purchaseOrderShipmentStates.PartiallyReceived;
                        }
                    }
                    else
                    {
                        @this.PurchaseOrderShipmentState = purchaseOrderShipmentStates.Na;
                    }

                    // PurchaseOrder Payment State
                    if (validOrderItems.All(v => v.PurchaseOrderItemPaymentState.IsPaid))
                    {
                        @this.PurchaseOrderPaymentState = purchaseOrderPaymentStates.Paid;
                    }
                    else if (validOrderItems.All(v => v.PurchaseOrderItemPaymentState.IsNotPaid))
                    {
                        @this.PurchaseOrderPaymentState = purchaseOrderPaymentStates.NotPaid;
                    }
                    else
                    {
                        @this.PurchaseOrderPaymentState = purchaseOrderPaymentStates.PartiallyPaid;
                    }

                    // PurchaseOrder OrderState
                    if (@this.PurchaseOrderState.IsSent
                        && (@this.PurchaseOrderShipmentState.IsReceived || @this.PurchaseOrderShipmentState.IsNa))
                    {
                        @this.PurchaseOrderState = new PurchaseOrderStates(@this.Strategy.Session).Completed;
                    }

                    if (@this.PurchaseOrderState.IsCompleted && @this.PurchaseOrderPaymentState.IsPaid)
                    {
                        @this.PurchaseOrderState = new PurchaseOrderStates(@this.Strategy.Session).Finished;
                    }
                }

                // Derive Totals
                var quantityOrderedByPart = new Dictionary<Part, decimal>();
                var totalBasePriceByPart = new Dictionary<Part, decimal>();
                foreach (PurchaseOrderItem item in @this.ValidOrderItems)
                {
                    if (item.ExistPart)
                    {
                        if (!quantityOrderedByPart.ContainsKey(item.Part))
                        {
                            quantityOrderedByPart.Add(item.Part, item.QuantityOrdered);
                            totalBasePriceByPart.Add(item.Part, item.TotalBasePrice);
                        }
                        else
                        {
                            quantityOrderedByPart[item.Part] += item.QuantityOrdered;
                            totalBasePriceByPart[item.Part] += item.TotalBasePrice;
                        }
                    }
                }

                @this.TotalBasePrice = 0;
                @this.TotalDiscount = 0;
                @this.TotalSurcharge = 0;
                @this.TotalVat = 0;
                @this.TotalIrpf = 0;
                @this.TotalExVat = 0;
                @this.TotalExtraCharge = 0;
                @this.TotalIncVat = 0;
                @this.GrandTotal = 0;

                foreach (PurchaseOrderItem orderItem in @this.ValidOrderItems)
                {
                    @this.TotalBasePrice += orderItem.TotalBasePrice;
                    @this.TotalDiscount += orderItem.TotalDiscount;
                    @this.TotalSurcharge += orderItem.TotalSurcharge;
                    @this.TotalVat += orderItem.TotalVat;
                    @this.TotalIrpf += orderItem.TotalIrpf;
                    @this.TotalExVat += orderItem.TotalExVat;
                    @this.TotalIncVat += orderItem.TotalIncVat;
                    @this.GrandTotal += orderItem.GrandTotal;
                }

                @this.PreviousTakenViaSupplier = @this.TakenViaSupplier;

                // Derive Workflow
                @this.WorkItemDescription = $"PurchaseOrder: {@this.OrderNumber} [{@this.TakenViaSupplier?.PartyName}]";
                var openTasks = @this.TasksWhereWorkItem.Where(v => !v.ExistDateClosed).ToArray();
                if (@this.PurchaseOrderState.IsAwaitingApprovalLevel1)
                {
                    if (!openTasks.OfType<PurchaseOrderApprovalLevel1>().Any())
                    {
                        new PurchaseOrderApprovalLevel1Builder(@this.Session()).WithPurchaseOrder(@this).Build();
                    }
                }

                if (@this.PurchaseOrderState.IsAwaitingApprovalLevel2)
                {
                    if (!openTasks.OfType<PurchaseOrderApprovalLevel2>().Any())
                    {
                        new PurchaseOrderApprovalLevel2Builder(@this.Session()).WithPurchaseOrder(@this).Build();
                    }
                }

                @this.ResetPrintDocument();

            }
        }
    }
}
