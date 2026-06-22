// <copyright file="PartWeightedAverageRule.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class PartWeightedAverageRule : Rule
    {
        public PartWeightedAverageRule(MetaPopulation m) : base(m, new Guid("b7f3e9a1-2c4d-4e6f-8a1b-3d5c7e9f0a2b")) =>
            this.Patterns = new Pattern[]
            {
                m.InventoryItemTransaction.RolePattern(v => v.Quantity),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            // The weighted average cost of a part only changes when stock is received (a transaction
            // whose reason increases quantity on hand). Consumption transactions - e.g. the parts a
            // worker scans on the shopfloor - never change it. By matching the changed transactions
            // directly and skipping non-receipts, a scan no longer re-reads the part's entire
            // transaction history just to recompute the same average.
            var parts = matches.Cast<InventoryItemTransaction>()
                .Where(v => v.ExistPart && v.Reason?.IncreasesQuantityOnHand == true)
                .Select(v => v.Part)
                .Distinct();

            foreach (var part in parts)
            {
                var quantityOnHand = 0M;
                var totalCost = 0M;
                var averageCost = part.PartWeightedAverage.AverageCostInApplicationCurrency;

                foreach (var inventoryTransaction in part.InventoryItemTransactionsWherePart)
                {
                    var reason = inventoryTransaction.Reason;

                    if (reason?.IncreasesQuantityOnHand == true)
                    {
                        quantityOnHand += inventoryTransaction.Quantity;

                        var transactionCost = inventoryTransaction.Quantity * inventoryTransaction.CostInApplicationCurrency;
                        totalCost += transactionCost;

                        averageCost = quantityOnHand > 0 ? decimal.Round(totalCost / quantityOnHand, 2) : 0M;
                    }
                    else if (reason?.IncreasesQuantityOnHand == false)
                    {
                        quantityOnHand -= inventoryTransaction.Quantity;

                        totalCost = quantityOnHand * averageCost;
                    }
                }

                // Guard the write so an unchanged average does not re-enter the changelog and
                // re-trigger downstream cost/selling-price derivations.
                if (part.PartWeightedAverage.AverageCostInApplicationCurrency != averageCost)
                {
                    part.PartWeightedAverage.AverageCostInApplicationCurrency = averageCost;
                }
            }
        }
    }
}
