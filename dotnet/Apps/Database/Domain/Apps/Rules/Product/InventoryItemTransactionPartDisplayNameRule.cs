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

    public class InventoryItemTransactionPartDisplayNameRule : Rule
    {
        public InventoryItemTransactionPartDisplayNameRule(MetaPopulation m) : base(m, new Guid("894bb293-5de2-45b1-ba60-5144a827149f")) =>
            this.Patterns = new Pattern[]
            {
                m.InventoryItemTransaction.RolePattern(v => v.Part),
                m.Part.RolePattern(v => v.DisplayName, v => v.InventoryItemTransactionsWherePart),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<InventoryItemTransaction>())
            {
                @this.DeriveInventoryItemTransactionPartDisplayName(validation);
            }
        }
    }

    public static class InventoryItemTransactionPartDisplayNameRuleExtensions
    {
        public static void DeriveInventoryItemTransactionPartDisplayName(this InventoryItemTransaction @this, IValidation validation) => @this.PartDisplayName = @this.Part?.DisplayName;
    }
}
