// <copyright file="InventoryItemSearchStringDerivation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class SerialisedItemCharacteristicSearchStringRule : Rule
    {
        public SerialisedItemCharacteristicSearchStringRule(MetaPopulation m) : base(m, new Guid("b3e95031-b4dc-43b4-b4fd-13083e216d67")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItemCharacteristic.RolePattern(v => v.SerialisedItemCharacteristicType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItemCharacteristic>())
            {
                @this.DeriveSerialisedItemCharacteristicSearchString(validation);
            }
        }
    }

    public static class SerialisedItemCharacteristicSearchStringRuleExtensions
    {
        public static void DeriveSerialisedItemCharacteristicSearchString(this SerialisedItemCharacteristic @this, IValidation validation)
        {
            var array = new string[] {
                    @this.SerialisedItemCharacteristicType?.Name,
                };

            if (array.Any(s => !string.IsNullOrEmpty(s)))
            {
                @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
            else
            {
                @this.RemoveSearchString();
            }
        }
    }
}
