// <copyright file="PartDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations.Rules;
    using Meta;

    public class PartRule : Rule
    {
        public PartRule(MetaPopulation m) : base(m, new Guid("4F894B49-4922-4FC8-9172-DC600CCDB1CA")) =>
            this.Patterns = new Pattern[]
            {
                m.Part.RolePattern(v => v.ProductType),
                m.ProductType.RolePattern(v => v.SerialisedItemCharacteristicTypes, v => v.PartsWhereProductType),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var m = cycle.Transaction.Database.Services.Get<MetaPopulation>();

            foreach (var @this in matches.Cast<Part>())
            {
                var characteristicsToDelete = @this.SerialisedItemCharacteristics.ToList();

                if (@this.ExistProductType)
                {
                    foreach (var characteristicType in @this.ProductType.SerialisedItemCharacteristicTypes)
                    {
                        var characteristic = @this.SerialisedItemCharacteristics.FirstOrDefault(v => Equals(v.SerialisedItemCharacteristicType, characteristicType));
                        if (characteristic == null)
                        {
                            @this.AddSerialisedItemCharacteristic(
                                                        new SerialisedItemCharacteristicBuilder(@this.Strategy.Transaction)
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
                    @this.RemoveSerialisedItemCharacteristic(characteristic);
                }
            }
        }
    }
}
