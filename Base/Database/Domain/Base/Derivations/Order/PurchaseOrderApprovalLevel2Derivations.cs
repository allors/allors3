// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class PurchaseOrderApprovalLevel2CreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdPurchaseOrderApprovalLevel2 = changeSet.Created.Select(session.Instantiate).OfType<PurchaseOrderApprovalLevel2>();

                foreach (var purchaseOrderApprovalLevel2 in createdPurchaseOrderApprovalLevel2)
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

        public static void PurchaseOrderApprovalLevel2RegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("23450338-38dc-4eaf-a807-10186610bf2e")] = new SalesOrderCreationDerivation();
        }
    }
}
