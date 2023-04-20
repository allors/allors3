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
    using Derivations.Rules;

    public class VatClauseDeniedPermissionRule : Rule
    {
        public VatClauseDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("9a58021a-e81e-4a0b-8eb7-3739b327dc0d")) =>
            this.Patterns = new Pattern[]
        {
            m.VatClause.AssociationPattern(v => v.SalesInvoicesWhereDerivedVatClause, m.VatClause),
            m.VatClause.AssociationPattern(v => v.SalesInvoiceVersionsWhereDerivedVatClause, m.VatClause),
            m.VatClause.AssociationPattern(v => v.SalesOrdersWhereDerivedVatClause, m.VatClause),
            m.VatClause.AssociationPattern(v => v.SalesOrderVersionsWhereDerivedVatClause, m.VatClause),
            m.VatClause.AssociationPattern(v => v.QuotesWhereDerivedVatClause, m.VatClause),
            m.VatClause.AssociationPattern(v => v.QuoteVersionsWhereDerivedVatClause, m.VatClause),
            m.SalesInvoice.RolePattern(v => v.DerivedVatClause, v => v.DerivedVatClause),
            m.SalesOrder.RolePattern(v => v.DerivedVatClause, v => v.DerivedVatClause),
            m.Quote.RolePattern(v => v.DerivedVatClause, v => v.DerivedVatClause),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<VatClause>())
            {
                @this.DeriveVatClauseDeniedPermission(validation);
            }
        }
    }

    public static class VatClauseDeniedPermissionRuleExtensions
    {
        public static void DeriveVatClauseDeniedPermission(this VatClause @this, IValidation validation)
        {
            var deleteRevocation = new Revocations(@this.Strategy.Transaction).VatClauseDeleteRevocation;

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
