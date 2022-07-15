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

    public class SupplierOfferingRule : Rule
    {
        public SupplierOfferingRule(MetaPopulation m) : base(m, new Guid("0927C224-0233-4211-BB4F-5F62506D9635")) =>
            this.Patterns = new Pattern[]
            {
                m.SupplierOffering.RolePattern(v => v.FromDate),
                m.SupplierOffering.RolePattern(v => v.ThroughDate),
                m.SupplierOffering.RolePattern(v => v.Part),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var m = cycle.Transaction.Database.Services.Get<MetaPopulation>();
            foreach (var @this in matches.Cast<SupplierOffering>())
            {
                if (@this.ExistPart && @this.Part.ExistInventoryItemKind &&
                    @this.Part.InventoryItemKind.Equals(new InventoryItemKinds(@this.Strategy.Transaction).NonSerialised))
                {
                    var warehouses = @this.Strategy.Transaction.Extent<Facility>();
                    warehouses.Filter.AddEquals(this.M.Facility.FacilityType, new FacilityTypes(@this.Transaction()).Warehouse);

                    foreach (Facility facility in warehouses)
                    {
                        var inventoryItem = @this.Part.InventoryItemsWherePart.FirstOrDefault(v => Equals(facility, v.Facility));
                        if (inventoryItem == null)
                        {
                            new NonSerialisedInventoryItemBuilder(@this.Strategy.Transaction)
                                .WithPart(@this.Part)
                                .WithFacility(facility)
                                .WithUnitOfMeasure(@this.Part.UnitOfMeasure)
                                .WithNonSerialisedInventoryItemState(new NonSerialisedInventoryItemStates(@this.Strategy.Transaction).Good)
                                .Build();
                        }
                    }
                }
            }
        }
    }
}
