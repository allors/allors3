// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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
                m.VatRate.RolePattern(v => v.Rate, v => v.InvoicesWhereDerivedVatRate.ObjectType.AsPurchaseInvoice),
                m.PurchaseInvoice.RolePattern(v => v.TotalVat),
                m.IrpfRate.RolePattern(v => v.Rate, v => v.InvoicesWhereDerivedIrpfRate.ObjectType.AsPurchaseInvoice),
                m.PurchaseInvoice.RolePattern(v => v.TotalIrpf),
                m.PurchaseInvoice.RolePattern(v => v.TotalIncVat),
                m.PurchaseInvoice.RolePattern(v => v.GrandTotal),
                m.Currency.RolePattern(v => v.IsoCode, v => v.InvoicesWhereDerivedCurrency.ObjectType.AsPurchaseInvoice),

                m.PurchaseInvoice.RolePattern(v => v.BilledTo),
                m.Organisation.RolePattern(v => v.DisplayName, v => v.PurchaseInvoicesWhereBilledTo),
                m.EmailAddress.RolePattern(v => v.ElectronicAddressString, v => v.PartiesWhereGeneralEmail.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.ElectronicAddress.RolePattern(v => v.ElectronicAddressString, v => v.PartiesWhereInternetAddress.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.PurchaseInvoicesWhereBilledTo),
                m.Organisation.RolePattern(v => v.BillingInquiriesPhone, v => v.PurchaseInvoicesWhereBilledTo),
                m.Organisation.RolePattern(v => v.GeneralPhoneNumber, v => v.PurchaseInvoicesWhereBilledTo),
                m.TelecommunicationsNumber.RolePattern(v => v.CountryCode, v => v.PartiesWhereBillingInquiriesPhone.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo.ObjectType),
                m.TelecommunicationsNumber.RolePattern(v => v.AreaCode, v => v.PartiesWhereBillingInquiriesPhone.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.TelecommunicationsNumber.RolePattern(v => v.ContactNumber, v => v.PartiesWhereBillingInquiriesPhone.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.TelecommunicationsNumber.RolePattern(v => v.CountryCode, v => v.PartiesWhereGeneralPhoneNumber.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.TelecommunicationsNumber.RolePattern(v => v.AreaCode, v => v.PartiesWhereGeneralPhoneNumber.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.TelecommunicationsNumber.RolePattern(v => v.ContactNumber, v => v.PartiesWhereGeneralPhoneNumber.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Organisation.RolePattern(v => v.GeneralCorrespondence, v => v.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.ObjectType.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Organisation.RolePattern(v => v.BankAccounts, v => v.PurchaseInvoicesWhereBilledTo),
                m.BankAccount.RolePattern(v => v.Iban, v => v.PartyWhereBankAccount.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.BankAccount.RolePattern(v => v.Bank, v => v.PartyWhereBankAccount.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Bank.RolePattern(v => v.Name, v => v.BankAccountsWhereBank.ObjectType.PartyWhereBankAccount.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Bank.RolePattern(v => v.SwiftCode, v => v.BankAccountsWhereBank.ObjectType.PartyWhereBankAccount.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Bank.RolePattern(v => v.Bic, v => v.BankAccountsWhereBank.ObjectType.PartyWhereBankAccount.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),

                m.PurchaseInvoice.RolePattern(v => v.BilledFrom),
                m.PurchaseInvoice.RolePattern(v => v.BilledFromContactPerson),
                m.PurchaseInvoice.RolePattern(v => v.DerivedBilledFromContactMechanism),
                m.Organisation.RolePattern(v => v.DisplayName, v => v.PurchaseInvoicesWhereBilledFrom),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.PurchaseInvoicesWhereBilledFrom),
                m.Person.RolePattern(v => v.DisplayName, v => v.PurchaseInvoicesWhereBilledFromContactPerson),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PurchaseInvoicesWhereDerivedBilledFromContactMechanism),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PurchaseInvoicesWhereDerivedBilledFromContactMechanism),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.PurchaseInvoicesWhereDerivedBilledFromContactMechanism),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PurchaseInvoicesWhereDerivedBilledFromContactMechanism),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PurchaseInvoicesWhereDerivedBilledFromContactMechanism),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PurchaseInvoicesWhereDerivedBilledFromContactMechanism),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.ObjectType.PurchaseInvoicesWhereDerivedBilledFromContactMechanism),
                m.ElectronicAddress.RolePattern(v => v.ElectronicAddressString, v => v.PurchaseInvoicesWhereDerivedBilledFromContactMechanism),

                m.PurchaseInvoice.RolePattern(v => v.BilledTo),
                m.Party.RolePattern(v => v.DisplayName, v => v.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.PurchaseInvoicesWhereBilledTo),
                m.Person.RolePattern(v => v.DisplayName, v => v.PurchaseInvoicesWhereBilledToContactPerson),
                m.Person.RolePattern(v => v.DisplayName, v => v.PurchaseInvoicesWhereBilledToContactPerson),
                m.PurchaseInvoice.RolePattern(v => v.DerivedShipToCustomerAddress),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PurchaseInvoicesWhereDerivedShipToCustomerAddress),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PurchaseInvoicesWhereDerivedShipToCustomerAddress),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.PurchaseInvoicesWhereDerivedShipToCustomerAddress),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PurchaseInvoicesWhereDerivedShipToCustomerAddress),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PurchaseInvoicesWhereDerivedShipToCustomerAddress),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PurchaseInvoicesWhereDerivedShipToCustomerAddress),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.ObjectType.PurchaseInvoicesWhereDerivedShipToCustomerAddress),
                m.Party.RolePattern(v => v.ShippingAddress, v => v.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereShippingAddress.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereShippingAddress.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereShippingAddress.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereShippingAddress.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereShippingAddress.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereShippingAddress.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.ObjectType.PartiesWhereShippingAddress.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Party.RolePattern(v => v.GeneralCorrespondence, v => v.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.ObjectType.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseInvoicesWhereBilledTo),

                m.PurchaseInvoice.RolePattern(v => v.PurchaseInvoiceItems),
                m.Part.RolePattern(v => v.Name, v => v.PurchaseInvoiceItemsWherePart.ObjectType.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseInvoiceItem.RolePattern(v => v.Description, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseInvoiceType.RolePattern(v => v.Name, v => v.PurchaseInvoicesWherePurchaseInvoiceType),
                m.PurchaseInvoiceItem.RolePattern(v => v.Quantity, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseInvoiceItem.RolePattern(v => v.UnitPrice, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseInvoiceItem.RolePattern(v => v.TotalExVat, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseInvoiceItem.RolePattern(v => v.Comment, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.SupplierOffering.RolePattern(v => v.SupplierProductId, v => v.Part.ObjectType.PurchaseInvoiceItemsWherePart.ObjectType.PurchaseInvoiceWherePurchaseInvoiceItem),

                m.InvoiceItem.AssociationPattern(v => v.OrderItemBillingsWhereInvoiceItem, v => v.AsPurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.OrderItemBilling.RolePattern(v => v.OrderItem, v => v.InvoiceItem.ObjectType.AsPurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseOrder.RolePattern(v => v.OrderNumber, v => v.PurchaseOrderItems.ObjectType.PurchaseInvoiceItemsWherePurchaseOrderItem.ObjectType.PurchaseInvoiceWherePurchaseInvoiceItem),

                m.PurchaseOrder.RolePattern(v => v.OrderAdjustments, v => v.PurchaseInvoicesWherePurchaseOrder),
                m.OrderAdjustment.AssociationPattern(v => v.InvoiceWhereOrderAdjustment, v => v.OrderWhereOrderAdjustment.ObjectType.AsPurchaseOrder),
                m.OrderAdjustment.RolePattern(v => v.Description, v => v.OrderWhereOrderAdjustment.ObjectType.AsPurchaseOrder),
                m.Invoice.RolePattern(v => v.TotalDiscount, v => v.OrderAdjustments.ObjectType.OrderWhereOrderAdjustment.ObjectType.AsPurchaseOrder),
                m.Invoice.RolePattern(v => v.TotalSurcharge, v => v.OrderAdjustments.ObjectType.OrderWhereOrderAdjustment.ObjectType.AsPurchaseOrder),
                m.Invoice.RolePattern(v => v.TotalFee, v => v.OrderAdjustments.ObjectType.OrderWhereOrderAdjustment.ObjectType.AsPurchaseOrder),
                m.Invoice.RolePattern(v => v.TotalShippingAndHandling, v => v.OrderAdjustments.ObjectType.OrderWhereOrderAdjustment.ObjectType.AsPurchaseOrder),
                m.Invoice.RolePattern(v => v.TotalExtraCharge, v => v.OrderAdjustments.ObjectType.OrderWhereOrderAdjustment.ObjectType.AsPurchaseOrder),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>())
            {
                @this.ResetPrintDocument();
            }
        }
    }
}
