
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

    public class WorkTaskRule : Rule
    {
        public WorkTaskRule(MetaPopulation m) : base(m, new Guid("88da27ac-99cb-4a21-b9be-37628f1239d6")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffort.AssociationPattern(v => v.WorkEffortInventoryAssignmentsWhereAssignment),
            m.WorkTask.RolePattern(v => v.WorkEffortState),
            m.WorkEffortInventoryAssignment.RolePattern(v => v.Quantity, v => v.Assignment),
            m.WorkEffortInventoryAssignment.RolePattern(v => v.InventoryItem, v => v.Assignment),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkTask>())
            {
                foreach (var inventoryAssignment in @this.WorkEffortInventoryAssignmentsWhereAssignment)
                {
                    foreach (var createReason in @this.WorkEffortState.InventoryTransactionReasonsToCreate)
                    {
                        SyncInventoryTransactions(validation, inventoryAssignment, inventoryAssignment.InventoryItem, inventoryAssignment.Quantity, createReason, false);
                    }

                    foreach (var cancelReason in @this.WorkEffortState.InventoryTransactionReasonsToCancel)
                    {
                        SyncInventoryTransactions(validation,inventoryAssignment, inventoryAssignment.InventoryItem, inventoryAssignment.Quantity, cancelReason, true);
                    }
                }

                static void SyncInventoryTransactions(IValidation validation,WorkEffortInventoryAssignment workEffortInventoryAssignment, InventoryItem inventoryItem, decimal initialQuantity, InventoryTransactionReason reason, bool isCancellation)
                {
                    var adjustmentQuantity = 0M;
                    var existingQuantity = 0M;
                    var matchingTransactions = workEffortInventoryAssignment.InventoryItemTransactions
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
                            validation.AddError(workEffortInventoryAssignment, workEffortInventoryAssignment.M.NonSerialisedInventoryItem.QuantityOnHand, ErrorMessages.InsufficientStock);
                        }
                    }

                    if (adjustmentQuantity != 0)
                    {
                        workEffortInventoryAssignment.AddInventoryItemTransaction(new InventoryItemTransactionBuilder(workEffortInventoryAssignment.Transaction())
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
    }
}
