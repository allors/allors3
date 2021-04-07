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
    using Resources;

    public class WorkEffortInventoryAssignmentSyncInventoryTransactionsRule : Rule
    {
        public WorkEffortInventoryAssignmentSyncInventoryTransactionsRule(MetaPopulation m) : base(m, new Guid("cd533d3e-922c-4938-a12d-cfacd6c3b9d9")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.WorkEffortInventoryAssignment, m.WorkEffortInventoryAssignment.InventoryItem),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortInventoryAssignment>())
            {
                if (@this.ExistCurrentVersion
                    && !Equals(@this.CurrentVersion.InventoryItem, @this.InventoryItem))
                {
                    // CurrentVersion is updated on PostDerive
                    var previousInventoryItem = @this.CurrentVersion.InventoryItem;
                    var previousQuantity = @this.CurrentVersion.Quantity;
                    var state = @this.CurrentVersion.Assignment.PreviousWorkEffortState ??
                            @this.CurrentVersion.Assignment.WorkEffortState;

                    foreach (InventoryTransactionReason createReason in state.InventoryTransactionReasonsToCreate)
                    {
                        this.SyncInventoryTransactions(@this, validation, previousInventoryItem, previousQuantity, createReason, true);
                    }

                    foreach (InventoryTransactionReason cancelReason in state.InventoryTransactionReasonsToCancel)
                    {
                        this.SyncInventoryTransactions(@this, validation, previousInventoryItem, previousQuantity, cancelReason, true);
                    }
                }
            }
        }

        public void SyncInventoryTransactions(WorkEffortInventoryAssignment @this,IDomainValidation validation, InventoryItem inventoryItem, decimal initialQuantity, InventoryTransactionReason reason, bool isCancellation)
        {
            // TODO: Move sync to new derivations

            var adjustmentQuantity = 0M;
            var existingQuantity = 0M;
            var matchingTransactions = @this.InventoryItemTransactions
                .Where(t => t.Reason.Equals(reason) && t.Part.Equals(inventoryItem.Part)).ToArray();

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

                if (inventoryItem is NonSerialisedInventoryItem nonserialisedInventoryItem && nonserialisedInventoryItem.QuantityOnHand < adjustmentQuantity)
                {
                    validation.AddError($"{@this}, {@this.M.NonSerialisedInventoryItem.QuantityOnHand}, {ErrorMessages.InsufficientStock}");
                }
            }

            if (adjustmentQuantity != 0)
            {
                @this.AddInventoryItemTransaction(new InventoryItemTransactionBuilder(@this.Transaction())
                    .WithPart(inventoryItem.Part)
                    .WithFacility(inventoryItem.Facility)
                    .WithQuantity(adjustmentQuantity)
                    .WithCost(inventoryItem.Part.PartWeightedAverage.AverageCost)
                    .WithReason(reason)
                    .Build());
            }
        }
    }
}
