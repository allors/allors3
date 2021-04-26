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

    public class PurchaseInvoiceCreatedCurrencyRule : Rule
    {
        public PurchaseInvoiceCreatedCurrencyRule(MetaPopulation m) : base(m, new Guid("488995ea-3b77-4c84-8679-fe0a6f071feb")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseInvoice.RolePattern(v => v.AssignedCurrency),
                m.PurchaseInvoice.RolePattern(v => v.BilledTo),
                m.PurchaseInvoice.RolePattern(v => v.InvoiceDate),
                m.Organisation.RolePattern(v => v.PreferredCurrency, v => v.PurchaseInvoicesWhereBilledTo),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>().Where(v => v.PurchaseInvoiceState.IsCreated))
            {
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.BilledTo?.PreferredCurrency;

                if (@this.ExistInvoiceDate
                    && @this.ExistBilledTo
                    && @this.DerivedCurrency != @this.BilledTo.PreferredCurrency)
                {
                    var exchangeRate = @this.DerivedCurrency.ExchangeRatesWhereFromCurrency.Where(v => v.ValidFrom <= @this.InvoiceDate && v.ToCurrency.Equals(@this.BilledTo.PreferredCurrency)).OrderByDescending(v => v.ValidFrom).FirstOrDefault();

                    if (exchangeRate == null)
                    {
                        exchangeRate = @this.BilledTo.PreferredCurrency.ExchangeRatesWhereFromCurrency.Where(v => v.ValidFrom <= @this.InvoiceDate && v.ToCurrency.Equals(@this.DerivedCurrency)).OrderByDescending(v => v.ValidFrom).FirstOrDefault();
                    }

                    if (exchangeRate == null)
                    {
                        validation.AddError($"{@this}, {@this.Meta.AssignedCurrency}, {ErrorMessages.CurrencyNotAllowed}");
                    }
                }
            }
        }
    }
}
