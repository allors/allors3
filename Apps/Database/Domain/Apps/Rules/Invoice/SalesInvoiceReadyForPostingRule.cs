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
    using Resources;

    public class SalesInvoiceReadyForPostingRule : Rule
    {
        public SalesInvoiceReadyForPostingRule(M m) : base(m, new Guid("9685bfd9-ec69-49ab-9cc4-7e9a93a6f03c")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.SalesInvoice, m.SalesInvoice.BilledFrom),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.Store),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.BillToCustomer),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.BillToEndCustomer),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.ShipToCustomer),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.ShipToEndCustomer),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.AssignedVatRegime),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.AssignedIrpfRegime),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.AssignedCurrency),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.AssignedVatClause),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.AssignedShipToAddress),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.AssignedBilledFromContactMechanism),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.AssignedBillToContactMechanism),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.AssignedBillToEndCustomerContactMechanism),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.AssignedShipToEndCustomerAddress),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.Locale),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.InvoiceDate),
            new RolePattern(m.Party, m.Party.Locale) { Steps = new IPropertyType[] { this.M.Party.SalesInvoicesWhereBillToCustomer }},
            new RolePattern(m.Party, m.Party.BillingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesInvoicesWhereBillToCustomer }},
            new RolePattern(m.Party, m.Party.BillingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesInvoicesWhereBillToEndCustomer }},
            new RolePattern(m.Party, m.Party.ShippingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesInvoicesWhereShipToCustomer}},
            new RolePattern(m.Party, m.Party.ShippingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesInvoicesWhereShipToEndCustomer}},
            new RolePattern(m.Organisation, m.Organisation.Locale) { Steps = new IPropertyType[] { this.M.Organisation.SalesInvoicesWhereBilledFrom }},
            new RolePattern(m.Party, m.Party.PreferredCurrency) { Steps = new IPropertyType[] { this.M.Party.SalesInvoicesWhereBillToCustomer }},
            new RolePattern(m.Organisation, m.Organisation.PreferredCurrency) { Steps = new IPropertyType[] { this.M.Organisation.SalesInvoicesWhereBilledFrom }},
            new RolePattern(m.Organisation, m.Organisation.BillingAddress) { Steps = new IPropertyType[] { this.M.Organisation.SalesInvoicesWhereBilledFrom }},
            new RolePattern(m.Organisation, m.Organisation.GeneralCorrespondence) { Steps = new IPropertyType[] { this.M.Organisation.SalesInvoicesWhereBilledFrom }},
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>().Where(v => v.SalesInvoiceState.IsReadyForPosting))
            {
                @this.DerivedLocale = @this.Locale ?? @this.BillToCustomer?.Locale ?? @this.BilledFrom?.Locale;
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.BillToCustomer?.PreferredCurrency ?? @this.BillToCustomer?.Locale?.Country?.Currency ?? @this.BilledFrom?.PreferredCurrency;
                @this.DerivedVatRegime = @this.AssignedVatRegime;
                @this.DerivedIrpfRegime = @this.AssignedIrpfRegime;
                @this.DerivedBilledFromContactMechanism = @this.AssignedBilledFromContactMechanism ?? @this.BilledFrom?.BillingAddress ?? @this.BilledFrom?.GeneralCorrespondence;
                @this.DerivedBillToContactMechanism = @this.AssignedBillToContactMechanism ?? @this.BillToCustomer?.BillingAddress;
                @this.DerivedBillToEndCustomerContactMechanism = @this.AssignedBillToEndCustomerContactMechanism ?? @this.BillToEndCustomer?.BillingAddress;
                @this.DerivedShipToEndCustomerAddress = @this.AssignedShipToEndCustomerAddress ?? @this.ShipToEndCustomer?.ShippingAddress;
                @this.DerivedShipToAddress = @this.AssignedShipToAddress ?? @this.ShipToCustomer?.ShippingAddress;

                if (@this.ExistInvoiceDate)
                {
                    @this.DerivedVatRate = @this.DerivedVatRegime?.VatRates.First(v => v.FromDate <= @this.InvoiceDate && (!v.ExistThroughDate || v.ThroughDate >= @this.InvoiceDate));
                    @this.DerivedIrpfRate = @this.DerivedIrpfRegime?.IrpfRates.First(v => v.FromDate <= @this.InvoiceDate && (!v.ExistThroughDate || v.ThroughDate >= @this.InvoiceDate));
                }

                if (@this.ExistDerivedVatRegime)
                {
                    if (@this.DerivedVatRegime.ExistVatClause)
                    {
                        @this.DerivedVatClause = @this.DerivedVatRegime.VatClause;
                    }
                    else
                    {
                        @this.RemoveDerivedVatClause();
                    }
                }
                else
                {
                    @this.RemoveDerivedVatClause();
                }

                @this.DerivedVatClause = @this.ExistAssignedVatClause ? @this.AssignedVatClause : @this.DerivedVatClause;
            }
        }
    }
}
