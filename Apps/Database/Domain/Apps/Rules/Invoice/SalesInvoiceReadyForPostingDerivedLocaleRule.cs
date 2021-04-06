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

    public class SalesInvoiceReadyForPostingDerivedLocaleRule : Rule
    {
        public SalesInvoiceReadyForPostingDerivedLocaleRule(MetaPopulation m) : base(m, new Guid("fc682f78-6855-4f87-a95f-4183bcde68f9")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.SalesInvoice, m.SalesInvoice.Locale),
            new RolePattern(m.Party, m.Party.Locale) { Steps = new IPropertyType[] { this.M.Party.SalesInvoicesWhereBillToCustomer }},
            new RolePattern(m.Organisation, m.Organisation.Locale) { Steps = new IPropertyType[] { this.M.Organisation.SalesInvoicesWhereBilledFrom }},
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>().Where(v => v.SalesInvoiceState.IsReadyForPosting))
            {
                @this.DerivedLocale = @this.Locale ?? @this.BillToCustomer?.Locale ?? @this.BilledFrom?.Locale;
            }
        }
    }
}
