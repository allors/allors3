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

    public class PurchaseOrderDisplayNameRule : Rule
    {
        public PurchaseOrderDisplayNameRule(MetaPopulation m) : base(m, new Guid("68428b63-67f6-4525-bc94-01d8fd45ced9")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrder.RolePattern(v => v.OrderNumber),
                m.PurchaseOrder.RolePattern(v => v.TakenViaSupplier),
                m.Organisation.RolePattern(v => v.DisplayName, v => v.PurchaseOrdersWhereTakenViaSupplier),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>())
            {
                @this.DerivePurchaseOrderDisplayName(validation);
            }
        }
    }

    public static class PurchaseOrderDisplayNameRuleExtensions
    {
        public static void DerivePurchaseOrderDisplayName(this PurchaseOrder @this, IValidation validation)
        {
            var array = new string[] { @this?.OrderNumber, @this?.TakenViaSupplier?.DisplayName };

            if (array.Any(s => !string.IsNullOrEmpty(s)))
            {
                @this.DisplayName = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
            else
            {
                @this.RemoveDisplayName();
            }
        }
    }
}
