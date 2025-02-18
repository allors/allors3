// <copyright file="NonUnifiedPartDerivation.cs" company="Allors bv">
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

    public class SerialisedItemCharacteristicTypeDisplayNameRule : Rule
    {
        public SerialisedItemCharacteristicTypeDisplayNameRule(MetaPopulation m) : base(m, new Guid("b0928f9d-720d-4f0c-b365-e8a482b3b28a")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItemCharacteristicType.RolePattern(v => v.Name),
                m.SerialisedItemCharacteristicType.RolePattern(v => v.UnitOfMeasure),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItemCharacteristicType>())
            {
                @this.DeriveSerialisedItemCharacteristicTypeDisplayName(validation);
            }
        }
    }

    public static class SerialisedItemCharacteristicTypeDisplayNameRuleExtensions
    {
        public static void DeriveSerialisedItemCharacteristicTypeDisplayName(this SerialisedItemCharacteristicType @this, IValidation validation)
        {
            @this.DisplayName = @this.ExistUnitOfMeasure ?
                @this.Name + " (" + @this.UnitOfMeasure.Abbreviation + ")"
                : @this.Name ?? "";
        }
    }
}
