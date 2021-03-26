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
                new RolePattern(m.PurchaseInvoice, m.PurchaseInvoice.PurchaseInvoiceState),
                new RolePattern(m.PurchaseInvoice, m.PurchaseInvoice.AssignedVatRegime),
                new RolePattern(m.PurchaseInvoice, m.PurchaseInvoice.AssignedIrpfRegime),
                new RolePattern(m.PurchaseInvoice, m.PurchaseInvoice.AssignedCurrency),
                new RolePattern(m.PurchaseInvoice, m.PurchaseInvoice.BilledFrom),
                new RolePattern(m.PurchaseInvoice, m.PurchaseInvoice.InvoiceDate),
                new RolePattern(m.Organisation, m.Organisation.PreferredCurrency) { Steps = new IPropertyType[] { m.Organisation.PurchaseInvoicesWhereBilledTo }},
                new RolePattern(m.OrderItemBilling, m.OrderItemBilling.InvoiceItem) { Steps = new IPropertyType[] { m.OrderItemBilling.InvoiceItem, m.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>().Where(v => v.PurchaseInvoiceState.IsCreated))
            {
                @this.DerivedVatRegime = @this.AssignedVatRegime;
                @this.DerivedIrpfRegime = @this.AssignedIrpfRegime;
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.BilledTo?.PreferredCurrency;
                @this.PurchaseOrders = @this.InvoiceItems.SelectMany(v => v.OrderItemBillingsWhereInvoiceItem).Select(v => v.OrderItem.OrderWhereValidOrderItem).ToArray();

                if (@this.ExistInvoiceDate)
                {
                    @this.DerivedVatRate = @this.DerivedVatRegime?.VatRates.First(v => v.FromDate <= @this.InvoiceDate && (!v.ExistThroughDate || v.ThroughDate >= @this.InvoiceDate));
                    @this.DerivedIrpfRate = @this.DerivedIrpfRegime?.IrpfRates.First(v => v.FromDate <= @this.InvoiceDate && (!v.ExistThroughDate || v.ThroughDate >= @this.InvoiceDate));
                }
            }
        }
    }
}
