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

    public class IrpfRateDeniedPermissionRule : Rule
    {
        public IrpfRateDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("1ac534d4-b653-4080-a607-d6644b6eef28")) =>
            this.Patterns = new Pattern[]
        {
            m.IrpfRate.AssociationPattern(v => v.InvoiceItemsWhereIrpfRate, m.IrpfRate),
            m.IrpfRate.AssociationPattern(v => v.InvoiceItemVersionsWhereIrpfRate, m.IrpfRate),
            m.IrpfRate.AssociationPattern(v => v.InvoicesWhereDerivedIrpfRate, m.IrpfRate),
            m.IrpfRate.AssociationPattern(v => v.OrderItemsWhereIrpfRate, m.IrpfRate),
            m.IrpfRate.AssociationPattern(v => v.OrderItemVersionsWhereIrpfRate, m.IrpfRate),
            m.IrpfRate.AssociationPattern(v => v.OrdersWhereDerivedIrpfRate, m.IrpfRate),
            m.IrpfRate.AssociationPattern(v => v.QuoteItemsWhereIrpfRate, m.IrpfRate),
            m.IrpfRate.AssociationPattern(v => v.QuoteItemVersionsWhereIrpfRate, m.IrpfRate),
            m.IrpfRate.AssociationPattern(v => v.QuotesWhereDerivedIrpfRate, m.IrpfRate),
            m.InvoiceItem.RolePattern(v => v.IrpfRate, v => v.IrpfRate),
            m.Invoice.RolePattern(v => v.DerivedIrpfRate, v => v.DerivedIrpfRate),
            m.OrderItem.RolePattern(v => v.IrpfRate, v => v.IrpfRate),
            m.Order.RolePattern(v => v.DerivedIrpfRate, v => v.DerivedIrpfRate),
            m.QuoteItem.RolePattern(v => v.IrpfRate, v => v.IrpfRate),
            m.Quote.RolePattern(v => v.DerivedIrpfRate, v => v.DerivedIrpfRate),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<IrpfRate>())
            {
                @this.DeriveIrpfRateDeniedPermission(validation);
            }
        }
    }

    public static class IrpfRateDeniedPermissionRuleExtensions
    {
        public static void DeriveIrpfRateDeniedPermission(this IrpfRate @this, IValidation validation)
        {
            var deleteRevocation = new Revocations(@this.Strategy.Transaction).IrpfRateDeleteRevocation;

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
