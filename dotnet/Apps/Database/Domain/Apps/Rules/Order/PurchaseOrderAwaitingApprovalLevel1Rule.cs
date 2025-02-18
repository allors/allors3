// <copyright file="PurchaseOrderAwaitingApprovalLevel1Derivation.cs" company="Allors bv">
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

    public class PurchaseOrderAwaitingApprovalLevel1Rule : Rule
    {
        public PurchaseOrderAwaitingApprovalLevel1Rule(MetaPopulation m) : base(m, new Guid("22dc128b-8487-4606-8596-4337d87ed420")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrder.RolePattern(v => v.PurchaseOrderState)
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>().Where(v => v.PurchaseOrderState.IsAwaitingApprovalLevel1))
            {
                var openTasks = @this.TasksWhereWorkItem.Where(v => !v.ExistDateClosed).ToArray();

                if (!openTasks.OfType<PurchaseOrderApprovalLevel1>().Any())
                {
                    new PurchaseOrderApprovalLevel1Builder(@this.Transaction()).WithPurchaseOrder(@this).Build();
                }
            }
        }
    }
}
