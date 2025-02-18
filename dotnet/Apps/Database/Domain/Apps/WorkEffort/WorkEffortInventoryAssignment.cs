// <copyright file="WorkEffortInventoryAssignment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;
    using Database.Derivations;
    using Resources;

    public partial class WorkEffortInventoryAssignment
    {
        public void AppsDelete(DeletableDelete method)
        {
            var transaction = this.strategy.Transaction;

            foreach (var deletable in this.AllVersions)
            {
                deletable.Strategy.Delete();
            }

            // TODO: Avoid creating a Derivation
            var derivation = this.Strategy.Transaction.Database.Services.Get<IDerivationService>().CreateDerivation(transaction);
            this.SyncInventoryTransactions(derivation, this.InventoryItem, this.Quantity, new InventoryTransactionReasons(transaction).Consumption, true);

            this.Assignment.ResetPrintDocument();
        }

        public void SyncInventoryTransactions(IDerivation derivation, InventoryItem inventoryItem, decimal initialQuantity, InventoryTransactionReason reason, bool isCancellation)
        {
            // TODO: Move sync to new derivations  

            var adjustmentQuantity = 0M;
            var existingQuantity = 0M;
            var matchingTransactions = this.InventoryItemTransactions
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
                    derivation.Validation.AddError(this, this.M.NonSerialisedInventoryItem.QuantityOnHand, ErrorMessages.InsufficientStock);
                }
            }

            if (adjustmentQuantity != 0)
            {
                var transaction = new InventoryItemTransactionBuilder(this.Transaction())
                    .WithPart(inventoryItem.Part)
                    .WithFacility(inventoryItem.Facility)
                    .WithQuantity(adjustmentQuantity)
                    .WithCostInApplicationCurrency(inventoryItem.Part.PartWeightedAverage.AverageCostInApplicationCurrency)
                    .WithReason(reason)
                    .Build();

                // this object (WorkEffortInventoryAssignment) is about to be deleted. Retain the workeffort number
                transaction.WorkEffortNumber = this.WorkEffortNumber;
            }
        }
    }
}
