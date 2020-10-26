// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class NonSerialisedInventoryItemDerivation : DomainDerivation
    {
        public NonSerialisedInventoryItemDerivation(M m) : base(m, new Guid("DDB383AD-3B4C-43BE-8F30-7E3A8D16F6BE")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.NonSerialisedInventoryItem.Class),
                new CreatedPattern(this.M.InventoryItemTransaction.Class)
                {
                    Steps = new IPropertyType[]
                    {
                        this.M.InventoryItemTransaction.Part,
                        this.M.Part.InventoryItemsWherePart
                    },
                    OfType = this.M.NonSerialisedInventoryItem.Class
                },
                new ChangedPattern(this.M.InventoryItemTransaction.InventoryItem)
                {
                    Steps = new IPropertyType[]
                    {
                        this.M.InventoryItemTransaction.InventoryItem,
                    },
                },
                new ChangedPattern(this.M.SalesOrderItem.ReservedFromNonSerialisedInventoryItem)
                {
                    Steps = new IPropertyType[]
                    {
                        this.M.SalesOrderItem.ReservedFromNonSerialisedInventoryItem
                    }
                },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var nonSerialisedInventoryItem in matches.Cast<NonSerialisedInventoryItem>())
            {
                var settings = nonSerialisedInventoryItem.Strategy.Session.GetSingleton().Settings;

                if (!nonSerialisedInventoryItem.ExistName)
                {
                    nonSerialisedInventoryItem.Name = $"{nonSerialisedInventoryItem.Part?.Name} at {nonSerialisedInventoryItem.Facility?.Name} with state {nonSerialisedInventoryItem.NonSerialisedInventoryItemState?.Name}";
                }

                if (nonSerialisedInventoryItem.ExistPart)
                {
                    nonSerialisedInventoryItem.UnitOfMeasure = nonSerialisedInventoryItem.Part.UnitOfMeasure;
                }

                this.QuantityOnHand(nonSerialisedInventoryItem, settings);

                this.QuantityCommittedOut(nonSerialisedInventoryItem);

                this.AvaibleToPromise(nonSerialisedInventoryItem);

                this.QuantityExpectedIn(nonSerialisedInventoryItem);
            }
        }

        private void QuantityOnHand(NonSerialisedInventoryItem nonSerialisedInventoryItem, Settings settings)
        {
            var quantityOnHand = 0M;

            if (!settings.InventoryStrategy.OnHandNonSerialisedStates.Contains(nonSerialisedInventoryItem.NonSerialisedInventoryItemState))
            {
                nonSerialisedInventoryItem.QuantityOnHand = 0;
            }

            foreach (InventoryItemTransaction inventoryTransaction in nonSerialisedInventoryItem.InventoryItemTransactionsWhereInventoryItem)
            {
                var reason = inventoryTransaction.Reason;

                if (reason.IncreasesQuantityOnHand == true)
                {
                    quantityOnHand += inventoryTransaction.Quantity;
                }
                else if (reason.IncreasesQuantityOnHand == false)
                {
                    quantityOnHand -= inventoryTransaction.Quantity;
                }
            }

            foreach (PickListItem pickListItem in nonSerialisedInventoryItem.PickListItemsWhereInventoryItem)
            {
                if (pickListItem.PickListWherePickListItem.PickListState.Equals(new PickListStates(nonSerialisedInventoryItem.Strategy.Session).Picked))
                {
                    foreach (ItemIssuance itemIssuance in pickListItem.ItemIssuancesWherePickListItem)
                    {
                        if (!itemIssuance.ShipmentItem.ShipmentItemState.Shipped)
                        {
                            quantityOnHand -= pickListItem.QuantityPicked;
                        }
                    }
                }
            }

            nonSerialisedInventoryItem.QuantityOnHand = quantityOnHand;
        }

        private void QuantityCommittedOut(NonSerialisedInventoryItem nonSerialisedInventoryItem)
        {
            var quantityCommittedOut = 0M;

            foreach (InventoryItemTransaction inventoryTransaction in nonSerialisedInventoryItem.InventoryItemTransactionsWhereInventoryItem)
            {
                var reason = inventoryTransaction.Reason;

                if (reason.IncreasesQuantityCommittedOut == true)
                {
                    quantityCommittedOut += inventoryTransaction.Quantity;
                }
                else if (reason.IncreasesQuantityCommittedOut == false)
                {
                    quantityCommittedOut -= inventoryTransaction.Quantity;
                }
            }

            foreach (PickListItem pickListItem in nonSerialisedInventoryItem.PickListItemsWhereInventoryItem)
            {
                if (pickListItem.PickListWherePickListItem.PickListState.Equals(new PickListStates(nonSerialisedInventoryItem.Strategy.Session).Picked))
                {
                    foreach (ItemIssuance itemIssuance in pickListItem.ItemIssuancesWherePickListItem)
                    {
                        if (!itemIssuance.ShipmentItem.ShipmentItemState.Shipped)
                        {
                            quantityCommittedOut -= pickListItem.QuantityPicked;
                        }
                    }
                }
            }

            if (quantityCommittedOut < 0)
            {
                quantityCommittedOut = 0;
            }

            nonSerialisedInventoryItem.QuantityCommittedOut = quantityCommittedOut;
        }

        private void QuantityExpectedIn(NonSerialisedInventoryItem nonSerialisedInventoryItem)
        {
            var quantityExpectedIn = 0M;

            foreach (PurchaseOrderItem purchaseOrderItem in nonSerialisedInventoryItem.Part.PurchaseOrderItemsWherePart)
            {
                var facility = purchaseOrderItem.PurchaseOrderWherePurchaseOrderItem.StoredInFacility;
                if ((purchaseOrderItem.PurchaseOrderItemState.Equals(new PurchaseOrderItemStates(nonSerialisedInventoryItem.Strategy.Session).InProcess)
                     || purchaseOrderItem.PurchaseOrderItemState.Equals(new PurchaseOrderItemStates(nonSerialisedInventoryItem.Strategy.Session).Sent))
                    && nonSerialisedInventoryItem.Facility.Equals(facility))
                {
                    quantityExpectedIn += purchaseOrderItem.QuantityOrdered;
                    quantityExpectedIn -= purchaseOrderItem.QuantityReceived;
                }
            }

            nonSerialisedInventoryItem.QuantityExpectedIn = quantityExpectedIn;

            if (nonSerialisedInventoryItem.ExistPreviousQuantityOnHand && nonSerialisedInventoryItem.QuantityOnHand > nonSerialisedInventoryItem.PreviousQuantityOnHand)
            {
                this.ReplenishSalesOrders(nonSerialisedInventoryItem);
            }

            if (nonSerialisedInventoryItem.ExistPreviousQuantityOnHand && nonSerialisedInventoryItem.QuantityOnHand < nonSerialisedInventoryItem.PreviousQuantityOnHand)
            {
                this.DepleteSalesOrders(nonSerialisedInventoryItem);
            }

            nonSerialisedInventoryItem.PreviousQuantityOnHand = nonSerialisedInventoryItem.QuantityOnHand;
        }

        private void AvaibleToPromise(NonSerialisedInventoryItem nonSerialisedInventoryItem)
        {
            var availableToPromise = nonSerialisedInventoryItem.QuantityOnHand - nonSerialisedInventoryItem.QuantityCommittedOut;

            if (availableToPromise < 0)
            {
                availableToPromise = 0;
            }

            nonSerialisedInventoryItem.AvailableToPromise = availableToPromise;
        }

        private void ReplenishSalesOrders(NonSerialisedInventoryItem nonSerialisedInventoryItem)
        {

            var salesOrderItems = nonSerialisedInventoryItem.Strategy.Session.Extent<SalesOrderItem>();
            salesOrderItems.Filter.AddEquals(this.M.SalesOrderItem.SalesOrderItemState, new SalesOrderItemStates(nonSerialisedInventoryItem.Strategy.Session).InProcess);
            salesOrderItems.AddSort(this.M.OrderItem.DeliveryDate, SortDirection.Ascending);
            var nonUnifiedGoods = nonSerialisedInventoryItem.Part.NonUnifiedGoodsWherePart;
            var unifiedGood = nonSerialisedInventoryItem.Part as UnifiedGood;

            if (nonUnifiedGoods.Count > 0 || unifiedGood != null)
            {
                if (unifiedGood != null)
                {
                    salesOrderItems.Filter.AddEquals(this.M.SalesOrderItem.Product, unifiedGood);
                }

                if (nonUnifiedGoods.Count > 0)
                {
                    salesOrderItems.Filter.AddContainedIn(this.M.SalesOrderItem.Product, (Extent)nonUnifiedGoods);
                }

                salesOrderItems = nonSerialisedInventoryItem.Strategy.Session.Instantiate(salesOrderItems);

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

        private void DepleteSalesOrders(NonSerialisedInventoryItem nonSerialisedInventoryItem)
        {
            var salesOrderItems = nonSerialisedInventoryItem.Strategy.Session.Extent<SalesOrderItem>();
            salesOrderItems.Filter.AddEquals(this.M.SalesOrderItem.SalesOrderItemState, new SalesOrderItemStates(nonSerialisedInventoryItem.Strategy.Session).InProcess);
            salesOrderItems.Filter.AddExists(this.M.OrderItem.DeliveryDate);
            salesOrderItems.AddSort(this.M.OrderItem.DeliveryDate, SortDirection.Descending);

            salesOrderItems = nonSerialisedInventoryItem.Strategy.Session.Instantiate(salesOrderItems);

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
