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

    public class SalesOrderDeniedPermissionDerivation : DomainDerivation
    {
        public SalesOrderDeniedPermissionDerivation(M m) : base(m, new Guid("6e383218-1d0f-41bb-83ed-7f6f3bf551ca")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.SalesOrder.TransitionalDeniedPermissions),
            new RolePattern(m.SalesOrder.CanShip),
            new RolePattern(m.SalesOrder.CanInvoice),
            new RolePattern(m.SalesOrder.Quote),
            new RolePattern(m.SalesOrder.SalesOrderInvoiceState),
            new RolePattern(m.SalesOrder.SalesOrderShipmentState),
            new RolePattern(m.SalesOrderItem.SalesOrderItemState) { Steps = new IPropertyType[] {m.SalesOrderItem.SalesOrderWhereSalesOrderItem } },
            new AssociationPattern(m.OrderItemBilling.OrderItem) { Steps =  new IPropertyType[] { m.SalesOrderItem.SalesOrderWhereSalesOrderItem }, OfType = m.SalesOrder.Class },
            new AssociationPattern(m.OrderShipment.OrderItem) { Steps =  new IPropertyType[] { m.SalesOrderItem.SalesOrderWhereSalesOrderItem }, OfType = m.SalesOrder.Class },
            new AssociationPattern(m.OrderRequirementCommitment.OrderItem) { Steps =  new IPropertyType[] { m.SalesOrderItem.SalesOrderWhereSalesOrderItem }, OfType = m.SalesOrder.Class },
            new AssociationPattern(m.WorkEffort.OrderItemFulfillment) { Steps =  new IPropertyType[] { m.SalesOrderItem.SalesOrderWhereSalesOrderItem }, OfType = m.SalesOrder.Class },
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
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Ship));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Ship));
                }

                if (@this.CanInvoice)
                {
                    @this.RemoveDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Invoice));
                }
                else
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Invoice));
                }

                if (!@this.SalesOrderInvoiceState.IsNotInvoiced || !@this.SalesOrderShipmentState.IsNotShipped)
                {
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.SetReadyForPosting));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Post));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Reopen));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Approve));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Hold));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Continue));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Accept));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Revise));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Complete));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Reject));
                    @this.AddDeniedPermission(new Permissions(@this.Strategy.Transaction).Get(@this.Meta.Class, @this.Meta.Cancel));

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

                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta.ObjectType, @this.Meta.Delete);
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
