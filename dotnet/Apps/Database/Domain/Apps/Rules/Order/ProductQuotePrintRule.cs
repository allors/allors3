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

    public class ProductQuotePrintRule : Rule
    {
        public ProductQuotePrintRule(MetaPopulation m) : base(m, new Guid("e74e0eee-7aba-4897-be74-e88b81a9b29f")) =>
            this.Patterns = new Pattern[]
            {
                m.ProductQuote.RolePattern(v => v.Description),
                m.ProductQuote.RolePattern(v => v.QuoteNumber),
                m.ProductQuote.RolePattern(v => v.IssueDate),
                m.ProductQuote.RolePattern(v => v.ValidFromDate),
                m.ProductQuote.RolePattern(v => v.ValidThroughDate),
                m.ProductQuote.RolePattern(v => v.TotalBasePrice),
                m.ProductQuote.RolePattern(v => v.TotalExVat),
                m.ProductQuote.RolePattern(v => v.DerivedVatRate),
                m.ProductQuote.RolePattern(v => v.TotalVat),
                m.ProductQuote.RolePattern(v => v.TotalIrpf),
                m.ProductQuote.RolePattern(v => v.DerivedIrpfRate),
                m.ProductQuote.RolePattern(v => v.TotalIncVat),
                m.ProductQuote.RolePattern(v => v.GrandTotal),

                m.Request.RolePattern(v => v.RequestNumber, v => v.QuoteWhereRequest.ObjectType.AsProductQuote),

                m.Organisation.RolePattern(v => v.DisplayName, v => v.QuotesWhereIssuer.ObjectType.AsProductQuote),
                m.Organisation.RolePattern(v => v.GeneralEmail, v => v.QuotesWhereIssuer.ObjectType.AsProductQuote),
                m.Organisation.RolePattern(v => v.InternetAddress, v => v.QuotesWhereIssuer.ObjectType.AsProductQuote),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.QuotesWhereIssuer.ObjectType.AsProductQuote),
                m.Organisation.RolePattern(v => v.CurrentPartyContactMechanisms, v => v.QuotesWhereIssuer.ObjectType.AsProductQuote),
                m.Organisation.RolePattern(v => v.CurrentPartyContactMechanisms, v => v.QuotesWhereReceiver.ObjectType.AsProductQuote),
                m.Person.RolePattern(v => v.CurrentPartyContactMechanisms, v => v.QuotesWhereContactPerson.ObjectType.AsProductQuote),
                m.Organisation.RolePattern(v => v.GeneralCorrespondence, v => v.QuotesWhereIssuer.ObjectType.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.QuotesWhereIssuer.ObjectType.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.QuotesWhereIssuer.ObjectType.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.QuotesWhereIssuer.ObjectType.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.QuotesWhereIssuer.ObjectType.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.QuotesWhereIssuer.ObjectType.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.QuotesWhereIssuer.ObjectType.AsProductQuote),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.ObjectType.PartiesWhereGeneralCorrespondence.ObjectType.AsOrganisation.QuotesWhereIssuer.ObjectType.AsProductQuote),
                m.Organisation.RolePattern(v => v.BankAccounts, v => v.QuotesWhereIssuer.ObjectType.AsProductQuote),
                m.BankAccount.RolePattern(v => v.Iban, v => v.PartyWhereBankAccount.ObjectType.AsOrganisation.QuotesWhereIssuer.ObjectType.AsProductQuote),
                m.Bank.RolePattern(v => v.Name, v => v.BankAccountsWhereBank.ObjectType.PartyWhereBankAccount.ObjectType.AsOrganisation.QuotesWhereIssuer.ObjectType.AsProductQuote),
                m.Bank.RolePattern(v => v.SwiftCode, v => v.BankAccountsWhereBank.ObjectType.PartyWhereBankAccount.ObjectType.AsOrganisation.QuotesWhereIssuer.ObjectType.AsProductQuote),
                m.Bank.RolePattern(v => v.Bic, v => v.BankAccountsWhereBank.ObjectType.PartyWhereBankAccount.ObjectType.AsOrganisation.QuotesWhereIssuer.ObjectType.AsProductQuote),

                m.ProductQuote.RolePattern(v => v.FullfillContactMechanism),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.QuotesWhereFullfillContactMechanism.ObjectType.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.QuotesWhereFullfillContactMechanism.ObjectType.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.QuotesWhereFullfillContactMechanism.ObjectType.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.QuotesWhereFullfillContactMechanism.ObjectType.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Region, v => v.QuotesWhereFullfillContactMechanism.ObjectType.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.QuotesWhereFullfillContactMechanism.ObjectType.AsProductQuote),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.ObjectType.QuotesWhereFullfillContactMechanism.ObjectType.AsProductQuote),
                m.ElectronicAddress.RolePattern(v => v.ElectronicAddressString, v => v.QuotesWhereFullfillContactMechanism.ObjectType.AsProductQuote),

                m.ProductQuote.RolePattern(v => v.Receiver),
                m.Party.RolePattern(v => v.DisplayName, v => v.QuotesWhereReceiver.ObjectType.AsProductQuote),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.QuotesWhereReceiver.ObjectType.AsProductQuote),
                m.Person.RolePattern(v => v.DisplayName, v => v.QuotesWhereContactPerson.ObjectType.AsProductQuote),
                m.Person.RolePattern(v => v.FirstName, v => v.QuotesWhereContactPerson.ObjectType.AsProductQuote),
                m.Salutation.RolePattern(v => v.Name, v => v.PeopleWhereSalutation.ObjectType.QuotesWhereContactPerson.ObjectType.AsProductQuote),
                m.Person.RolePattern(v => v.Function, v => v.QuotesWhereContactPerson.ObjectType.AsProductQuote),
                m.EmailAddress.RolePattern(v => v.ElectronicAddressString, v => v.PartyContactMechanismsWhereContactMechanism.ObjectType.PartyWhereCurrentPartyContactMechanism.ObjectType.AsPerson.QuotesWhereContactPerson.ObjectType.AsProductQuote),
                m.TelecommunicationsNumber.RolePattern(v => v.CountryCode, v => v.PartyContactMechanismsWhereContactMechanism.ObjectType.PartyWhereCurrentPartyContactMechanism.ObjectType.AsPerson.QuotesWhereContactPerson.ObjectType.AsProductQuote),
                m.TelecommunicationsNumber.RolePattern(v => v.AreaCode, v => v.PartyContactMechanismsWhereContactMechanism.ObjectType.PartyWhereCurrentPartyContactMechanism.ObjectType.AsPerson.QuotesWhereContactPerson.ObjectType.AsProductQuote),
                m.TelecommunicationsNumber.RolePattern(v => v.ContactNumber, v => v.PartyContactMechanismsWhereContactMechanism.ObjectType.PartyWhereCurrentPartyContactMechanism.ObjectType.AsPerson.QuotesWhereContactPerson.ObjectType.AsProductQuote),
                m.TelecommunicationsNumber.RolePattern(v => v.CountryCode, v => v.PartyContactMechanismsWhereContactMechanism.ObjectType.PartyWhereCurrentPartyContactMechanism.ObjectType.QuotesWhereReceiver.ObjectType.AsProductQuote),
                m.TelecommunicationsNumber.RolePattern(v => v.AreaCode, v => v.PartyContactMechanismsWhereContactMechanism.ObjectType.PartyWhereCurrentPartyContactMechanism.ObjectType.QuotesWhereReceiver.ObjectType.AsProductQuote),
                m.TelecommunicationsNumber.RolePattern(v => v.ContactNumber, v => v.PartyContactMechanismsWhereContactMechanism.ObjectType.PartyWhereCurrentPartyContactMechanism.ObjectType.QuotesWhereReceiver.ObjectType.AsProductQuote),

                m.ProductQuote.RolePattern(v => v.QuoteItems),
                m.QuoteItem.RolePattern(v => v.Product, v => v.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.QuoteItem.RolePattern(v => v.SerialisedItem, v => v.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.InvoiceItemType.RolePattern(v => v.Name, v => v.QuoteItemsWhereInvoiceItemType.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.SerialisedItem.RolePattern(v => v.DisplayName, v => v.QuoteItemsWhereSerialisedItem.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.Product.RolePattern(v => v.Name, v => v.QuoteItemsWhereProduct.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.SerialisedItem.RolePattern(v => v.Description, v => v.QuoteItemsWhereSerialisedItem.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.Product.RolePattern(v => v.Description, v => v.QuoteItemsWhereProduct.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.QuoteItem.RolePattern(v => v.Details, v => v.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.QuoteItem.RolePattern(v => v.Quantity, v => v.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.QuoteItem.RolePattern(v => v.UnitPrice, v => v.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.QuoteItem.RolePattern(v => v.TotalExVat, v => v.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.QuoteItem.RolePattern(v => v.Comment, v => v.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.Product.AssociationPattern(v => v.ProductCategoriesWhereProduct, v => v.QuoteItemsWhereProduct.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.Brand.RolePattern(v => v.Name, v => v.PartsWhereBrand.ObjectType.AsUnifiedGood.QuoteItemsWhereProduct.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.Model.RolePattern(v => v.Name, v => v.PartsWhereModel.ObjectType.AsUnifiedGood.QuoteItemsWhereProduct.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.Brand.RolePattern(v => v.Name, v => v.PartsWhereBrand.ObjectType.AsNonUnifiedPart.NonUnifiedGoodsWherePart.ObjectType.QuoteItemsWhereProduct.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.Model.RolePattern(v => v.Name, v => v.PartsWhereModel.ObjectType.AsNonUnifiedPart.NonUnifiedGoodsWherePart.ObjectType.QuoteItemsWhereProduct.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.SerialisedItem.RolePattern(v => v.ItemNumber, v => v.QuoteItemsWhereSerialisedItem.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.SerialisedItem.RolePattern(v => v.Comment, v => v.QuoteItemsWhereSerialisedItem.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.SerialisedItem.RolePattern(v => v.ManufacturingYear, v => v.QuoteItemsWhereSerialisedItem.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.SerialisedItemCharacteristic.AssociationPattern(v => v.SerialisedItemWhereSerialisedItemCharacteristic, v => v.SerialisedItemWhereSerialisedItemCharacteristic.ObjectType.QuoteItemsWhereSerialisedItem.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.SerialisedItemCharacteristic.RolePattern(v => v.Value, v => v.SerialisedItemWhereSerialisedItemCharacteristic.ObjectType.QuoteItemsWhereSerialisedItem.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.SerialisedItemCharacteristic.AssociationPattern(v => v.PartWhereSerialisedItemCharacteristic, v => v.PartWhereSerialisedItemCharacteristic.ObjectType.QuoteItemsWhereProduct.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.SerialisedItemCharacteristic.RolePattern(v => v.Value, v => v.PartWhereSerialisedItemCharacteristic.ObjectType.QuoteItemsWhereProduct.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.UnitOfMeasure.RolePattern(v => v.Abbreviation, v => v.SerialisedItemCharacteristicTypesWhereUnitOfMeasure.ObjectType.SerialisedItemCharacteristicsWhereSerialisedItemCharacteristicType.ObjectType.SerialisedItemWhereSerialisedItemCharacteristic.ObjectType.QuoteItemsWhereSerialisedItem.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.SerialisedItem.RolePattern(v => v.PrimaryPhoto, v => v.QuoteItemsWhereSerialisedItem.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.SerialisedItem.RolePattern(v => v.AdditionalPhotos, v => v.QuoteItemsWhereSerialisedItem.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.ProductIdentification.RolePattern(v => v.Identification, v => v.UnifiedProductWhereProductIdentification.ObjectType.AsProduct.QuoteItemsWhereProduct.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.Product.RolePattern(v => v.PrimaryPhoto, v => v.QuoteItemsWhereProduct.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),
                m.Product.RolePattern(v => v.Photos, v => v.QuoteItemsWhereProduct.ObjectType.QuoteWhereQuoteItem.ObjectType.AsProductQuote),

                m.ProductQuote.RolePattern(v => v.OrderAdjustments),
                m.OrderAdjustment.RolePattern(v => v.Description, v => v.QuoteWhereOrderAdjustment.ObjectType.AsProductQuote),
                m.ProductQuote.RolePattern(v => v.TotalDiscount),
                m.ProductQuote.RolePattern(v => v.TotalSurcharge),
                m.ProductQuote.RolePattern(v => v.TotalFee),
                m.ProductQuote.RolePattern(v => v.TotalShippingAndHandling),
                m.ProductQuote.RolePattern(v => v.TotalExtraCharge),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<ProductQuote>())
            {
                @this.ResetPrintDocument();
            }
        }
    }
}
