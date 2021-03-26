// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class PurchaseOrderDeniedPermissionDerivation : DomainDerivation
    {
        public PurchaseOrderDeniedPermissionDerivation(M m) : base(m, new Guid("23ec4c76-b156-406f-ad16-4b83a17db3c6")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.PurchaseOrder.TransitionalDeniedPermissions),
            new RolePattern(m.PurchaseOrder.PurchaseOrderShipmentState),
            new AssociationPattern(m.WorkEffortPurchaseOrderItemAssignment.PurchaseOrder),
            new AssociationPattern(m.PurchaseInvoice.PurchaseOrders),
            new AssociationPattern(m.SerialisedItem.PurchaseOrder),
            new RolePattern(m.PurchaseOrderItem.PurchaseOrderItemState) { Steps =  new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem} },
            new AssociationPattern(m.OrderItemBilling.OrderItem) { Steps =  new IPropertyType[] { m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem }, OfType = m.PurchaseOrder.Class },
            new AssociationPattern(m.OrderShipment.OrderItem) { Steps =  new IPropertyType[] { m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem }, OfType = m.PurchaseOrder.Class },
            new AssociationPattern(m.OrderRequirementCommitment.OrderItem) { Steps =  new IPropertyType[] { m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem }, OfType = m.PurchaseOrder.Class },
            new AssociationPattern(m.WorkEffort.OrderItemFulfillment) { Steps =  new IPropertyType[] { m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem }, OfType = m.PurchaseOrder.Class },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                if (@this.CanInvoice)
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Invoice));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Invoice));
                }

                if (@this.CanRevise)
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Revise));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Revise));
                }

                if (@this.IsReceivable)
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.QuickReceive));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.QuickReceive));
                }

                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta.ObjectType, @this.Meta.Delete);
                if (@this.IsDeletable)
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }

                if (!@this.PurchaseOrderShipmentState.IsNotReceived && !@this.PurchaseOrderShipmentState.IsNa)
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Reject));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Cancel));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.QuickReceive));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Revise));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.SetReadyForProcessing));

                    var deniablePermissionByOperandTypeId = new Dictionary<OperandType, Permission>();

                    foreach (Permission permission in @this.Transaction().Extent<Permission>())
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
