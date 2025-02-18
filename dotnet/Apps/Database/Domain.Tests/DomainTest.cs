// <copyright file="DomainTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Adapters.Memory;
    using Configuration;
    using Database.Derivations;
    using Database.Services;
    using Meta;
    using TestPopulation;
    using Organisation = Domain.Organisation;
    using Person = Domain.Person;
    using User = Domain.User;

    public class DomainTest : IDisposable
    {
        public DomainTest(Fixture fixture, bool populate = true)
        {
            var database = new Database(
                new TestDatabaseServices(fixture.Engine),
                new Configuration
                {
                    ObjectFactory = new ObjectFactory(fixture.M, typeof(User)),
                });

            this.M = database.Services.Get<MetaPopulation>();

            this.Setup(database, populate);
        }

        public MetaPopulation M { get; }

        public virtual Config Config { get; } = new Config { SetupSecurity = false };

        public ITransaction Transaction { get; private set; }

        public ITime Time => this.Transaction.Database.Services.Get<ITime>();

        public ISecurity Security => this.Transaction.Database.Services.Get<ISecurity>();

        public DatabaseAccessControl CreateDatabaseAccessControl() => new DatabaseAccessControl(this.Security, this.Transaction.GetUser());

        public TimeSpan? TimeShift
        {
            get => this.Time.Shift;

            set => this.Time.Shift = value;
        }

        protected Organisation InternalOrganisation { get; set; }

        protected Person Administrator => this.GetPersonByUserName("administrator");

        protected Person OrderProcessor => this.GetPersonByUserName("orderProcessor");

        protected Person Purchaser => this.GetPersonByUserName("purchaser");

        public void Dispose()
        {
            this.Transaction.Rollback();
            this.Transaction = null;
        }

        protected IValidation Derive() => this.Transaction.Derive(false, true);

        protected void Setup(IDatabase database, bool populate)
        {
            database.Init();

            if (populate)
            {
                this.Populate(database);
            }

            this.Transaction ??= database.CreateTransaction();
        }

        private void Populate(IDatabase database)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
            CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

            new Setup(database, this.Config).Apply();

            this.Transaction = database.CreateTransaction();

            var administrator = new PersonBuilder(this.Transaction).WithUserName("administrator").Build();

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

            this.InternalOrganisation = new OrganisationBuilder(this.Transaction)
                .WithIsInternalOrganisation(true)
                .WithName("internalOrganisation")
                .WithPreferredCurrency(new Currencies(this.Transaction).CurrencyByCode["EUR"])
                .WithInvoiceSequence(new InvoiceSequences(this.Transaction).EnforcedSequence)
                .WithRequestSequence(new RequestSequences(this.Transaction).EnforcedSequence)
                .WithQuoteSequence(new QuoteSequences(this.Transaction).EnforcedSequence)
                .WithCustomerShipmentSequence(new CustomerShipmentSequences(this.Transaction).EnforcedSequence)
                .WithCustomerReturnSequence(new CustomerReturnSequences(this.Transaction).EnforcedSequence)
                .WithPurchaseShipmentSequence(new PurchaseShipmentSequences(this.Transaction).EnforcedSequence)
                .WithPurchaseReturnSequence(new PurchaseReturnSequences(this.Transaction).EnforcedSequence)
                .WithDropShipmentSequence(new DropShipmentSequences(this.Transaction).EnforcedSequence)
                .WithIncomingTransferSequence(new IncomingTransferSequences(this.Transaction).EnforcedSequence)
                .WithOutgoingTransferSequence(new OutgoingTransferSequences(this.Transaction).EnforcedSequence)
                .WithWorkEffortSequence(new WorkEffortSequences(this.Transaction).EnforcedSequence)
                .WithPurchaseShipmentNumberPrefix("incoming shipmentno: ")
                .WithPurchaseInvoiceNumberPrefix("incoming invoiceno: ")
                .WithPurchaseOrderNumberPrefix("purchase orderno: ")
                .WithCountry(new Countries(this.Transaction).FindBy(M.Country.IsoCode, "NL"))
                .WithExportAccounting(true)
                .Build();

            this.Transaction.Derive();

            var bank = new BankBuilder(this.Transaction).WithCountry(belgium).WithName("ING België").WithBic("BBRUBEBB").Build();

            var accountClassification = new GeneralLedgerAccountClassificationBuilder(this.Transaction)
                .WithName("accountGroup")
                .WithReferenceNumber("SCode")
                .WithReferenceCode("001")
                .WithSortCode("A")
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

            new OrganisationGlAccountBuilder(this.Transaction)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithGeneralLedgerAccount(glAccount0001)
                .Build();

            var glAccount0002 = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("A2")
                .WithSortCode("A2")
                .WithReferenceNumber("0002")
                .WithName("Accounts Receivable")
                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                .WithBalanceSide(new BalanceSides(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountClassification(accountClassification)
                .Build();

            new OrganisationGlAccountBuilder(this.Transaction)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithGeneralLedgerAccount(glAccount0002)
                .Build();

            var glAccount0003 = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("A3")
                .WithSortCode("A3")
                .WithReferenceNumber("0003")
                .WithName("Revenue")
                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                .WithBalanceSide(new BalanceSides(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountClassification(accountClassification)
                .Build();

            new OrganisationGlAccountBuilder(this.Transaction)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithGeneralLedgerAccount(glAccount0003)
                .Build();

            var glAccount0004 = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("A4")
                .WithSortCode("A4")
                .WithReferenceNumber("0004")
                .WithName("VAT")
                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                .WithBalanceSide(new BalanceSides(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountClassification(accountClassification)
                .Build();

            new OrganisationGlAccountBuilder(this.Transaction)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithGeneralLedgerAccount(glAccount0004)
                .Build();

            var glAccount0005 = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("A4")
                .WithSortCode("A4")
                .WithReferenceNumber("0005")
                .WithName("CalculationDifferences")
                .WithBalanceType(new BalanceTypes(this.Transaction).Balance)
                .WithBalanceSide(new BalanceSides(this.Transaction).Debit)
                .WithGeneralLedgerAccountType(new GeneralLedgerAccountTypeBuilder(this.Transaction).WithDescription("accountType").Build())
                .WithGeneralLedgerAccountClassification(accountClassification)
                .Build();

            new OrganisationGlAccountBuilder(this.Transaction)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithGeneralLedgerAccount(glAccount0005)
                .Build();

            foreach (var setting in this.InternalOrganisation.SettingsForAccounting.SettingsForVatRates)
            {
                setting.VatReceivableAccount = glAccount0004;
                setting.VatPayableAccount = glAccount0004;
            }

            foreach (var setting in this.InternalOrganisation.SettingsForAccounting.SettingsForInvoiceItemTypes)
            {
                setting.SalesGeneralLedgerAccount = glAccount0003;
            }

            this.InternalOrganisation.SettingsForAccounting.AccountsReceivable = glAccount0002;
            this.InternalOrganisation.SettingsForAccounting.CalculationDifferences = glAccount0005;

            this.InternalOrganisation.DefaultCollectionMethod = new OwnBankAccountBuilder(this.Transaction)
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
                                .WithContraAccount(glAccount0001)
                                .Build())
                .Build();

            new PartyContactMechanismBuilder(this.Transaction)
                .WithParty(InternalOrganisation)
                .WithUseAsDefault(true)
                .WithContactMechanism(postalAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).GeneralCorrespondence)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).BillingAddress)
                .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).ShippingAddress)
                .Build();

            singleton.Settings.DefaultFacility = new FacilityBuilder(this.Transaction)
                .WithFacilityType(new FacilityTypes(this.Transaction).Warehouse)
                .WithName("facility")
                .WithOwner(this.InternalOrganisation)
                .Build();

            var collectionMethod = new PaymentMethods(this.Transaction).Extent().FirstOrDefault();

            new StoreBuilder(this.Transaction)
                .WithName("store")
                .WithBillingProcess(new BillingProcesses(this.Transaction).BillingForShipmentItems)
                .WithInternalOrganisation(this.InternalOrganisation)
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

            this.InternalOrganisation.CreateEmployee("letmein", this.Transaction.Faker());
            this.InternalOrganisation.CreateB2BCustomer(this.Transaction.Faker());
            this.InternalOrganisation.CreateB2CCustomer(this.Transaction.Faker());
            this.InternalOrganisation.CreateSupplier(this.Transaction.Faker());
            this.InternalOrganisation.CreateSubContractor(this.Transaction.Faker());

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

            new EmploymentBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithEmployee(purchaser).WithEmployer(this.InternalOrganisation).Build();

            new EmploymentBuilder(this.Transaction).WithFromDate(this.Transaction.Now()).WithEmployee(orderProcessor).WithEmployer(this.InternalOrganisation).Build();

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

            var serialisedUnifiedGood = new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation).Build();

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

        private Person GetPersonByUserName(string userName) => new People(this.Transaction).FindBy(this.M.User.UserName, userName);
    }
}
