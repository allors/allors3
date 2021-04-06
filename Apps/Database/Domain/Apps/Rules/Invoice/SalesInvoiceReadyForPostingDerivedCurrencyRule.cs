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

    public class SalesInvoiceReadyForPostingDerivedCurrencyRule : Rule
    {
        public SalesInvoiceReadyForPostingDerivedCurrencyRule(MetaPopulation m) : base(m, new Guid("1f921f79-9a41-4bda-8aaf-e7474c067207")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.SalesInvoice, m.SalesInvoice.AssignedCurrency),
            new RolePattern(m.Party, m.Party.Locale) { Steps = new IPropertyType[] { this.M.Party.SalesInvoicesWhereBillToCustomer }},
            new RolePattern(m.Party, m.Party.PreferredCurrency) { Steps = new IPropertyType[] { this.M.Party.SalesInvoicesWhereBillToCustomer }},
            new RolePattern(m.Organisation, m.Organisation.PreferredCurrency) { Steps = new IPropertyType[] { this.M.Organisation.SalesInvoicesWhereBilledFrom }},

        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>().Where(v => v.SalesInvoiceState.IsReadyForPosting))
            {
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.BillToCustomer?.PreferredCurrency ?? @this.BillToCustomer?.Locale?.Country?.Currency ?? @this.BilledFrom?.PreferredCurrency;
            }
        }
    }
}
