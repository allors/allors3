// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class SupplierOfferingDerivation : DomainDerivation
    {
        public SupplierOfferingDerivation(M m) : base(m, new Guid("0927C224-0233-4211-BB4F-5F62506D9635")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SupplierOffering.FromDate),
                new RolePattern(m.SupplierOffering.ThroughDate),
                new RolePattern(m.SupplierOffering.Price),
                new RolePattern(m.SupplierOffering.Part),
                new RolePattern(m.SupplierOffering.Supplier),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var m = cycle.Transaction.Database.Context().M;
            foreach (var @this in matches.Cast<SupplierOffering>())
            {
                if (@this.ExistSupplier)
                {
                    foreach (var purchaseInvoice in @this.Supplier.PurchaseInvoicesWhereBilledFrom.Where(v => v.ExistPurchaseInvoiceState && (v.PurchaseInvoiceState.IsCreated || v.PurchaseInvoiceState.IsRevising)))
                    {
                        purchaseInvoice.DerivationTrigger = Guid.NewGuid();
                    }

                    foreach (var purchaseOrder in ((Organisation)@this.Supplier).PurchaseOrdersWhereTakenViaSupplier.Where(v => v.ExistPurchaseOrderState && v.PurchaseOrderState.IsCreated))
                    {
                        purchaseOrder.DerivationTrigger = Guid.NewGuid();
                    }
                }

                if (!@this.ExistCurrency)
                {
                    @this.Currency = @this.Transaction().GetSingleton().Settings.PreferredCurrency;
                }

                if (@this.ExistPart && @this.Part.ExistInventoryItemKind &&
                    @this.Part.InventoryItemKind.Equals(new InventoryItemKinds(@this.Strategy.Transaction).NonSerialised))
                {
                    var warehouses = @this.Strategy.Transaction.Extent<Facility>();
                    warehouses.Filter.AddEquals(this.M.Facility.FacilityType, new FacilityTypes(@this.Transaction()).Warehouse);

                    foreach (Facility facility in warehouses)
                    {
                        var inventoryItems = @this.Part.InventoryItemsWherePart;
                        inventoryItems.Filter.AddEquals(this.M.InventoryItem.Facility, facility);
                        var inventoryItem = inventoryItems.First;

                        if (inventoryItem == null)
                        {
                            new NonSerialisedInventoryItemBuilder(@this.Strategy.Transaction).WithPart(@this.Part).WithFacility(facility).WithUnitOfMeasure(@this.Part.UnitOfMeasure).Build();
                        }
                    }
                }
            }
        }
    }
}
