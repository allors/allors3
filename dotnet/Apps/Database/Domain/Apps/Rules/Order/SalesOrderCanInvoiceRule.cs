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

    public class SalesOrderCanInvoiceRule : Rule
    {
        public SalesOrderCanInvoiceRule(MetaPopulation m) : base(m, new Guid("18b732ae-c47a-4bd5-97cf-a41a69ec5005")) =>
            this.Patterns = new Pattern[]
        {
            // Do not listen for changes in Store.BillingProcess.

            m.SalesOrder.RolePattern(v => v.SalesOrderState),
            m.SalesOrderItem.RolePattern(v => v.SalesOrderItemState, v => v.SalesOrderWhereSalesOrderItem),
            m.OrderItem.AssociationPattern(v => v.OrderItemBillingsWhereOrderItem, v => v.AsSalesOrderItem.SalesOrderWhereSalesOrderItem , m.SalesOrder),
            m.OrderItemBilling.RolePattern(v => v.Amount, v => v.OrderItem.ObjectType.AsSalesOrderItem.SalesOrderWhereSalesOrderItem.ObjectType),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>())
            {
                var validOrderItems = @this.SalesOrderItems.Where(v => v.IsValid).ToArray();

                if (@this.ExistSalesOrderState
                    && @this.SalesOrderState.IsInProcess
                    && Equals(@this.Store.BillingProcess, new BillingProcesses(@this.Strategy.Transaction).BillingForOrderItems))
                {
                    @this.CanInvoice = false;

                    foreach (var salesOrderItem in validOrderItems)
                    {
                        var amountAlreadyInvoiced1 = salesOrderItem.OrderItemBillingsWhereOrderItem.Sum(v => v.Amount);

                        var leftToInvoice1 = salesOrderItem.QuantityOrdered * salesOrderItem.UnitPrice - amountAlreadyInvoiced1;

                        if (leftToInvoice1 > 0 || salesOrderItem.UnitPrice == 0)
                        {
                            @this.CanInvoice = true;
                        }
                    }
                }
                else
                {
                    @this.CanInvoice = false;
                }
            }
        }
    }
}
