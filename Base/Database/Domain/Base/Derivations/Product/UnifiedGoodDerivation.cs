// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class UnifiedGoodDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("B1C14106-C300-453D-989B-81E05767CFC4");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.UnifiedGood.Class),
            new CreatedPattern(M.InventoryItemTransaction.Class){Steps = new IPropertyType[]{M.InventoryItemTransaction.Part}, OfType = M.UnifiedGood.Class},
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var unifiedGood in matches.Cast<UnifiedGood>())
            {
                var defaultLocale = unifiedGood.Strategy.Session.GetSingleton().DefaultLocale;
                var settings = unifiedGood.Strategy.Session.GetSingleton().Settings;

                if (!unifiedGood.ExistDerivationTrigger)
                {
                    unifiedGood.DerivationTrigger = Guid.NewGuid();
                }

                if (cycle.ChangeSet.HasChangedRoles(unifiedGood, new RoleType[] { unifiedGood.Meta.UnitOfMeasure, unifiedGood.Meta.DefaultFacility }))
                {
                    // SyncDefaultInventoryItem
                    if (unifiedGood.InventoryItemKind.IsNonSerialised)
                    {
                        var inventoryItems = unifiedGood.InventoryItemsWherePart;

                        if (!inventoryItems.Any(i => i.Facility.Equals(unifiedGood.DefaultFacility) && i.UnitOfMeasure.Equals(unifiedGood.UnitOfMeasure)))
                        {
                            var inventoryItem = (InventoryItem)new NonSerialisedInventoryItemBuilder(unifiedGood.Strategy.Session)
                                .WithFacility(unifiedGood.DefaultFacility)
                                .WithUnitOfMeasure(unifiedGood.UnitOfMeasure)
                                .WithPart(unifiedGood)
                                .Build();
                        }
                    }
                }

                var identifications = unifiedGood.ProductIdentifications;
                identifications.Filter.AddEquals(M.ProductIdentification.ProductIdentificationType, new ProductIdentificationTypes(unifiedGood.Strategy.Session).Good);
                var goodIdentification = identifications.FirstOrDefault();

                if (goodIdentification == null && settings.UseProductNumberCounter)
                {
                    goodIdentification = new ProductNumberBuilder(unifiedGood.Strategy.Session)
                        .WithIdentification(settings.NextProductNumber())
                        .WithProductIdentificationType(new ProductIdentificationTypes(unifiedGood.Strategy.Session).Good).Build();

                    unifiedGood.AddProductIdentification(goodIdentification);
                }

                unifiedGood.ProductNumber = goodIdentification.Identification;

                if (!unifiedGood.ExistProductIdentifications)
                {
                    cycle.Validation.AssertExists(unifiedGood, M.Good.ProductIdentifications);
                }

                if (unifiedGood.LocalisedNames.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    unifiedGood.Name = unifiedGood.LocalisedNames.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                if (unifiedGood.LocalisedDescriptions.Any(x => x.Locale.Equals(defaultLocale)))
                {
                    unifiedGood.Description = unifiedGood.LocalisedDescriptions.First(x => x.Locale.Equals(defaultLocale)).Text;
                }

                foreach (SupplierOffering supplierOffering in unifiedGood.SupplierOfferingsWherePart)
                {
                    if (supplierOffering.FromDate <= unifiedGood.Session().Now()
                        && (!supplierOffering.ExistThroughDate || supplierOffering.ThroughDate >= unifiedGood.Session().Now()))
                    {
                        unifiedGood.AddSuppliedBy(supplierOffering.Supplier);
                    }

                    if (supplierOffering.FromDate > unifiedGood.Session().Now()
                        || (supplierOffering.ExistThroughDate && supplierOffering.ThroughDate < unifiedGood.Session().Now()))
                    {
                        unifiedGood.RemoveSuppliedBy(supplierOffering.Supplier);
                    }
                }

                this.DeriveVirtualProductPriceComponent(unifiedGood);
                this.DeriveProductCharacteristics(unifiedGood);
                this.DeriveQuantityOnHand(unifiedGood);
                this.DeriveAvailableToPromise(unifiedGood);
                this.DeriveQuantityCommittedOut(unifiedGood);
                this.DeriveQuantityExpectedIn(unifiedGood);

                var quantityOnHand = 0M;
                var totalCost = 0M;

                foreach (InventoryItemTransaction inventoryTransaction in unifiedGood.InventoryItemTransactionsWherePart)
                {
                    var reason = inventoryTransaction.Reason;

                    if (reason.IncreasesQuantityOnHand == true)
                    {
                        quantityOnHand += inventoryTransaction.Quantity;

                        var transactionCost = inventoryTransaction.Quantity * inventoryTransaction.Cost;
                        totalCost += transactionCost;

                        var averageCost = quantityOnHand > 0 ? totalCost / quantityOnHand : 0M;
                        (unifiedGood.PartWeightedAverage).AverageCost = decimal.Round(averageCost, 2);
                    }
                    else if (reason.IncreasesQuantityOnHand == false)
                    {
                        quantityOnHand -= inventoryTransaction.Quantity;

                        totalCost = quantityOnHand * unifiedGood.PartWeightedAverage.AverageCost;
                    }
                }

                var deletePermission = new Permissions(unifiedGood.Strategy.Session).Get(unifiedGood.Meta.ObjectType, unifiedGood.Meta.Delete, Operations.Execute);

                if (!unifiedGood.ExistDeploymentsWhereProductOffering &&
                                !unifiedGood.ExistEngagementItemsWhereProduct &&
                                !unifiedGood.ExistGeneralLedgerAccountsWhereCostUnitsAllowed &&
                                !unifiedGood.ExistGeneralLedgerAccountsWhereDefaultCostUnit &&
                                !unifiedGood.ExistQuoteItemsWhereProduct &&
                                !unifiedGood.ExistShipmentItemsWhereGood &&
                                !unifiedGood.ExistWorkEffortGoodStandardsWhereUnifiedProduct &&
                                !unifiedGood.ExistMarketingPackageWhereProductsUsedIn &&
                                !unifiedGood.ExistMarketingPackagesWhereProduct &&
                                !unifiedGood.ExistOrganisationGlAccountsWhereProduct &&
                                !unifiedGood.ExistProductConfigurationsWhereProductsUsedIn &&
                                !unifiedGood.ExistProductConfigurationsWhereProduct &&
                                !unifiedGood.ExistRequestItemsWhereProduct &&
                                !unifiedGood.ExistSalesInvoiceItemsWhereProduct &&
                                !unifiedGood.ExistSalesOrderItemsWhereProduct &&
                                !unifiedGood.ExistWorkEffortTypesWhereProductToProduce &&
                                !unifiedGood.ExistWorkEffortInventoryProducedsWherePart &&
                                !unifiedGood.ExistWorkEffortPartStandardsWherePart &&
                                !unifiedGood.ExistPartBillOfMaterialsWherePart &&
                                !unifiedGood.ExistPartBillOfMaterialsWhereComponentPart &&
                                !unifiedGood.ExistInventoryItemTransactionsWherePart &&
                                !unifiedGood.ExistSerialisedItems)
                {
                    unifiedGood.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    unifiedGood.AddDeniedPermission(deletePermission);
                }

                var builder = new StringBuilder();
                if (unifiedGood.ExistProductIdentifications)
                {
                    builder.Append(string.Join(" ", unifiedGood.ProductIdentifications.Select(v => v.Identification)));
                }

                if (unifiedGood.ExistProductCategoriesWhereAllProduct)
                {
                    builder.Append(string.Join(" ", unifiedGood.ProductCategoriesWhereAllProduct.Select(v => v.Name)));
                }

                if (unifiedGood.ExistProductCategoriesWhereAllPart)
                {
                    builder.Append(string.Join(" ", unifiedGood.ProductCategoriesWhereAllPart.Select(v => v.Name)));
                }

                if (unifiedGood.ExistSupplierOfferingsWherePart)
                {
                    builder.Append(string.Join(" ", unifiedGood.SupplierOfferingsWherePart.Select(v => v.Supplier.PartyName)));
                }

                if (unifiedGood.ExistSerialisedItems)
                {
                    builder.Append(string.Join(" ", unifiedGood.SerialisedItems.Select(v => v.SerialNumber)));
                    builder.Append(string.Join(" ", unifiedGood.SerialisedItems.Select(v => v.ItemNumber)));
                }

                if (unifiedGood.ExistProductType)
                {
                    builder.Append(string.Join(" ", unifiedGood.ProductType.Name));
                }

                if (unifiedGood.ExistBrand)
                {
                    builder.Append(string.Join(" ", unifiedGood.Brand.Name));
                }

                if (unifiedGood.ExistModel)
                {
                    builder.Append(string.Join(" ", unifiedGood.Model.Name));
                }

                foreach (PartCategory partCategory in unifiedGood.PartCategoriesWherePart)
                {
                    builder.Append(string.Join(" ", partCategory.Name));
                }

                builder.Append(string.Join(" ", unifiedGood.Keywords));

                unifiedGood.SearchString = builder.ToString();
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
