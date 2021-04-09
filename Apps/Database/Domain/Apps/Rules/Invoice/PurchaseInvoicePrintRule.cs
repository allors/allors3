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

    public class PurchaseInvoicePrintRule : Rule
    {
        public PurchaseInvoicePrintRule(MetaPopulation m) : base(m, new Guid("516d59fd-e6dd-4ba1-a1ba-cef09c0c582f")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseInvoice.RolePattern(v => v.Description),
                m.PurchaseInvoice.RolePattern(v => v.InvoiceNumber),
                m.PurchaseInvoice.RolePattern(v => v.InvoiceDate),
                m.PurchaseInvoice.RolePattern(v => v.CustomerReference),
                m.PurchaseInvoice.RolePattern(v => v.TotalBasePrice),
                m.PurchaseInvoice.RolePattern(v => v.TotalExVat),
                m.PurchaseInvoice.RolePattern(v => v.DerivedVatRate),
                m.VatRate.RolePattern(v => v.Rate, v => v.InvoicesWhereDerivedVatRate.Invoice.AsPurchaseInvoice),
                m.PurchaseInvoice.RolePattern(v => v.TotalVat),
                m.IrpfRate.RolePattern(v => v.Rate, v => v.InvoicesWhereDerivedIrpfRate.Invoice.AsPurchaseInvoice),
                m.PurchaseInvoice.RolePattern(v => v.TotalIrpf),
                m.PurchaseInvoice.RolePattern(v => v.TotalIncVat),
                m.PurchaseInvoice.RolePattern(v => v.GrandTotal),
                m.Currency.RolePattern(v => v.IsoCode, v => v.InvoicesWhereDerivedCurrency.Invoice.AsPurchaseInvoice),

                m.PurchaseInvoice.RolePattern(v => v.BilledTo),
                m.Organisation.RolePattern(v => v.PartyName, v => v.PurchaseInvoicesWhereBilledTo),
                m.EmailAddress.RolePattern(v => v.ElectronicAddressString, v => v.PartiesWhereGeneralEmail.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.ElectronicAddress.RolePattern(v => v.ElectronicAddressString, v => v.PartiesWhereInternetAddress.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.PurchaseInvoicesWhereBilledTo),
                m.Organisation.RolePattern(v => v.BillingInquiriesPhone, v => v.PurchaseInvoicesWhereBilledTo),
                m.Organisation.RolePattern(v => v.GeneralPhoneNumber, v => v.PurchaseInvoicesWhereBilledTo),
                m.TelecommunicationsNumber.RolePattern(v => v.CountryCode, v => v.PartiesWhereBillingInquiriesPhone.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo.PurchaseInvoice),
                m.TelecommunicationsNumber.RolePattern(v => v.AreaCode, v => v.PartiesWhereBillingInquiriesPhone.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.TelecommunicationsNumber.RolePattern(v => v.ContactNumber, v => v.PartiesWhereBillingInquiriesPhone.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.TelecommunicationsNumber.RolePattern(v => v.CountryCode, v => v.PartiesWhereGeneralPhoneNumber.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.TelecommunicationsNumber.RolePattern(v => v.AreaCode, v => v.PartiesWhereGeneralPhoneNumber.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.TelecommunicationsNumber.RolePattern(v => v.ContactNumber, v => v.PartiesWhereGeneralPhoneNumber.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Organisation.RolePattern(v => v.GeneralCorrespondence, v => v.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.PostalAddress.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Organisation.RolePattern(v => v.BankAccounts, v => v.PurchaseInvoicesWhereBilledTo),
                m.BankAccount.RolePattern(v => v.Iban, v => v.PartyWhereBankAccount.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.BankAccount.RolePattern(v => v.Bank, v => v.PartyWhereBankAccount.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Bank.RolePattern(v => v.Name, v => v.BankAccountsWhereBank.BankAccount.PartyWhereBankAccount.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Bank.RolePattern(v => v.SwiftCode, v => v.BankAccountsWhereBank.BankAccount.PartyWhereBankAccount.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Bank.RolePattern(v => v.Bic, v => v.BankAccountsWhereBank.BankAccount.PartyWhereBankAccount.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),

                m.PurchaseInvoice.RolePattern(v => v.BilledFrom),
                m.PurchaseInvoice.RolePattern(v => v.BilledFromContactPerson),
                m.PurchaseInvoice.RolePattern(v => v.DerivedBilledFromContactMechanism),
                m.Party.RolePattern(v => v.PartyName, v => v.PurchaseInvoicesWhereBilledFrom),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.PurchaseInvoicesWhereBilledFrom),
                m.Person.RolePattern(v => v.PartyName, v => v.PurchaseInvoicesWhereBilledFromContactPerson),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PurchaseInvoicesWhereDerivedBilledFromContactMechanism),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PurchaseInvoicesWhereDerivedBilledFromContactMechanism),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.PurchaseInvoicesWhereDerivedBilledFromContactMechanism),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PurchaseInvoicesWhereDerivedBilledFromContactMechanism),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PurchaseInvoicesWhereDerivedBilledFromContactMechanism),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PurchaseInvoicesWhereDerivedBilledFromContactMechanism),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.PostalAddress.PurchaseInvoicesWhereDerivedBilledFromContactMechanism),
                m.ElectronicAddress.RolePattern(v => v.ElectronicAddressString, v => v.PurchaseInvoicesWhereDerivedBilledFromContactMechanism),

                m.PurchaseInvoice.RolePattern(v => v.BilledTo),
                m.Party.RolePattern(v => v.PartyName, v => v.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.PurchaseInvoicesWhereBilledTo),
                m.Person.RolePattern(v => v.PartyName, v => v.PurchaseInvoicesWhereBilledToContactPerson),
                m.Person.RolePattern(v => v.PartyName, v => v.PurchaseInvoicesWhereBilledToContactPerson),
                m.PurchaseInvoice.RolePattern(v => v.DerivedShipToCustomerAddress),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PurchaseInvoicesWhereDerivedShipToCustomerAddress),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PurchaseInvoicesWhereDerivedShipToCustomerAddress),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.PurchaseInvoicesWhereDerivedShipToCustomerAddress),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PurchaseInvoicesWhereDerivedShipToCustomerAddress),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PurchaseInvoicesWhereDerivedShipToCustomerAddress),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PurchaseInvoicesWhereDerivedShipToCustomerAddress),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.PostalAddress.PurchaseInvoicesWhereDerivedShipToCustomerAddress),
                m.Party.RolePattern(v => v.ShippingAddress, v => v.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereShippingAddress.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereShippingAddress.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereShippingAddress.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereShippingAddress.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereShippingAddress.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereShippingAddress.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.PostalAddress.PartiesWhereShippingAddress.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Party.RolePattern(v => v.GeneralCorrespondence, v => v.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.PostalAddress.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.PurchaseInvoicesWhereBilledTo),

                m.PurchaseInvoice.RolePattern(v => v.PurchaseInvoiceItems),
                m.Part.RolePattern(v => v.Name, v => v.PurchaseInvoiceItemsWherePart.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseInvoiceItem.RolePattern(v => v.Description, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseInvoiceType.RolePattern(v => v.Name, v => v.PurchaseInvoicesWherePurchaseInvoiceType),
                m.PurchaseInvoiceItem.RolePattern(v => v.Quantity, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseInvoiceItem.RolePattern(v => v.UnitPrice, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseInvoiceItem.RolePattern(v => v.TotalExVat, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseInvoiceItem.RolePattern(v => v.Comment, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.SupplierOffering.RolePattern(v => v.SupplierProductId, v => v.Part.Part.PurchaseInvoiceItemsWherePart.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem),

                m.InvoiceItem.AssociationPattern(v => v.OrderItemBillingsWhereInvoiceItem, v => v.AsPurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.OrderItemBilling.RolePattern(v => v.OrderItem, v => v.InvoiceItem.InvoiceItem.AsPurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseOrder.RolePattern(v => v.OrderNumber, v => v.PurchaseOrderItems.PurchaseOrderItem.PurchaseInvoiceItemsWherePurchaseOrderItem.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem),

                m.PurchaseOrder.RolePattern(v => v.OrderAdjustments, v => v.PurchaseInvoicesWherePurchaseOrder),
                m.OrderAdjustment.AssociationPattern(v => v.InvoiceWhereOrderAdjustment, v => v.OrderWhereOrderAdjustment.Order.AsPurchaseOrder),
                m.OrderAdjustment.RolePattern(v => v.Description, v => v.OrderWhereOrderAdjustment.Order.AsPurchaseOrder),
                m.Invoice.RolePattern(v => v.TotalDiscount, v => v.OrderAdjustments.OrderAdjustment.OrderWhereOrderAdjustment.Order.AsPurchaseOrder),
                m.Invoice.RolePattern(v => v.TotalSurcharge, v => v.OrderAdjustments.OrderAdjustment.OrderWhereOrderAdjustment.Order.AsPurchaseOrder),
                m.Invoice.RolePattern(v => v.TotalFee, v => v.OrderAdjustments.OrderAdjustment.OrderWhereOrderAdjustment.Order.AsPurchaseOrder),
                m.Invoice.RolePattern(v => v.TotalShippingAndHandling, v => v.OrderAdjustments.OrderAdjustment.OrderWhereOrderAdjustment.Order.AsPurchaseOrder),
                m.Invoice.RolePattern(v => v.TotalExtraCharge, v => v.OrderAdjustments.OrderAdjustment.OrderWhereOrderAdjustment.Order.AsPurchaseOrder),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>())
            {
                @this.ResetPrintDocument();
            }
        }
    }
}
