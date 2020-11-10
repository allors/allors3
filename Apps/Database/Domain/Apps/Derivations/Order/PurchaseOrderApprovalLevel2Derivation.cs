// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class PurchaseOrderApprovalLevel2Derivation : DomainDerivation
    {
        public PurchaseOrderApprovalLevel2Derivation(M m) : base(m, new Guid("5AE4FAD8-8BF0-4EB0-8051-5564E874ED10")) =>
            this.Patterns = new[]
            {
                new ChangedPattern(this.M.PurchaseOrderApprovalLevel2.PurchaseOrder)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseOrderApprovalLevel2>())
            {
                @this.Title = "Approval of " + @this.PurchaseOrder.WorkItemDescription;

                @this.WorkItem = @this.PurchaseOrder;

                // Lifecycle
                if (!@this.ExistDateClosed && !@this.PurchaseOrder.PurchaseOrderState.IsAwaitingApprovalLevel2)
                {
                    @this.DateClosed = @this.Session().Now();
                }

                @this.DeriveParticipants();
            }
        }
    }
}
