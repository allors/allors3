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

    public class SalesInvoiceDueDateRule : Rule
    {
        public SalesInvoiceDueDateRule(MetaPopulation m) : base(m, new Guid("cb91f705-006d-43d8-8ca7-4ac0fe78c38f")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoice.RolePattern(v => v.InvoiceDate),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                @this.PaymentDays = @this.PaymentNetDays;

                if (@this.ExistInvoiceDate)
                {
                    @this.DueDate = @this.InvoiceDate.AddDays(@this.PaymentNetDays);
                }
            }
        }
    }
}
