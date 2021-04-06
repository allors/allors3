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
                new RolePattern(m.SalesInvoiceItem, m.SalesInvoiceItem.AssignedVatRegime),
                new RolePattern(m.SalesInvoice, m.SalesInvoice.InvoiceDate) { Steps =  new IPropertyType[] {this.M.SalesInvoice.SalesInvoiceItems} },
                new RolePattern(m.SalesInvoice, m.SalesInvoice.DerivedVatRegime) { Steps =  new IPropertyType[] {this.M.SalesInvoice.SalesInvoiceItems} },
                new AssociationPattern(m.SalesInvoice.SalesInvoiceItems),
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
