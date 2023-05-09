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

    public class IrpfRegimeDeniedPermissionRule : Rule
    {
        public IrpfRegimeDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("1bb52ead-1b05-4151-b960-9f2c32f12d4a")) =>
            this.Patterns = new Pattern[]
        {
            m.IrpfRegime.AssociationPattern(v => v.InvoiceItemsWhereDerivedIrpfRegime, m.IrpfRegime),
            m.IrpfRegime.AssociationPattern(v => v.InvoiceItemVersionsWhereDerivedIrpfRegime, m.IrpfRegime),
            m.IrpfRegime.AssociationPattern(v => v.InvoicesWhereDerivedIrpfRegime, m.IrpfRegime),
            m.IrpfRegime.AssociationPattern(v => v.InvoiceVersionsWhereDerivedIrpfRegime, m.IrpfRegime),
            m.IrpfRegime.AssociationPattern(v => v.OrderItemsWhereDerivedIrpfRegime, m.IrpfRegime),
            m.IrpfRegime.AssociationPattern(v => v.OrderItemVersionsWhereDerivedIrpfRegime, m.IrpfRegime),
            m.IrpfRegime.AssociationPattern(v => v.OrdersWhereDerivedIrpfRegime, m.IrpfRegime),
            m.IrpfRegime.AssociationPattern(v => v.OrderVersionsWhereDerivedIrpfRegime, m.IrpfRegime),
            m.IrpfRegime.AssociationPattern(v => v.QuoteItemsWhereDerivedIrpfRegime, m.IrpfRegime),
            m.IrpfRegime.AssociationPattern(v => v.QuoteItemVersionsWhereDerivedIrpfRegime, m.IrpfRegime),
            m.IrpfRegime.AssociationPattern(v => v.QuotesWhereDerivedIrpfRegime, m.IrpfRegime),
            m.IrpfRegime.AssociationPattern(v => v.QuoteVersionsWhereDerivedIrpfRegime, m.IrpfRegime),
            m.Invoice.RolePattern(v => v.DerivedIrpfRegime, v => v.DerivedIrpfRegime),
            m.InvoiceItem.RolePattern(v => v.DerivedIrpfRegime, v => v.DerivedIrpfRegime),
            m.Order.RolePattern(v => v.DerivedIrpfRegime, v => v.DerivedIrpfRegime),
            m.OrderItem.RolePattern(v => v.DerivedIrpfRegime, v => v.DerivedIrpfRegime),
            m.Quote.RolePattern(v => v.DerivedIrpfRegime, v => v.DerivedIrpfRegime),
            m.QuoteItem.RolePattern(v => v.DerivedIrpfRegime, v => v.DerivedIrpfRegime),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<IrpfRegime>())
            {
                @this.DeriveIrpfRegimeDeniedPermission(validation);
            }
        }
    }

    public static class IrpfRegimeDeniedPermissionRuleExtensions
    {
        public static void DeriveIrpfRegimeDeniedPermission(this IrpfRegime @this, IValidation validation)
        {
            var deleteRevocation = new Revocations(@this.Strategy.Transaction).IrpfRegimeDeleteRevocation;

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
