// <copyright file="NonUnifiedPartDerivation.cs" company="Allors bvba">
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

    public class NonUnifiedPartDerivation : DomainDerivation
    {
        public NonUnifiedPartDerivation(M m) : base(m, new Guid("280E12F5-C2EA-4D9A-BEDA-D30F229D46A3")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.NonUnifiedPart.ProductIdentifications),
                new ChangedPattern(this.M.NonUnifiedPart.UniqueId),
                new ChangedPattern(this.M.InventoryItemTransaction.Quantity) { Steps = new IPropertyType[]{ this.M.NonSerialisedInventoryItem.Part }, OfType = this.M.NonUnifiedPart.Class},
                new ChangedPattern(this.M.NonSerialisedInventoryItem.QuantityOnHand) { Steps = new IPropertyType[]{ this.M.NonSerialisedInventoryItem.Part },OfType = this.M.NonUnifiedPart.Class},
                new ChangedPattern(this.M.NonSerialisedInventoryItem.AvailableToPromise) { Steps = new IPropertyType[]{ this.M.NonSerialisedInventoryItem.Part },OfType = this.M.NonUnifiedPart.Class},
                new ChangedPattern(this.M.NonSerialisedInventoryItem.QuantityCommittedOut) { Steps = new IPropertyType[]{ this.M.NonSerialisedInventoryItem.Part },OfType = this.M.NonUnifiedPart.Class},
                new ChangedPattern(this.M.NonSerialisedInventoryItem.QuantityExpectedIn) { Steps = new IPropertyType[]{ this.M.NonSerialisedInventoryItem.Part },OfType = this.M.NonUnifiedPart.Class},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<NonUnifiedPart>())
            {
                var settings = @this.Strategy.Session.GetSingleton().Settings;

                if (cycle.ChangeSet.HasChangedRoles(@this, new RoleType[] { @this.Meta.UnitOfMeasure, @this.Meta.DefaultFacility }))
                {
                    this.SyncDefaultInventoryItem(@this);
                }

                if (!@this.ExistName)
                {
                    @this.Name = "Part " + (@this.PartIdentification() ?? @this.UniqueId.ToString());
                }

                var identifications = @this.ProductIdentifications;
                identifications.Filter.AddEquals(this.M.ProductIdentification.ProductIdentificationType, new ProductIdentificationTypes(@this.Strategy.Session).Part);
                var partIdentification = identifications.FirstOrDefault();

                if (partIdentification == null && settings.UsePartNumberCounter)
                {
                    partIdentification = new PartNumberBuilder(@this.Strategy.Session)
                        .WithIdentification(settings.NextPartNumber())
                        .WithProductIdentificationType(new ProductIdentificationTypes(@this.Strategy.Session).Part).Build();

                    @this.AddProductIdentification(partIdentification);
                }

                @this.ProductNumber = partIdentification.Identification;

                @this.RemoveSuppliedBy();
                foreach (SupplierOffering supplierOffering in @this.SupplierOfferingsWherePart)
                {
                    if (supplierOffering.FromDate <= @this.Session().Now()
                        && (!supplierOffering.ExistThroughDate || supplierOffering.ThroughDate >= @this.Session().Now()))
                    {
                        @this.AddSuppliedBy(supplierOffering.Supplier);
                    }
                }

                this.ProductCharacteristics(@this);
                this.QuantityOnHand(@this);
                this.AvailableToPromise(@this);
                this.QuantityCommittedOut(@this);
                this.QuantityExpectedIn(@this);

                var quantityOnHand = 0M;
                var totalCost = 0M;

                foreach (InventoryItemTransaction inventoryTransaction in @this.InventoryItemTransactionsWherePart)
                {
                    var reason = inventoryTransaction.Reason;

                    if (reason?.IncreasesQuantityOnHand == true)
                    {
                        quantityOnHand += inventoryTransaction.Quantity;

                        var transactionCost = inventoryTransaction.Quantity * inventoryTransaction.Cost;
                        totalCost += transactionCost;

                        var averageCost = quantityOnHand > 0 ? totalCost / quantityOnHand : 0M;
                        (@this.PartWeightedAverage).AverageCost = decimal.Round(averageCost, 2);
                    }
                    else if (reason?.IncreasesQuantityOnHand == false)
                    {
                        quantityOnHand -= inventoryTransaction.Quantity;

                        totalCost = quantityOnHand * @this.PartWeightedAverage.AverageCost;
                    }
                }
            }
        }

        private void SyncDefaultInventoryItem(NonUnifiedPart nonUnifiedPart)
        {
            if (nonUnifiedPart.InventoryItemKind.IsNonSerialised)
            {
                var inventoryItems = nonUnifiedPart.InventoryItemsWherePart;

                if (!inventoryItems.Any(i => i.Facility != null && i.Facility.Equals(nonUnifiedPart.DefaultFacility) && i.UnitOfMeasure.Equals(nonUnifiedPart.UnitOfMeasure)))
                {
                    var inventoryItem = (InventoryItem)new NonSerialisedInventoryItemBuilder(nonUnifiedPart.Strategy.Session)
                      .WithFacility(nonUnifiedPart.DefaultFacility)
                      .WithUnitOfMeasure(nonUnifiedPart.UnitOfMeasure)
                      .WithPart(nonUnifiedPart)
                      .Build();
                }
            }
        }

        private void ProductCharacteristics(NonUnifiedPart nonUnifiedPart)
        {
            var characteristicsToDelete = nonUnifiedPart.SerialisedItemCharacteristics.ToList();

            if (nonUnifiedPart.ExistProductType)
            {
                foreach (SerialisedItemCharacteristicType characteristicType in nonUnifiedPart.ProductType.SerialisedItemCharacteristicTypes)
                {
                    var characteristic = nonUnifiedPart.SerialisedItemCharacteristics.FirstOrDefault(v => Equals(v.SerialisedItemCharacteristicType, characteristicType));
                    if (characteristic == null)
                    {
                        nonUnifiedPart.AddSerialisedItemCharacteristic(
                            new SerialisedItemCharacteristicBuilder(nonUnifiedPart.Strategy.Session)
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
                nonUnifiedPart.RemoveSerialisedItemCharacteristic(characteristic);
            }
        }

        private void QuantityOnHand(NonUnifiedPart nonUnifiedPart)
        {
            nonUnifiedPart.QuantityOnHand = 0;

            foreach (InventoryItem inventoryItem in nonUnifiedPart.InventoryItemsWherePart)
            {
                if (inventoryItem is NonSerialisedInventoryItem nonSerialisedItem)
                {
                    nonUnifiedPart.QuantityOnHand += nonSerialisedItem.QuantityOnHand;
                }
                else if (inventoryItem is SerialisedInventoryItem serialisedItem)
                {
                    nonUnifiedPart.QuantityOnHand += serialisedItem.QuantityOnHand;
                }
            }
        }

        private void AvailableToPromise(NonUnifiedPart nonUnifiedPart)
        {
            nonUnifiedPart.AvailableToPromise = 0;

            foreach (InventoryItem inventoryItem in nonUnifiedPart.InventoryItemsWherePart)
            {
                if (inventoryItem is NonSerialisedInventoryItem nonSerialisedItem)
                {
                    nonUnifiedPart.AvailableToPromise += nonSerialisedItem.AvailableToPromise;
                }
                else if (inventoryItem is SerialisedInventoryItem serialisedItem)
                {
                    nonUnifiedPart.AvailableToPromise += serialisedItem.AvailableToPromise;
                }
            }
        }

        private void QuantityCommittedOut(NonUnifiedPart nonUnifiedPart)
        {
            nonUnifiedPart.QuantityCommittedOut = 0;

            foreach (InventoryItem inventoryItem in nonUnifiedPart.InventoryItemsWherePart)
            {
                if (inventoryItem is NonSerialisedInventoryItem nonSerialised)
                {
                    nonUnifiedPart.QuantityCommittedOut += nonSerialised.QuantityCommittedOut;
                }
            }
        }

        private void QuantityExpectedIn(NonUnifiedPart nonUnifiedPart)
        {
            nonUnifiedPart.QuantityExpectedIn = 0;

            foreach (InventoryItem inventoryItem in nonUnifiedPart.InventoryItemsWherePart)
            {
                if (inventoryItem is NonSerialisedInventoryItem nonSerialised)
                {
                    nonUnifiedPart.QuantityExpectedIn += nonSerialised.QuantityExpectedIn;
                }
            }
        }
    }
}
