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

    public class SerialisedItemPartNameRule : Rule
    {
        public SerialisedItemPartNameRule(MetaPopulation m) : base(m, new Guid("4ee9c110-1d11-4345-95dd-d25f162294f8")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.AssociationPattern(v => v.PartWhereSerialisedItem),
                m.Part.RolePattern(v => v.Name, v => v.SerialisedItems.SerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveSerialisedItemPartName(validation);
            }
        }
    }

    public static class SerialisedItemPartNameRuleExtensions
    {
        public static void DeriveSerialisedItemPartName(this SerialisedItem @this, IValidation validation) => @this.PartName = @this.PartWhereSerialisedItem?.Name;
    }
}
