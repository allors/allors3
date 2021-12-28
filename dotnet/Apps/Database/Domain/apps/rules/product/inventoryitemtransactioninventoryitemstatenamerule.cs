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

    public class InventoryItemTransactionInventoryItemStateNameRule : Rule
    {
        public InventoryItemTransactionInventoryItemStateNameRule(MetaPopulation m) : base(m, new Guid("ae39bdba-acd6-49de-aad8-03a6219cfd90")) =>
            this.Patterns = new Pattern[]
            {
                m.InventoryItemTransaction.RolePattern(v => v.NonSerialisedInventoryItemState),
                m.InventoryItemTransaction.RolePattern(v => v.SerialisedInventoryItemState),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<InventoryItemTransaction>())
            {
                @this.DeriveInventoryItemTransactionFacilityName(validation);
            }
        }
    }

    public static class InventoryItemTransactionInventoryItemStateNameRuleExtensions
    {
        public static void DeriveInventoryItemTransactionInventoryItemStateNameRule(this InventoryItemTransaction @this, IValidation validation)
        {
            if (@this.ExistNonSerialisedInventoryItemState)
            {
                @this.InventoryItemStateName = @this.NonSerialisedInventoryItemState?.Name;
            }
            else
            {
                @this.InventoryItemStateName = @this.SerialisedInventoryItemState?.Name;
            }
        }
    }
}
