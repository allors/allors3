// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Allors.Database.Domain.Derivations;
    using Allors.Database.Meta;
    using Database.Derivations;

    public class UnifiedGoodDerivation : DomainDerivation
    {
        public UnifiedGoodDerivation(M m) : base(m, new Guid("B1C14106-C300-453D-989B-81E05767CFC4")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.UnifiedGood.DerivationTrigger),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<UnifiedGood>())
            {
                var defaultLocale = @this.Strategy.Session.GetSingleton().DefaultLocale;
                var settings = @this.Strategy.Session.GetSingleton().Settings;

                if (!@this.ExistDerivationTrigger)
                {
                    @this.DerivationTrigger = Guid.NewGuid();
                }

                if (cycle.ChangeSet.HasChangedRoles(@this, new RoleType[] { @this.Meta.UnitOfMeasure, @this.Meta.DefaultFacility }))
                {
                    // SyncDefaultInventoryItem
                    if (@this.InventoryItemKind.IsNonSerialised)
                    {
                        var inventoryItems = @this.InventoryItemsWherePart;

                        if (!inventoryItems.Any(i => i.Facility.Equals(@this.DefaultFacility) && i.UnitOfMeasure.Equals(@this.UnitOfMeasure)))
                        {
                            var inventoryItem = (InventoryItem)new NonSerialisedInventoryItemBuilder(@this.Strategy.Session)
                                .WithFacility(@this.DefaultFacility)
                                .WithUnitOfMeasure(@this.UnitOfMeasure)
                                .WithPart(@this)
                                .Build();
                        }
                    }
                }

                var identifications = @this.ProductIdentifications;
                identifications.Filter.AddEquals(this.M.ProductIdentification.ProductIdentificationType, new ProductIdentificationTypes(@this.Strategy.Session).Good);
                var goodIdentification = identifications.FirstOrDefault();

                if (goodIdentification == null && settings.UseProductNumberCounter)
                {
                    goodIdentification = new ProductNumberBuilder(@this.Strategy.Session)
                        .WithIdentification(settings.NextProductNumber())
                        .WithProductIdentificationType(new ProductIdentificationTypes(@this.Strategy.Session).Good).Build();

                    @this.AddProductIdentification(goodIdentification);
                }

                @this.ProductNumber = goodIdentification.Identification;

                if (!@this.ExistProductIdentifications)
                {
                    cycle.Validation.AssertExists(@this, this.M.Good.ProductIdentifications);
                }

                if (@this.LocalisedNames.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Name = @this.LocalisedNames.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                if (@this.LocalisedDescriptions.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    @this.Description = @this.LocalisedDescriptions.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                foreach (SupplierOffering supplierOffering in @this.SupplierOfferingsWherePart)
                {
                    if (supplierOffering.FromDate <= @this.Session().Now()
                        && (!supplierOffering.ExistThroughDate || supplierOffering.ThroughDate >= @this.Session().Now()))
                    {
                        @this.AddSuppliedBy(supplierOffering.Supplier);
                    }

                    if (supplierOffering.FromDate > @this.Session().Now()
                        || (supplierOffering.ExistThroughDate && supplierOffering.ThroughDate < @this.Session().Now()))
                    {
                        @this.RemoveSuppliedBy(supplierOffering.Supplier);
                    }
                }

                this.DeriveVirtualProductPriceComponent(@this);
                this.DeriveProductCharacteristics(@this);
                this.DeriveQuantityOnHand(@this);
                this.DeriveAvailableToPromise(@this);
                this.DeriveQuantityCommittedOut(@this);
                this.DeriveQuantityExpectedIn(@this);

                var quantityOnHand = 0M;
                var totalCost = 0M;

                foreach (InventoryItemTransaction inventoryTransaction in @this.InventoryItemTransactionsWherePart)
                {
                    var reason = inventoryTransaction.Reason;

                    if (reason.IncreasesQuantityOnHand == true)
                    {
                        quantityOnHand += inventoryTransaction.Quantity;

                        var transactionCost = inventoryTransaction.Quantity * inventoryTransaction.Cost;
                        totalCost += transactionCost;

                        var averageCost = quantityOnHand > 0 ? totalCost / quantityOnHand : 0M;
                        (@this.PartWeightedAverage).AverageCost = decimal.Round(averageCost, 2);
                    }
                    else if (reason.IncreasesQuantityOnHand == false)
                    {
                        quantityOnHand -= inventoryTransaction.Quantity;

                        totalCost = quantityOnHand * @this.PartWeightedAverage.AverageCost;
                    }
                }

                var builder = new StringBuilder();
                if (@this.ExistProductIdentifications)
                {
                    builder.Append(string.Join(" ", @this.ProductIdentifications.Select(v => v.Identification)));
                }

                if (@this.ExistProductCategoriesWhereAllProduct)
                {
                    builder.Append(string.Join(" ", @this.ProductCategoriesWhereAllProduct.Select(v => v.Name)));
                }

                if (@this.ExistProductCategoriesWhereAllPart)
                {
                    builder.Append(string.Join(" ", @this.ProductCategoriesWhereAllPart.Select(v => v.Name)));
                }

                if (@this.ExistSupplierOfferingsWherePart)
                {
                    builder.Append(string.Join(" ", @this.SupplierOfferingsWherePart.Select(v => v.Supplier.PartyName)));
                }

                if (@this.ExistSerialisedItems)
                {
                    builder.Append(string.Join(" ", @this.SerialisedItems.Select(v => v.SerialNumber)));
                    builder.Append(string.Join(" ", @this.SerialisedItems.Select(v => v.ItemNumber)));
                }

                if (@this.ExistProductType)
                {
                    builder.Append(string.Join(" ", @this.ProductType.Name));
                }

                if (@this.ExistBrand)
                {
                    builder.Append(string.Join(" ", @this.Brand.Name));
                }

                if (@this.ExistModel)
                {
                    builder.Append(string.Join(" ", @this.Model.Name));
                }

                foreach (PartCategory partCategory in @this.PartCategoriesWherePart)
                {
                    builder.Append(string.Join(" ", partCategory.Name));
                }

                builder.Append(string.Join(" ", @this.Keywords));

                @this.SearchString = builder.ToString();
            }
        }

        private void DeriveProductCharacteristics(UnifiedGood unifiedGood)
        {
            var characteristicsToDelete = unifiedGood.SerialisedItemCharacteristics.ToList();

            if (unifiedGood.ExistProductType)
            {
                foreach (SerialisedItemCharacteristicType characteristicType in unifiedGood.ProductType.SerialisedItemCharacteristicTypes)
                {
                    var characteristic = unifiedGood.SerialisedItemCharacteristics.FirstOrDefault(v => Equals(v.SerialisedItemCharacteristicType, characteristicType));
                    if (characteristic == null)
                    {
                        unifiedGood.AddSerialisedItemCharacteristic(
                            new SerialisedItemCharacteristicBuilder(unifiedGood.Strategy.Session)
                                .WithSerialisedItemCharacteristicType(characteristicType)
                                .Build());
                    }
                    else
                    {
                        characteristicsToDelete.Remove(characteristic);
                    }
                }
            }

            foreach (var characteristic in characteristicsToDelete)
            {
                unifiedGood.RemoveSerialisedItemCharacteristic(characteristic);
            }
        }

        public void DeriveQuantityOnHand(UnifiedGood unifiedGood)
        {
            unifiedGood.QuantityOnHand = 0;

            foreach (InventoryItem inventoryItem in unifiedGood.InventoryItemsWherePart)
            {
                if (inventoryItem is NonSerialisedInventoryItem nonSerialisedItem)
                {
                    unifiedGood.QuantityOnHand += nonSerialisedItem.QuantityOnHand;
                }
                else if (inventoryItem is SerialisedInventoryItem serialisedItem)
                {
                    unifiedGood.QuantityOnHand += serialisedItem.QuantityOnHand;
                }
            }
        }

        public void DeriveAvailableToPromise(UnifiedGood unifiedGood)
        {
            unifiedGood.AvailableToPromise = 0;

            foreach (InventoryItem inventoryItem in unifiedGood.InventoryItemsWherePart)
            {
                if (inventoryItem is NonSerialisedInventoryItem nonSerialisedItem)
                {
                    unifiedGood.AvailableToPromise += nonSerialisedItem.AvailableToPromise;
                }
                else if (inventoryItem is SerialisedInventoryItem serialisedItem)
                {
                    unifiedGood.AvailableToPromise += serialisedItem.AvailableToPromise;
                }
            }
        }

        public void DeriveQuantityCommittedOut(UnifiedGood unifiedGood)
        {
            unifiedGood.QuantityCommittedOut = 0;

            foreach (InventoryItem inventoryItem in unifiedGood.InventoryItemsWherePart)
            {
                if (inventoryItem is NonSerialisedInventoryItem nonSerialised)
                {
                    unifiedGood.QuantityCommittedOut += nonSerialised.QuantityCommittedOut;
                }
            }
        }

        public void DeriveQuantityExpectedIn(UnifiedGood unifiedGood)
        {
            unifiedGood.QuantityExpectedIn = 0;

            foreach (InventoryItem inventoryItem in unifiedGood.InventoryItemsWherePart)
            {
                if (inventoryItem is NonSerialisedInventoryItem nonSerialised)
                {
                    unifiedGood.QuantityExpectedIn += nonSerialised.QuantityExpectedIn;
                }
            }
        }

        public void DeriveVirtualProductPriceComponent(UnifiedGood unifiedGood)
        {
            if (!unifiedGood.ExistProductWhereVariant)
            {
                unifiedGood.RemoveVirtualProductPriceComponents();
            }

            if (unifiedGood.ExistVariants)
            {
                unifiedGood.RemoveVirtualProductPriceComponents();

                var priceComponents = unifiedGood.PriceComponentsWhereProduct;

                foreach (Good product in unifiedGood.Variants)
                {
                    foreach (PriceComponent priceComponent in priceComponents)
                    {
                        // HACK: DerivedRoles
                        var productDerivedRoles = product;

                        productDerivedRoles.AddVirtualProductPriceComponent(priceComponent);

                        if (priceComponent is BasePrice basePrice && !priceComponent.ExistProductFeature)
                        {
                            productDerivedRoles.AddBasePrice(basePrice);
                        }
                    }
                }
            }
        }

    }
}
