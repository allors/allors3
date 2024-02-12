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

    public class SalesOrderStateRule : Rule
    {
        public SalesOrderStateRule(MetaPopulation m) : base(m, new Guid("1a17a901-7547-4c7f-bcf9-9c1d248efb4c")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.SalesOrderState),
                m.SalesOrder.RolePattern(v => v.SalesOrderItems),
                m.SalesOrderItem.RolePattern(v => v.DerivationTrigger, v => v.SalesOrderWhereSalesOrderItem),
                m.SalesOrderItem.RolePattern(v => v.SalesOrderItemState, v => v.SalesOrderWhereSalesOrderItem),
                m.SalesOrderItem.RolePattern(v => v.SalesOrderItemShipmentState, v => v.SalesOrderWhereSalesOrderItem),
                m.SalesOrderItem.RolePattern(v => v.SalesOrderItemPaymentState, v => v.SalesOrderWhereSalesOrderItem),
                m.SalesOrderItem.RolePattern(v => v.SalesOrderItemInvoiceState, v => v.SalesOrderWhereSalesOrderItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>())
            {
                @this.DeriveSalesOrderState(validation);
            }
        }
    }

    public static class SalesOrderStateRuleExtensions
    {
        public static void DeriveSalesOrderState(this SalesOrder @this, IValidation validation)
        {
            var transaction = @this.Strategy.Transaction;

            @this.ValidOrderItems = @this.SalesOrderItems.Where(v => v.IsValid).ToArray();
            var validOrderItems = @this.SalesOrderItems.Where(v => v.IsValid).ToArray();

            var salesOrderShipmentStates = new SalesOrderShipmentStates(@this.Strategy.Transaction);
            var salesOrderPaymentStates = new SalesOrderPaymentStates(@this.Strategy.Transaction);
            var salesOrderInvoiceStates = new SalesOrderInvoiceStates(@this.Strategy.Transaction);

            var salesOrderItemShipmentStates = new SalesOrderItemShipmentStates(transaction);
            var salesOrderItemPaymentStates = new SalesOrderItemPaymentStates(transaction);
            var salesOrderItemInvoiceStates = new SalesOrderItemInvoiceStates(transaction);

            // SalesOrder Shipment State
            if (validOrderItems.Any())
            {
                if (validOrderItems.All(v => v.SalesOrderItemShipmentState.IsNotApplicable))
                {
                    @this.SalesOrderShipmentState = salesOrderShipmentStates.NotApplicable;
                }
                else if (validOrderItems.All(v => v.SalesOrderItemShipmentState.IsShipped || v.SalesOrderItemShipmentState.IsNotApplicable))
                {
                    @this.SalesOrderShipmentState = salesOrderShipmentStates.Shipped;
                }
                else if (validOrderItems.All(v => v.SalesOrderItemShipmentState.IsNotShipped || v.SalesOrderItemShipmentState.IsNotApplicable))
                {
                    @this.SalesOrderShipmentState = salesOrderShipmentStates.NotShipped;
                }
                else if (validOrderItems.Any(v => v.SalesOrderItemShipmentState.IsInProgress))
                {
                    @this.SalesOrderShipmentState = salesOrderShipmentStates.InProgress;
                }
                else
                {
                    @this.SalesOrderShipmentState = salesOrderShipmentStates.PartiallyShipped;
                }

                // SalesOrder Payment State
                if (validOrderItems.All(v => v.SalesOrderItemPaymentState.IsPaid))
                {
                    @this.SalesOrderPaymentState = salesOrderPaymentStates.Paid;
                }
                else if (validOrderItems.All(v => v.SalesOrderItemPaymentState.IsNotPaid))
                {
                    @this.SalesOrderPaymentState = salesOrderPaymentStates.NotPaid;
                }
                else
                {
                    @this.SalesOrderPaymentState = salesOrderPaymentStates.PartiallyPaid;
                }

                // SalesOrder Invoice State
                if (validOrderItems.All(v => v.SalesOrderItemInvoiceState.IsInvoiced))
                {
                    @this.SalesOrderInvoiceState = salesOrderInvoiceStates.Invoiced;
                }
                else if (validOrderItems.All(v => v.SalesOrderItemInvoiceState.IsNotInvoiced))
                {
                    @this.SalesOrderInvoiceState = salesOrderInvoiceStates.NotInvoiced;
                }
                else
                {
                    @this.SalesOrderInvoiceState = salesOrderInvoiceStates.PartiallyInvoiced;
                }

                // SalesOrder OrderState
                if ((@this.SalesOrderShipmentState.IsShipped || @this.SalesOrderShipmentState.IsNotApplicable)
                    && @this.SalesOrderInvoiceState.IsInvoiced
                    && !@this.SalesOrderState.IsCompleted
                    && !@this.SalesOrderState.IsFinished)
                {
                    @this.SalesOrderState = new SalesOrderStates(@this.Strategy.Transaction).Completed;
                }

                if (@this.SalesOrderState.IsCompleted && @this.SalesOrderPaymentState.IsPaid)
                {
                    @this.SalesOrderState = new SalesOrderStates(@this.Strategy.Transaction).Finished;
                }
            }

            if (@this.SalesOrderState.IsInProcess
                && (!@this.ExistLastSalesOrderState || !@this.LastSalesOrderState.IsInProcess)
                && @this.TakenBy.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(@this.Transaction()).SalesOrderAccept))
            {
                foreach (SalesOrderItem item in @this.ValidOrderItems.Where(v => ((SalesOrderItem)v).ExistSerialisedItem))
                {
                    if (item.ExistNextSerialisedItemAvailability)
                    {
                        item.SerialisedItem.SerialisedItemAvailability = item.NextSerialisedItemAvailability;

                        if (item.NextSerialisedItemAvailability.Equals(new SerialisedItemAvailabilities(@this.Transaction()).Sold))
                        {
                            item.SerialisedItem.OwnedBy = @this.ShipToCustomer;
                            item.SerialisedItem.Ownership = new Ownerships(@this.Transaction()).ThirdParty;
                        }

                        if (item.NextSerialisedItemAvailability.Equals(new SerialisedItemAvailabilities(@this.Transaction()).InRent))
                        {
                            item.SerialisedItem.RentedBy = @this.ShipToCustomer;
                        }
                    }

                    item.SerialisedItem.AvailableForSale = false;
                }
            }
        }
    }
}
