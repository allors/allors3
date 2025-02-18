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

    public class InventoryItemTransactionWorkEffortNumberRule : Rule
    {
        public InventoryItemTransactionWorkEffortNumberRule(MetaPopulation m) : base(m, new Guid("6cbdb2c6-b81e-436b-b846-06227808801b")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkEffortInventoryAssignment.RolePattern(v => v.InventoryItemTransactions, v => v.InventoryItemTransactions),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<InventoryItemTransaction>())
            {
                @this.DeriveInventoryItemTransactionWorkEffortNumber(validation);
            }
        }
    }

    public static class InventoryItemTransactionWorkEffortNumberRuleExtensions
    {
        public static void DeriveInventoryItemTransactionWorkEffortNumber(this InventoryItemTransaction @this, IValidation validation)
        {
            // Assign only once
            if (!@this.ExistWorkEffortNumber)
            {
                @this.WorkEffortNumber = @this.WorkEffortInventoryAssignmentWhereInventoryItemTransaction?.WorkEffortNumber;
            }
        }
    }
}
