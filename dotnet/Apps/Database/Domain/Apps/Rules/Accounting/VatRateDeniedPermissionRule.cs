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

    public class VatRateDeniedPermissionRule : Rule
    {
        public VatRateDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("4ea3f974-f75f-41a4-8ea6-66cd9604afe2")) =>
            this.Patterns = new Pattern[]
        {
            m.VatRate.AssociationPattern(v => v.InvoicesWhereDerivedVatRate, m.VatRate),
            m.VatRate.AssociationPattern(v => v.OrdersWhereDerivedVatRate, m.VatRate),
            m.VatRate.AssociationPattern(v => v.PriceablesWhereVatRate, m.VatRate),
            m.VatRate.AssociationPattern(v => v.PriceableVersionsWhereVatRate, m.VatRate),
            m.VatRate.AssociationPattern(v => v.QuotesWhereDerivedVatRate, m.VatRate),
            m.Invoice.RolePattern(v => v.DerivedVatRate, v => v.DerivedVatRate),
            m.Order.RolePattern(v => v.DerivedVatRate, v => v.DerivedVatRate),
            m.Quote.RolePattern(v => v.DerivedVatRate, v => v.DerivedVatRate),
            m.Priceable.RolePattern(v => v.VatRate, v => v.VatRate),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<VatRate>())
            {
                @this.DeriveVatRateDeniedPermission(validation);
            }
        }
    }

    public static class VatRateDeniedPermissionRuleExtensions
    {
        public static void DeriveVatRateDeniedPermission(this VatRate @this, IValidation validation)
        {
            var deleteRevocation = new Revocations(@this.Strategy.Transaction).VatRateDeleteRevocation;

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
