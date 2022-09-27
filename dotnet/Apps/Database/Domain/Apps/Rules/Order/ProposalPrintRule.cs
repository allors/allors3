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

    public class ProposalPrintRule : Rule
    {
        public ProposalPrintRule(MetaPopulation m) : base(m, new Guid("fdd2e051-229e-40a8-acf7-9cb22573757e")) =>
            this.Patterns = new Pattern[]
            {
                m.Proposal.RolePattern(v => v.Description),
                m.Proposal.RolePattern(v => v.QuoteNumber),
                m.Proposal.RolePattern(v => v.IssueDate),
                m.Proposal.RolePattern(v => v.ValidFromDate),
                m.Proposal.RolePattern(v => v.ValidThroughDate),
                m.Proposal.RolePattern(v => v.TotalBasePrice),
                m.Proposal.RolePattern(v => v.TotalExVat),
                m.Proposal.RolePattern(v => v.DerivedVatRate),
                m.Proposal.RolePattern(v => v.TotalVat),
                m.Proposal.RolePattern(v => v.TotalIrpf),
                m.Proposal.RolePattern(v => v.DerivedIrpfRate),
                m.Proposal.RolePattern(v => v.TotalIncVat),
                m.Proposal.RolePattern(v => v.GrandTotal),

                m.Request.RolePattern(v => v.RequestNumber, v => v.QuoteWhereRequest.Quote.AsProposal),

                m.Proposal.RolePattern(v => v.Issuer, v => v.Issuer.InternalOrganisation.AsOrganisation.QuotesWhereIssuer.Quote.AsProposal),
                m.Organisation.RolePattern(v => v.DisplayName, v => v.QuotesWhereIssuer.Quote.AsProposal),
                m.Organisation.RolePattern(v => v.GeneralEmail, v => v.QuotesWhereIssuer.Quote.AsProposal),
                m.Organisation.RolePattern(v => v.InternetAddress, v => v.QuotesWhereIssuer.Quote.AsProposal),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.QuotesWhereIssuer.Quote.AsProposal),
                m.Organisation.RolePattern(v => v.CurrentPartyContactMechanisms, v => v.QuotesWhereIssuer.Quote.AsProposal),
                m.Organisation.RolePattern(v => v.GeneralCorrespondence, v => v.QuotesWhereIssuer.Quote.AsProposal),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProposal),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProposal),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProposal),
                m.PostalAddress.RolePattern(v => v.Locality, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProposal),
                m.PostalAddress.RolePattern(v => v.Region, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProposal),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProposal),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.PostalAddress.PartiesWhereGeneralCorrespondence.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProposal),
                m.Organisation.RolePattern(v => v.BankAccounts, v => v.QuotesWhereIssuer.Quote.AsProposal),
                m.BankAccount.RolePattern(v => v.Iban, v => v.PartyWhereBankAccount.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProposal),
                m.Bank.RolePattern(v => v.Name, v => v.BankAccountsWhereBank.BankAccount.PartyWhereBankAccount.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProposal),
                m.Bank.RolePattern(v => v.SwiftCode, v => v.BankAccountsWhereBank.BankAccount.PartyWhereBankAccount.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProposal),
                m.Bank.RolePattern(v => v.Bic, v => v.BankAccountsWhereBank.BankAccount.PartyWhereBankAccount.Party.AsOrganisation.QuotesWhereIssuer.Quote.AsProposal),

                m.Proposal.RolePattern(v => v.FullfillContactMechanism),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.QuotesWhereFullfillContactMechanism.Quote.AsProposal),
                m.PostalAddress.RolePattern(v => v.Address2, v => v.QuotesWhereFullfillContactMechanism.Quote.AsProposal),
                m.PostalAddress.RolePattern(v => v.Address3, v => v.QuotesWhereFullfillContactMechanism.Quote.AsProposal),
                m.PostalAddress.RolePattern(v => v.Address1, v => v.QuotesWhereFullfillContactMechanism.Quote.AsProposal),
                m.PostalAddress.RolePattern(v => v.Region, v => v.QuotesWhereFullfillContactMechanism.Quote.AsProposal),
                m.PostalAddress.RolePattern(v => v.PostalCode, v => v.QuotesWhereFullfillContactMechanism.Quote.AsProposal),
                m.Country.RolePattern(v => v.Name, v => v.PostalAddressesWhereCountry.PostalAddress.QuotesWhereFullfillContactMechanism.Quote.AsProposal),
                m.ElectronicAddress.RolePattern(v => v.ElectronicAddressString, v => v.QuotesWhereFullfillContactMechanism.Quote.AsProposal),

                m.Proposal.RolePattern(v => v.Receiver),
                m.Party.RolePattern(v => v.DisplayName, v => v.QuotesWhereReceiver.Quote.AsProposal),
                m.Organisation.RolePattern(v => v.TaxNumber, v => v.QuotesWhereReceiver.Quote.AsProposal),
                m.Person.RolePattern(v => v.DisplayName, v => v.QuotesWhereContactPerson.Quote.AsProposal),
                m.Person.RolePattern(v => v.FirstName, v => v.QuotesWhereContactPerson.Quote.AsProposal),
                m.Salutation.RolePattern(v => v.Name, v => v.PeopleWhereSalutation.Person.QuotesWhereContactPerson.Quote.AsProposal),
                m.Person.RolePattern(v => v.Function, v => v.QuotesWhereContactPerson.Quote.AsProposal),
                m.EmailAddress.RolePattern(v => v.ElectronicAddressString, v => v.PartyContactMechanismsWhereContactMechanism.PartyContactMechanism.PartyWhereCurrentPartyContactMechanism.Party.AsPerson.QuotesWhereContactPerson.Quote.AsProposal),
                m.TelecommunicationsNumber.RolePattern(v => v.CountryCode, v => v.PartyContactMechanismsWhereContactMechanism.PartyContactMechanism.PartyWhereCurrentPartyContactMechanism.Party.AsPerson.QuotesWhereContactPerson.Quote.AsProposal),
                m.TelecommunicationsNumber.RolePattern(v => v.AreaCode, v => v.PartyContactMechanismsWhereContactMechanism.PartyContactMechanism.PartyWhereCurrentPartyContactMechanism.Party.AsPerson.QuotesWhereContactPerson.Quote.AsProposal),
                m.TelecommunicationsNumber.RolePattern(v => v.ContactNumber, v => v.PartyContactMechanismsWhereContactMechanism.PartyContactMechanism.PartyWhereCurrentPartyContactMechanism.Party.AsPerson.QuotesWhereContactPerson.Quote.AsProposal),

                m.Proposal.RolePattern(v => v.QuoteItems),
                m.QuoteItem.RolePattern(v => v.Product, v => v.QuoteWhereQuoteItem.Quote.AsProposal),
                m.QuoteItem.RolePattern(v => v.SerialisedItem, v => v.QuoteWhereQuoteItem.Quote.AsProposal),
                m.InvoiceItemType.RolePattern(v => v.Name, v => v.QuoteItemsWhereInvoiceItemType.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.SerialisedItem.RolePattern(v => v.DisplayName, v => v.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.Product.RolePattern(v => v.Name, v => v.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.SerialisedItem.RolePattern(v => v.Description, v => v.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.Product.RolePattern(v => v.Description, v => v.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.QuoteItem.RolePattern(v => v.Details, v => v.QuoteWhereQuoteItem.Quote.AsProposal),
                m.QuoteItem.RolePattern(v => v.Quantity, v => v.QuoteWhereQuoteItem.Quote.AsProposal),
                m.QuoteItem.RolePattern(v => v.UnitPrice, v => v.QuoteWhereQuoteItem.Quote.AsProposal),
                m.QuoteItem.RolePattern(v => v.TotalExVat, v => v.QuoteWhereQuoteItem.Quote.AsProposal),
                m.QuoteItem.RolePattern(v => v.Comment, v => v.QuoteWhereQuoteItem.Quote.AsProposal),
                m.Product.AssociationPattern(v => v.ProductCategoriesWhereProduct, v => v.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.Brand.RolePattern(v => v.Name, v => v.PartsWhereBrand.Part.AsUnifiedGood.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.Model.RolePattern(v => v.Name, v => v.PartsWhereModel.Part.AsUnifiedGood.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.Brand.RolePattern(v => v.Name, v => v.PartsWhereBrand.Part.AsNonUnifiedPart.NonUnifiedGoodsWherePart.NonUnifiedGood.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.Model.RolePattern(v => v.Name, v => v.PartsWhereModel.Part.AsNonUnifiedPart.NonUnifiedGoodsWherePart.NonUnifiedGood.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.SerialisedItem.RolePattern(v => v.ItemNumber, v => v.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.SerialisedItem.RolePattern(v => v.ManufacturingYear, v => v.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.SerialisedItemCharacteristic.AssociationPattern(v => v.SerialisedItemWhereSerialisedItemCharacteristic, v => v.SerialisedItemWhereSerialisedItemCharacteristic.SerialisedItem.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.SerialisedItemCharacteristic.RolePattern(v => v.Value, v => v.SerialisedItemWhereSerialisedItemCharacteristic.SerialisedItem.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.UnitOfMeasure.RolePattern(v => v.Abbreviation, v => v.SerialisedItemCharacteristicTypesWhereUnitOfMeasure.SerialisedItemCharacteristicType.SerialisedItemCharacteristicsWhereSerialisedItemCharacteristicType.SerialisedItemCharacteristic.SerialisedItemWhereSerialisedItemCharacteristic.SerialisedItem.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.SerialisedItem.RolePattern(v => v.PrimaryPhoto, v => v.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.SerialisedItem.RolePattern(v => v.AdditionalPhotos, v => v.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.ProductIdentification.RolePattern(v => v.Identification, v => v.UnifiedProductWhereProductIdentification.UnifiedProduct.AsProduct.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.Product.RolePattern(v => v.PrimaryPhoto, v => v.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),
                m.Product.RolePattern(v => v.Photos, v => v.QuoteItemsWhereProduct.QuoteItem.QuoteWhereQuoteItem.Quote.AsProposal),

                m.Proposal.RolePattern(v => v.OrderAdjustments),
                m.OrderAdjustment.RolePattern(v => v.Description, v => v.QuoteWhereOrderAdjustment.Quote.AsProposal),
                m.Proposal.RolePattern(v => v.TotalDiscount),
                m.Proposal.RolePattern(v => v.TotalSurcharge),
                m.Proposal.RolePattern(v => v.TotalFee),
                m.Proposal.RolePattern(v => v.TotalShippingAndHandling),
                m.Proposal.RolePattern(v => v.TotalExtraCharge),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Proposal>())
            {
                @this.ResetPrintDocument();
            }
        }
    }
}
