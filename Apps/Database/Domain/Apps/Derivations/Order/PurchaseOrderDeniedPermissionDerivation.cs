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
            new ChangedPattern(m.PurchaseOrder.TransitionalDeniedPermissions),
            new ChangedPattern(m.PurchaseOrder.PurchaseOrderShipmentState),
            new ChangedPattern(m.WorkEffortPurchaseOrderItemAssignment.PurchaseOrder) { Steps =  new IPropertyType[] {m.WorkEffortPurchaseOrderItemAssignment.PurchaseOrder} },
            new ChangedPattern(m.PurchaseInvoice.PurchaseOrders) { Steps =  new IPropertyType[] {m.PurchaseInvoice.PurchaseOrders} },
            new ChangedPattern(m.SerialisedItem.PurchaseOrder) { Steps =  new IPropertyType[] {m.SerialisedItem.PurchaseOrder} },
            new ChangedPattern(m.PurchaseOrderItem.PurchaseOrderItemState) { Steps =  new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem} },
            new ChangedPattern(m.OrderItemBilling.OrderItem) { Steps =  new IPropertyType[] {m.OrderItemBilling.OrderItem, m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem }, OfType = m.PurchaseOrder.Class },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                if (@this.CanInvoice)
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Invoice));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Session).Get(@this.Meta.Class, @this.Meta.Invoice));
                }

                if (@this.CanRevise)
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
