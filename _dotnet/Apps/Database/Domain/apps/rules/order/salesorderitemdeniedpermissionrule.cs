// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class SalesOrderItemDeniedPermissionRule : Rule
    {
        public SalesOrderItemDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("84ff5d0c-c15d-426d-a019-ad8ab0bdbcf2")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesOrderItem.RolePattern(v => v.TransitionalRevocations),
            m.SalesOrderItem.RolePattern(v => v.SalesOrderItemInvoiceState),
            m.SalesOrderItem.RolePattern(v => v.SalesOrderItemShipmentState),
            m.OrderItem.AssociationPattern(v => v.OrderItemBillingsWhereOrderItem, m.SalesOrderItem),
            m.OrderItem.AssociationPattern(v => v.OrderShipmentsWhereOrderItem, m.SalesOrderItem),
            m.OrderItem.AssociationPattern(v => v.OrderRequirementCommitmentsWhereOrderItem, m.SalesOrderItem),
            m.OrderItem.AssociationPattern(v => v.WorkEffortsWhereOrderItemFulfillment, m.SalesOrderItem),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesOrderItem>())
            {
                @this.Revocations = @this.TransitionalRevocations;

                if (!@this.SalesOrderItemInvoiceState.IsNotInvoiced || !@this.SalesOrderItemShipmentState.IsNotShipped)
                {
                    var deniablePermissionByOperandTypeId = new Dictionary<IOperandType, Permission>();

                    foreach (Permission permission in @this.Transaction().Extent<Permission>())
                    {
                        if (permission.ClassPointer == @this.Strategy.Class.Id
                            && (permission.Operation == Operations.Write || permission.Operation == Operations.Execute))
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
