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

    public class SalesInvoicePrintRule : Rule
    {
        public SalesInvoicePrintRule(MetaPopulation m) : base(m, new Guid("6df02414-c7d2-46c5-97a4-9b528c5da228")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoice.RolePattern(v => v.SalesInvoiceType),
            m.SalesInvoice.RolePattern(v => v.Description),
            m.SalesInvoice.RolePattern(v => v.DerivedCurrency),
            m.SalesInvoice.RolePattern(v => v.InvoiceNumber),
            m.SalesInvoice.RolePattern(v => v.InvoiceDate),
            m.SalesInvoice.RolePattern(v => v.DueDate),
            m.SalesInvoice.RolePattern(v => v.CustomerReference),
            m.SalesInvoice.RolePattern(v => v.TotalBasePrice),
            m.SalesInvoice.RolePattern(v => v.AdvancePayment),
            m.SalesInvoice.RolePattern(v => v.TotalExVat),
            m.SalesInvoice.RolePattern(v => v.DerivedVatRate),
            m.SalesInvoice.RolePattern(v => v.TotalVat),
            m.SalesInvoice.RolePattern(v => v.DerivedIrpfRate),
            m.SalesInvoice.RolePattern(v => v.TotalIrpf),
            m.SalesInvoice.RolePattern(v => v.TotalIncVat),
            m.SalesInvoice.RolePattern(v => v.GrandTotal),
            m.SalesInvoice.RolePattern(v => v.AdvancePayment),
            m.SalesTerm.AssociationPattern(v => v.InvoiceWhereSalesTerm, m.SalesInvoice),
            m.SalesTerm.RolePattern(v => v.TermValue, v => v.InvoiceWhereSalesTerm.Invoice.AsSalesInvoice),
            m.SalesInvoice.RolePattern(v => v.BilledFrom),
            m.Party.AssociationPattern(v => v.CustomerRelationshipsWhereCustomer, v => v.SalesInvoicesWhereBillToCustomer),
            m.Store.RolePattern(v => v.PaymentNetDays, v => v.SalesInvoicesWhereStore),
            m.SalesInvoice.RolePattern(v => v.DerivedVatClause),
            m.SalesInvoice.RolePattern(v => v.DerivedShipToAddress),
            m.Organisation.RolePattern(v => v.PartyName, v => v.SalesInvoicesWhereBilledFrom),
            m.Organisation.RolePattern(v => v.GeneralEmail, v => v.SalesInvoicesWhereBilledFrom),
            m.Organisation.RolePattern(v => v.InternetAddress, v => v.SalesInvoicesWhereBilledFrom),
            m.Organisation.RolePattern(v => v.TaxNumber, v => v.SalesInvoicesWhereBilledFrom),
            m.Organisation.RolePattern(v => v.BillingInquiriesPhone, v => v.SalesInvoicesWhereBilledFrom),
            m.Organisation.RolePattern(v => v.GeneralPhoneNumber, v => v.SalesInvoicesWhereBilledFrom),
            m.TelecommunicationsNumber.RolePattern(v => v.CountryCode, v => v.PartiesWhereBillingInquiriesPhone.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.TelecommunicationsNumber.RolePattern(v => v.CountryCode, v => v.PartiesWhereGeneralPhoneNumber.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.TelecommunicationsNumber.RolePattern(v => v.AreaCode, v => v.PartiesWhereBillingInquiriesPhone.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.TelecommunicationsNumber.RolePattern(v => v.AreaCode, v => v.PartiesWhereGeneralPhoneNumber.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.TelecommunicationsNumber.RolePattern(v => v.ContactNumber, v => v.PartiesWhereBillingInquiriesPhone.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.TelecommunicationsNumber.RolePattern(v => v.ContactNumber, v => v.PartiesWhereGeneralPhoneNumber.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.Organisation.RolePattern(v => v.GeneralCorrespondence, v => v.SalesInvoicesWhereBilledFrom),
            m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.PostalAddress.RolePattern(v => v.Country, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.Organisation.RolePattern(v => v.BankAccounts, v => v.SalesInvoicesWhereBilledFrom),
            m.BankAccount.RolePattern(v => v.Iban, v => v.PartyWhereBankAccount.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.BankAccount.RolePattern(v => v.Bank, v => v.PartyWhereBankAccount.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.Bank.RolePattern(v => v.Name, v => v.BankAccountsWhereBank.BankAccount.PartyWhereBankAccount.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.Bank.RolePattern(v => v.SwiftCode, v => v.BankAccountsWhereBank.BankAccount.PartyWhereBankAccount.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.Bank.RolePattern(v => v.Bic, v => v.BankAccountsWhereBank.BankAccount.PartyWhereBankAccount.Party.AsOrganisation.SalesInvoicesWhereBilledFrom),
            m.SalesInvoice.RolePattern(v => v.BillToCustomer),
            m.SalesInvoice.RolePattern(v => v.BillToContactPerson),
            m.SalesInvoice.RolePattern(v => v.DerivedBillToContactMechanism),
            m.Party.RolePattern(v => v.PartyName, v => v.SalesInvoicesWhereBillToCustomer),
            m.Organisation.RolePattern(v => v.TaxNumber, v => v.SalesInvoicesWhereBillToCustomer),
            m.Person.RolePattern(v => v.PartyName, v => v.SalesInvoicesWhereBillToContactPerson),
            m.ElectronicAddress.RolePattern(v => v.ElectronicAddressString, v => v.PurchaseInvoicesWhereDerivedBilledFromContactMechanism),
            m.SalesInvoice.RolePattern(v => v.ShipToCustomer),
            m.Party.RolePattern(v => v.PartyName, v => v.SalesInvoicesWhereShipToCustomer),
            m.Party.RolePattern(v => v.PartyName, v => v.SalesInvoicesWhereBillToCustomer),
            m.Organisation.RolePattern(v => v.TaxNumber, v => v.SalesInvoicesWhereShipToCustomer),
            m.Organisation.RolePattern(v => v.TaxNumber, v => v.SalesInvoicesWhereBillToCustomer),
            m.SalesInvoice.RolePattern(v => v.ShipToContactPerson),
            m.SalesInvoice.RolePattern(v => v.DerivedShipToAddress),
            m.Party.RolePattern(v => v.ShippingAddress, v => v.SalesInvoicesWhereShipToCustomer),
            m.Party.RolePattern(v => v.GeneralCorrespondence, v => v.SalesInvoicesWhereShipToCustomer),
            m.Party.RolePattern(v => v.GeneralCorrespondence, v => v.SalesInvoicesWhereBillToCustomer),
            m.Party.RolePattern(v => v.GeneralCorrespondence, v => v.SalesInvoicesWhereBillToCustomer),
            m.PostalAddress.AssociationPattern(v => v.SalesInvoicesWhereDerivedBillToContactMechanism, v => v.SalesInvoicesWhereDerivedBillToContactMechanism.SalesInvoice),
            m.PostalAddress.RolePattern(v => v.Address1, v => v.SalesInvoicesWhereDerivedShipToAddress),
            m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereShippingAddress.Party.SalesInvoicesWhereShipToCustomer),
            m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereGeneralCorrespondence.Party.SalesInvoicesWhereShipToCustomer),
            m.PostalAddress.RolePattern(v => v.Address1, v => v.SalesInvoicesWhereDerivedBillToContactMechanism),
            m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereShippingAddress.Party.SalesInvoicesWhereBillToCustomer),
            m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereGeneralCorrespondence.Party.SalesInvoicesWhereBillToCustomer),
            m.PostalAddress.RolePattern(v => v.Address2, v => v.SalesInvoicesWhereDerivedShipToAddress),
            m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereShippingAddress.Party.SalesInvoicesWhereShipToCustomer),
            m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereGeneralCorrespondence.Party.SalesInvoicesWhereShipToCustomer),
            m.PostalAddress.RolePattern(v => v.Address2, v => v.SalesInvoicesWhereDerivedBillToContactMechanism),
            m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereShippingAddress.Party.SalesInvoicesWhereBillToCustomer),
            m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereGeneralCorrespondence.Party.SalesInvoicesWhereBillToCustomer),
            m.PostalAddress.RolePattern(v => v.Address3, v => v.SalesInvoicesWhereDerivedShipToAddress),
            m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereShippingAddress.Party.SalesInvoicesWhereShipToCustomer),
            m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereGeneralCorrespondence.Party.SalesInvoicesWhereShipToCustomer),
            m.PostalAddress.RolePattern(v => v.Address3, v => v.SalesInvoicesWhereDerivedBillToContactMechanism),
            m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereShippingAddress.Party.SalesInvoicesWhereBillToCustomer),
            m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereGeneralCorrespondence.Party.SalesInvoicesWhereBillToCustomer),
            m.PostalAddress.RolePattern(v => v.Locality, v => v.SalesInvoicesWhereDerivedShipToAddress),
            m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereShippingAddress.Party.SalesInvoicesWhereShipToCustomer),
            m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereGeneralCorrespondence.Party.SalesInvoicesWhereShipToCustomer),
            m.PostalAddress.RolePattern(v => v.Locality, v => v.SalesInvoicesWhereDerivedBillToContactMechanism),
            m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereShippingAddress.Party.SalesInvoicesWhereBillToCustomer),
            m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereGeneralCorrespondence.Party.SalesInvoicesWhereBillToCustomer),
            m.PostalAddress.RolePattern(v => v.Region, v => v.SalesInvoicesWhereDerivedShipToAddress),
            m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereShippingAddress.Party.SalesInvoicesWhereShipToCustomer),
            m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereGeneralCorrespondence.Party.SalesInvoicesWhereShipToCustomer),
            m.PostalAddress.RolePattern(v => v.Region, v => v.SalesInvoicesWhereDerivedBillToContactMechanism),
            m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereShippingAddress.Party.SalesInvoicesWhereBillToCustomer),
            m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereGeneralCorrespondence.Party.SalesInvoicesWhereBillToCustomer),
            m.PostalAddress.RolePattern(v => v.PostalCode, v => v.SalesInvoicesWhereDerivedShipToAddress),
            m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereShippingAddress.Party.SalesInvoicesWhereShipToCustomer),
            m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereGeneralCorrespondence.Party.SalesInvoicesWhereShipToCustomer),
            m.PostalAddress.RolePattern(v => v.PostalCode, v => v.SalesInvoicesWhereDerivedBillToContactMechanism),
            m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereShippingAddress.Party.SalesInvoicesWhereBillToCustomer),
            m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereGeneralCorrespondence.Party.SalesInvoicesWhereBillToCustomer),
            m.PostalAddress.RolePattern(v => v.Country, v => v.SalesInvoicesWhereDerivedShipToAddress),
            m.PostalAddress.RolePattern(v => v.Country, v => v.PartiesWhereShippingAddress.Party.SalesInvoicesWhereShipToCustomer),
            m.PostalAddress.RolePattern(v => v.Country, v => v.PartiesWhereGeneralCorrespondence.Party.SalesInvoicesWhereShipToCustomer),
            m.PostalAddress.RolePattern(v => v.Country, v => v.SalesInvoicesWhereDerivedBillToContactMechanism),
            m.PostalAddress.RolePattern(v => v.Country, v => v.PartiesWhereShippingAddress.Party.SalesInvoicesWhereBillToCustomer),
            m.PostalAddress.RolePattern(v => v.Country, v => v.PartiesWhereGeneralCorrespondence.Party.SalesInvoicesWhereBillToCustomer),
            m.Currency.RolePattern(v => v.IsoCode, v => v.InvoicesWhereDerivedCurrency.Invoice.AsSalesInvoice),
            m.SalesInvoice.RolePattern(v => v.SalesInvoiceItems),
            m.InvoiceItemType.RolePattern(v => v.Name, v => v.SalesInvoiceItemsWhereInvoiceItemType.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem),
            m.SalesInvoiceItem.RolePattern(v => v.Product, v => v.SalesInvoiceWhereSalesInvoiceItem),
            m.SalesInvoiceItem.RolePattern(v => v.Part, v => v.SalesInvoiceWhereSalesInvoiceItem),
            m.SalesInvoiceItem.RolePattern(v => v.Description, v => v.SalesInvoiceWhereSalesInvoiceItem),
            m.SalesInvoiceItem.RolePattern(v => v.UnitPrice, v => v.SalesInvoiceWhereSalesInvoiceItem),
            m.SalesInvoiceItem.RolePattern(v => v.TotalExVat, v => v.SalesInvoiceWhereSalesInvoiceItem),
            m.SalesInvoiceItem.RolePattern(v => v.Comment, v => v.SalesInvoiceWhereSalesInvoiceItem),
            m.SalesInvoiceItem.RolePattern(v => v.Quantity, v => v.SalesInvoiceWhereSalesInvoiceItem),
            m.SalesInvoice.RolePattern(v => v.SalesTerms),
            m.TermType.RolePattern(v => v.Name, v => v.AgreementTermsWhereTermType.AgreementTerm.AsSalesTerm.InvoiceWhereSalesTerm.Invoice.AsSalesInvoice),
            m.SalesInvoice.RolePattern(v => v.OrderAdjustments),
            m.OrderAdjustment.RolePattern(v => v.Description, v => v.InvoiceWhereOrderAdjustment.Invoice.AsSalesInvoice),
            m.SalesInvoice.RolePattern(v => v.TotalDiscount),
            m.SalesInvoice.RolePattern(v => v.TotalSurcharge),
            m.SalesInvoice.RolePattern(v => v.TotalFee),
            m.SalesInvoice.RolePattern(v => v.TotalShippingAndHandling),
            m.SalesInvoice.RolePattern(v => v.TotalExtraCharge),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                @this.ResetPrintDocument();
            }
        }
    }
}
