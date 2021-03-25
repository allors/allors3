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

    public class SalesInvoiceReadyForPostingDerivation : DomainDerivation
    {
        public SalesInvoiceReadyForPostingDerivation(M m) : base(m, new Guid("9685bfd9-ec69-49ab-9cc4-7e9a93a6f03c")) =>
            this.Patterns = new Pattern[]
        {
            new AssociationPattern(this.M.SalesInvoice.BilledFrom),
            new AssociationPattern(this.M.SalesInvoice.Store),
            new AssociationPattern(this.M.SalesInvoice.BillToCustomer),
            new AssociationPattern(this.M.SalesInvoice.BillToEndCustomer),
            new AssociationPattern(this.M.SalesInvoice.ShipToCustomer),
            new AssociationPattern(this.M.SalesInvoice.ShipToEndCustomer),
            new AssociationPattern(this.M.SalesInvoice.AssignedVatRegime),
            new AssociationPattern(this.M.SalesInvoice.AssignedIrpfRegime),
            new AssociationPattern(this.M.SalesInvoice.AssignedCurrency),
            new AssociationPattern(this.M.SalesInvoice.AssignedVatClause),
            new AssociationPattern(this.M.SalesInvoice.AssignedShipToAddress),
            new AssociationPattern(this.M.SalesInvoice.AssignedBilledFromContactMechanism),
            new AssociationPattern(this.M.SalesInvoice.AssignedBillToContactMechanism),
            new AssociationPattern(this.M.SalesInvoice.AssignedBillToEndCustomerContactMechanism),
            new AssociationPattern(this.M.SalesInvoice.AssignedShipToEndCustomerAddress),
            new AssociationPattern(this.M.SalesInvoice.Locale),
            new AssociationPattern(this.M.SalesInvoice.InvoiceDate),
            new AssociationPattern(this.M.Party.Locale) { Steps = new IPropertyType[] { this.M.Party.SalesInvoicesWhereBillToCustomer }},
            new AssociationPattern(this.M.Party.BillingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesInvoicesWhereBillToCustomer }},
            new AssociationPattern(this.M.Party.BillingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesInvoicesWhereBillToEndCustomer }},
            new AssociationPattern(this.M.Party.ShippingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesInvoicesWhereShipToCustomer}},
            new AssociationPattern(this.M.Party.ShippingAddress) { Steps = new IPropertyType[] { this.M.Party.SalesInvoicesWhereShipToEndCustomer}},
            new AssociationPattern(this.M.Organisation.Locale) { Steps = new IPropertyType[] { this.M.Organisation.SalesInvoicesWhereBilledFrom }},
            new AssociationPattern(this.M.Party.PreferredCurrency) { Steps = new IPropertyType[] { this.M.Party.SalesInvoicesWhereBillToCustomer }},
            new AssociationPattern(this.M.Organisation.PreferredCurrency) { Steps = new IPropertyType[] { this.M.Organisation.SalesInvoicesWhereBilledFrom }},
            new AssociationPattern(this.M.Organisation.BillingAddress) { Steps = new IPropertyType[] { this.M.Organisation.SalesInvoicesWhereBilledFrom }},
            new AssociationPattern(this.M.Organisation.GeneralCorrespondence) { Steps = new IPropertyType[] { this.M.Organisation.SalesInvoicesWhereBilledFrom }},
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
