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
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class SerialisedItemRule : Rule
    {
        public SerialisedItemRule(MetaPopulation m) : base(m, new Guid("A871B4BB-3285-418F-9E10-5A786A6284DA")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.AcquiredDate),
                m.SerialisedItem.RolePattern(v => v.AcquisitionYear),
                m.SerialisedItem.RolePattern(v => v.SerialNumber),
                m.SerialisedItem.RolePattern(v => v.PurchaseOrder),
                m.SerialisedItem.RolePattern(v => v.SerialisedItemAvailability),
                m.Part.RolePattern(v => v.ProductType, v => v.SerialisedItems),
                m.ProductType.RolePattern(v => v.SerialisedItemCharacteristicTypes, v => v.PartsWhereProductType.Part.SerialisedItems),
                m.SerialisedItem.AssociationPattern(v => v.PartWhereSerialisedItem),
                m.Part.AssociationPattern(v => v.SupplierOfferingsWherePart, v => v.SerialisedItems),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                validation.AssertExistsAtMostOne(@this, @this.Meta.AcquiredDate, @this.Meta.AcquisitionYear);

                var doubles = @this.PartWhereSerialisedItem?.SerialisedItems.Where(v =>
                    !string.IsNullOrEmpty(v.SerialNumber)
                    && v.SerialNumber != "."
                    && v.SerialNumber.Equals(@this.SerialNumber)).ToArray();
                if (doubles?.Length > 1)
                {
                    validation.AddError($"{@this} {@this.Meta.SerialNumber} {ErrorMessages.SameSerialNumber}");
                }

                var characteristicsToDelete = @this.SerialisedItemCharacteristics.ToList();
                var part = @this.PartWhereSerialisedItem;

                if (@this.ExistPartWhereSerialisedItem && part.ExistProductType)
                {
                    foreach (SerialisedItemCharacteristicType characteristicType in part.ProductType.SerialisedItemCharacteristicTypes)
                    {
                        var characteristic = @this.SerialisedItemCharacteristics.FirstOrDefault(v2 => Equals(v2.SerialisedItemCharacteristicType, characteristicType));
                        if (characteristic == null)
                        {
                            var newCharacteristic = new SerialisedItemCharacteristicBuilder(@this.Strategy.Transaction)
                                .WithSerialisedItemCharacteristicType(characteristicType).Build();
                            @this.AddSerialisedItemCharacteristic(newCharacteristic);

                            var partCharacteristics = part.SerialisedItemCharacteristics;
                            partCharacteristics.Filter.AddEquals(this.M.SerialisedItemCharacteristic.SerialisedItemCharacteristicType, characteristicType);
                            var fromPart = partCharacteristics.FirstOrDefault();

                            if (fromPart != null)
                            {
                                newCharacteristic.Value = fromPart.Value;
                            }
                        }
                        else
                        {
                            characteristicsToDelete.Remove(characteristic);
                        }
                    }
                }

                foreach (var characteristic in characteristicsToDelete)
                {
                    @this.RemoveSerialisedItemCharacteristic(characteristic);
                }
            }
        }
    }
}
