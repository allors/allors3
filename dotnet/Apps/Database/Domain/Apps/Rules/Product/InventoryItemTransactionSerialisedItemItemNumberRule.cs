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

    public class InventoryItemTransactionSerialisedItemItemNumberRule : Rule
    {
        public InventoryItemTransactionSerialisedItemItemNumberRule(MetaPopulation m) : base(m, new Guid("71166ea2-854c-4335-a85b-5e6beff03294")) =>
            this.Patterns = new Pattern[]
            {
                m.InventoryItemTransaction.RolePattern(v => v.SerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<InventoryItemTransaction>())
            {
                @this.DeriveInventoryItemTransactionSerialisedItemItemNumber(validation);
            }
        }
    }

    public static class InventoryItemTransactionSerialisedItemItemNumberRuleExtensions
    {
        public static void DeriveInventoryItemTransactionSerialisedItemItemNumber(this InventoryItemTransaction @this, IValidation validation) => @this.SerialisedItemItemNumber = @this.SerialisedItem?.ItemNumber;
    }
}
