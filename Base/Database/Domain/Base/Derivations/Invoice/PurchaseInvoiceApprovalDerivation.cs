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

    public class PurchaseInvoiceApprovalDerivation : DomainDerivation
    {
        public PurchaseInvoiceApprovalDerivation(M m) : base(m, new Guid("5F1021C3-39B5-4BAB-936D-F7203F04281F")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(M.PurchaseInvoiceApproval.Class)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var purchaseInvoiceApproval in matches.Cast<PurchaseInvoiceApproval>())
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
}
