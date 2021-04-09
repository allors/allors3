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

                m.Request.RolePattern(v => v.RequestNumber, v => v.QuoteWhereRequest.Quote.AsProductQuote),

                m.ProductQuote.RolePattern(v => v.Issuer, v => v.Issuer.InternalOrganisation.AsOrganisation.QuotesWhereIssuer.Quote.AsProductQuote),
                m.Organisation.RolePattern(v => v.PartyName, v => v.QuotesWhereIssuer.Quote.AsProductQuote),
                m.Organisation.RolePattern(v => v.GeneralEmail, v => v.QuotesWhereIssuer.Quote.AsProductQuote),
                m.Organisation.RolePattern(v => v.InternetAddress, v => v.QuotesWhereIssuer.Quote.AsProductQuote),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.QuotesWhereIssuer.Quote.AsProductQuote),
                m.Organisation.RolePattern(v => v.CurrentPartyContactMechanisms, v => v.QuotesWhereIssuer.Quote.AsProductQuote),
                m.Organisation.RolePattern(v => v.GeneralCorrespondence, v => v.QuotesWhereIssuer.Quote.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProductQuote),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.PostalAddress.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProductQuote),
                m.Organisation.RolePattern(v => v.BankAccounts, v => v.QuotesWhereIssuer.Quote.AsProductQuote),
                m.BankAccount.RolePattern(v => v.Iban, v => v.PartyWhereBankAccount.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProductQuote),
                m.Bank.RolePattern(v => v.Name, v => v.BankAccountsWhereBank.BankAccount.PartyWhereBankAccount.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProductQuote),
                m.Bank.RolePattern(v => v.SwiftCode, v => v.BankAccountsWhereBank.BankAccount.PartyWhereBankAccount.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProductQuote),
                m.Bank.RolePattern(v => v.Bic, v => v.BankAccountsWhereBank.BankAccount.PartyWhereBankAccount.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProductQuote),

                m.ProductQuote.RolePattern(v => v.FullfillContactMechanism),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.QuotesWhereFullfillContactMechanism.Quote.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.QuotesWhereFullfillContactMechanism.Quote.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.QuotesWhereFullfillContactMechanism.Quote.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.QuotesWhereFullfillContactMechanism.Quote.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.Region, v => v.QuotesWhereFullfillContactMechanism.Quote.AsProductQuote),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.QuotesWhereFullfillContactMechanism.Quote.AsProductQuote),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.PostalAddress.QuotesWhereFullfillContactMechanism.Quote.AsProductQuote),
                m.ElectronicAddress.RolePattern(v => v.ElectronicAddressString, v => v.QuotesWhereFullfillContactMechanism.Quote.AsProductQuote),

                m.ProductQuote.RolePattern(v => v.Receiver),
                m.Party.RolePattern(v => v.PartyName, v => v.QuotesWhereReceiver.Quote.AsProductQuote),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.QuotesWhereReceiver.Quote.AsProductQuote),
                m.Person.RolePattern(v => v.PartyName, v => v.QuotesWhereContactPerson.Quote.AsProductQuote),
                m.Person.RolePattern(v => v.FirstName, v => v.QuotesWhereContactPerson.Quote.AsProductQuote),
                m.Salutation.RolePattern(v => v.Name, v => v.PeopleWhereSalutation.Person.QuotesWhereContactPerson.Quote.AsProductQuote),
                m.Person.RolePattern(v => v.Function, v => v.QuotesWhereContactPerson.Quote.AsProductQuote),
                m.EmailAddress.RolePattern(v => v.ElectronicAddressString, v => v.PartyContactMechanismsWhereContactMechanism.PartyContactMechanism.PartyWhereCurrentPartyContactMechanism.Party.AsPerson.QuotesWhereContactPerson.Quote.AsProductQuote),
                m.TelecommunicationsNumber.RolePattern(v => v.CountryCode, v => v.PartyContactMechanismsWhereContactMechanism.PartyContactMechanism.PartyWhereCurrentPartyContactMechanism.Party.AsPerson.QuotesWhereContactPerson.Quote.AsProductQuote),
                m.TelecommunicationsNumber.RolePattern(v => v.AreaCode, v => v.PartyContactMechanismsWhereContactMechanism.PartyContactMechanism.PartyWhereCurrentPartyContactMechanism.Party.AsPerson.QuotesWhereContactPerson.Quote.AsProductQuote),
                m.TelecommunicationsNumber.RolePattern(v => v.ContactNumber, v => v.PartyContactMechanismsWhereContactMechanism.PartyContactMechanism.PartyWhereCurrentPartyContactMechanism.Party.AsPerson.QuotesWhereContactPerson.Quote.AsProductQuote),

                m.ProductQuote.RolePattern(v => v.QuoteItems),
                m.QuoteItem.RolePattern(v => v.Product, v => v.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.QuoteItem.RolePattern(v => v.SerialisedItem, v => v.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.InvoiceItemType.RolePattern(v => v.Name, v => v.QuoteItemsWhereInvoiceItemType.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.SerialisedItem.RolePattern(v => v.Name, v => v.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.Product.RolePattern(v => v.Name, v => v.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.SerialisedItem.RolePattern(v => v.Description, v => v.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.Product.RolePattern(v => v.Description, v => v.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.QuoteItem.RolePattern(v => v.Details, v => v.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.QuoteItem.RolePattern(v => v.Quantity, v => v.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.QuoteItem.RolePattern(v => v.UnitPrice, v => v.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.QuoteItem.RolePattern(v => v.TotalExVat, v => v.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.QuoteItem.RolePattern(v => v.Comment, v => v.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.Product.AssociationPattern(v => v.ProductCategoriesWhereProduct, v => v.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.Brand.RolePattern(v => v.Name, v => v.PartsWhereBrand.Part.AsUnifiedGood.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.Model.RolePattern(v => v.Name, v => v.PartsWhereModel.Part.AsUnifiedGood.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.Brand.RolePattern(v => v.Name, v => v.PartsWhereBrand.Part.AsNonUnifiedPart.NonUnifiedGoodsWherePart.NonUnifiedGood.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.Model.RolePattern(v => v.Name, v => v.PartsWhereModel.Part.AsNonUnifiedPart.NonUnifiedGoodsWherePart.NonUnifiedGood.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.SerialisedItem.RolePattern(v => v.ItemNumber, v => v.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.SerialisedItem.RolePattern(v => v.ManufacturingYear, v => v.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.SerialisedItemCharacteristic.AssociationPattern(v => v.SerialisedItemWhereSerialisedItemCharacteristic, v => v.SerialisedItemWhereSerialisedItemCharacteristic.SerialisedItem.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.SerialisedItemCharacteristic.RolePattern(v => v.Value, v => v.SerialisedItemWhereSerialisedItemCharacteristic.SerialisedItem.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.UnitOfMeasure.RolePattern(v => v.Abbreviation, v => v.SerialisedItemCharacteristicTypesWhereUnitOfMeasure.SerialisedItemCharacteristicType.SerialisedItemCharacteristicsWhereSerialisedItemCharacteristicType.SerialisedItemCharacteristic.SerialisedItemWhereSerialisedItemCharacteristic.SerialisedItem.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.SerialisedItem.RolePattern(v => v.PrimaryPhoto, v => v.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.SerialisedItem.RolePattern(v => v.AdditionalPhotos, v => v.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.ProductIdentification.RolePattern(v => v.Identification, v => v.UnifiedProductWhereProductIdentification.UnifiedProduct.AsProduct.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.Product.RolePattern(v => v.PrimaryPhoto, v => v.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),
                m.Product.RolePattern(v => v.Photos, v => v.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProductQuote),

                m.ProductQuote.RolePattern(v => v.OrderAdjustments),
                m.OrderAdjustment.RolePattern(v => v.Description, v => v.QuoteWhereOrderAdjustment.Quote.AsProductQuote),
                m.ProductQuote.RolePattern(v => v.TotalDiscount),
                m.ProductQuote.RolePattern(v => v.TotalSurcharge),
                m.ProductQuote.RolePattern(v => v.TotalFee),
                m.ProductQuote.RolePattern(v => v.TotalShippingAndHandling),
                m.ProductQuote.RolePattern(v => v.TotalExtraCharge),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<ProductQuote>())
            {
                @this.ResetPrintDocument();
            }
        }
    }
}
