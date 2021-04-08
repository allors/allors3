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
            m.SalesInvoice.RolePattern(v => v.BilledFrom),
            m.SalesInvoice.RolePattern(v => v.AssignedCurrency),
            m.Party.RolePattern(v => v.Locale, v => v.SalesInvoicesWhereBillToCustomer),
            m.Party.RolePattern(v => v.PreferredCurrency, v => v.SalesInvoicesWhereBillToCustomer),
            m.Organisation.RolePattern(v => v.PreferredCurrency, v => v.SalesInvoicesWhereBilledFrom)
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
