// <copyright file="PurchaseOrderAwaitingApprovalLevel1Derivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class PurchaseOrderAwaitingApprovalLevel1Derivation : DomainDerivation
    {
        public PurchaseOrderAwaitingApprovalLevel1Derivation(M m) : base(m, new Guid("22dc128b-8487-4606-8596-4337d87ed420")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseOrder.PurchaseOrderState)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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
