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

    public class SalesOrderCanInvoiceRule : Rule
    {
        public SalesOrderCanInvoiceRule(M m) : base(m, new Guid("18b732ae-c47a-4bd5-97cf-a41a69ec5005")) =>
            this.Patterns = new Pattern[]
        {
            // Do not listen for changes in Store.BillingProcess.

            new RolePattern(m.SalesOrder, m.SalesOrder.SalesOrderState),
            new RolePattern(m.SalesOrderItem, m.SalesOrderItem.SalesOrderItemState) { Steps =  new IPropertyType[] { m.SalesOrderItem.SalesOrderWhereSalesOrderItem } },
            new AssociationPattern(m.OrderItemBilling.OrderItem) { Steps =  new IPropertyType[] { m.SalesOrderItem.SalesOrderWhereSalesOrderItem }, OfType = m.SalesOrder.Class },
            new RolePattern(m.OrderItemBilling, m.OrderItemBilling.Amount) { Steps =  new IPropertyType[] { m.OrderItemBilling.OrderItem, m.SalesOrderItem.SalesOrderWhereSalesOrderItem }, OfType = m.SalesOrder.Class },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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

                        var leftToInvoice1 = (salesOrderItem.QuantityOrdered * salesOrderItem.UnitPrice) - amountAlreadyInvoiced1;

                        if (leftToInvoice1 > 0)
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
