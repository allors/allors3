// <copyright file="Organisations.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;

    public partial class Organisations
    {
        public static void Daily(ITransaction transaction)
        {
            var organisations = new Organisations(transaction).Extent();

            foreach (Organisation organisation in organisations)
            {
                organisation.DeriveRelationships();
            }
        }

        public static Organisation CreateInternalOrganisation(
            ITransaction transaction,
            string name,
            string address,
            string postalCode,
            string locality,
            Country country,
            string phone1CountryCode,
            string phone1,
            ContactMechanismPurpose phone1Purpose,
            string phone2CountryCode,
            string phone2,
            ContactMechanismPurpose phone2Purpose,
            string emailAddress,
            string websiteAddress,
            string taxNumber,
            string bankName,
            string facilityName,
            string bic,
            string iban,
            Currency currency,
            string logo,
            string storeName,
            BillingProcess billingProcess,
            string customerShipmentNumberPrefix,
            string salesInvoiceNumberPrefix,
            string salesOrderNumberPrefix,
            string purchaseOrderNumberPrefix,
            string purchaseInvoiceNumberPrefix,
            string requestNumberPrefix,
            string quoteNumberPrefix,
            string productNumberPrefix,
            string workEffortPrefix,
            string creditNoteNumberPrefix,
            bool isImmediatelyPicked,
            bool autoGenerateShipmentPackage,
            bool isImmediatelyPacked,
            bool isAutomaticallyShipped,
            bool autoGenerateCustomerShipment,
            bool isAutomaticallyReceived,
            bool autoGeneratePurchaseShipment,
            bool useCreditNoteSequence,
            int? requestCounterValue,
            int? quoteCounterValue,
            int? orderCounterValue,
            int? purchaseOrderCounterValue,
            int? invoiceCounterValue,
            int? purchaseInvoiceCounterValue,
            bool purchaseOrderNeedsApproval,
            decimal? purchaseOrderApprovalThresholdLevel1,
            decimal? purchaseOrderApprovalThresholdLevel2,
            SerialisedItemSoldOn[] serialisedItemSoldOns,
            bool collectiveWorkEffortInvoice,
            InvoiceSequence invoiceSequence,
            RequestSequence requestSequence,
            QuoteSequence quoteSequence,
            CustomerShipmentSequence customerShipmentSequence,
            PurchaseShipmentSequence purchaseShipmentSequence,
            WorkEffortSequence workEffortSequence)
            {
            var m = transaction.Database.Context().M;

            var postalAddress1 = new PostalAddressBuilder(transaction)
                    .WithAddress1(address)
                    .WithPostalCode(postalCode)
                    .WithLocality(locality)
                    .WithCountry(country)
                    .Build();

            var webSite = new WebAddressBuilder(transaction)
                .WithElectronicAddressString(websiteAddress)
                .Build();

            BankAccount bankAccount = null;
            if (!string.IsNullOrEmpty(bic) && !string.IsNullOrEmpty(iban))
            {
                var bank = new BankBuilder(transaction).WithName(bankName).WithBic(bic).WithCountry(country).Build();
                bankAccount = new BankAccountBuilder(transaction)
                    .WithBank(bank)
                    .WithIban(iban)
                    .WithNameOnAccount(name)
                    .WithCurrency(currency)
                    .Build();
            }

            var internalOrganisation = new OrganisationBuilder(transaction)
                .WithIsInternalOrganisation(true)
                .WithTaxNumber(taxNumber)
                .WithName(name)
                .WithPreferredCurrency(new Currencies(transaction).FindBy(m.Currency.IsoCode, "EUR"))
                .WithInvoiceSequence(invoiceSequence)
                .WithRequestSequence(requestSequence)
                .WithQuoteSequence(quoteSequence)
                .WithCustomerShipmentSequence(customerShipmentSequence)
                .WithCustomerReturnSequence(new CustomerReturnSequences(transaction).EnforcedSequence)
                .WithPurchaseShipmentSequence(purchaseShipmentSequence)
                .WithPurchaseReturnSequence(new PurchaseReturnSequences(transaction).EnforcedSequence)
                .WithDropShipmentSequence(new DropShipmentSequences(transaction).EnforcedSequence)
                .WithIncomingTransferSequence(new IncomingTransferSequences(transaction).EnforcedSequence)
                .WithOutgoingTransferSequence(new OutgoingTransferSequences(transaction).EnforcedSequence)
                .WithWorkEffortSequence(workEffortSequence)
                .WithDoAccounting(false)
                .WithPurchaseOrderNeedsApproval(purchaseOrderNeedsApproval)
                .WithPurchaseOrderApprovalThresholdLevel1(purchaseOrderApprovalThresholdLevel1)
                .WithPurchaseOrderApprovalThresholdLevel2(purchaseOrderApprovalThresholdLevel2)
                .WithAutoGeneratePurchaseShipment(autoGeneratePurchaseShipment)
                .WithIsAutomaticallyReceived(isAutomaticallyReceived)
                .WithCollectiveWorkEffortInvoice(collectiveWorkEffortInvoice)
                .WithCountry(country)
                .Build();

            internalOrganisation.SerialisedItemSoldOns = serialisedItemSoldOns;

            if (invoiceSequence == new InvoiceSequences(transaction).RestartOnFiscalYear)
            {
                var sequenceNumbers = internalOrganisation.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == transaction.Now().Year);
                if (sequenceNumbers == null)
                {
                    sequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(transaction).WithFiscalYear(transaction.Now().Year).Build();
                    internalOrganisation.AddFiscalYearsInternalOrganisationSequenceNumber(sequenceNumbers);
                }

                sequenceNumbers.PurchaseOrderNumberPrefix = purchaseOrderNumberPrefix;
                sequenceNumbers.PurchaseInvoiceNumberPrefix = purchaseInvoiceNumberPrefix;

                if (purchaseOrderCounterValue != null)
                {
                    sequenceNumbers.PurchaseOrderNumberCounter = new CounterBuilder(transaction).WithValue(purchaseOrderCounterValue).Build();
                }

                if (purchaseInvoiceCounterValue != null)
                {
                    sequenceNumbers.PurchaseInvoiceNumberCounter = new CounterBuilder(transaction).WithValue(purchaseInvoiceCounterValue).Build();
                }
            }
            else
            {
                internalOrganisation.PurchaseOrderNumberPrefix = purchaseOrderNumberPrefix;
                internalOrganisation.PurchaseInvoiceNumberPrefix = purchaseInvoiceNumberPrefix;

                if (purchaseOrderCounterValue != null)
                {
                    internalOrganisation.PurchaseOrderNumberCounter = new CounterBuilder(transaction).WithValue(purchaseOrderCounterValue).Build();
                }

                if (purchaseInvoiceCounterValue != null)
                {
                    internalOrganisation.PurchaseInvoiceNumberCounter = new CounterBuilder(transaction).WithValue(purchaseInvoiceCounterValue).Build();
                }
            }

            if (requestSequence == new RequestSequences(transaction).RestartOnFiscalYear)
            {
                var sequenceNumbers = internalOrganisation.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == transaction.Now().Year);
                if (sequenceNumbers == null)
                {
                    sequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(transaction).WithFiscalYear(transaction.Now().Year).Build();
                    internalOrganisation.AddFiscalYearsInternalOrganisationSequenceNumber(sequenceNumbers);
                }

                sequenceNumbers.RequestNumberPrefix = requestNumberPrefix;

                if (requestCounterValue != null)
                {
                    sequenceNumbers.RequestNumberCounter = new CounterBuilder(transaction).WithValue(requestCounterValue).Build();
                }
            }
            else
            {
                internalOrganisation.RequestNumberPrefix = requestNumberPrefix;

                if (requestCounterValue != null)
                {
                    internalOrganisation.RequestNumberCounter = new CounterBuilder(transaction).WithValue(requestCounterValue).Build();
                }
            }

            if (quoteSequence == new QuoteSequences(transaction).RestartOnFiscalYear)
            {
                var sequenceNumbers = internalOrganisation.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == transaction.Now().Year);
                if (sequenceNumbers == null)
                {
                    sequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(transaction).WithFiscalYear(transaction.Now().Year).Build();
                    internalOrganisation.AddFiscalYearsInternalOrganisationSequenceNumber(sequenceNumbers);
                }

                sequenceNumbers.QuoteNumberPrefix = quoteNumberPrefix;

                if (quoteCounterValue != null)
                {
                    sequenceNumbers.QuoteNumberCounter = new CounterBuilder(transaction).WithValue(quoteCounterValue).Build();
                }
            }
            else
            {
                internalOrganisation.QuoteNumberPrefix = quoteNumberPrefix;

                if (quoteCounterValue != null)
                {
                    internalOrganisation.QuoteNumberCounter = new CounterBuilder(transaction).WithValue(quoteCounterValue).Build();
                }
            }

            if (workEffortSequence == new WorkEffortSequences(transaction).RestartOnFiscalYear)
            {
                var sequenceNumbers = internalOrganisation.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == transaction.Now().Year);
                if (sequenceNumbers == null)
                {
                    sequenceNumbers = new FiscalYearInternalOrganisationSequenceNumbersBuilder(transaction).WithFiscalYear(transaction.Now().Year).Build();
                    internalOrganisation.AddFiscalYearsInternalOrganisationSequenceNumber(sequenceNumbers);
                }

                sequenceNumbers.WorkEffortNumberPrefix = workEffortPrefix;
            }
            else
            {
                internalOrganisation.WorkEffortNumberPrefix = workEffortPrefix;
            }

            OwnBankAccount defaultCollectionMethod = null;
            if (bankAccount != null)
            {
                internalOrganisation.AddBankAccount(bankAccount);
                defaultCollectionMethod = new OwnBankAccountBuilder(transaction).WithBankAccount(bankAccount).WithDescription("Huisbank").Build();
                internalOrganisation.DefaultCollectionMethod = defaultCollectionMethod;
            }

            if (!string.IsNullOrEmpty(emailAddress))
            {
                var email = new EmailAddressBuilder(transaction)
                    .WithElectronicAddressString(emailAddress)
                    .Build();

                internalOrganisation.AddPartyContactMechanism(new PartyContactMechanismBuilder(transaction)
                    .WithUseAsDefault(true)
                    .WithContactMechanism(email)
                    .WithContactPurpose(new ContactMechanismPurposes(transaction).GeneralEmail)
                    .Build());
            }

            internalOrganisation.AddPartyContactMechanism(new PartyContactMechanismBuilder(transaction)
                .WithUseAsDefault(true)
                .WithContactMechanism(postalAddress1)
                .WithContactPurpose(new ContactMechanismPurposes(transaction).RegisteredOffice)
                .WithContactPurpose(new ContactMechanismPurposes(transaction).GeneralCorrespondence)
                .WithContactPurpose(new ContactMechanismPurposes(transaction).BillingAddress)
                .WithContactPurpose(new ContactMechanismPurposes(transaction).ShippingAddress)
                .Build());
            internalOrganisation.AddPartyContactMechanism(new PartyContactMechanismBuilder(transaction)
                .WithUseAsDefault(true)
                .WithContactMechanism(webSite)
                .WithContactPurpose(new ContactMechanismPurposes(transaction).InternetAddress)
                .Build());

            TelecommunicationsNumber phoneNumber1 = null;
            if (!string.IsNullOrEmpty(phone1))
            {
                phoneNumber1 = new TelecommunicationsNumberBuilder(transaction).WithContactNumber(phone1).Build();
                if (!string.IsNullOrEmpty(phone1CountryCode))
                {
                    phoneNumber1.CountryCode = phone1CountryCode;
                }
            }

            if (phoneNumber1 != null)
            {
                internalOrganisation.AddPartyContactMechanism(new PartyContactMechanismBuilder(transaction)
                    .WithUseAsDefault(true)
                    .WithContactMechanism(phoneNumber1)
                    .WithContactPurpose(phone1Purpose)
                    .Build());
            }

            TelecommunicationsNumber phoneNumber2 = null;
            if (!string.IsNullOrEmpty(phone2))
            {
                phoneNumber2 = new TelecommunicationsNumberBuilder(transaction).WithContactNumber(phone2).Build();
                if (!string.IsNullOrEmpty(phone2CountryCode))
                {
                    phoneNumber2.CountryCode = phone2CountryCode;
                }
            }

            if (phoneNumber2 != null)
            {
                internalOrganisation.AddPartyContactMechanism(new PartyContactMechanismBuilder(transaction)
                    .WithUseAsDefault(true)
                    .WithContactMechanism(phoneNumber2)
                    .WithContactPurpose(phone2Purpose)
                    .Build());
            }

            if (!string.IsNullOrWhiteSpace(logo))
            {
                var singleton = transaction.GetSingleton();
                internalOrganisation.LogoImage = new MediaBuilder(transaction).WithInFileName(logo).WithInData(singleton.GetResourceBytes(logo)).Build();
            }

            Facility facility = null;
            if (facilityName != null)
            {
                facility = new FacilityBuilder(transaction)
                    .WithName(facilityName)
                    .WithFacilityType(new FacilityTypes(transaction).Warehouse)
                    .WithOwner(internalOrganisation)
                    .Build();
            }

            var store = new StoreBuilder(transaction)
                .WithName(storeName)
                .WithDefaultCollectionMethod(defaultCollectionMethod)
                .WithDefaultShipmentMethod(new ShipmentMethods(transaction).Ground)
                .WithDefaultCarrier(new Carriers(transaction).Fedex)
                .WithBillingProcess(billingProcess)
                .WithIsImmediatelyPicked(isImmediatelyPicked)
                .WithAutoGenerateShipmentPackage(autoGenerateShipmentPackage)
                .WithIsImmediatelyPacked(isImmediatelyPacked)
                .WithIsAutomaticallyShipped(isAutomaticallyShipped)
                .WithAutoGenerateCustomerShipment(autoGenerateCustomerShipment)
                .WithUseCreditNoteSequence(useCreditNoteSequence)
                .WithInternalOrganisation(internalOrganisation)
                .Build();

            if (defaultCollectionMethod == null)
            {
                store.DefaultCollectionMethod = new CashBuilder(transaction).Build();
            }
            else
            {
                store.DefaultCollectionMethod = defaultCollectionMethod;
            }

            if (facility != null)
            {
                store.DefaultFacility = facility;
            }

            if (invoiceSequence == new InvoiceSequences(transaction).RestartOnFiscalYear)
            {
                var sequenceNumbers = new FiscalYearStoreSequenceNumbersBuilder(transaction).WithFiscalYear(transaction.Now().Year).Build();

                sequenceNumbers.CreditNoteNumberPrefix = creditNoteNumberPrefix;
                sequenceNumbers.CustomerShipmentNumberPrefix = customerShipmentNumberPrefix;
                sequenceNumbers.SalesInvoiceNumberPrefix = salesInvoiceNumberPrefix;
                sequenceNumbers.SalesOrderNumberPrefix = salesOrderNumberPrefix;

                if (orderCounterValue != null)
                {
                    sequenceNumbers.SalesOrderNumberCounter = new CounterBuilder(transaction).WithValue(orderCounterValue).Build();
                }

                if (invoiceCounterValue != null)
                {
                    sequenceNumbers.SalesInvoiceNumberCounter = new CounterBuilder(transaction).WithValue(invoiceCounterValue).Build();
                }

                store.AddFiscalYearsStoreSequenceNumber(sequenceNumbers);
            }
            else
            {
                store.CreditNoteNumberPrefix = creditNoteNumberPrefix;
                store.CustomerShipmentNumberPrefix = customerShipmentNumberPrefix;
                store.SalesInvoiceNumberPrefix = salesInvoiceNumberPrefix;
                store.SalesOrderNumberPrefix = salesOrderNumberPrefix;

                if (orderCounterValue != null)
                {
                    store.SalesOrderNumberCounter = new CounterBuilder(transaction).WithValue(orderCounterValue).Build();
                }

                if (invoiceCounterValue != null)
                {
                    store.SalesInvoiceNumberCounter = new CounterBuilder(transaction).WithValue(invoiceCounterValue).Build();
                }
            }

            return internalOrganisation;
        }

        // TODO: Remove Extent
        public Organisation[] InternalOrganisations()
        {
            var internalOrganisations = this.Extent();
            internalOrganisations.Filter.AddEquals(this.M.InternalOrganisation.IsInternalOrganisation, true);

            return internalOrganisations.ToArray();
        }

        protected override void AppsPrepare(Setup setup)
        {
            setup.AddDependency(this.Meta.ObjectType, this.M.InventoryStrategy);
            setup.AddDependency(this.Meta.ObjectType, this.M.Role);
            setup.AddDependency(this.Meta.ObjectType, this.M.OrganisationRole);
            setup.AddDependency(this.Meta.ObjectType, this.M.InvoiceSequence);
            setup.AddDependency(this.Meta.ObjectType, this.M.RequestSequence);
            setup.AddDependency(this.Meta.ObjectType, this.M.QuoteSequence);
            setup.AddDependency(this.Meta.ObjectType, this.M.CustomerShipmentSequence);
            setup.AddDependency(this.Meta.ObjectType, this.M.CustomerReturnSequence);
            setup.AddDependency(this.Meta.ObjectType, this.M.DropShipmentSequence);
            setup.AddDependency(this.Meta.ObjectType, this.M.PurchaseShipmentSequence);
            setup.AddDependency(this.Meta.ObjectType, this.M.PurchaseReturnSequence);
            setup.AddDependency(this.Meta.ObjectType, this.M.IncomingTransferSequence);
            setup.AddDependency(this.Meta.ObjectType, this.M.OutgoingTransferSequence);
            setup.AddDependency(this.Meta.ObjectType, this.M.WorkEffortSequence);
            setup.AddDependency(this.Meta.ObjectType, this.M.Singleton);
            setup.AddDependency(this.Meta.ObjectType, this.M.FacilityType);
        }
    }
}
