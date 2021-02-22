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
    using Database.Derivations;

    public class PurchaseInvoiceCreatedDerivation : DomainDerivation
    {
        public PurchaseInvoiceCreatedDerivation(M m) : base(m, new Guid("1982652b-9855-4f32-92b7-a2c46a887051")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.PurchaseInvoice.PurchaseInvoiceState),
                new ChangedPattern(m.PurchaseInvoice.AssignedVatRegime),
                new ChangedPattern(m.PurchaseInvoice.AssignedIrpfRegime),
                new ChangedPattern(m.PurchaseInvoice.AssignedCurrency),
                new ChangedPattern(m.PurchaseInvoice.BilledFrom),
                new ChangedPattern(m.Party.VatRegime) { Steps = new IPropertyType[] { m.Party.PurchaseInvoicesWhereBilledFrom}},
                new ChangedPattern(m.Party.IrpfRegime) { Steps = new IPropertyType[] { m.Party.PurchaseInvoicesWhereBilledFrom }},
                new ChangedPattern(m.Organisation.PreferredCurrency) { Steps = new IPropertyType[] { m.Organisation.PurchaseInvoicesWhereBilledTo }},
                new ChangedPattern(m.OrderItemBilling.InvoiceItem) { Steps = new IPropertyType[] { m.OrderItemBilling.InvoiceItem, m.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>().Where(v => v.PurchaseInvoiceState.IsCreated))
            {
                @this.DerivedVatRegime = @this.AssignedVatRegime ?? @this.BilledFrom?.VatRegime;
                @this.DerivedIrpfRegime = @this.AssignedIrpfRegime ?? (@this.BilledFrom as Organisation)?.IrpfRegime;
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.BilledTo?.PreferredCurrency;
                @this.PurchaseOrders = @this.InvoiceItems.SelectMany(v => v.OrderItemBillingsWhereInvoiceItem).Select(v => v.OrderItem.OrderWhereValidOrderItem).ToArray();
            }
        }
    }
}
