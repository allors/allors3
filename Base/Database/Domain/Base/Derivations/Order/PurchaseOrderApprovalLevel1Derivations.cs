// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static partial class DabaseExtensions
    {
        public class PurchaseOrderApprovalLevel1CreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdPurchaseOrderApprovals = changeSet.Created.Select(v=>v.GetObject()).OfType<PurchaseOrderApprovalLevel1>();

                foreach(var purchaseOrderApproval in createdPurchaseOrderApprovals)
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

        public static void PurchaseOrderApprovalLevel1RegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("6fb0e972-ee23-49a2-be69-e72ed2949cc7")] = new PurchaseOrderApprovalLevel1CreationDerivation();
        }
    }
}
