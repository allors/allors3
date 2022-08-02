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

    public class InventoryItemTransactionFacilityNameRule : Rule
    {
        public InventoryItemTransactionFacilityNameRule(MetaPopulation m) : base(m, new Guid("88ea839c-55a4-4635-b7f5-5dece4370566")) =>
            this.Patterns = new Pattern[]
            {
                m.InventoryItemTransaction.RolePattern(v => v.Facility),
                m.Facility.RolePattern(v => v.Name, v => v.InventoryItemTransactionsWhereFacility),
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

    public static class InventoryItemTransactionFacilityNameRuleExtensions
    {
        public static void DeriveInventoryItemTransactionFacilityName(this InventoryItemTransaction @this, IValidation validation) => @this.FacilityName = @this.Facility?.Name;
    }
}
