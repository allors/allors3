// <copyright file="Domain.cs" company="Allors bvba">
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

    public class PurchaseInvoiceApprovalRule : Rule
    {
        public PurchaseInvoiceApprovalRule(MetaPopulation m) : base(m, new Guid("5F1021C3-39B5-4BAB-936D-F7203F04281F")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseInvoiceApproval.RolePattern(v => v.DateClosed),
                m.PurchaseInvoiceApproval.RolePattern(v => v.PurchaseInvoice),
                m.PurchaseInvoice.RolePattern(v => v.PurchaseInvoiceState, v => v.PurchaseInvoiceApprovalsWherePurchaseInvoice),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseInvoiceApproval>())
            {
                @this.Title = "Approval of " + @this.PurchaseInvoice.WorkItemDescription;

                @this.WorkItem = @this.PurchaseInvoice;

                // Services
                if (!@this.ExistDateClosed && !@this.PurchaseInvoice.PurchaseInvoiceState.IsAwaitingApproval)
                {
                    @this.DateClosed = @this.Transaction().Now();
                }
            }
        }
    }
}
