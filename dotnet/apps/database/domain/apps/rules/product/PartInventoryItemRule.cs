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

    public class PartInventoryItemRule : Rule
    {
        public PartInventoryItemRule(MetaPopulation m) : base(m, new Guid("36a94e99-2576-4bb2-8e24-efdddb93f3e2")) =>
            this.Patterns = new Pattern[]
            {
                m.Part.RolePattern(v => v.DefaultFacility),
                m.Part.RolePattern(v => v.UnitOfMeasure),
                m.Part.AssociationPattern(v => v.InventoryItemsWherePart),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var m = cycle.Transaction.Database.Services().M;

            foreach (var @this in matches.Cast<Part>())
            {
                if (@this.InventoryItemKind.IsNonSerialised)
                {
                    var inventoryItems = @this.InventoryItemsWherePart;

                    if (!inventoryItems.Any(i => i.ExistFacility && i.Facility.Equals(@this.DefaultFacility)
                                                && i.ExistUnitOfMeasure && i.UnitOfMeasure.Equals(@this.UnitOfMeasure)))
                    {
                        var inventoryItem = (InventoryItem)new NonSerialisedInventoryItemBuilder(@this.Strategy.Transaction)
                          .WithFacility(@this.DefaultFacility)
                          .WithUnitOfMeasure(@this.UnitOfMeasure)
                          .WithPart(@this)
                          .Build();
                    }
                }
            }
        }
    }
}
