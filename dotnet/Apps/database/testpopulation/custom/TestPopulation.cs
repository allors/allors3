// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Setup.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
//
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
//
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
//
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors
{
    using System.Globalization;
    using System.Linq;
    using Database;
    using Database.Domain;
    using Database.Domain.TestPopulation;

    public partial class TestPopulation
    {
        public TestPopulation(ITransaction transaction)
        {
            this.Transaction = transaction;
        }

        public ITransaction Transaction { get; }

        public void Populate()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
            CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

            var administrator = new PersonBuilder(this.Transaction).WithUserName("jane@example.com").Build();
            administrator.SetPassword("letmein");

            //var user = new PersonBuilder(this.Transaction).WithUserName("john@example.com").Build();
            //user.SetPassword("letmein");

            this.Transaction.Derive();
            this.Transaction.Commit();

            new UserGroups(this.Transaction).Administrators.AddMember(administrator);

            this.Transaction.Services.Get<IUserService>().User = administrator;

            this.Transaction.Derive();
            this.Transaction.Commit();

            var singleton = this.Transaction.GetSingleton();

            var belgium = new Countries(this.Transaction).CountryByIsoCode["BE"];
            var euro = belgium.Currency;

            singleton.AddAdditionalLocale(belgium.LocalesWhereCountry.FirstOrDefault());

            var postalAddress = new PostalAddressBuilder(this.Transaction)
                .WithAddress1("Kleine Nieuwedijkstraat 2")
                .WithLocality("Mechelen")
                .WithCountry(belgium)
                .Build();

            var serialisedItemSoldOns = new SerialisedItemSoldOn[] { new SerialisedItemSoldOns(this.Transaction).SalesInvoiceSend, new SerialisedItemSoldOns(this.Transaction).PurchaseInvoiceConfirm };

            var internalOrganisation = Organisations.CreateInternalOrganisation(
                transaction: this.Transaction,
                name: "internalOrganisation",
                address1: "address",
                postalCode: "code",
                locality: "city",
                country: belgium,
                phone1CountryCode: "+32",
                phone1: "111",
                phone1Purpose: new ContactMechanismPurposes(this.Transaction).GeneralPhoneNumber,
                phone2CountryCode: string.Empty,
                phone2: string.Empty,
                phone2Purpose: null,
                emailAddress: "email@internalOrganisation.com",
                websiteAddress: "www.internalOrganisation.com",
                taxNumber: "BE 1234567",
                bankName: "ING",
                facilityName: "Warehouse",
                bic: "BBRUBEBB",
                iban: "BE89 3200 1467 7685",
                currency: euro,
                logo: "allors.png",
                storeName: "Store",
                billingProcess: new BillingProcesses(this.Transaction).BillingForOrderItems,
                customerShipmentNumberPrefix: "i-CS",
                customerReturnNumberPrefix: "i-CR",
                purchaseShipmentNumberPrefix: "i-PS",
                purchaseReturnNumberPrefix: "i-PR",
                salesInvoiceNumberPrefix: "i-SI",
                salesOrderNumberPrefix: "i-SO",
                purchaseOrderNumberPrefix: "purchase orderno: ",
                purchaseInvoiceNumberPrefix: "incoming invoiceno: ",
                requestNumberPrefix: "i-RFQ",
                productQuoteNumberPrefix: "i-PQ",
                statementOfWorkNumberPrefix: "i-WQ",
                productNumberPrefix: "i-",
                workEffortPrefix: "i-WO-",
                requirementPrefix: "i-REQ-",
                creditNoteNumberPrefix: "i-CN-",
                isImmediatelyPicked: true,
                autoGenerateShipmentPackage: true,
                isImmediatelyPacked: true,
                isAutomaticallyShipped: true,
                autoGenerateCustomerShipment: true,
                isAutomaticallyReceived: false,
                autoGeneratePurchaseShipment: false,
                useCreditNoteSequence: true,
                requestCounterValue: 0,
                productQuoteCounterValue: 0,
                statementOfWorkCounterValue: 0,
                orderCounterValue: 0,
                purchaseOrderCounterValue: 0,
                invoiceCounterValue: 0,
                purchaseInvoiceCounterValue: 0,
                purchaseOrderNeedsApproval: false,
                purchaseOrderApprovalThresholdLevel1: null,
                purchaseOrderApprovalThresholdLevel2: null,
                serialisedItemSoldOns: serialisedItemSoldOns,
                collectiveWorkEffortInvoice: true,
                invoiceSequence: new InvoiceSequences(this.Transaction).EnforcedSequence,
                requestSequence: new RequestSequences(this.Transaction).EnforcedSequence,
                quoteSequence: new QuoteSequences(this.Transaction).EnforcedSequence,
                customerShipmentSequence: new CustomerShipmentSequences(this.Transaction).EnforcedSequence,
                purchaseShipmentSequence: new PurchaseShipmentSequences(this.Transaction).EnforcedSequence,
                workEffortSequence: new WorkEffortSequences(this.Transaction).EnforcedSequence,
                requirementSequence: new RequirementSequences(this.Transaction).EnforcedSequence);

            internalOrganisation.PurchaseShipmentNumberPrefix = "incoming shipmentno: ";
            internalOrganisation.CustomerReturnSequence = new CustomerReturnSequences(this.Transaction).EnforcedSequence;
            internalOrganisation.PurchaseReturnSequence = new PurchaseReturnSequences(this.Transaction).EnforcedSequence;
            internalOrganisation.DropShipmentSequence = new DropShipmentSequences(this.Transaction).EnforcedSequence;
            internalOrganisation.IncomingTransferSequence = new IncomingTransferSequences(this.Transaction).EnforcedSequence;
            internalOrganisation.OutgoingTransferSequence = new OutgoingTransferSequences(this.Transaction).EnforcedSequence;

            this.Transaction.Derive();

            var bank = new BankBuilder(this.Transaction).WithCountry(belgium).WithName("ING België").WithBic("BBRUBEBB").Build();

            var accountClassification = new GeneralLedgerAccountClassificationBuilder(this.Transaction)
                .WithName("accountGroup")
                .WithReferenceCode("AA")
                .WithSortCode("AA")
                .WithReferenceNumber("A1")
                .Build();

            var glAccount0001 = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("A")
                .WithSortCode("A")
                .WithReferenceNumber("0001")
                .WithName("GeneralLedgerAccount")
                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                .WithBalanceSide(new BalanceSides(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountClassification(accountClassification)
                .Build();

            var organisationGlAccount = new OrganisationGlAccountBuilder(this.Transaction)
                .WithInternalOrganisation(internalOrganisation)
                .WithGeneralLedgerAccount(glAccount0001)
                .Build();

            internalOrganisation.DefaultCollectionMethod = new OwnBankAccountBuilder(this.Transaction)
                .WithBankAccount(new BankAccountBuilder(this.Transaction).WithBank(bank)
                                    .WithCurrency(euro)
                                    .WithIban("BE68539007547034")
                                    .WithNameOnAccount("Koen")
                                    .Build())
                .WithDescription("Main bank account")
                .WithJournal(new JournalBuilder(this.Transaction)
                                .WithName("name")
                                .WithCurrency(euro)
                                .WithJournalType(new JournalTypes(this.Transaction).Bank)
                                .WithContraAccount(organisationGlAccount)
                                .Build())
                .Build();

            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(internalOrganisation)
                .WithUseAsDefault(true)
                .WithContactMechanism(postalAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).GeneralCorrespondence)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .Build();

            singleton.Settings.DefaultFacility = new FacilityBuilder(this.Transaction)
                .WithFacilityType(new FacilityTypes(this.Transaction).Warehouse)
                .WithName("facility")
                .WithOwner(internalOrganisation)
                .Build();

            var collectionMethod = new PaymentMethods(this.Transaction).Extent().FirstOrDefault();

            new StoreBuilder(this.Transaction)
                .WithName("store")
                .WithBillingProcess(new BillingProcesses(this.Transaction).BillingForShipmentItems)
                .WithInternalOrganisation(internalOrganisation)
                .WithCustomerShipmentNumberPrefix("shipmentno: ")
                .WithSalesInvoiceNumberPrefix("invoiceno: ")
                .WithSalesOrderNumberPrefix("orderno: ")
                .WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
                .WithDefaultCarrier(new Carriers(this.Transaction).Fedex)
                .WithCreditLimit(500)
                .WithPaymentGracePeriod(10)
                .WithDefaultCollectionMethod(collectionMethod)
                .WithIsImmediatelyPicked(false)
                .WithAutoGenerateShipmentPackage(false)
                .WithIsImmediatelyPacked(true)
                .Build();

            new ProductCategoryBuilder(this.Transaction).WithName("Primary Category").Build();

            internalOrganisation.CreateEmployee("letmein", this.Transaction.Faker());
            internalOrganisation.CreateB2BCustomer(this.Transaction.Faker());
            internalOrganisation.CreateB2CCustomer(this.Transaction.Faker());
            internalOrganisation.CreateSupplier(this.Transaction.Faker());
            internalOrganisation.CreateSubContractor(this.Transaction.Faker());

            var purchaser = new PersonBuilder(this.Transaction).WithFirstName("The").WithLastName("purchaser").WithUserName("purchaser").Build();
            var orderProcessor = new PersonBuilder(this.Transaction).WithFirstName("The").WithLastName("orderProcessor").WithUserName("orderProcessor").Build();

            // Adding newly created persons to EmployeeUserGroup as employees do not have any permission when created
            var employeesUserGroup = new UserGroups(this.Transaction).Employees;
            employeesUserGroup.AddMember(purchaser);
            employeesUserGroup.AddMember(orderProcessor);
            employeesUserGroup.AddMember(administrator);

            new UserGroups(this.Transaction).Creators.AddMember(purchaser);
            new UserGroups(this.Transaction).Creators.AddMember(orderProcessor);
            new UserGroups(this.Transaction).Creators.AddMember(administrator);

            new EmploymentBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithEmployee(purchaser).WithEmployer(internalOrganisation).Build();

            new EmploymentBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithEmployee(orderProcessor).WithEmployer(internalOrganisation).Build();

            var good1 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("1")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithName("good1")
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                    .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                        .WithIdentification("1")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build())
                .Build();

            var good2 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("2")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithName("good2")
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                    .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                        .WithIdentification("2")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build())
                .Build();

            var good3 = new NonUnifiedGoodBuilder(this.Transaction)
                .WithProductIdentification(new ProductNumberBuilder(this.Transaction)
                    .WithIdentification("3")
                    .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Good).Build())
                .WithName("good3")
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPart(new NonUnifiedPartBuilder(this.Transaction)
                    .WithProductIdentification(new PartNumberBuilder(this.Transaction)
                        .WithIdentification("3")
                        .WithProductIdentificationType(new ProductIdentificationTypes(this.Transaction).Part).Build())
                    .WithInventoryItemKind(new InventoryItemKinds(this.Transaction).NonSerialised).Build())
                .Build();

            var serialisedUnifiedGood = new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(internalOrganisation).Build();

            var catMain = new ProductCategoryBuilder(this.Transaction).WithName("main cat").Build();

            var cat1 = new ProductCategoryBuilder(this.Transaction)
                .WithName("cat for good1")
                .WithPrimaryParent(catMain)
                .WithProduct(good1)
                .Build();

            var cat2 = new ProductCategoryBuilder(this.Transaction)
                .WithName("cat for good2")
                .WithPrimaryParent(catMain)
                .WithProduct(good2)
                .WithProduct(good3)
                .Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

        }
    }
}
