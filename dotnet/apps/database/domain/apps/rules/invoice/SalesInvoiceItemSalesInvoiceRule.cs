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

    public class SalesInvoiceItemSalesInvoiceRule : Rule
    {
        public SalesInvoiceItemSalesInvoiceRule(MetaPopulation m) : base(m, new Guid("a5d2bc57-d83e-4518-b580-588ead561a0b")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesInvoiceItem.AssociationPattern(v => v.SalesInvoiceWhereSalesInvoiceItem),
                m.SalesInvoice.RolePattern(v => v.SalesInvoiceState, v => v.SalesInvoiceItems),
                m.SalesInvoice.RolePattern(v => v.DerivedVatRegime, v => v.SalesInvoiceItems),
                m.SalesInvoice.RolePattern(v => v.DerivedIrpfRegime, v => v.SalesInvoiceItems),
                m.SalesInvoice.RolePattern(v => v.InvoiceDate, v => v.SalesInvoiceItems),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;
            var changeSet = cycle.ChangeSet;

            foreach (var @this in matches.Cast<SalesInvoiceItem>())
            {
                var salesInvoice = @this.SalesInvoiceWhereSalesInvoiceItem;
                var salesInvoiceItemStates = new SalesInvoiceItemStates(transaction);

                if (salesInvoice != null
                    && salesInvoice.ExistSalesInvoiceState
                    && salesInvoice.SalesInvoiceState.IsReadyForPosting
                    && @this.ExistSalesInvoiceItemState
                    && @this.SalesInvoiceItemState.IsCancelledByInvoice)
                {
                    @this.SalesInvoiceItemState = salesInvoiceItemStates.ReadyForPosting;
                }

                // SalesInvoiceItem States
                if (salesInvoice != null
                    && salesInvoice.ExistSalesInvoiceState
                    && @this.ExistSalesInvoiceItemState
                    && @this.IsValid)
                {
                    if (salesInvoice.SalesInvoiceState.IsWrittenOff)
                    {
                        @this.SalesInvoiceItemState = salesInvoiceItemStates.WrittenOff;
                    }

                    if (salesInvoice.SalesInvoiceState.IsCancelled)
                    {
                        @this.SalesInvoiceItemState = salesInvoiceItemStates.CancelledByInvoice;
                    }
                }
            }
        }
    }
}
