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

    public class NonSerialisedInventoryItemDerivation : DomainDerivation
    {
        public NonSerialisedInventoryItemDerivation(M m) : base(m, new Guid("DDB383AD-3B4C-43BE-8F30-7E3A8D16F6BE")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.InventoryItemTransaction.Quantity) { Steps = new IPropertyType[] {this.M.InventoryItemTransaction.InventoryItem }, OfType = m.NonSerialisedInventoryItem.Class },
                new ChangedPattern(this.M.SalesOrderItem.ReservedFromNonSerialisedInventoryItem) { Steps = new IPropertyType[] {this.M.SalesOrderItem.ReservedFromNonSerialisedInventoryItem } },
            };

        
                //if (@this.PurchaseOrderItemState.Equals(states.InProcess) ||
                //    @this.PurchaseOrderItemState.Equals(states.Cancelled) ||
                //    @this.PurchaseOrderItemState.Equals(states.Rejected))
                //{
                //    NonSerialisedInventoryItem inventoryItem = null;

                //    if (@this.ExistPart)
                //    {
                //        var inventoryItems = @this.Part.InventoryItemsWherePart;
                //        inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, @this.PurchaseOrderWherePurchaseOrderItem.StoredInFacility);
                //        inventoryItem = inventoryItems.First as NonSerialisedInventoryItem;
                //    }

                //    if (@this.PurchaseOrderItemState.Equals(new PurchaseOrderItemStates(@this.Strategy.Session).InProcess))
                //    {
                //        if (!@this.ExistPreviousQuantity || !@this.QuantityOrdered.Equals(@this.PreviousQuantity))
                //        {
                //            // TODO: Remove OnDerive
                //            //inventoryItem?.OnDerive(x => x.WithDerivation(derivation));
                //        }
                //    }

                //    if (@this.PurchaseOrderItemState.Equals(new PurchaseOrderItemStates(@this.Strategy.Session).Cancelled) ||
                //        @this.PurchaseOrderItemState.Equals(new PurchaseOrderItemStates(@this.Strategy.Session).Rejected))
                //    {
                //        // TODO: Remove OnDerive
                //        //inventoryItem?.OnDerive(x => x.WithDerivation(derivation));
                //    }
                //}


        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<NonSerialisedInventoryItem>())
            {
                var settings = @this.Strategy.Session.GetSingleton().Settings;

                if (!@this.ExistName)
                {
                    @this.Name = $"{@this.Part?.Name} at {@this.Facility?.Name} with state {@this.NonSerialisedInventoryItemState?.Name}";
                }

                if (@this.ExistPart)
                {
                    @this.UnitOfMeasure = @this.Part.UnitOfMeasure;
                }

                this.QuantityOnHand(@this, settings);

                this.QuantityCommittedOut(@this);

                this.AvaibleToPromise(@this);

                this.QuantityExpectedIn(@this);
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

            if (quantityOnHand != nonSerialisedInventoryItem.QuantityOnHand)
            {
                nonSerialisedInventoryItem.QuantityOnHand = quantityOnHand;
            }
        }

        private void QuantityCommittedOut(NonSerialisedInventoryItem nonSerialisedInventoryItem)
        {
            var quantityCommittedOut = 0M;

            foreach (InventoryItemTransaction inventoryItemTransaction in nonSerialisedInventoryItem.InventoryItemTransactionsWhereInventoryItem)
            {
                var reason = inventoryItemTransaction.Reason;

                if (reason.IncreasesQuantityCommittedOut == true)
                {
                    quantityCommittedOut += inventoryItemTransaction.Quantity;
                }
                else if (reason.IncreasesQuantityCommittedOut == false)
                {
                    quantityCommittedOut -= inventoryItemTransaction.Quantity;
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

            if (quantityCommittedOut != nonSerialisedInventoryItem.QuantityCommittedOut)
            {
                nonSerialisedInventoryItem.QuantityCommittedOut = quantityCommittedOut;
            }
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

            if (quantityExpectedIn != nonSerialisedInventoryItem.QuantityExpectedIn)
            {
                nonSerialisedInventoryItem.QuantityExpectedIn = quantityExpectedIn;
            }

            if (nonSerialisedInventoryItem.ExistPreviousQuantityOnHand && nonSerialisedInventoryItem.QuantityOnHand > nonSerialisedInventoryItem.PreviousQuantityOnHand)
            {
                this.ReplenishSalesOrders(nonSerialisedInventoryItem);
            }

            if (nonSerialisedInventoryItem.ExistPreviousQuantityOnHand && nonSerialisedInventoryItem.QuantityOnHand < nonSerialisedInventoryItem.PreviousQuantityOnHand)
            {
                this.DepleteSalesOrders(nonSerialisedInventoryItem);
            }

            if (nonSerialisedInventoryItem.QuantityOnHand != nonSerialisedInventoryItem.PreviousQuantityOnHand)
            {
                nonSerialisedInventoryItem.PreviousQuantityOnHand = nonSerialisedInventoryItem.QuantityOnHand;
            }
        }

        private void AvaibleToPromise(NonSerialisedInventoryItem nonSerialisedInventoryItem)
        {
            var availableToPromise = nonSerialisedInventoryItem.QuantityOnHand - nonSerialisedInventoryItem.QuantityCommittedOut;

            if (availableToPromise < 0)
            {
                availableToPromise = 0;
            }

            if (availableToPromise != nonSerialisedInventoryItem.AvailableToPromise)
            {
                nonSerialisedInventoryItem.AvailableToPromise = availableToPromise;
            }
        }

        private void ReplenishSalesOrders(NonSerialisedInventoryItem nonSerialisedInventoryItem)
        {

            var salesOrderItems = nonSerialisedInventoryItem.Strategy.Session.Extent<SalesOrderItem>();
            salesOrderItems.Filter.AddEquals(this.M.SalesOrderItem.SalesOrderItemState, new SalesOrderItemStates(nonSerialisedInventoryItem.Strategy.Session).InProcess);
            salesOrderItems.AddSort(this.M.OrderItem.DerivedDeliveryDate, SortDirection.Ascending);
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
            salesOrderItems.Filter.AddExists(this.M.OrderItem.DerivedDeliveryDate);
            salesOrderItems.AddSort(this.M.OrderItem.DerivedDeliveryDate, SortDirection.Descending);

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
