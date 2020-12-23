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

    public class SalesOrderItemInventoryAssignmentDerivation : DomainDerivation
    {
        public SalesOrderItemInventoryAssignmentDerivation(M m) : base(m, new Guid("2B36132A-5557-4FBD-8611-F80302E8550C")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.SalesOrderItemInventoryAssignment.InventoryItemTransactions),
                new ChangedPattern(m.SalesOrderItemInventoryAssignment.Quantity),
                new ChangedPattern(m.SalesOrderItem.SalesOrderItemState) { Steps = new IPropertyType[] {m.SalesOrderItem.SalesOrderItemInventoryAssignments} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SalesOrderItemInventoryAssignment>())
            {
                var salesOrderItem = @this.SalesOrderItemWhereSalesOrderItemInventoryAssignment;
                var state = salesOrderItem.SalesOrderItemState;
                var inventoryItemChanged = @this.ExistCurrentVersion && (!Equals(@this.CurrentVersion.InventoryItem, @this.InventoryItem));

                foreach (InventoryTransactionReason createReason in state.InventoryTransactionReasonsToCreate)
                {
                    SyncInventoryTransactions(@this, @this.InventoryItem, @this.Quantity, createReason, false);
                }

                foreach (InventoryTransactionReason cancelReason in state.InventoryTransactionReasonsToCancel)
                {
                    SyncInventoryTransactions(@this, @this.InventoryItem, @this.Quantity, cancelReason, true);
                }

                if (inventoryItemChanged)
                {
                    // CurrentVersion is Previous Version until PostDerive
                    var previousInventoryItem = @this.CurrentVersion.InventoryItem;
                    var previousQuantity = @this.CurrentVersion.Quantity;
                    state = salesOrderItem.PreviousSalesOrderItemState ?? salesOrderItem.SalesOrderItemState;

                    foreach (InventoryTransactionReason createReason in state.InventoryTransactionReasonsToCreate)
                    {
                        SyncInventoryTransactions(@this, previousInventoryItem, previousQuantity, createReason, true);
                    }

                    foreach (InventoryTransactionReason cancelReason in state.InventoryTransactionReasonsToCancel)
                    {
                        SyncInventoryTransactions(@this, previousInventoryItem, previousQuantity, cancelReason, true);
                    }
                }

                if (!salesOrderItem.IsValid
                    && salesOrderItem.WasValid
                    && salesOrderItem.ExistReservedFromNonSerialisedInventoryItem
                    && salesOrderItem.ExistQuantityCommittedOut)
                {
                    var quantity = 0 - salesOrderItem.QuantityCommittedOut;
                    if (quantity != @this.Quantity)
                    {
                        @this.Quantity = 0 - salesOrderItem.QuantityCommittedOut;
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
                        .WithInventoryItem(inventoryItem)
                        .WithPart(inventoryItem.Part)
                        .WithQuantity(adjustmentQuantity)
                        .WithReason(reason)
                        .WithFacility(inventoryItem.Facility)
                        .Build();

                    if (inventoryItem is SerialisedInventoryItem serialisedInventoryItem)
                    {
                        newTransaction.SerialisedItem = serialisedInventoryItem.SerialisedItem;
                    }

                    salesOrderItemInventoryAssignment.AddInventoryItemTransaction(newTransaction);
                }
            }
        }
    }
}
