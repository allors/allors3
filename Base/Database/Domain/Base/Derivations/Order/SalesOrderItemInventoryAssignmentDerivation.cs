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

    public class SalesOrderItemInventoryAssignmentDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("2B36132A-5557-4FBD-8611-F80302E8550C");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.SalesOrderItemInventoryAssignment.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var salesOrderItemInventoryAssignment in matches.Cast<SalesOrderItemInventoryAssignment>())
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
}
