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

    public class SalesInvoiceDueDateRule : Rule
    {
        public SalesInvoiceDueDateRule(MetaPopulation m) : base(m, new Guid("cb91f705-006d-43d8-8ca7-4ac0fe78c38f")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoice.RolePattern(v => v.InvoiceDate),
            m.SalesInvoice.RolePattern(v => v.SalesTerms),
            m.SalesTerm.RolePattern(v => v.TermValue, v => v.InvoiceWhereSalesTerm, m.SalesInvoice),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
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
