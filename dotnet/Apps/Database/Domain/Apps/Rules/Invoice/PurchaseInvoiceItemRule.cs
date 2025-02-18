// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class PurchaseInvoiceItemRule : Rule
    {
        public PurchaseInvoiceItemRule(MetaPopulation m) : base(m, new Guid("54b6b925-f89f-4c77-9113-4b5eb72f9cfb")) =>
            this.Patterns = new Pattern[]
            {
                m.PaymentApplication.RolePattern(v => v.AmountApplied, v => v.InvoiceItem, m.PurchaseInvoiceItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;
            var changeSet = cycle.ChangeSet;

            foreach (var @this in matches.Cast<PurchaseInvoiceItem>())
            {
                var amountPaid = 0M;
                foreach (var paymentApplication in @this.PaymentApplicationsWhereInvoiceItem)
                {
                    amountPaid += paymentApplication.AmountApplied;
                }

                if (amountPaid != @this.AmountPaid)
                {
                    @this.AmountPaid = amountPaid;
                }
            }
        }
    }
}
