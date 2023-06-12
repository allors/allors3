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
    using Resources;

    public class NonSerialisedInventoryItemQuantitiesRule : Rule
    {
        public NonSerialisedInventoryItemQuantitiesRule(MetaPopulation m) : base(m, new Guid("36bb6207-ff7d-4bc1-afaf-a2c12d649c1c")) =>
            this.Patterns = new Pattern[]
            {
                m.NonSerialisedInventoryItem.RolePattern(v => v.NonSerialisedInventoryItemState),
                m.InventoryItemTransaction.RolePattern(v => v.InventoryItem, v => v.InventoryItem, m.NonSerialisedInventoryItem),
                m.InventoryItemTransaction.RolePattern(v => v.Quantity, v => v.InventoryItem, m.NonSerialisedInventoryItem),
                m.PickList.RolePattern(v => v.PickListState, v => v.PickListItems.ObjectType.InventoryItem, m.NonSerialisedInventoryItem),
                m.PickListItem.RolePattern(v => v.QuantityPicked, v => v.InventoryItem, m.NonSerialisedInventoryItem),
                m.PurchaseOrderItem.RolePattern(v => v.QuantityOrdered, v => v.Part.ObjectType.InventoryItemsWherePart, m.NonSerialisedInventoryItem),
                m.PurchaseOrderItem.RolePattern(v => v.PurchaseOrderItemState, v => v.Part.ObjectType.InventoryItemsWherePart, m.NonSerialisedInventoryItem),
                m.InventoryItem.AssociationPattern(v => v.PickListItemsWhereInventoryItem, m.NonSerialisedInventoryItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            foreach (var @this in matches.Cast<NonSerialisedInventoryItem>())
            {
                @this.DeriveNonSerialisedInventoryItemQuantities(validation);
            }
        }
    }

    public static class NonSerialisedInventoryItemQuantitiesRuleExtensions
    {
        public static void DeriveNonSerialisedInventoryItemQuantities(this NonSerialisedInventoryItem @this, IValidation validation)
        {
            var settings = @this.Strategy.Transaction.GetSingleton().Settings;

            var quantityOnHand = @this.CalculateQuantityOnHand(settings);
            if (quantityOnHand != @this.QuantityOnHand)
            {
                @this.QuantityOnHand = quantityOnHand;
            }

            if (@this.QuantityOnHand < 0)
            {
                validation.AddError(@this, @this.Meta.QuantityOnHand, ErrorMessages.InvalidQuantity);
            }

            var quantityCommittedOut = @this.CalculateQuantityCommittedOut();
            if (quantityCommittedOut != @this.QuantityCommittedOut)
            {
                @this.QuantityCommittedOut = quantityCommittedOut;
            }

            var availableToPromise = @this.CalculateAvailableToPromise(settings);
            if (availableToPromise != @this.AvailableToPromise)
            {
                @this.AvailableToPromise = availableToPromise;
            }

            if (@this.ExistPart && @this.Part.ExistPurchaseOrderItemsWherePart)
            {
                // QuantityExpectedIn
                var quantityExpectedIn = 0M;

                foreach (var purchaseOrderItem in @this.Part.PurchaseOrderItemsWherePart)
                {
                    var facility = purchaseOrderItem.StoredInFacility;
                    if ((purchaseOrderItem.PurchaseOrderItemState.Equals(new PurchaseOrderItemStates(@this.Strategy.Transaction).InProcess)
                         || purchaseOrderItem.PurchaseOrderItemState.Equals(new PurchaseOrderItemStates(@this.Strategy.Transaction).Sent))
                        && @this.Facility.Equals(facility))
                    {
                        quantityExpectedIn += purchaseOrderItem.QuantityOrdered;
                        quantityExpectedIn -= purchaseOrderItem.QuantityReceived;
                    }
                }

                if (quantityExpectedIn != @this.QuantityExpectedIn)
                {
                    @this.QuantityExpectedIn = quantityExpectedIn;
                }
            }

            if (@this.ExistPreviousQuantityOnHand && @this.QuantityOnHand > @this.PreviousQuantityOnHand)
            {
                ReplenishSalesOrders(@this);
            }

            if (@this.ExistPreviousQuantityOnHand && @this.QuantityOnHand < @this.PreviousQuantityOnHand)
            {
                DepleteSalesOrders(@this);
            }

            if (@this.QuantityOnHand != @this.PreviousQuantityOnHand)
            {
                @this.PreviousQuantityOnHand = @this.QuantityOnHand;
            }
        }

        private static void ReplenishSalesOrders(NonSerialisedInventoryItem nonSerialisedInventoryItem)
        {
            var m = nonSerialisedInventoryItem.Strategy.Transaction.Database.Services.Get<MetaPopulation>();

            var salesOrderItems = nonSerialisedInventoryItem.Strategy.Transaction.Extent<SalesOrderItem>();
            salesOrderItems.Filter.AddEquals(m.SalesOrderItem.SalesOrderItemState, new SalesOrderItemStates(nonSerialisedInventoryItem.Strategy.Transaction).InProcess);
            salesOrderItems.AddSort(m.OrderItem.DerivedDeliveryDate, SortDirection.Ascending);
            var nonUnifiedGoods = nonSerialisedInventoryItem.Part.NonUnifiedGoodsWherePart.ToArray();
            var unifiedGood = nonSerialisedInventoryItem.Part as UnifiedGood;

            if (nonUnifiedGoods.Length > 0 || unifiedGood != null)
            {
                if (unifiedGood != null)
                {
                    salesOrderItems.Filter.AddEquals(m.SalesOrderItem.Product, unifiedGood);
                }

                if (nonUnifiedGoods.Length > 0)
                {
                    salesOrderItems.Filter.AddContainedIn(m.SalesOrderItem.Product, (IEnumerable<IObject>)nonUnifiedGoods);
                }

                salesOrderItems = nonSerialisedInventoryItem.Strategy.Transaction.Instantiate(salesOrderItems);

                var extra = nonSerialisedInventoryItem.QuantityOnHand - nonSerialisedInventoryItem.PreviousQuantityOnHand;

                foreach (SalesOrderItem salesOrderItem in salesOrderItems)
                {
                    if (extra > 0 && salesOrderItem.QuantityShortFalled > 0)
                    {
                        decimal diff;
                        if (extra >= salesOrderItem.QuantityShortFalled)
                        {
                            diff = salesOrderItem.QuantityShortFalled;
                        }
                        else
                        {
                            diff = extra;
                        }

                        extra -= diff;

                        // HACK: DerivedRoles
                        var salesOrderItemDerivedRoles = salesOrderItem;

                        salesOrderItemDerivedRoles.QuantityShortFalled -= diff;

                        // var inventoryAssignment = salesOrderItem.SalesOrderItemInventoryAssignmentsWhereSalesOrderItem.FirstOrDefault();
                        // if (inventoryAssignment != null)
                        // {
                        //    inventoryAssignment.Quantity += diff;
                        // }
                    }
                }
            }
        }

        private static void DepleteSalesOrders(NonSerialisedInventoryItem nonSerialisedInventoryItem)
        {
            var m = nonSerialisedInventoryItem.Strategy.Transaction.Database.Services.Get<MetaPopulation>();

            var salesOrderItems = nonSerialisedInventoryItem.Strategy.Transaction.Extent<SalesOrderItem>();
            salesOrderItems.Filter.AddEquals(m.SalesOrderItem.SalesOrderItemState, new SalesOrderItemStates(nonSerialisedInventoryItem.Strategy.Transaction).InProcess);
            salesOrderItems.Filter.AddExists(m.OrderItem.DerivedDeliveryDate);
            salesOrderItems.AddSort(m.OrderItem.DerivedDeliveryDate, SortDirection.Descending);

            salesOrderItems = nonSerialisedInventoryItem.Strategy.Transaction.Instantiate(salesOrderItems);

            var subtract = nonSerialisedInventoryItem.PreviousQuantityOnHand - nonSerialisedInventoryItem.QuantityOnHand;

            foreach (SalesOrderItem salesOrderItem in salesOrderItems)
            {
                if (subtract > 0 && salesOrderItem.QuantityRequestsShipping > 0)
                {
                    decimal diff;
                    if (subtract >= salesOrderItem.QuantityRequestsShipping)
                    {
                        diff = salesOrderItem.QuantityRequestsShipping;
                    }
                    else
                    {
                        diff = subtract;
                    }

                    subtract -= diff;

                    var inventoryAssignment = salesOrderItem.SalesOrderItemInventoryAssignments.FirstOrDefault();
                    if (inventoryAssignment != null)
                    {
                        inventoryAssignment.Quantity = diff;
                    }
                }
            }
        }
    }
}
