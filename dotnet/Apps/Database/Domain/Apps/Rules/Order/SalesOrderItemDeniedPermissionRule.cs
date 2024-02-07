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

                var deleteRevocation = new Revocations(@this.Strategy.Transaction).SalesOrderItemDeleteRevocation;
                var writeRevocation = new Revocations(@this.Strategy.Transaction).SalesOrderItemWriteRevocation;
                var executeRevocation = new Revocations(@this.Strategy.Transaction).SalesOrderItemExecuteRevocation;

                if (!@this.SalesOrderItemInvoiceState.IsNotInvoiced
                    || @this.SalesOrderItemShipmentState.IsInProgress
                    || @this.SalesOrderItemShipmentState.IsPartiallyShipped
                    || @this.SalesOrderItemShipmentState.IsShipped)
                {
                    @this.AddRevocation(writeRevocation);
                    @this.AddRevocation(executeRevocation);
                }

                if (@this.IsDeletable)
                {
                    @this.RemoveRevocation(deleteRevocation);
                }
                else
                {
                    @this.AddRevocation(deleteRevocation);
                }
            }
        }
    }
}
