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
    using Derivations.Rules;
    using Resources;

    public class SalesOrderProvisionalCurrencyRule : Rule
    {
        public SalesOrderProvisionalCurrencyRule(MetaPopulation m) : base(m, new Guid("d61da60f-f6aa-4103-a8cc-eefe74c0a27e")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesOrder.RolePattern(v => v.SalesOrderState),
                m.SalesOrder.RolePattern(v => v.AssignedCurrency),
                m.SalesOrder.RolePattern(v => v.BillToCustomer),
                m.SalesOrder.RolePattern(v => v.TakenBy),
                m.SalesOrder.RolePattern(v => v.OrderDate),
                m.Party.RolePattern(v => v.PreferredCurrency, v => v.SalesOrdersWhereBillToCustomer),
                m.Organisation.RolePattern(v => v.PreferredCurrency, v => v.SalesOrdersWhereTakenBy),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<SalesOrder>().Where(v => v.SalesOrderState.IsProvisional))
            {
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.BillToCustomer?.PreferredCurrency ?? @this.BillToCustomer?.Locale?.Country?.Currency ?? @this.TakenBy?.PreferredCurrency;

                if (@this.ExistOrderDate
                    && @this.ExistTakenBy
                    && @this.DerivedCurrency != @this.TakenBy.PreferredCurrency)
                {
                    var exchangeRate = @this.DerivedCurrency.ExchangeRatesWhereFromCurrency.Where(v => v.ValidFrom.Date <= @this.OrderDate.Date && v.ToCurrency.Equals(@this.TakenBy.PreferredCurrency)).OrderByDescending(v => v.ValidFrom).FirstOrDefault();

                    if (exchangeRate == null)
                    {
                        exchangeRate = @this.TakenBy.PreferredCurrency.ExchangeRatesWhereFromCurrency.Where(v => v.ValidFrom.Date <= @this.OrderDate.Date && v.ToCurrency.Equals(@this.DerivedCurrency)).OrderByDescending(v => v.ValidFrom).FirstOrDefault();
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
