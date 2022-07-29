// <copyright file="PurchaseInvoiceAwaitingApprovalDerivation.cs" company="Allors bvba">
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
    using Derivations.Rules;

    public class PurchaseInvoiceAwaitingApprovalRule : Rule
    {
        public PurchaseInvoiceAwaitingApprovalRule(MetaPopulation m) : base(m, new Guid("6a5b8d85-c783-4b24-8272-7aa43ff05094")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseInvoice.RolePattern(v => v.PurchaseInvoiceState),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>().Where(v => v.PurchaseInvoiceState.IsAwaitingApproval))
            {
                if (!@this.OpenTasks.OfType<PurchaseInvoiceApproval>().Any())
                {
                    var approval = new PurchaseInvoiceApprovalBuilder(@this.Transaction()).WithPurchaseInvoice(@this).Build();
                    approval.WorkItem = approval.PurchaseInvoice;
                }
            }
        }
    }
}
