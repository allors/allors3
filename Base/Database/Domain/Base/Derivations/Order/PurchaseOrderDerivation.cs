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

    public class PurchaseOrderDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("C98B629B-12F8-4297-B6DA-FB0C36C56C39");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.PurchaseOrder.Class),
            new ChangedRolePattern(M.PurchaseOrder.PurchaseOrderState),
            new ChangedRolePattern(M.PurchaseOrder.PurchaseOrderItems),
            new ChangedRolePattern(M.PurchaseOrderItem.PurchaseOrderItemState)  { Steps = new IPropertyType[] {M.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem}},
            new ChangedRolePattern(M.InternalOrganisation.ActiveSuppliers) { Steps = new IPropertyType[] {M.InternalOrganisation.ActiveSuppliers, M.Organisation.PurchaseOrdersWhereOrderedBy}},
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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

            foreach (var purchaseOrder in matches.Cast<PurchaseOrder>().Where(v => v.OrderedBy?.ActiveSuppliers.Any() == true))
            {
                var validation = cycle.Validation;

                if (!purchaseOrder.ExistOrderNumber)
                {
                    purchaseOrder.OrderNumber = purchaseOrder.OrderedBy?.NextPurchaseOrderNumber(purchaseOrder.OrderDate.Year);
                    purchaseOrder.SortableOrderNumber = purchaseOrder.Session().GetSingleton().SortableNumber(purchaseOrder.OrderedBy?.PurchaseOrderNumberPrefix, purchaseOrder.OrderNumber, purchaseOrder.OrderDate.Year.ToString());
                }

                if (purchaseOrder.TakenViaSupplier is Organisation supplier)
                {
                    if (!purchaseOrder.OrderedBy.ActiveSuppliers.Contains(supplier))
                    {
                        validation.AddError($"{purchaseOrder} {purchaseOrder.Meta.TakenViaSupplier} {ErrorMessages.PartyIsNotASupplier}");
                    }
                }

                if (purchaseOrder.TakenViaSubcontractor is Organisation subcontractor)
                {
                    if (!purchaseOrder.OrderedBy.ActiveSubContractors.Contains(subcontractor))
                    {
                        validation.AddError($"{purchaseOrder} {purchaseOrder.Meta.TakenViaSubcontractor} {ErrorMessages.PartyIsNotASubcontractor}");
                    }
                }

                validation.AssertExistsAtMostOne(purchaseOrder, purchaseOrder.Meta.TakenViaSupplier, purchaseOrder.Meta.TakenViaSubcontractor);
                validation.AssertAtLeastOne(purchaseOrder, purchaseOrder.Meta.TakenViaSupplier, purchaseOrder.Meta.TakenViaSubcontractor);

                if (!purchaseOrder.ExistShipToAddress)
                {
                    purchaseOrder.ShipToAddress = purchaseOrder.OrderedBy.ShippingAddress;
                }

                if (!purchaseOrder.ExistBillToContactMechanism)
                {
                    purchaseOrder.BillToContactMechanism = purchaseOrder.OrderedBy.ExistBillingAddress ? purchaseOrder.OrderedBy.BillingAddress : purchaseOrder.OrderedBy.GeneralCorrespondence;
                }

                if (!purchaseOrder.ExistTakenViaContactMechanism && purchaseOrder.ExistTakenViaSupplier)
                {
                    purchaseOrder.TakenViaContactMechanism = purchaseOrder.TakenViaSupplier.OrderAddress;
                }

                purchaseOrder.VatRegime ??= purchaseOrder.TakenViaSupplier?.VatRegime;
                purchaseOrder.IrpfRegime ??= purchaseOrder.TakenViaSupplier?.IrpfRegime;

                purchaseOrder.Locale = purchaseOrder.Strategy.Session.GetSingleton().DefaultLocale;

                var validOrderItems = purchaseOrder.PurchaseOrderItems.Where(v => v.IsValid).ToArray();
                purchaseOrder.ValidOrderItems = validOrderItems;

                var purchaseOrderShipmentStates = new PurchaseOrderShipmentStates(purchaseOrder.Strategy.Session);
                var purchaseOrderPaymentStates = new PurchaseOrderPaymentStates(purchaseOrder.Strategy.Session);
                var purchaseOrderItemStates = new PurchaseOrderItemStates(cycle.Session);

                // PurchaseOrder Shipment State
                if (validOrderItems.Any())
                {
                    if ((validOrderItems.Any(v => v.ExistPart) && validOrderItems.Where(v => v.ExistPart).All(v => v.PurchaseOrderItemShipmentState.IsReceived)) ||
                        (validOrderItems.Any(v => !v.ExistPart) && validOrderItems.Where(v => !v.ExistPart).All(v => v.PurchaseOrderItemShipmentState.IsReceived)))
                    {
                        purchaseOrder.PurchaseOrderShipmentState = purchaseOrderShipmentStates.Received;
                    }
                    else if (validOrderItems.All(v => v.PurchaseOrderItemShipmentState.IsNotReceived))
                    {
                        purchaseOrder.PurchaseOrderShipmentState = purchaseOrderShipmentStates.NotReceived;
                    }
                    else if (validOrderItems.All(v => v.PurchaseOrderItemShipmentState.IsNa))
                    {
                        purchaseOrder.PurchaseOrderShipmentState = purchaseOrderShipmentStates.Na;
                    }
                    else
                    {
                        purchaseOrder.PurchaseOrderShipmentState = purchaseOrderShipmentStates.PartiallyReceived;
                    }

                    // PurchaseOrder Payment State
                    if (validOrderItems.All(v => v.PurchaseOrderItemPaymentState.IsPaid))
                    {
                        purchaseOrder.PurchaseOrderPaymentState = purchaseOrderPaymentStates.Paid;
                    }
                    else if (validOrderItems.All(v => v.PurchaseOrderItemPaymentState.IsNotPaid))
                    {
                        purchaseOrder.PurchaseOrderPaymentState = purchaseOrderPaymentStates.NotPaid;
                    }
                    else
                    {
                        purchaseOrder.PurchaseOrderPaymentState = purchaseOrderPaymentStates.PartiallyPaid;
                    }

                    // PurchaseOrder OrderState
                    if (purchaseOrder.PurchaseOrderState.IsSent
                        && (purchaseOrder.PurchaseOrderShipmentState.IsReceived || purchaseOrder.PurchaseOrderShipmentState.IsNa))
                    {
                        purchaseOrder.PurchaseOrderState = new PurchaseOrderStates(purchaseOrder.Strategy.Session).Completed;
                    }

                    if (purchaseOrder.PurchaseOrderState.IsCompleted && purchaseOrder.PurchaseOrderPaymentState.IsPaid)
                    {
                        purchaseOrder.PurchaseOrderState = new PurchaseOrderStates(purchaseOrder.Strategy.Session).Finished;
                    }
                }

                // Derive Totals
                var quantityOrderedByPart = new Dictionary<Part, decimal>();
                var totalBasePriceByPart = new Dictionary<Part, decimal>();
                foreach (PurchaseOrderItem item in purchaseOrder.ValidOrderItems)
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

                purchaseOrder.TotalBasePrice = 0;
                purchaseOrder.TotalDiscount = 0;
                purchaseOrder.TotalSurcharge = 0;
                purchaseOrder.TotalVat = 0;
                purchaseOrder.TotalIrpf = 0;
                purchaseOrder.TotalExVat = 0;
                purchaseOrder.TotalExtraCharge = 0;
                purchaseOrder.TotalIncVat = 0;
                purchaseOrder.GrandTotal = 0;

                foreach (PurchaseOrderItem orderItem in purchaseOrder.ValidOrderItems)
                {
                    purchaseOrder.TotalBasePrice += orderItem.TotalBasePrice;
                    purchaseOrder.TotalDiscount += orderItem.TotalDiscount;
                    purchaseOrder.TotalSurcharge += orderItem.TotalSurcharge;
                    purchaseOrder.TotalVat += orderItem.TotalVat;
                    purchaseOrder.TotalIrpf += orderItem.TotalIrpf;
                    purchaseOrder.TotalExVat += orderItem.TotalExVat;
                    purchaseOrder.TotalIncVat += orderItem.TotalIncVat;
                    purchaseOrder.GrandTotal += orderItem.GrandTotal;
                }

                purchaseOrder.PreviousTakenViaSupplier = purchaseOrder.TakenViaSupplier;

                // Derive Workflow
                purchaseOrder.WorkItemDescription = $"PurchaseOrder: {purchaseOrder.OrderNumber} [{purchaseOrder.TakenViaSupplier?.PartyName}]";
                var openTasks = purchaseOrder.TasksWhereWorkItem.Where(v => !v.ExistDateClosed).ToArray();
                if (purchaseOrder.PurchaseOrderState.IsAwaitingApprovalLevel1)
                {
                    if (!openTasks.OfType<PurchaseOrderApprovalLevel1>().Any())
                    {
                        new PurchaseOrderApprovalLevel1Builder(purchaseOrder.Session()).WithPurchaseOrder(purchaseOrder).Build();
                    }
                }

                if (purchaseOrder.PurchaseOrderState.IsAwaitingApprovalLevel2)
                {
                    if (!openTasks.OfType<PurchaseOrderApprovalLevel2>().Any())
                    {
                        new PurchaseOrderApprovalLevel2Builder(purchaseOrder.Session()).WithPurchaseOrder(purchaseOrder).Build();
                    }
                }

                purchaseOrder.ResetPrintDocument();

                if (CanInvoice(purchaseOrder))
                {
                    purchaseOrder.RemoveDeniedPermission(new Permissions(purchaseOrder.Strategy.Session).Get(purchaseOrder.Meta.Class, purchaseOrder.Meta.Invoice, Operations.Execute));
                }
                else
                {
                    purchaseOrder.AddDeniedPermission(new Permissions(purchaseOrder.Strategy.Session).Get(purchaseOrder.Meta.Class, purchaseOrder.Meta.Invoice, Operations.Execute));
                }

                if (CanRevise(purchaseOrder))
                {
                    purchaseOrder.RemoveDeniedPermission(new Permissions(purchaseOrder.Strategy.Session).Get(purchaseOrder.Meta.Class, purchaseOrder.Meta.Revise, Operations.Execute));
                }
                else
                {
                    purchaseOrder.AddDeniedPermission(new Permissions(purchaseOrder.Strategy.Session).Get(purchaseOrder.Meta.Class, purchaseOrder.Meta.Revise, Operations.Execute));
                }

                if (purchaseOrder.IsReceivable)
                {
                    purchaseOrder.RemoveDeniedPermission(new Permissions(purchaseOrder.Strategy.Session).Get(purchaseOrder.Meta.Class, purchaseOrder.Meta.QuickReceive, Operations.Execute));
                }
                else
                {
                    purchaseOrder.AddDeniedPermission(new Permissions(purchaseOrder.Strategy.Session).Get(purchaseOrder.Meta.Class, purchaseOrder.Meta.QuickReceive, Operations.Execute));
                }

                var deletePermission = new Permissions(purchaseOrder.Strategy.Session).Get(purchaseOrder.Meta.ObjectType, purchaseOrder.Meta.Delete, Operations.Execute);
                if (IsDeletable(purchaseOrder))
                {
                    purchaseOrder.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    purchaseOrder.AddDeniedPermission(deletePermission);
                }

                if (!purchaseOrder.PurchaseOrderShipmentState.IsNotReceived && !purchaseOrder.PurchaseOrderShipmentState.IsNa)
                {
                    purchaseOrder.AddDeniedPermission(new Permissions(purchaseOrder.Strategy.Session).Get(purchaseOrder.Meta.Class, purchaseOrder.Meta.Reject, Operations.Execute));
                    purchaseOrder.AddDeniedPermission(new Permissions(purchaseOrder.Strategy.Session).Get(purchaseOrder.Meta.Class, purchaseOrder.Meta.Cancel, Operations.Execute));
                    purchaseOrder.AddDeniedPermission(new Permissions(purchaseOrder.Strategy.Session).Get(purchaseOrder.Meta.Class, purchaseOrder.Meta.QuickReceive, Operations.Execute));
                    purchaseOrder.AddDeniedPermission(new Permissions(purchaseOrder.Strategy.Session).Get(purchaseOrder.Meta.Class, purchaseOrder.Meta.Revise, Operations.Execute));
                    purchaseOrder.AddDeniedPermission(new Permissions(purchaseOrder.Strategy.Session).Get(purchaseOrder.Meta.Class, purchaseOrder.Meta.SetReadyForProcessing, Operations.Execute));

                    var deniablePermissionByOperandTypeId = new Dictionary<Guid, Permission>();

                    foreach (Permission permission in purchaseOrder.Session().Extent<Permission>())
                    {
                        if (permission.ConcreteClassPointer == purchaseOrder.Strategy.Class.Id && permission.Operation == Operations.Write)
                        {
                            deniablePermissionByOperandTypeId.Add(permission.OperandTypePointer, permission);
                        }
                    }

                    foreach (var keyValuePair in deniablePermissionByOperandTypeId)
                    {
                        purchaseOrder.AddDeniedPermission(keyValuePair.Value);
                    }
                }
            }
        }

    }
}
