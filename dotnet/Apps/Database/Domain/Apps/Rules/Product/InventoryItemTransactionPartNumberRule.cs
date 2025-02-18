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

    public class InventoryItemTransactionPartNumberRule : Rule
    {
        public InventoryItemTransactionPartNumberRule(MetaPopulation m) : base(m, new Guid("ab2731c4-11eb-4501-8323-3fe458d6a500")) =>
            this.Patterns = new Pattern[]
            {
                m.InventoryItemTransaction.RolePattern(v => v.Part),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<InventoryItemTransaction>())
            {
                @this.DeriveInventoryItemTransactionPartNumber(validation);
            }
        }
    }

    public static class InventoryItemTransactionPartNumberRuleExtensions
    {
        public static void DeriveInventoryItemTransactionPartNumber(this InventoryItemTransaction @this, IValidation validation) => @this.PartNumber = @this.Part?.ProductNumber;
    }
}
