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
                new ChangedPattern(m.NonUnifiedPart.ProductIdentifications),
                new ChangedPattern(m.NonUnifiedPart.DefaultFacility),
                new ChangedPattern(m.NonUnifiedPart.UnitOfMeasure),
                new ChangedPattern(m.NonUnifiedPart.ProductType),
                new ChangedPattern(m.ProductType.SerialisedItemCharacteristicTypes) { Steps = new IPropertyType[]{ this.M.ProductType.PartsWhereProductType }, OfType = this.M.NonUnifiedPart.Class},
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

                @this.ProductNumber = partIdentification?.Identification;

                this.ProductCharacteristics(@this);
            }
        }

        private void SyncDefaultInventoryItem(NonUnifiedPart nonUnifiedPart)
        {
            if (nonUnifiedPart.InventoryItemKind.IsNonSerialised)
            {
                var inventoryItems = nonUnifiedPart.InventoryItemsWherePart;

                if (!inventoryItems.Any(i => i.ExistFacility && i.Facility.Equals(nonUnifiedPart.DefaultFacility)
                                            && i.ExistUnitOfMeasure && i.UnitOfMeasure.Equals(nonUnifiedPart.UnitOfMeasure)))
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
    }
}
