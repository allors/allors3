// <copyright file="PartDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations.Rules;
    using Meta;

    public class PartSyncInventoryItemsRule : Rule
    {
        public PartSyncInventoryItemsRule(MetaPopulation m) : base(m, new Guid("e5580f5b-a6be-4546-86ab-14db5bafca8e")) =>
            this.Patterns = new Pattern[]
            {
                m.Part.AssociationPattern(v => v.InventoryItemsWherePart),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Part>())
            {
                foreach (InventoryItem inventoryItem in @this.InventoryItemsWherePart)
                {
                    inventoryItem.Sync(@this);
                }
            }
        }
    }
}
