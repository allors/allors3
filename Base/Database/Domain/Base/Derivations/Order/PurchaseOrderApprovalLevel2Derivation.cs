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
                new CreatedPattern(M.PurchaseOrderApprovalLevel2.Class)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var purchaseOrderApprovalLevel2 in matches.Cast<PurchaseOrderApprovalLevel2>())
            {
                purchaseOrderApprovalLevel2.Title = "Approval of " + purchaseOrderApprovalLevel2.PurchaseOrder.WorkItemDescription;

                purchaseOrderApprovalLevel2.WorkItem = purchaseOrderApprovalLevel2.PurchaseOrder;

                // Lifecycle
                if (!purchaseOrderApprovalLevel2.ExistDateClosed && !purchaseOrderApprovalLevel2.PurchaseOrder.PurchaseOrderState.IsAwaitingApprovalLevel2)
                {
                    purchaseOrderApprovalLevel2.DateClosed = purchaseOrderApprovalLevel2.Session().Now();
                }

                if (purchaseOrderApprovalLevel2.Participants.Count == 0)
                {
                    // Assignments
                    var participants = purchaseOrderApprovalLevel2.ExistDateClosed
                                           ? (IEnumerable<Person>)Array.Empty<Person>()
                                           : new UserGroups(purchaseOrderApprovalLevel2.Strategy.Session).Administrators.Members.Select(v => (Person)v).ToArray();
                    purchaseOrderApprovalLevel2.AssignParticipants(participants);
                }
            }
        }
    }
}
