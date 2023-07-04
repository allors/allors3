// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

    public class SerialisedItemModelNameRule : Rule
    {
        public SerialisedItemModelNameRule(MetaPopulation m) : base(m, new Guid("d7cda333-7b42-4a0e-8eb5-d23e364da0a9")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.AssociationPattern(v => v.PartWhereSerialisedItem),
                m.Part.RolePattern(v => v.Model, v => v.SerialisedItems.ObjectType),
                m.Model.RolePattern(v => v.Name, v => v.PartsWhereModel.ObjectType.SerialisedItems.ObjectType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveSerialisedItemModelName(validation);
            }
        }
    }

    public static class SerialisedItemModelNameRuleExtensions
    {
        public static void DeriveSerialisedItemModelName(this SerialisedItem @this, IValidation validation) => @this.ModelName = @this.PartWhereSerialisedItem?.Model?.Name;
    }
}
