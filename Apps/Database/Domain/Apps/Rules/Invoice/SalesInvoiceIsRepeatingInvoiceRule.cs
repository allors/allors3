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
    using Resources;

    public class SalesInvoiceIsRepeatingInvoiceRule : Rule
    {
        public SalesInvoiceIsRepeatingInvoiceRule(MetaPopulation m) : base(m, new Guid("c316e3a6-03a9-414a-a12f-319db4356475")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.RepeatingSalesInvoice, m.RepeatingSalesInvoice.NextExecutionDate) { Steps =  new IPropertyType[] {m.RepeatingSalesInvoice.Source} },
            new RolePattern(m.RepeatingSalesInvoice, m.RepeatingSalesInvoice.FinalExecutionDate) { Steps =  new IPropertyType[] {m.RepeatingSalesInvoice.Source} },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                @this.IsRepeatingInvoice = @this.ExistRepeatingSalesInvoiceWhereSource
                        && (!@this.RepeatingSalesInvoiceWhereSource.ExistFinalExecutionDate
                            || @this.RepeatingSalesInvoiceWhereSource.FinalExecutionDate.Value.Date >= @this.Strategy.Transaction.Now().Date);
            }
        }
    }
}
