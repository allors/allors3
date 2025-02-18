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

    public class SerialisedItemProductTypeNameRule : Rule
    {
        public SerialisedItemProductTypeNameRule(MetaPopulation m) : base(m, new Guid("c1a01030-5223-4bd5-a905-6fe4410236f9")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.AssociationPattern(v => v.PartWhereSerialisedItem),
                m.Part.RolePattern(v => v.ProductType, v => v.SerialisedItems.ObjectType),
                m.ProductType.RolePattern(v => v.Name, v => v.PartsWhereProductType.ObjectType.SerialisedItems.ObjectType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveSerialisedItemProductTypeName(validation);
            }
        }
    }

    public static class SerialisedItemProductTypeNameRuleExtensions
    {
        public static void DeriveSerialisedItemProductTypeName(this SerialisedItem @this, IValidation validation) => @this.ProductTypeName = @this.PartWhereSerialisedItem?.ProductType?.Name;
    }
}
