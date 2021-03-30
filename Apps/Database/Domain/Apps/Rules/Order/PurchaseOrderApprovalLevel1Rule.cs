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

    public class PurchaseOrderApprovalLevel1Rule : Rule
    {
        public PurchaseOrderApprovalLevel1Rule(M m) : base(m, new Guid("C2585A88-209B-4C1D-9781-04138F4CFBF7")) =>
            this.Patterns = new[]
            {
                new RolePattern(m.PurchaseOrderApprovalLevel1, m.PurchaseOrderApprovalLevel1.PurchaseOrder)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseOrderApprovalLevel1>())
            {
                @this.Title = "Approval of " + @this.PurchaseOrder.WorkItemDescription;

                @this.WorkItem = @this.PurchaseOrder;

                // Lifecycle
                if (!@this.ExistDateClosed && !@this.PurchaseOrder.PurchaseOrderState.IsAwaitingApprovalLevel1)
                {
                    @this.DateClosed = @this.Transaction().Now();
                }
            }
        }
    }
}
