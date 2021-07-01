// <copyright file="NonUnifiedPartQuantitiesDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Derivations.Rules;

    public class PartQuantitiesRule : Rule
    {
        public PartQuantitiesRule(MetaPopulation m) : base(m, new Guid("d0fc5096-5ea8-4c50-8979-0ac66d43e6d0")) =>
            this.Patterns = new Pattern[]
            {
                m.InventoryItemTransaction.RolePattern(v => v.Quantity, v => v.InventoryItem.InventoryItem.Part),
                m.SerialisedInventoryItem.RolePattern(v => v.Quantity, v => v.Part),
                m.SerialisedInventoryItem.RolePattern(v => v.SerialisedInventoryItemState, v => v.Part),
                m.NonSerialisedInventoryItem.RolePattern(v => v.QuantityOnHand, v => v.Part),
                m.NonSerialisedInventoryItem.RolePattern(v => v.AvailableToPromise, v => v.Part),
                m.NonSerialisedInventoryItem.RolePattern(v => v.QuantityCommittedOut, v => v.Part),
                m.NonSerialisedInventoryItem.RolePattern(v => v.QuantityExpectedIn, v => v.Part),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Part>())
            {
                @this.QuantityOnHand = 0;
                @this.AvailableToPromise = 0;
                @this.QuantityCommittedOut = 0;
                @this.QuantityExpectedIn = 0;

                foreach (var inventoryItem in @this.InventoryItemsWherePart)
                {
                    if (inventoryItem is NonSerialisedInventoryItem nonSerialisedInventoryItem)
                    {
                        @this.QuantityOnHand += nonSerialisedInventoryItem.QuantityOnHand;
                        @this.AvailableToPromise += nonSerialisedInventoryItem.AvailableToPromise;
                        @this.QuantityCommittedOut += nonSerialisedInventoryItem.QuantityCommittedOut;
                        @this.QuantityExpectedIn += nonSerialisedInventoryItem.QuantityExpectedIn;
                    }
                    else if (inventoryItem is SerialisedInventoryItem serialisedInventoryItem)
                    {
                        @this.QuantityOnHand += serialisedInventoryItem.QuantityOnHand;
                        @this.AvailableToPromise += serialisedInventoryItem.AvailableToPromise;
                    }
                }

                var quantityOnHand = 0M;
                var totalCost = 0M;

                foreach (var inventoryTransaction in @this.InventoryItemTransactionsWherePart)
                {
                    var reason = inventoryTransaction.Reason;

                    if (reason?.IncreasesQuantityOnHand == true)
                    {
                        quantityOnHand += inventoryTransaction.Quantity;

                        var transactionCost = inventoryTransaction.Quantity * inventoryTransaction.Cost;
                        totalCost += transactionCost;

                        var averageCost = quantityOnHand > 0 ? totalCost / quantityOnHand : 0M;
                        @this.PartWeightedAverage.AverageCost = decimal.Round(averageCost, 2);
                    }
                    else if (reason?.IncreasesQuantityOnHand == false)
                    {
                        quantityOnHand -= inventoryTransaction.Quantity;

                        totalCost = quantityOnHand * @this.PartWeightedAverage.AverageCost;
                    }
                }
            }
        }
    }
}
