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

    public class SalesInvoiceDeniedPermissionRule : Rule
    {
        public SalesInvoiceDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("8726348f-85af-429b-a514-55d00dbb14d9")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoice.RolePattern(v => v.TransitionalRevocations),
            m.SalesInvoice.RolePattern(v => v.IsRepeatingInvoice),
            m.SalesInvoice.RolePattern(v => v.SalesOrders),
            m.SalesInvoice.RolePattern(v => v.PurchaseInvoice),
            m.SalesInvoiceItem.RolePattern(v => v.SalesInvoiceItemState, v => v.SalesInvoiceWhereSalesInvoiceItem),
            m.SalesInvoice.AssociationPattern(v => v.RepeatingSalesInvoiceWhereSource),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                @this.Revocations = @this.TransitionalRevocations;

                var deleteRevocation = new Revocations(@this.Strategy.Transaction).SalesInvoiceDeleteRevocation;

                if (@this.IsDeletable)
                {
                    @this.RemoveRevocation(deleteRevocation);
                }
                else
                {
                    @this.AddRevocation(deleteRevocation);
                }

                var cancelRevocation = new Revocations(@this.Strategy.Transaction).SalesInvoiceCancelRevocation;
                var reviseRevocation = new Revocations(@this.Strategy.Transaction).SalesInvoiceReviseRevocation;

                if (@this.SalesInvoiceType.IsCreditNote)
                {
                    if (@this.SalesInvoiceState.IsNotPaid)
                    {
                        @this.RemoveRevocation(cancelRevocation);
                        @this.RemoveRevocation(reviseRevocation);
                    }
                    else
                    {
                        @this.AddRevocation(cancelRevocation);
                        @this.AddRevocation(reviseRevocation);
                    }
                }
                else if (@this.SalesInvoiceState.IsNotPaid)
                {
                    @this.AddRevocation(cancelRevocation);
                    @this.AddRevocation(reviseRevocation);
                }
            }
        }
    }
}
