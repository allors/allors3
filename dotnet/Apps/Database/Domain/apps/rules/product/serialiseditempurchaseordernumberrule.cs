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

    public class SerialisedItemPurchaseOrderNumberRule : Rule
    {
        public SerialisedItemPurchaseOrderNumberRule(MetaPopulation m) : base(m, new Guid("fb0ed763-b05c-4959-befd-1cac701c74ae")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.PurchaseOrder),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveSerialisedItemPurchaseOrderNumber(validation);
            }
        }
    }

    public static class SerialisedItemPurchaseOrderNumberRuleExtensions
    {
        public static void DeriveSerialisedItemPurchaseOrderNumber(this SerialisedItem @this, IValidation validation) => @this.PurchaseOrderNumber = @this.PurchaseOrder?.OrderNumber;
    }
}
