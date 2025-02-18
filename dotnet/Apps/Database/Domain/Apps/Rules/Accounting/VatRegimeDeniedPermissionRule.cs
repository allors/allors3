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

    public class VatRegimeDeniedPermissionRule : Rule
    {
        public VatRegimeDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("e7b39743-7e8f-4224-bc05-215ab5bb7c09")) =>
            this.Patterns = new Pattern[]
        {
            m.VatRegime.AssociationPattern(v => v.CountriesWhereDerivedVatRegime, m.VatRegime),
            m.VatRegime.AssociationPattern(v => v.InvoicesWhereDerivedVatRegime, m.VatRegime),
            m.VatRegime.AssociationPattern(v => v.InvoiceVersionsWhereDerivedVatRegime, m.VatRegime),
            m.VatRegime.AssociationPattern(v => v.OrdersWhereDerivedVatRegime, m.VatRegime),
            m.VatRegime.AssociationPattern(v => v.OrderVersionsWhereDerivedVatRegime, m.VatRegime),
            m.VatRegime.AssociationPattern(v => v.PriceablesWhereDerivedVatRegime, m.VatRegime),
            m.VatRegime.AssociationPattern(v => v.PriceableVersionsWhereDerivedVatRegime, m.VatRegime),
            m.VatRegime.AssociationPattern(v => v.QuotesWhereDerivedVatRegime, m.VatRegime),
            m.VatRegime.AssociationPattern(v => v.QuoteVersionsWhereDerivedVatRegime, m.VatRegime),
            m.VatRegime.AssociationPattern(v => v.ProductFeaturesWhereVatRegime, m.VatRegime),
            m.VatRegime.AssociationPattern(v => v.ProductsWhereVatRegime, m.VatRegime),
            m.Country.RolePattern(v => v.DerivedVatRegimes, v => v.DerivedVatRegimes),
            m.Invoice.RolePattern(v => v.DerivedVatRegime, v => v.DerivedVatRegime),
            m.Order.RolePattern(v => v.DerivedVatRegime, v => v.DerivedVatRegime),
            m.Quote.RolePattern(v => v.DerivedVatRegime, v => v.DerivedVatRegime),
            m.Priceable.RolePattern(v => v.DerivedVatRegime, v => v.DerivedVatRegime),
            m.ProductFeature.RolePattern(v => v.VatRegime, v => v.VatRegime),
            m.Product.RolePattern(v => v.VatRegime, v => v.VatRegime),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<VatRegime>())
            {
                @this.DeriveVatRegimeDeniedPermission(validation);
            }
        }
    }

    public static class VatRegimeDeniedPermissionRuleExtensions
    {
        public static void DeriveVatRegimeDeniedPermission(this VatRegime @this, IValidation validation)
        {
            var deleteRevocation = new Revocations(@this.Strategy.Transaction).VatRegimeDeleteRevocation;

            if (@this.IsDeletable)
            {
                @this.RemoveRevocation(deleteRevocation);
            }
            else
            {
                @this.AddRevocation(deleteRevocation);
            }
        }
    }
}
