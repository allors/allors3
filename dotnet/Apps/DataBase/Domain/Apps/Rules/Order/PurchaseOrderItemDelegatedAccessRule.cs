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

    public class PurchaseOrderItemDelegatedAccessRule : Rule
    {
        public PurchaseOrderItemDelegatedAccessRule(MetaPopulation m) : base(m, new Guid("cb5e2f2c-f8c4-4c3f-bd6e-1713fe1b6162")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrder.RolePattern(v => v.PurchaseOrderItems),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseOrder>())
            {
                foreach (var salesOrderItem in @this.PurchaseOrderItems)
                {
                    salesOrderItem.DelegatedAccess = @this;
                }
            }
        }
    }
}
