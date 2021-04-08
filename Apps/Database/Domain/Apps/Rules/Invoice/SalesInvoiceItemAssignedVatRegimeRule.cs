// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class SalesInvoiceItemAssignedVatRegimeRule : Rule
    {
        public SalesInvoiceItemAssignedVatRegimeRule(MetaPopulation m) : base(m, new Guid("6d795531-522d-4e57-9534-f4e6361836a3")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesInvoiceItem.RolePattern(v => v.AssignedVatRegime),
                m.SalesInvoice.RolePattern(v => v.InvoiceDate, v => v.SalesInvoiceItems),
                m.SalesInvoice.RolePattern(v => v.DerivedVatRegime, v => v.SalesInvoiceItems),
                m.SalesInvoiceItem.AssociationPattern(v => v.SalesInvoiceWhereSalesInvoiceItem),
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

                @this.DerivedVatRegime = @this.ExistAssignedVatRegime ? @this.AssignedVatRegime : @this.SalesInvoiceWhereSalesInvoiceItem?.DerivedVatRegime;
                @this.VatRate = @this.DerivedVatRegime?.VatRates.First(v => v.FromDate <= salesInvoice.InvoiceDate && (!v.ExistThroughDate || v.ThroughDate >= salesInvoice.InvoiceDate));
            }
        }
    }
}
