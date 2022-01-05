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

    public class SerialisedItemBuyerNameRule : Rule
    {
        public SerialisedItemBuyerNameRule(MetaPopulation m) : base(m, new Guid("6110c399-1f9e-4cd5-bc8e-3e60bd4488ed")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.Buyer),
                m.InternalOrganisation.RolePattern(v => v.DisplayName, v => v.SerialisedItemsWhereBuyer.SerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveSerialisedItemBuyerName(validation);
            }
        }
    }

    public static class SerialisedItemBuyerNameRuleExtensions
    {
        public static void DeriveSerialisedItemBuyerName(this SerialisedItem @this, IValidation validation) => @this.BuyerName = @this.ExistBuyer ? @this.Buyer.DisplayName : string.Empty;
    }
}
