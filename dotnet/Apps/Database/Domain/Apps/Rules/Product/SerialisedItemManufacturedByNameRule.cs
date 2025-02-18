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

    public class SerialisedItemManufacturedByNameRule : Rule
    {
        public SerialisedItemManufacturedByNameRule(MetaPopulation m) : base(m, new Guid("174060ca-48c9-4329-add6-c9c1655b8f45")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.AssociationPattern(v => v.PartWhereSerialisedItem),
                m.Part.RolePattern(v => v.ManufacturedBy, v => v.SerialisedItems.ObjectType),
                m.Organisation.RolePattern(v => v.DisplayName, v => v.PartsWhereManufacturedBy.ObjectType.SerialisedItems.ObjectType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveSerialisedItemManufacturedByName(validation);
            }
        }
    }

    public static class SerialisedItemManufacturedByNameRuleExtensions
    {
        public static void DeriveSerialisedItemManufacturedByName(this SerialisedItem @this, IValidation validation) => @this.ManufacturedByName = @this.PartWhereSerialisedItem?.ManufacturedBy?.DisplayName;
    }
}
