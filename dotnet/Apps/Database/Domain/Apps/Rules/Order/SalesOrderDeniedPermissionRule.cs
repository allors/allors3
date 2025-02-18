// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class SalesOrderDeniedPermissionRule : Rule
    {
        public SalesOrderDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("6e383218-1d0f-41bb-83ed-7f6f3bf551ca")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesOrder.RolePattern(v => v.TransitionalRevocations),
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

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesOrder>())
            {
                @this.Revocations = @this.TransitionalRevocations;

                var deleteRevocation = new Revocations(@this.Strategy.Transaction).SalesOrderDeleteRevocation;
                var invoiceRevocation = new Revocations(@this.Strategy.Transaction).SalesOrderInvoiceRevocation;
                var shipRevocation = new Revocations(@this.Strategy.Transaction).SalesOrderShipRevocation;
                var stateRevocation = new Revocations(@this.Strategy.Transaction).SalesOrderStateRevocation;
                var writeRevocation = new Revocations(@this.Strategy.Transaction).SalesOrderWriteRevocation;

                if (@this.CanShip)
                {
                    @this.RemoveRevocation(shipRevocation);
                }
                else
                {
                    @this.AddRevocation(shipRevocation);
                }

                if (@this.CanInvoice)
                {
                    @this.RemoveRevocation(invoiceRevocation);
                }
                else
                {
                    @this.AddRevocation(invoiceRevocation);
                }

                if (@this.SalesOrderInvoiceState.IsPartiallyInvoiced
                    || @this.SalesOrderInvoiceState.IsInvoiced
                    || @this.SalesOrderShipmentState.IsInProgress
                    || @this.SalesOrderShipmentState.IsPartiallyShipped
                    || @this.SalesOrderShipmentState.IsShipped)
                {
                    @this.AddRevocation(stateRevocation);
                    @this.AddRevocation(writeRevocation);
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
