// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Resources;

    public class QuoteCreatedCurrencyRule : Rule
    {
        public QuoteCreatedCurrencyRule(MetaPopulation m) : base(m, new Guid("383cdf87-ac0e-4ab6-998d-9f8cef6fcd83")) =>
            this.Patterns = new Pattern[]
            {
                m.Quote.RolePattern(v => v.QuoteState),
                m.Quote.RolePattern(v => v.Issuer),
                m.Quote.RolePattern(v => v.Receiver),
                m.Quote.RolePattern(v => v.AssignedCurrency),
                m.Quote.RolePattern(v => v.IssueDate),
                m.Party.RolePattern(v => v.PreferredCurrency, v => v.QuotesWhereReceiver),
                m.Organisation.RolePattern(v => v.PreferredCurrency, v => v.QuotesWhereReceiver),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var m = cycle.Transaction.Database.Context().M;

            foreach (var @this in matches.Cast<Quote>().Where(v => v.QuoteState.IsCreated))
            {
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.Receiver?.PreferredCurrency ?? @this.Receiver?.Locale?.Country?.Currency ?? @this.Issuer?.PreferredCurrency;

                if (@this.ExistIssueDate
                    && @this.ExistIssuer
                    && @this.DerivedCurrency != @this.Issuer.PreferredCurrency)
                {
                    var exchangeRate = @this.DerivedCurrency.ExchangeRatesWhereFromCurrency.Where(v => v.ValidFrom.Date <= @this.IssueDate.Date && v.ToCurrency.Equals(@this.Issuer.PreferredCurrency)).OrderByDescending(v => v.ValidFrom).FirstOrDefault();

                    if (exchangeRate == null)
                    {
                        exchangeRate = @this.Issuer.PreferredCurrency.ExchangeRatesWhereFromCurrency.Where(v => v.ValidFrom.Date <= @this.IssueDate.Date && v.ToCurrency.Equals(@this.DerivedCurrency)).OrderByDescending(v => v.ValidFrom).FirstOrDefault();
                    }

                    if (exchangeRate == null)
                    {
                        validation.AddError($"{@this}, {m.Quote.AssignedCurrency}, {ErrorMessages.CurrencyNotAllowed}");
                    }
                }
            }
        }
    }
}
