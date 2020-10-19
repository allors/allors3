// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class PurchaseOrderApprovalLevel1Derivation : DomainDerivation
    {
        public PurchaseOrderApprovalLevel1Derivation(M m) : base(m, new Guid("C2585A88-209B-4C1D-9781-04138F4CFBF7")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(this.M.PurchaseOrderApprovalLevel1.Class)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var purchaseOrderApproval in matches.Cast<PurchaseOrderApprovalLevel1>())
            {
                purchaseOrderApproval.Title = "Approval of " + purchaseOrderApproval.PurchaseOrder.WorkItemDescription;

                purchaseOrderApproval.WorkItem = purchaseOrderApproval.PurchaseOrder;

                // Lifecycle
                if (!purchaseOrderApproval.ExistDateClosed && !purchaseOrderApproval.PurchaseOrder.PurchaseOrderState.IsAwaitingApprovalLevel1)
                {
                    purchaseOrderApproval.DateClosed = purchaseOrderApproval.Session().Now();
                }

                if (purchaseOrderApproval.Participants.Count == 0)
                {
                    // Assignments
                    var participants = purchaseOrderApproval.ExistDateClosed
                                           ? (IEnumerable<Person>)Array.Empty<Person>()
                                           : new UserGroups(purchaseOrderApproval.Strategy.Session).Administrators.Members.Select(v => (Person)v).ToArray();
                    purchaseOrderApproval.AssignParticipants(participants);
                }
            }
        }
    }
}