// <copyright file="Domain.cs" company="Allors bv">
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

    public class SerialisedItemBrandNameRule : Rule
    {
        public SerialisedItemBrandNameRule(MetaPopulation m) : base(m, new Guid("44553ef2-ccee-4d69-8af9-5faef106c29f")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.AssociationPattern(v => v.PartWhereSerialisedItem),
                m.Part.RolePattern(v => v.Brand, v => v.SerialisedItems.ObjectType),
                m.Brand.RolePattern(v => v.Name, v => v.PartsWhereBrand.ObjectType.SerialisedItems.ObjectType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveSerialisedItemBrandName(validation);
            }
        }
    }

    public static class SerialisedItemBrandNameRuleExtensions
    {
        public static void DeriveSerialisedItemBrandName(this SerialisedItem @this, IValidation validation) => @this.BrandName = @this.PartWhereSerialisedItem?.Brand?.Name;
    }
}
