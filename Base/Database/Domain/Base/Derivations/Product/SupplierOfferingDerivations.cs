// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class SupplierOfferingDerivation : DomainDerivation
    {
        public SupplierOfferingDerivation(M m) : base(m, new Guid("0927C224-0233-4211-BB4F-5F62506D9635")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.SupplierOffering.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var m = cycle.Session.Database.State().M;
            foreach (var supplierOffering in matches.Cast<SupplierOffering>())
            {
                if (!supplierOffering.ExistCurrency)
                {
                    supplierOffering.Currency = supplierOffering.Session().GetSingleton().Settings.PreferredCurrency;
                }

                if (supplierOffering.ExistPart && supplierOffering.Part.ExistInventoryItemKind &&
                    supplierOffering.Part.InventoryItemKind.Equals(new InventoryItemKinds(supplierOffering.Strategy.Session).NonSerialised))
                {
                    var warehouses = supplierOffering.Strategy.Session.Extent<Facility>();
                    warehouses.Filter.AddEquals(this.M.Facility.FacilityType, new FacilityTypes(supplierOffering.Session()).Warehouse);

                    foreach (Facility facility in warehouses)
                    {
                        var inventoryItems = supplierOffering.Part.InventoryItemsWherePart;
                        inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, facility);
                        var inventoryItem = inventoryItems.First;

                        if (inventoryItem == null)
                        {
                            new NonSerialisedInventoryItemBuilder(supplierOffering.Strategy.Session).WithPart(supplierOffering.Part).WithFacility(facility).Build();
                        }
                    }
                }
            }
        }
    }
}
