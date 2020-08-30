// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class NonUnifiedPartCreationDerivation : IDomainDerivation
        {

            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var empty = Array.Empty<NonUnifiedPart>();

                var createdNonUnifiedPart = changeSet.Created.Select(v=>v.GetObject()).OfType<NonUnifiedPart>();

                var createdInventoryItemTransaction= changeSet.Created.Select(v=>v.GetObject()).OfType<InventoryItemTransaction>();

                changeSet.AssociationsByRoleType.TryGetValue(M.InventoryItemTransaction.Part, out var inventoryItemTransactions);
                var inventoryItemTransactionParts = inventoryItemTransactions?.Select(session.Instantiate).OfType<InventoryItemTransaction>().Select(v => v.Part).OfType<NonUnifiedPart>();

                changeSet.AssociationsByRoleType.TryGetValue(M.NonSerialisedInventoryItem.QuantityOnHand, out var nonSerialisedInventoryItems);
                var nonSerialisedInventoryItemParts = nonSerialisedInventoryItems?.Select(session.Instantiate).OfType<NonSerialisedInventoryItem>().Select(v => v.Part).OfType<NonUnifiedPart>();

                var parts = createdNonUnifiedPart
                    .Union(inventoryItemTransactionParts ?? empty)
                    .Union(nonSerialisedInventoryItemParts ?? empty)
                    .Union(createdInventoryItemTransaction.Select(v => v.Part as NonUnifiedPart) ?? empty);

                foreach (var nonUnifiedPart in parts.Where(v => v != null))
                {
                    var setings = nonUnifiedPart.Strategy.Session.GetSingleton().Settings;

                    if (changeSet.HasChangedRoles(nonUnifiedPart, new RoleType[] { nonUnifiedPart.Meta.UnitOfMeasure, nonUnifiedPart.Meta.DefaultFacility }))
                    {
                        this.SyncDefaultInventoryItem(nonUnifiedPart);
                    }

                    DeriveName(nonUnifiedPart);

                    var identifications = nonUnifiedPart.ProductIdentifications;
                    identifications.Filter.AddEquals(M.ProductIdentification.ProductIdentificationType, new ProductIdentificationTypes(nonUnifiedPart.Strategy.Session).Part);
                    var partIdentification = identifications.FirstOrDefault();

                    if (partIdentification == null && setings.UsePartNumberCounter)
                    {
                        partIdentification = new PartNumberBuilder(nonUnifiedPart.Strategy.Session)
                            .WithIdentification(setings.NextPartNumber())
                            .WithProductIdentificationType(new ProductIdentificationTypes(nonUnifiedPart.Strategy.Session).Part).Build();

                        nonUnifiedPart.AddProductIdentification(partIdentification);
                    }

                    nonUnifiedPart.ProductNumber = partIdentification.Identification;

                    nonUnifiedPart.RemoveSuppliedBy();
                    foreach (SupplierOffering supplierOffering in nonUnifiedPart.SupplierOfferingsWherePart)
                    {
                        if (supplierOffering.FromDate <= nonUnifiedPart.Session().Now()
                            && (!supplierOffering.ExistThroughDate || supplierOffering.ThroughDate >= nonUnifiedPart.Session().Now()))
                        {
                            nonUnifiedPart.AddSuppliedBy(supplierOffering.Supplier);
                        }
                    }

                    this.DeriveProductCharacteristics(nonUnifiedPart);
                    this.DeriveQuantityOnHand(nonUnifiedPart);
                    this.DeriveAvailableToPromise(nonUnifiedPart);
                    this.DeriveQuantityCommittedOut(nonUnifiedPart);
                    this.DeriveQuantityExpectedIn(nonUnifiedPart);

                    var quantityOnHand = 0M;
                    var totalCost = 0M;

                    foreach (InventoryItemTransaction inventoryTransaction in nonUnifiedPart.InventoryItemTransactionsWherePart)
                    {
                        var reason = inventoryTransaction.Reason;

                        if (reason.IncreasesQuantityOnHand == true)
                        {
                            quantityOnHand += inventoryTransaction.Quantity;

                            var transactionCost = inventoryTransaction.Quantity * inventoryTransaction.Cost;
                            totalCost += transactionCost;

                            var averageCost = quantityOnHand > 0 ? totalCost / quantityOnHand : 0M;
                            (nonUnifiedPart.PartWeightedAverage).AverageCost = decimal.Round(averageCost, 2);
                        }
                        else if (reason.IncreasesQuantityOnHand == false)
                        {
                            quantityOnHand -= inventoryTransaction.Quantity;

                            totalCost = quantityOnHand * nonUnifiedPart.PartWeightedAverage.AverageCost;
                        }

                        var deletePermission = new Permissions(nonUnifiedPart.Strategy.Session).Get(nonUnifiedPart.Meta.ObjectType, nonUnifiedPart.Meta.Delete, Operations.Execute);
                        if (!nonUnifiedPart.ExistWorkEffortInventoryProducedsWherePart &&
                               !nonUnifiedPart.ExistWorkEffortPartStandardsWherePart &&
                               !nonUnifiedPart.ExistPartBillOfMaterialsWherePart &&
                               !nonUnifiedPart.ExistPartBillOfMaterialsWhereComponentPart &&
                               !nonUnifiedPart.ExistInventoryItemTransactionsWherePart)
                        {
                            nonUnifiedPart.RemoveDeniedPermission(deletePermission);
                        }
                        else
                        {
                            nonUnifiedPart.AddDeniedPermission(deletePermission);
                        }
                    }
                }
            }

            private void DeriveName(NonUnifiedPart nonUnifiedPart)
            {
                if (!nonUnifiedPart.ExistName)
                {
                    nonUnifiedPart.Name = "Part " + (nonUnifiedPart.PartIdentification() ?? nonUnifiedPart.UniqueId.ToString());
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

            private void DeriveProductCharacteristics(NonUnifiedPart nonUnifiedPart)
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

            private void DeriveQuantityOnHand(NonUnifiedPart nonUnifiedPart)
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

            private void DeriveAvailableToPromise(NonUnifiedPart nonUnifiedPart)
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

            private void DeriveQuantityCommittedOut(NonUnifiedPart nonUnifiedPart)
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

            private void DeriveQuantityExpectedIn(NonUnifiedPart nonUnifiedPart)
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

        public static void NonUnifiedPartRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("6bc857a0-3411-4fe2-ab20-a8c5fcfa6000")] = new NonUnifiedPartCreationDerivation();
        }
    }
}
