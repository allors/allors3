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
    using Derivations.Rules;

    public class PurchaseOrderDeniedPermissionRule : Rule
    {
        public PurchaseOrderDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("23ec4c76-b156-406f-ad16-4b83a17db3c6")) =>
            this.Patterns = new Pattern[]
        {
            m.PurchaseOrder.RolePattern(v => v.TransitionalDeniedPermissions),
            m.PurchaseOrder.RolePattern(v => v.PurchaseOrderShipmentState),
            m.PurchaseOrder.AssociationPattern(v => v.WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrder),
            m.PurchaseOrder.AssociationPattern(v => v.PurchaseInvoicesWherePurchaseOrder),
            m.PurchaseOrder.AssociationPattern(v => v.SerialisedItemsWherePurchaseOrder),
            m.PurchaseOrderItem.RolePattern(v => v.PurchaseOrderItemState, v => v.PurchaseOrderWherePurchaseOrderItem),
            m.OrderItem.AssociationPattern(v => v.OrderItemBillingsWhereOrderItem, v => v.AsPurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem, m.PurchaseOrder),
            m.OrderItem.AssociationPattern(v => v.OrderShipmentsWhereOrderItem, v => v.AsPurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem, m.PurchaseOrder),
            m.OrderItem.AssociationPattern(v => v.OrderRequirementCommitmentsWhereOrderItem, v => v.AsPurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem, m.PurchaseOrder),
            m.OrderItem.AssociationPattern(v => v.WorkEffortsWhereOrderItemFulfillment, v => v.AsPurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem, m.PurchaseOrder),
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
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Invoice));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Invoice));
                }

                if (@this.CanRevise)
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Revise));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Revise));
                }

                if (@this.IsReceivable)
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.QuickReceive));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.QuickReceive));
                }

                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Delete);
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
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Reject));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Cancel));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.QuickReceive));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Revise));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.SetReadyForProcessing));

                    var deniablePermissionByOperandTypeId = new Dictionary<IOperandType, Permission>();

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
