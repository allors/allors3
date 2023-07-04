// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

    public class PurchaseOrderPrintRule : Rule
    {
        public PurchaseOrderPrintRule(MetaPopulation m) : base(m, new Guid("dee4f5a7-6976-4a27-9372-2875b92c2f5d")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrder.RolePattern(v => v.Description),
                m.PurchaseOrder.RolePattern(v => v.OrderNumber),
                m.PurchaseOrder.RolePattern(v => v.OrderDate),
                m.PurchaseOrder.RolePattern(v => v.CustomerReference),
                m.PurchaseOrder.RolePattern(v => v.TotalBasePrice),
                m.PurchaseOrder.RolePattern(v => v.TotalExVat),
                m.VatRate.RolePattern(v => v.Rate, v => v.OrdersWhereDerivedVatRate.ObjectType.AsPurchaseOrder),
                m.PurchaseOrder.RolePattern(v => v.TotalVat),
                m.IrpfRate.RolePattern(v => v.Rate, v => v.OrdersWhereDerivedIrpfRate.ObjectType.AsPurchaseOrder),
                m.PurchaseOrder.RolePattern(v => v.TotalIrpf),
                m.PurchaseOrder.RolePattern(v => v.TotalIncVat),
                m.Currency.RolePattern(v => v.IsoCode, v => v.OrdersWhereDerivedCurrency.ObjectType.AsPurchaseOrder),
                m.PurchaseOrder.RolePattern(v => v.GrandTotal),

                m.PurchaseOrder.RolePattern(v => v.OrderedBy),
                m.Organisation.RolePattern(v => v.DisplayName, v => v.PurchaseOrdersWhereOrderedBy),
                m.EmailAddress.RolePattern(v => v.ElectronicAddressString, v => v.PartiesWhereGeneralEmail.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.EmailAddress.RolePattern(v => v.ElectronicAddressString, v => v.PartiesWhereInternetAddress.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.PurchaseOrdersWhereOrderedBy),
                m.Organisation.RolePattern(v => v.BillingInquiriesPhone, v => v.PurchaseOrdersWhereOrderedBy),
                m.Organisation.RolePattern(v => v.GeneralPhoneNumber, v => v.PurchaseOrdersWhereOrderedBy),
                m.TelecommunicationsNumber.RolePattern(v => v.CountryCode, v => v.PartiesWhereBillingInquiriesPhone.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.TelecommunicationsNumber.RolePattern(v => v.AreaCode, v => v.PartiesWhereBillingInquiriesPhone.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.TelecommunicationsNumber.RolePattern(v => v.ContactNumber, v => v.PartiesWhereBillingInquiriesPhone.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.TelecommunicationsNumber.RolePattern(v => v.CountryCode, v => v.PartiesWhereGeneralPhoneNumber.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.TelecommunicationsNumber.RolePattern(v => v.AreaCode, v => v.PartiesWhereGeneralPhoneNumber.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.TelecommunicationsNumber.RolePattern(v => v.ContactNumber, v => v.PartiesWhereGeneralPhoneNumber.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.Organisation.RolePattern(v => v.GeneralCorrespondence, v => v.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.ObjectType.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.Organisation.RolePattern(v => v.BankAccounts, v => v.PurchaseOrdersWhereOrderedBy),
                m.BankAccount.RolePattern(v => v.Iban, v => v.PartyWhereBankAccount.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.BankAccount.RolePattern(v => v.Bank, v => v.PartyWhereBankAccount.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.Bank.RolePattern(v => v.Name, v => v.BankAccountsWhereBank.ObjectType.PartyWhereBankAccount.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.Bank.RolePattern(v => v.SwiftCode, v => v.BankAccountsWhereBank.ObjectType.PartyWhereBankAccount.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.Bank.RolePattern(v => v.Bic, v => v.BankAccountsWhereBank.ObjectType.PartyWhereBankAccount.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),

                m.PurchaseOrder.RolePattern(v => v.TakenViaSupplier),
                m.PurchaseOrder.RolePattern(v => v.TakenViaSubcontractor),
                m.PurchaseOrder.RolePattern(v => v.TakenViaContactPerson),
                m.PurchaseOrder.RolePattern(v => v.DerivedTakenViaContactMechanism),
                m.Organisation.RolePattern(v => v.DisplayName, v => v.PurchaseOrdersWhereTakenViaSupplier),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.PurchaseOrdersWhereTakenViaSupplier),
                m.Organisation.RolePattern(v => v.DisplayName, v => v.PurchaseOrdersWhereTakenViaSubcontractor),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.PurchaseOrdersWhereTakenViaSubcontractor),
                m.Person.RolePattern(v => v.DisplayName, v => v.PurchaseOrdersWhereTakenViaContactPerson),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PurchaseOrdersWhereDerivedTakenViaContactMechanism),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PurchaseOrdersWhereDerivedTakenViaContactMechanism),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.PurchaseOrdersWhereDerivedTakenViaContactMechanism),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PurchaseOrdersWhereDerivedTakenViaContactMechanism),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PurchaseOrdersWhereDerivedTakenViaContactMechanism),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PurchaseOrdersWhereDerivedTakenViaContactMechanism),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.ObjectType.PurchaseOrdersWhereDerivedTakenViaContactMechanism),
                m.ElectronicAddress.RolePattern(v => v.ElectronicAddressString, v => v.PurchaseOrdersWhereDerivedTakenViaContactMechanism),

                m.PurchaseOrder.RolePattern(v => v.OrderedBy),
                m.InternalOrganisation.RolePattern(v => v.DisplayName, v => v.PurchaseOrdersWhereOrderedBy),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.PurchaseOrdersWhereOrderedBy),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.PurchaseOrdersWhereOrderedBy),
                m.Person.RolePattern(v => v.DisplayName, v => v.PurchaseOrdersWhereShipToContactPerson),
                m.PurchaseOrder.RolePattern(v => v.DerivedShipToAddress),
                m.InternalOrganisation.RolePattern(v => v.ShippingAddress, v => v.PurchaseOrdersWhereOrderedBy),
                m.InternalOrganisation.RolePattern(v => v.GeneralCorrespondence, v => v.PurchaseOrdersWhereOrderedBy),
                m.PurchaseOrder.RolePattern(v => v.DerivedBillToContactMechanism),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PurchaseOrdersWhereDerivedShipToAddress),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PurchaseOrdersWhereDerivedShipToAddress),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.PurchaseOrdersWhereDerivedShipToAddress),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PurchaseOrdersWhereDerivedShipToAddress),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PurchaseOrdersWhereDerivedShipToAddress),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PurchaseOrdersWhereDerivedShipToAddress),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.ObjectType.PurchaseOrdersWhereDerivedShipToAddress),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereShippingAddress.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereShippingAddress.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereShippingAddress.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereShippingAddress.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereShippingAddress.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereShippingAddress.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.ObjectType.PartiesWhereShippingAddress.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.ObjectType.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.PurchaseOrdersWhereOrderedBy),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PurchaseOrdersWhereDerivedBillToContactMechanism),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PurchaseOrdersWhereDerivedBillToContactMechanism),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.PurchaseOrdersWhereDerivedBillToContactMechanism),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PurchaseOrdersWhereDerivedBillToContactMechanism),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PurchaseOrdersWhereDerivedBillToContactMechanism),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PurchaseOrdersWhereDerivedBillToContactMechanism),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.ObjectType.PurchaseOrdersWhereDerivedBillToContactMechanism),

                m.PurchaseOrder.RolePattern(v => v.PurchaseOrderItems),
                m.Part.RolePattern(v => v.Name, v => v.PurchaseOrderItemsWherePart.ObjectType.PurchaseOrderWherePurchaseOrderItem),
                m.PurchaseOrderItem.RolePattern(v => v.Description, v => v.PurchaseOrderWherePurchaseOrderItem),
                m.InvoiceItemType.RolePattern(v => v.Name, v => v.PurchaseOrderItemsWhereInvoiceItemType.ObjectType.PurchaseOrderWherePurchaseOrderItem),
                m.PurchaseOrderItem.RolePattern(v => v.QuantityOrdered, v => v.PurchaseOrderWherePurchaseOrderItem),
                m.PurchaseOrderItem.RolePattern(v => v.UnitPrice, v => v.PurchaseOrderWherePurchaseOrderItem),
                m.PurchaseOrderItem.RolePattern(v => v.TotalExVat, v => v.PurchaseOrderWherePurchaseOrderItem),
                m.PurchaseOrderItem.RolePattern(v => v.Comment, v => v.PurchaseOrderWherePurchaseOrderItem),
                m.SupplierOffering.RolePattern(v => v.SupplierProductId, v => v.Part.ObjectType.PurchaseOrderItemsWherePart.ObjectType.PurchaseOrderWherePurchaseOrderItem),

            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>())
            {
                @this.ResetPrintDocument();
            }
        }
    }
}
