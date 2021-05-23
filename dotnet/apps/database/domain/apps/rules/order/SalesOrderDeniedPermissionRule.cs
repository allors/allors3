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

    public class SalesOrderDeniedPermissionRule : Rule
    {
        public SalesOrderDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("6e383218-1d0f-41bb-83ed-7f6f3bf551ca")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesOrder.RolePattern(v => v.TransitionalDeniedPermissions),
            m.SalesOrder.RolePattern(v => v.CanShip),
            m.SalesOrder.RolePattern(v => v.CanInvoice),
            m.SalesOrder.RolePattern(v => v.Quote),
            m.SalesOrder.RolePattern(v => v.SalesOrderInvoiceState),
            m.SalesOrder.RolePattern(v => v.SalesOrderShipmentState),
            m.SalesOrderItem.RolePattern(v => v.SalesOrderItemState, v => v.SalesOrderWhereSalesOrderItem),
            m.OrderItem.AssociationPattern(v => v.OrderItemBillingsWhereOrderItem, v => v.AsSalesOrderItem.SalesOrderWhereSalesOrderItem),
            m.OrderItem.AssociationPattern(v => v.OrderShipmentsWhereOrderItem, v => v.AsSalesOrderItem.SalesOrderWhereSalesOrderItem, m.SalesOrder),
            m.OrderItem.AssociationPattern(v => v.OrderRequirementCommitmentsWhereOrderItem, v => v.AsSalesOrderItem.SalesOrderWhereSalesOrderItem, m.SalesOrder),
            m.OrderItem.AssociationPattern(v => v.WorkEffortsWhereOrderItemFulfillment, v => v.AsSalesOrderItem.SalesOrderWhereSalesOrderItem, m.SalesOrder),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesOrder>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;

                if (@this.CanShip)
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Ship));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Ship));
                }

                if (@this.CanInvoice)
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Invoice));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Invoice));
                }

                if (!@this.SalesOrderInvoiceState.IsNotInvoiced || !@this.SalesOrderShipmentState.IsNotShipped)
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.SetReadyForPosting));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Post));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Reopen));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Approve));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Hold));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Continue));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Accept));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Revise));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Complete));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Reject));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Cancel));

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

                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Delete);
                if (@this.IsDeletable)
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }
            }
        }
    }
}
