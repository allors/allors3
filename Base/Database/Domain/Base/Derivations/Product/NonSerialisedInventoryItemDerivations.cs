// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class NonSerialisedInventoryItemCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdNonSerialisedInventoryItems = changeSet.Created.Select(session.Instantiate).OfType<NonSerialisedInventoryItem>();

                changeSet.AssociationsByRoleType.TryGetValue(M.NonSerialisedInventoryItem.InventoryItemTransactionsWhereInventoryItem.RoleType, out var changedNonSerialisedInventoryItem);
                var nonSerialisedInventoryItemWhereInventoryItemTransactionsWhereInventoryItemChanged = changedNonSerialisedInventoryItem?.Select(session.Instantiate).OfType<NonSerialisedInventoryItem>();

                var createdInventoryItemTransaction = changeSet.Created.Select(session.Instantiate).OfType<InventoryItemTransaction>();

                changeSet.AssociationsByRoleType.TryGetValue(M.SalesOrderItem.ReservedFromNonSerialisedInventoryItem, out var changeSalesOrderItem);
                var salesOrderItemWhereReservedFromNonSerialisedInventoryItemChanged = changeSalesOrderItem?.Select(session.Instantiate).OfType<SalesOrderItem>();

                foreach (var nonSerialisedInventoryItem in createdNonSerialisedInventoryItems)
                {
                    var settings = nonSerialisedInventoryItem.Strategy.Session.GetSingleton().Settings;

                    if (!nonSerialisedInventoryItem.ExistName)
                    {
                        nonSerialisedInventoryItem.Name = $"{nonSerialisedInventoryItem.Part?.Name} at {nonSerialisedInventoryItem.Facility?.Name} with state {nonSerialisedInventoryItem.NonSerialisedInventoryItemState?.Name}";
                    }

                    BaseOnDeriveUnitOfMeasure(nonSerialisedInventoryItem);

                    // QuantityOnHand
                    ValidateQuantityOnHand(nonSerialisedInventoryItem, settings);

                    // quantityCommittedOut
                    ValidateQuantityCommittedOut(nonSerialisedInventoryItem);

                    // AvailableToPromise
                    ValidateAvaibleToPromise(nonSerialisedInventoryItem);

                    // QuantityExpectedIn
                    ValidateQuantityExpectedIn(nonSerialisedInventoryItem);

                }

                if (nonSerialisedInventoryItemWhereInventoryItemTransactionsWhereInventoryItemChanged?.Any() == true)
                {
                    foreach (var nonSerialisedInventoryItem in nonSerialisedInventoryItemWhereInventoryItemTransactionsWhereInventoryItemChanged)
                    {
                        ValidateQuantityOnHand(nonSerialisedInventoryItem, nonSerialisedInventoryItem.Strategy.Session.GetSingleton().Settings);
                        ValidateQuantityCommittedOut(nonSerialisedInventoryItem);
                        ValidateAvaibleToPromise(nonSerialisedInventoryItem);
                        ValidateQuantityExpectedIn(nonSerialisedInventoryItem);
                    }
                }

                if (salesOrderItemWhereReservedFromNonSerialisedInventoryItemChanged?.Any() == true)
                {
                    foreach (var salesOrderItem in salesOrderItemWhereReservedFromNonSerialisedInventoryItemChanged)
                    {
                        var nonSerialisedInventoryItem = (NonSerialisedInventoryItem)salesOrderItem.ReservedFromNonSerialisedInventoryItem;
                        ValidateQuantityOnHand(nonSerialisedInventoryItem, nonSerialisedInventoryItem.Strategy.Session.GetSingleton().Settings);
                        ValidateQuantityCommittedOut(nonSerialisedInventoryItem);
                        ValidateAvaibleToPromise(nonSerialisedInventoryItem);
                        ValidateQuantityExpectedIn(nonSerialisedInventoryItem);
                    }
                }

                foreach (var inventoryItemTransaction in createdInventoryItemTransaction)
                {
                    foreach (InventoryItem inventoryItem in inventoryItemTransaction.Part.InventoryItemsWherePart)
                    {
                        if (inventoryItem is NonSerialisedInventoryItem nonSerialisedInventoryItem)
                        {
                            ValidateQuantityOnHand(nonSerialisedInventoryItem, nonSerialisedInventoryItem.Strategy.Session.GetSingleton().Settings);
                            ValidateQuantityCommittedOut(nonSerialisedInventoryItem);
                            ValidateAvaibleToPromise(nonSerialisedInventoryItem);
                            ValidateQuantityExpectedIn(nonSerialisedInventoryItem);
                        }
                    }
                }

                static void ValidateQuantityOnHand(NonSerialisedInventoryItem nonSerialisedInventoryItem, Settings settings)
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

                static void ValidateQuantityCommittedOut(NonSerialisedInventoryItem nonSerialisedInventoryItem)
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

                void ValidateQuantityExpectedIn(NonSerialisedInventoryItem nonSerialisedInventoryItem)
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
                        BaseReplenishSalesOrders(nonSerialisedInventoryItem);
                    }

                    if (nonSerialisedInventoryItem.ExistPreviousQuantityOnHand && nonSerialisedInventoryItem.QuantityOnHand < nonSerialisedInventoryItem.PreviousQuantityOnHand)
                    {
                        BaseDepleteSalesOrders(nonSerialisedInventoryItem);
                    }

                    nonSerialisedInventoryItem.PreviousQuantityOnHand = nonSerialisedInventoryItem.QuantityOnHand;
                }

                static void ValidateAvaibleToPromise(NonSerialisedInventoryItem nonSerialisedInventoryItem)
                {
                    var availableToPromise = nonSerialisedInventoryItem.QuantityOnHand - nonSerialisedInventoryItem.QuantityCommittedOut;

                    if (availableToPromise < 0)
                    {
                        availableToPromise = 0;
                    }

                    nonSerialisedInventoryItem.AvailableToPromise = availableToPromise;
                }
            }

            public void BaseReplenishSalesOrders(NonSerialisedInventoryItem nonSerialisedInventoryItem)
            {

                var salesOrderItems = nonSerialisedInventoryItem.Strategy.Session.Extent<SalesOrderItem>();
                salesOrderItems.Filter.AddEquals(M.SalesOrderItem.SalesOrderItemState, new SalesOrderItemStates(nonSerialisedInventoryItem.Strategy.Session).InProcess);
                salesOrderItems.AddSort(M.OrderItem.DeliveryDate, SortDirection.Ascending);
                var nonUnifiedGoods = nonSerialisedInventoryItem.Part.NonUnifiedGoodsWherePart;
                var unifiedGood = nonSerialisedInventoryItem.Part as UnifiedGood;

                if (nonUnifiedGoods.Count > 0 || unifiedGood != null)
                {
                    if (unifiedGood != null)
                    {
                        salesOrderItems.Filter.AddEquals(M.SalesOrderItem.Product, unifiedGood);
                    }

                    if (nonUnifiedGoods.Count > 0)
                    {
                        salesOrderItems.Filter.AddContainedIn(M.SalesOrderItem.Product, (Extent)nonUnifiedGoods);
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

            public void BaseDepleteSalesOrders(NonSerialisedInventoryItem nonSerialisedInventoryItem)
            {
                var salesOrderItems = nonSerialisedInventoryItem.Strategy.Session.Extent<SalesOrderItem>();
                salesOrderItems.Filter.AddEquals(M.SalesOrderItem.SalesOrderItemState, new SalesOrderItemStates(nonSerialisedInventoryItem.Strategy.Session).InProcess);
                salesOrderItems.Filter.AddExists(M.OrderItem.DeliveryDate);
                salesOrderItems.AddSort(M.OrderItem.DeliveryDate, SortDirection.Descending);

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

            public void BaseOnDeriveUnitOfMeasure(NonSerialisedInventoryItem nonSerialisedInventoryItem)
            {
                if (nonSerialisedInventoryItem.ExistPart)
                {
                    nonSerialisedInventoryItem.UnitOfMeasure = nonSerialisedInventoryItem.Part.UnitOfMeasure;
                }
            }
        }

        public static void NonSerialisedInventoryItemRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("c1aa25e7-ef16-42bd-a56a-e69e73eddd03")] = new NonSerialisedInventoryItemCreationDerivation();
        }
    }
}
