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

    public class PurchaseOrderCreatedCurrencyRule : Rule
    {
        public PurchaseOrderCreatedCurrencyRule(MetaPopulation m) : base(m, new Guid("99e76e43-51ec-4a57-b8c6-438ad3dc9f57")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrder.RolePattern(v => v.PurchaseOrderState),
                m.PurchaseOrder.RolePattern(v => v.AssignedCurrency),
                m.PurchaseOrder.RolePattern(v => v.OrderedBy),
                m.PurchaseOrder.RolePattern(v => v.OrderDate),
                m.Organisation.RolePattern(v => v.PreferredCurrency, v => v.PurchaseOrdersWhereOrderedBy),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>().Where(v => v.PurchaseOrderState.IsCreated))
            {
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.OrderedBy?.PreferredCurrency;

                if (@this.ExistOrderDate
                    && @this.ExistOrderedBy
                    && @this.DerivedCurrency != @this.OrderedBy.PreferredCurrency)
                {
                    var exchangeRate = @this.DerivedCurrency.ExchangeRatesWhereFromCurrency.Where(v => v.ValidFrom.Date <= @this.OrderDate.Date && v.ToCurrency.Equals(@this.OrderedBy.PreferredCurrency)).OrderByDescending(v => v.ValidFrom).FirstOrDefault();

                    if (exchangeRate == null)
                    {
                        exchangeRate = @this.OrderedBy.PreferredCurrency.ExchangeRatesWhereFromCurrency.Where(v => v.ValidFrom.Date <= @this.OrderDate.Date && v.ToCurrency.Equals(@this.DerivedCurrency)).OrderByDescending(v => v.ValidFrom).FirstOrDefault();
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
