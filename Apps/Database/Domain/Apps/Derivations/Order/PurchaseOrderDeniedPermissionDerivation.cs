// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class PurchaseOrderDeniedPermissionDerivation : DomainDerivation
    {
        public PurchaseOrderDeniedPermissionDerivation(M m) : base(m, new Guid("23ec4c76-b156-406f-ad16-4b83a17db3c6")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(this.M.PurchaseOrder.TransitionalDeniedPermissions),
            new ChangedPattern(this.M.PurchaseOrder.DerivationTrigger),
            new ChangedPattern(this.M.PurchaseOrder.PurchaseOrderShipmentState),
            new ChangedPattern(this.M.WorkEffortPurchaseOrderItemAssignment.PurchaseOrder) { Steps =  new IPropertyType[] {m.WorkEffortPurchaseOrderItemAssignment.PurchaseOrder} },
            new ChangedPattern(this.M.PurchaseInvoice.PurchaseOrders) { Steps =  new IPropertyType[] {m.PurchaseInvoice.PurchaseOrders} },
            new ChangedPattern(this.M.PurchaseOrderItem.PurchaseOrderItemState) { Steps =  new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem} },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            bool IsDeletable(PurchaseOrder purchaseOrder) => (purchaseOrder.PurchaseOrderState.Equals(new PurchaseOrderStates(purchaseOrder.Strategy.Session).Created)
                 || purchaseOrder.PurchaseOrderState.Equals(new PurchaseOrderStates(purchaseOrder.Strategy.Session).Cancelled)
                 || purchaseOrder.PurchaseOrderState.Equals(new PurchaseOrderStates(purchaseOrder.Strategy.Session).Rejected))
             && !purchaseOrder.ExistPurchaseInvoicesWherePurchaseOrder
             && !purchaseOrder.ExistSerialisedItemsWherePurchaseOrder
             && !purchaseOrder.ExistWorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrder
             && purchaseOrder.PurchaseOrderItems.All(v => v.IsDeletable);

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

            foreach (var @this in matches.Cast<PurchaseOrder>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                if (CanInvoice(@this))
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Invoice));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Invoice));
                }

                if (CanRevise(@this))
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Revise));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Revise));
                }

                if (@this.IsReceivable)
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.QuickReceive));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.QuickReceive));
                }

                var deletePermission = new Permissions(@this.Strategy.Session).Get(@this.Meta.ObjectType, @this.Meta.Delete);
                if (IsDeletable(@this))
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }

                if (!@this.PurchaseOrderShipmentState.IsNotReceived && !@this.PurchaseOrderShipmentState.IsNa)
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Reject));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Cancel));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.QuickReceive));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Revise));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.SetReadyForProcessing));

                    var deniablePermissionByOperandTypeId = new Dictionary<OperandType, Permission>();

                    foreach (Permission permission in @this.Session().Extent<Permission>())
                    {
                        if (permission.ClassPointer == @this.Strategy.Class.Id && permission.Operation == Operations.Write)
                        {
                            deniablePermissionByOperandTypeId.Add(permission.OperandType, permission);
                        }
                    }

                    foreach (var keyValuePair in deniablePermissionByOperandTypeId)
                    {
                        @this.AddDeniedPermission(keyValuePair.Value);
                    }
                }
            }
        }
    }
}
