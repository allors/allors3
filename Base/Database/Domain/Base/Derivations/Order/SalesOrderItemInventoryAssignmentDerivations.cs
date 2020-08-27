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
        public class SalesOrderItemInventoryAssignmentCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdSalesOrderItemInventoryAssignment = changeSet.Created.Select(session.Instantiate).OfType<SalesOrderItemInventoryAssignment>();

                foreach (var salesOrderItemInventoryAssignment in createdSalesOrderItemInventoryAssignment)
                {
                    var salesOrderItem = salesOrderItemInventoryAssignment.SalesOrderItemWhereSalesOrderItemInventoryAssignment;
                    var state = salesOrderItem.SalesOrderItemState;
                    var inventoryItemChanged = salesOrderItemInventoryAssignment.ExistCurrentVersion && (!Equals(salesOrderItemInventoryAssignment.CurrentVersion.InventoryItem, salesOrderItemInventoryAssignment.InventoryItem));

                    foreach (InventoryTransactionReason createReason in state.InventoryTransactionReasonsToCreate)
                    {
                        SyncInventoryTransactions(salesOrderItemInventoryAssignment, salesOrderItemInventoryAssignment.InventoryItem, salesOrderItemInventoryAssignment.Quantity, createReason, false);
                    }

                    foreach (InventoryTransactionReason cancelReason in state.InventoryTransactionReasonsToCancel)
                    {
                        SyncInventoryTransactions(salesOrderItemInventoryAssignment, salesOrderItemInventoryAssignment.InventoryItem, salesOrderItemInventoryAssignment.Quantity, cancelReason, true);
                    }

                    if (inventoryItemChanged)
                    {
                        // CurrentVersion is Previous Version until PostDerive
                        var previousInventoryItem = salesOrderItemInventoryAssignment.CurrentVersion.InventoryItem;
                        var previousQuantity = salesOrderItemInventoryAssignment.CurrentVersion.Quantity;
                        state = salesOrderItem.PreviousSalesOrderItemState ?? salesOrderItem.SalesOrderItemState;

                        foreach (InventoryTransactionReason createReason in state.InventoryTransactionReasonsToCreate)
                        {
                            SyncInventoryTransactions(salesOrderItemInventoryAssignment, previousInventoryItem, previousQuantity, createReason, true);
                        }

                        foreach (InventoryTransactionReason cancelReason in state.InventoryTransactionReasonsToCancel)
                        {
                            SyncInventoryTransactions(salesOrderItemInventoryAssignment, previousInventoryItem, previousQuantity, cancelReason, true);
                        }
                    }
                }

                void SyncInventoryTransactions(SalesOrderItemInventoryAssignment salesOrderItemInventoryAssignment, InventoryItem inventoryItem, decimal initialQuantity, InventoryTransactionReason reason, bool isCancellation)
                {
                    var adjustmentQuantity = 0M;
                    var existingQuantity = 0M;
                    var matchingTransactions = salesOrderItemInventoryAssignment.InventoryItemTransactions.Where(t => t.Reason.Equals(reason) && t.Part.Equals(inventoryItem.Part) && t.InventoryItem.Equals(inventoryItem)).ToArray();

                    if (matchingTransactions.Length > 0)
                    {
                        existingQuantity = matchingTransactions.Sum(t => t.Quantity);
                    }

                    if (isCancellation)
                    {
                        adjustmentQuantity = 0 - existingQuantity;
                    }
                    else
                    {
                        adjustmentQuantity = initialQuantity - existingQuantity;
                    }

                    if (adjustmentQuantity != 0)
                    {
                        var newTransaction = new InventoryItemTransactionBuilder(salesOrderItemInventoryAssignment.Session())
                            .WithPart(inventoryItem.Part)
                            .WithQuantity(adjustmentQuantity)
                            .WithReason(reason)
                            .WithFacility(inventoryItem.Facility)
                            .Build();

                        if (inventoryItem is SerialisedInventoryItem serialisedInventoryItem)
                        {
                            newTransaction.SerialisedItem = serialisedInventoryItem.SerialisedItem;
                        }

                        newTransaction.InventoryItem = inventoryItem;
                        salesOrderItemInventoryAssignment.AddInventoryItemTransaction(newTransaction);
                    }
                }
            }
        }

        public static void SalesOrderItemInventoryAssignmentRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("9be3a95a-6732-4fbc-b9a6-6e0436d5c4bc")] = new SalesOrderItemInventoryAssignmentCreationDerivation();
        }
    }
}
