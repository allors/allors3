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
    using Resources;

    public static partial class DabaseExtensions
    {
        public class PurchaseInvoiceApprovalCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdPurchaseInvoiceApprovals = changeSet.Created.Select(v=>v.GetObject()).OfType<PurchaseInvoiceApproval>();

                foreach(var purchaseInvoiceApproval in createdPurchaseInvoiceApprovals)
                {
                    purchaseInvoiceApproval.Title = "Approval of " + purchaseInvoiceApproval.PurchaseInvoice.WorkItemDescription;

                    purchaseInvoiceApproval.WorkItem = purchaseInvoiceApproval.PurchaseInvoice;

                    // Lifecycle
                    if (!purchaseInvoiceApproval.ExistDateClosed && !purchaseInvoiceApproval.PurchaseInvoice.PurchaseInvoiceState.IsAwaitingApproval)
                    {
                        purchaseInvoiceApproval.DateClosed = purchaseInvoiceApproval.Session().Now();
                    }

                    if (purchaseInvoiceApproval.Participants.Count == 0)
                    {
                        // Assignments
                        var participants = purchaseInvoiceApproval.ExistDateClosed
                            ? (IEnumerable<Person>)Array.Empty<Person>()
                            : new UserGroups(purchaseInvoiceApproval.Strategy.Session).Administrators.Members.Select(v => (Person)v).ToArray();
                        purchaseInvoiceApproval.AssignParticipants(participants);
                    }
                }

            }
        }

        public static void PurchaseInvoiceApprovalRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("d6ecdbe1-7041-4e2d-ba6f-89cf28956ef9")] = new PurchaseInvoiceApprovalCreationDerivation();
        }
    }
}
