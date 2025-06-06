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

    public class SalesInvoiceTemporaryInvoiceNumberRule : Rule
    {
        public SalesInvoiceTemporaryInvoiceNumberRule(MetaPopulation m) : base(m, new Guid("8aac4637-663a-4176-b0c3-a3147799469c")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoice.RolePattern(v => v.Store),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                if (!@this.ExistInvoiceNumber && @this.ExistStore)
                {
                    @this.InvoiceNumber = @this.Store.NextTemporaryInvoiceNumber();
                    @this.SortableInvoiceNumber = NumberFormatter.SortableNumber(null, @this.InvoiceNumber, @this.InvoiceDate.Year.ToString());
                }
            }
        }
    }
}
