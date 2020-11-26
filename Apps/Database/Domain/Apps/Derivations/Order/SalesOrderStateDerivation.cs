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

    public class SalesOrderStateDerivation : DomainDerivation
    {
        public SalesOrderStateDerivation(M m) : base(m, new Guid("1a17a901-7547-4c7f-bcf9-9c1d248efb4c")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.SalesOrder.SalesOrderState),
                new ChangedPattern(this.M.SalesOrder.SalesOrderItems),
                new ChangedPattern(this.M.SalesOrderItem.DerivationTrigger) { Steps =  new IPropertyType[] {m.SalesOrderItem.SalesOrderWhereSalesOrderItem } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<SalesOrder>())
            {
                @this.ValidOrderItems = @this.SalesOrderItems.Where(v => v.IsValid).ToArray();
                var validOrderItems = @this.SalesOrderItems.Where(v => v.IsValid).ToArray();

                var salesOrderShipmentStates = new SalesOrderShipmentStates(@this.Strategy.Session);
                var salesOrderPaymentStates = new SalesOrderPaymentStates(@this.Strategy.Session);
                var salesOrderInvoiceStates = new SalesOrderInvoiceStates(@this.Strategy.Session);

                var salesOrderItemShipmentStates = new SalesOrderItemShipmentStates(session);
                var salesOrderItemPaymentStates = new SalesOrderItemPaymentStates(session);
                var salesOrderItemInvoiceStates = new SalesOrderItemInvoiceStates(session);

                // SalesOrder Shipment State
                if (validOrderItems.Any())
                {
                    if (validOrderItems.All(v => v.SalesOrderItemShipmentState.Shipped))
                    {
                        @this.SalesOrderShipmentState = salesOrderShipmentStates.Shipped;
                    }
                    else if (validOrderItems.All(v => v.SalesOrderItemShipmentState.NotShipped))
                    {
                        @this.SalesOrderShipmentState = salesOrderShipmentStates.NotShipped;
                    }
                    else if (validOrderItems.Any(v => v.SalesOrderItemShipmentState.InProgress))
                    {
                        @this.SalesOrderShipmentState = salesOrderShipmentStates.InProgress;
                    }
                    else
                    {
                        @this.SalesOrderShipmentState = salesOrderShipmentStates.PartiallyShipped;
                    }

                    // SalesOrder Payment State
                    if (validOrderItems.All(v => v.SalesOrderItemPaymentState.Paid))
                    {
                        @this.SalesOrderPaymentState = salesOrderPaymentStates.Paid;
                    }
                    else if (validOrderItems.All(v => v.SalesOrderItemPaymentState.NotPaid))
                    {
                        @this.SalesOrderPaymentState = salesOrderPaymentStates.NotPaid;
                    }
                    else
                    {
                        @this.SalesOrderPaymentState = salesOrderPaymentStates.PartiallyPaid;
                    }

                    // SalesOrder Invoice State
                    if (validOrderItems.All(v => v.SalesOrderItemInvoiceState.Invoiced))
                    {
                        @this.SalesOrderInvoiceState = salesOrderInvoiceStates.Invoiced;
                    }
                    else if (validOrderItems.All(v => v.SalesOrderItemInvoiceState.NotInvoiced))
                    {
                        @this.SalesOrderInvoiceState = salesOrderInvoiceStates.NotInvoiced;
                    }
                    else
                    {
                        @this.SalesOrderInvoiceState = salesOrderInvoiceStates.PartiallyInvoiced;
                    }

                    // SalesOrder OrderState
                    if (@this.SalesOrderShipmentState.Shipped && @this.SalesOrderInvoiceState.Invoiced)
                    {
                        @this.SalesOrderState = new SalesOrderStates(@this.Strategy.Session).Completed;
                    }

                    if (@this.SalesOrderState.IsCompleted && @this.SalesOrderPaymentState.Paid)
                    {
                        @this.SalesOrderState = new SalesOrderStates(@this.Strategy.Session).Finished;
                    }
                }
            }
        }
    }
}
