// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Database.Derivations;

    public class SupplierOfferingDerivation : DomainDerivation
    {
        public SupplierOfferingDerivation(M m) : base(m, new Guid("0927C224-0233-4211-BB4F-5F62506D9635")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.SupplierOffering.Currency),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var m = cycle.Session.Database.State().M;
            foreach (var @this in matches.Cast<SupplierOffering>())
            {
                if (!@this.ExistCurrency)
                {
                    @this.Currency = @this.Session().GetSingleton().Settings.PreferredCurrency;
                }

                if (@this.ExistPart && @this.Part.ExistInventoryItemKind &&
                    @this.Part.InventoryItemKind.Equals(new InventoryItemKinds(@this.Strategy.Session).NonSerialised))
                {
                    var warehouses = @this.Strategy.Session.Extent<Facility>();
                    warehouses.Filter.AddEquals(this.M.Facility.FacilityType, new FacilityTypes(@this.Session()).Warehouse);

                    foreach (Facility facility in warehouses)
                    {
                        var inventoryItems = @this.Part.InventoryItemsWherePart;
                        inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, facility);
                        var inventoryItem = inventoryItems.First;

                        if (inventoryItem == null)
                        {
                            new NonSerialisedInventoryItemBuilder(@this.Strategy.Session).WithPart(@this.Part).WithFacility(facility).Build();
                        }
                    }
                }
            }
        }
    }
}
