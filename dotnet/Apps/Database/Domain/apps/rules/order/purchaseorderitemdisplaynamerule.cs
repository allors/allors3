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

    public class PurchaseOrderItemDisplayNameRule : Rule
    {
        public PurchaseOrderItemDisplayNameRule(MetaPopulation m) : base(m, new Guid("a2149daf-bb84-4eef-8dab-d1e9ae33ff2e")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrder.RolePattern(v => v.DisplayName, v => v.PurchaseOrderItems),
                m.Part.RolePattern(v => v.DisplayName, v => v.PurchaseOrderItemsWherePart),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseOrderItem>())
            {
                var array = new string[] { @this.PurchaseOrderWherePurchaseOrderItem?.DisplayName, @this.Part?.DisplayName };
                @this.DisplayName = string.Join(", ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
        }
    }
}
