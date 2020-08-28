// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class SupplierOfferingCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
               var createdSupplierOfferings = changeSet.Created.Select(session.Instantiate).OfType<SupplierOffering>();

                foreach(var supplierOffering in createdSupplierOfferings)
                {
                    if (!supplierOffering.ExistCurrency)
                    {
                        supplierOffering.Currency = supplierOffering.Session().GetSingleton().Settings.PreferredCurrency;
                    }

                    BaseOnDeriveInventoryItem(supplierOffering);
                }

                void BaseOnDeriveInventoryItem(SupplierOffering supplierOffering)
                {
                    if (supplierOffering.ExistPart && supplierOffering.Part.ExistInventoryItemKind &&
                        supplierOffering.Part.InventoryItemKind.Equals(new InventoryItemKinds(supplierOffering.Strategy.Session).NonSerialised))
                    {
                        var warehouses = supplierOffering.Strategy.Session.Extent<Facility>();
                        warehouses.Filter.AddEquals(M.Facility.FacilityType, new FacilityTypes(supplierOffering.Session()).Warehouse);

                        foreach (Facility facility in warehouses)
                        {
                            var inventoryItems = supplierOffering.Part.InventoryItemsWherePart;
                            inventoryItems.Filter.AddEquals(M.InventoryItem.Facility, facility);
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

        public static void SupplierOfferingRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("17ee9a7a-1ac5-4ab2-b742-8d143b872a72")] = new SupplierOfferingCreationDerivation();
        }
    }
}
