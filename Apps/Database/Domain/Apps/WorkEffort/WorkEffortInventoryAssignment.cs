// <copyright file="WorkEffortInventoryAssignment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;
    using Resources;

    public partial class WorkEffortInventoryAssignment
    {
        public void AppsDelete(DeletableDelete method)
        {
            var session = this.strategy.Session;
            var derivation = new Derivations.Default.Derivation(session);
            this.SyncInventoryTransactions(derivation, this.InventoryItem, this.Quantity, new InventoryTransactionReasons(session).Consumption, true);
        }

        public void AppsCalculateBillableQuantity(WorkEffortInventoryAssignmentCalculateBillableQuantity method)
        {
            if (!method.Result.HasValue)
            {
                this.DerivedBillableQuantity = this.AssignedBillableQuantity ?? this.Quantity;

                method.Result = true;
            }
        }

        public void AppsCalculatePurchasePrice(WorkEffortInventoryAssignmentCalculatePurchasePrice method)
        {
            if (!method.Result.HasValue)
            {
                this.CostOfGoodsSold = this.Quantity * this.InventoryItem.Part.PartWeightedAverage.AverageCost;

                method.Result = true;
            }
        }

        public void AppsCalculateSellingPrice(WorkEffortInventoryAssignmentCalculateSellingPrice method)
        {
            if (!method.Result.HasValue)
            {
                if (this.AssignedUnitSellingPrice.HasValue)
                {
                    this.UnitSellingPrice = this.AssignedUnitSellingPrice.Value;
                }
                else
                {
                    var part = this.InventoryItem.Part;

                    var currentPriceComponents = this.Assignment.TakenBy?.PriceComponentsWherePricedBy
                        .Where(v => v.FromDate <= this.Assignment.ScheduledStart && (!v.ExistThroughDate || v.ThroughDate >= this.Assignment.ScheduledStart))
                        .ToArray();

                    var currentPartPriceComponents = part.GetPriceComponents(currentPriceComponents);

                    var price = currentPartPriceComponents.OfType<BasePrice>().Max(v => v.Price);
                    this.UnitSellingPrice = price ?? 0M;
                }

                method.Result = true;
            }
        }

        public void AppsDelegateAccess(DelegatedAccessControlledObjectDelegateAccess method)
        {
            if (method.SecurityTokens == null)
            {
                method.SecurityTokens = this.Assignment?.SecurityTokens.ToArray();
            }

            if (method.DeniedPermissions == null)
            {
                method.DeniedPermissions = this.Assignment?.DeniedPermissions.ToArray();
            }
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
                this.AddInventoryItemTransaction(new InventoryItemTransactionBuilder(this.Session())
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
