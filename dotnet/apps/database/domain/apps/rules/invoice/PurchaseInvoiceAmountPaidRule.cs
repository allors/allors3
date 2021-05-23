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

    public class PurchaseInvoiceAmountPaidRule : Rule
    {
        public PurchaseInvoiceAmountPaidRule(MetaPopulation m) : base(m, new Guid("d8166960-ce4a-44ad-9f3b-599d4b447cb3")) =>
            this.Patterns = new Pattern[]
            {
                m.Invoice.AssociationPattern(v => v.PaymentApplicationsWhereInvoice, m.PurchaseInvoice),
                m.PurchaseInvoiceItem.RolePattern(v => v.AmountPaid, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>())
            {
                @this.AmountPaid = @this.PaymentApplicationsWhereInvoice.Sum(v => v.AmountApplied);

                //// Perhaps payments are recorded at the item level.
                if (@this.AmountPaid == 0)
                {
                    @this.AmountPaid = @this.ValidInvoiceItems.Sum(v => v.AmountPaid);
                }
            }
        }
    }
}
