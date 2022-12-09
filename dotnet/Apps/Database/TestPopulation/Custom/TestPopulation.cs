// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestPopulation.cs" company="Allors bvba">
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
        public TestPopulation(ITransaction transaction, Config config)
        {
            this.Transaction = transaction;
            this.Config = config;
        }

        public ITransaction Transaction { get; }
        public Config Config { get; }

        public void Populate(IDatabase database)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
            CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

            new Setup(database, this.Config).Apply();

            var administrator = new PersonBuilder(this.Transaction).WithUserName("jane@example.com").Build();
            administrator.SetPassword("letmein");

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
                shipmentIsAutomaticallyReturned: false,
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

            internalOrganisation.DefaultCollectionMethod = new OwnBankAccountBuilder(this.Transaction)
                .WithBankAccount(new BankAccountBuilder(this.Transaction).WithBank(bank)
                                    .WithCurrency(euro)
                                    .WithIban("BE68539007547034")
                                    .WithNameOnAccount("Koen")
                                    .Build())
                .WithDescription("Main bank account")
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

            if (this.Config.SetupAccounting)
            {
                this.AccountingPopulate();
            }
        }

        private void AccountingPopulate()
        {
            var netherlands = new Countries(this.Transaction).CountryByIsoCode["NL"];
            var euro = netherlands.Currency;

            var serialisedItemSoldOns = new SerialisedItemSoldOn[] { new SerialisedItemSoldOns(this.Transaction).SalesInvoiceSend, new SerialisedItemSoldOns(this.Transaction).PurchaseInvoiceConfirm };

            var dutchInternalOrganisation = Organisations.CreateInternalOrganisation(
                transaction: this.Transaction,
                name: "dutchInternalOrganisation",
                address1: "address",
                postalCode: "code",
                locality: "city",
                country: netherlands,
                phone1CountryCode: "+31",
                phone1: "112",
                phone1Purpose: new ContactMechanismPurposes(this.Transaction).GeneralPhoneNumber,
                phone2CountryCode: string.Empty,
                phone2: string.Empty,
                phone2Purpose: null,
                emailAddress: "email@dutchInternalOrganisation.com",
                websiteAddress: "www.dutchInternalOrganisation.com",
                taxNumber: "NL 1234567",
                bankName: "ING",
                facilityName: "Warehouse",
                bic: "INGBNL2A",
                iban: "NL39INGB3528973196",
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
                shipmentIsAutomaticallyReturned: false,
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

            dutchInternalOrganisation.PurchaseShipmentNumberPrefix = "incoming shipmentno: ";
            dutchInternalOrganisation.CustomerReturnSequence = new CustomerReturnSequences(this.Transaction).EnforcedSequence;
            dutchInternalOrganisation.PurchaseReturnSequence = new PurchaseReturnSequences(this.Transaction).EnforcedSequence;
            dutchInternalOrganisation.DropShipmentSequence = new DropShipmentSequences(this.Transaction).EnforcedSequence;
            dutchInternalOrganisation.IncomingTransferSequence = new IncomingTransferSequences(this.Transaction).EnforcedSequence;
            dutchInternalOrganisation.OutgoingTransferSequence = new OutgoingTransferSequences(this.Transaction).EnforcedSequence;

            this.Transaction.Derive();

            var nedBelastingdienst = new OrganisationBuilder(this.Transaction)
                .WithName("Ned. Belastingdienst")
                .WithEuListingState(netherlands)
                .WithLegalForm(new LegalForms(this.Transaction).Extent().First)
                .WithLocale(new Locales(this.Transaction).LocaleByName["nl"])
                .WithTaxNumber("NL123123")
                .WithComment("Comment")
                .Build();

            var federaleOverheidsDienst = new OrganisationBuilder(this.Transaction)
                .WithName("Federale OverheidsDienst FINANCIËN")
                .WithEuListingState(netherlands)
                .WithLegalForm(new LegalForms(this.Transaction).Extent().First)
                .WithLocale(new Locales(this.Transaction).LocaleByName["nl"])
                .WithTaxNumber("NL12341234")
                .WithComment("Comment")
                .Build();

            this.Transaction.Derive();

            var debit = new BalanceSides(this.Transaction).Debit;
            var credit = new BalanceSides(this.Transaction).Credit;

            var balance = new BalanceTypes(this.Transaction).Balance;
            var profitLoss = new BalanceTypes(this.Transaction).ProfitLoss;

            var BVrd = new GeneralLedgerAccountTypeBuilder(this.Transaction)
                .WithDescription("Voorraden")
                .Build();

            var BVrdHanVoo = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BVrdHanVoo")
                .WithSortCode("E.E.010")
                .WithReferenceNumber("3001010")
                .WithName("Handelsgoederen, bruto")
                .WithDescription("Handelsgoederen, bruto handelsgoederen")
                .WithBalanceSide(debit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BVrd)
                .WithRgsLevel(4)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithExcludeRgsAfrekSyst(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
                .WithGeneralLedgerAccount(BVrdHanVoo)
                .WithInternalOrganisation(dutchInternalOrganisation)
                .Build();

            var Bvor = new GeneralLedgerAccountTypeBuilder(this.Transaction)
                .WithDescription("Vorderingen")
                .Build();

            var BVorDebHad = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BVorDebHad")
                .WithSortCode("G.A.010")
                .WithReferenceNumber("1101010")
                .WithName("Handelsdebiteuren nominaal")
                .WithDescription("Handelsdebiteuren nominaal")
                .WithBalanceSide(debit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(Bvor)
                .WithRgsLevel(4)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .WithExcludeRgsAfrekSyst(true)
                .WithExcludeRgsUitbr5(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
                .WithGeneralLedgerAccount(BVorDebHad)
                .WithInternalOrganisation(dutchInternalOrganisation)
                .Build();

            var BVorOvaVof = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BVorOvaVof")
                .WithSortCode("G.LA")
                .WithReferenceNumber("1104010")
                .WithName("Vooruitbetaalde facturen")
                .WithDescription("Vooruitbetaalde facturen overlopende activa")
                .WithBalanceSide(debit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(Bvor)
                .WithRgsLevel(4)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsUseWithWoco(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
                .WithGeneralLedgerAccount(BVorOvaVof)
                .WithInternalOrganisation(dutchInternalOrganisation)
                .Build();

            var BEiv = new GeneralLedgerAccountTypeBuilder(this.Transaction)
              .WithDescription("Groepsvermogen - Eigen vermogen - Kapitaal")
              .Build();

            var BEivOreRvh = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BEivOreRvh")
                .WithSortCode("J.L.B")
                .WithReferenceNumber("0506020")
                .WithName("Resultaat van het boekjaar")
                .WithDescription("Resultaat van het boekjaar")
                .WithBalanceSide(credit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BEiv)
                .WithRgsLevel(4)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .WithExcludeRgsBV(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
               .WithGeneralLedgerAccount(BEivOreRvh)
               .WithInternalOrganisation(dutchInternalOrganisation)
               .Build();

            var BEivKapOnd = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BEivKapOnd")
                .WithSortCode("J.M.A")
                .WithReferenceNumber("0509010")
                .WithName("Ondernemingsvermogen exclusief fiscale reserves fiscaal")
                .WithDescription("Ondernemingsvermogen exclusief fiscale reserves fiscaal eigen vermogen onderneming natuurlijke personen")
                .WithBalanceSide(credit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BEiv)
                .WithRgsLevel(4)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
               .WithGeneralLedgerAccount(BEivKapOnd)
               .WithInternalOrganisation(dutchInternalOrganisation)
               .Build();

            var BSch = new GeneralLedgerAccountTypeBuilder(this.Transaction)
              .WithDescription("Kortlopende schulden")
              .Build();

            var BSchCreHac = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BSchCreHac")
                .WithSortCode("M.D.A")
                .WithReferenceNumber("1203010")
                .WithName("Handelscrediteuren nominaal")
                .WithDescription("Handelscrediteuren nominaal schulden aan leveranciers en handelskredieten")
                .WithBalanceSide(credit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BSch)
                .WithRgsLevel(4)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .WithExcludeRgsAfrekSyst(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
               .WithGeneralLedgerAccount(BSchCreHac)
               .WithInternalOrganisation(dutchInternalOrganisation)
               .Build();

            var BVorVbkTvo = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BVorVbkTvo")
                .WithSortCode("G.G.B")
                .WithReferenceNumber("1102010")
                .WithName("Terug te vorderen Omzetbelasting")
                .WithDescription("Terug te vorderen Omzetbelasting vorderingen uit hoofde van belastingen")
                .WithBalanceSide(debit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(Bvor)
                .WithRgsLevel(4)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
               .WithGeneralLedgerAccount(BVorVbkTvo)
               .WithInternalOrganisation(dutchInternalOrganisation)
               .Build();

            var BSchBepBtw = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BSchBepBtw")
                .WithCounterPartAccount(BVorVbkTvo)
                .WithSortCode("M.K.A")
                .WithReferenceNumber("1205010")
                .WithName("Te betalen omzetbelasting")
                .WithDescription("Te betalen Omzetbelasting belastingen en premies sociale verzekeringen")
                .WithBalanceSide(credit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BSch)
                .WithRgsLevel(4)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
               .WithGeneralLedgerAccount(BSchBepBtw)
               .WithInternalOrganisation(dutchInternalOrganisation)
               .Build();

            BVorVbkTvo.CounterPartAccount = BSchBepBtw;

            var BSchBepBtwOla = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BSchBepBtwOla")
                .WithSortCode("M.K.A020")
                .WithReferenceNumber("1205010.02")
                .WithName("1a. Omzetbelasting leveringen/diensten belast met hoog tarief")
                .WithDescription("Omzetbelasting leveringen/diensten algemeen tarief")
                .WithBalanceSide(credit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BSch)
                .WithRgsLevel(5)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .WithExcludeRgsNivo5(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
               .WithGeneralLedgerAccount(BSchBepBtwOla)
               .WithInternalOrganisation(dutchInternalOrganisation)
               .Build();

            var BSchBepBtwOlv = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BSchBepBtwOlv")
                .WithSortCode("M.K.A030")
                .WithReferenceNumber("1205010.03")
                .WithName("1b. Omzetbelasting leveringen/diensten belast met laag tarief")
                .WithDescription("Omzetbelasting leveringen/diensten verlaagd tarief")
                .WithBalanceSide(credit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BSch)
                .WithRgsLevel(5)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .WithExcludeRgsNivo5(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
               .WithGeneralLedgerAccount(BSchBepBtwOlv)
               .WithInternalOrganisation(dutchInternalOrganisation)
               .Build();

            var BSchBepBtwOlo = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BSchBepBtwOlo")
                .WithSortCode("M.K.A040")
                .WithReferenceNumber("1205010.04")
                .WithName("1c. Omzetbelasting leveringen/diensten belast met overige tarieven, behalve 0%")
                .WithDescription("Omzetbelasting leveringen/diensten overige tarieven")
                .WithBalanceSide(credit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BSch)
                .WithRgsLevel(5)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .WithExcludeRgsNivo5(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
               .WithGeneralLedgerAccount(BSchBepBtwOlo)
               .WithInternalOrganisation(dutchInternalOrganisation)
               .Build();

            var BSchBepBtwOop = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BSchBepBtwOop")
                .WithSortCode("M.K.A050")
                .WithReferenceNumber("1205010.05")
                .WithName("1d. Omzetbelasting over privégebruik")
                .WithDescription("Omzetbelasting over privégebruik")
                .WithBalanceSide(credit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BSch)
                .WithRgsLevel(5)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .WithExcludeRgsNivo5(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
               .WithGeneralLedgerAccount(BSchBepBtwOop)
               .WithInternalOrganisation(dutchInternalOrganisation)
               .Build();

            var BSchBepBtwOlw = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BSchBepBtwOlw")
                .WithSortCode("M.K.A060")
                .WithReferenceNumber("1205010.06")
                .WithName("2a. Omzetbelasting leveringen/diensten waarbij de omzetbelasting naar u is verlegd")
                .WithDescription("Omzetbelasting leveringen/diensten waarbij heffing is verlegd")
                .WithBalanceSide(credit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BSch)
                .WithRgsLevel(5)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .WithExcludeRgsNivo5(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
               .WithGeneralLedgerAccount(BSchBepBtwOlw)
               .WithInternalOrganisation(dutchInternalOrganisation)
               .Build();

            var BSchBepBtwOlb = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BSchBepBtwOlb")
                .WithSortCode("M.K.A070")
                .WithReferenceNumber("1205010.07")
                .WithName("4a. Omzetbelasting leveringe/diensten uit landen buiten de EU")
                .WithDescription("Omzetbelasting leveringen/diensten uit landen buiten de EU ")
                .WithBalanceSide(credit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BSch)
                .WithRgsLevel(5)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .WithExcludeRgsNivo5(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
               .WithGeneralLedgerAccount(BSchBepBtwOlb)
               .WithInternalOrganisation(dutchInternalOrganisation)
               .Build();

            var BSchBepBtwOlu = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BSchBepBtwOlu")
                .WithSortCode("M.K.A080")
                .WithReferenceNumber("1205010.08")
                .WithName("4b. Omzetbelasting leveringen/diensten uit landen binnen EU")
                .WithDescription("Omzetbelasting leveringen/diensten uit landen binnen EU")
                .WithBalanceSide(credit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BSch)
                .WithRgsLevel(5)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .WithExcludeRgsNivo5(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
               .WithGeneralLedgerAccount(BSchBepBtwOlu)
               .WithInternalOrganisation(dutchInternalOrganisation)
               .Build();

            var BSchBepBtwVoo = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BSchBepBtwVoo")
                .WithSortCode("M.K.A090")
                .WithReferenceNumber("1205010.09")
                .WithName("5b. Voorbelasting")
                .WithDescription("Voorbelasting")
                .WithBalanceSide(debit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BSch)
                .WithRgsLevel(5)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .WithExcludeRgsNivo5(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
              .WithGeneralLedgerAccount(BSchBepBtwVoo)
              .WithInternalOrganisation(dutchInternalOrganisation)
              .Build();

            var BSchBepBtwSda = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BSchBepBtwSda")
                .WithSortCode("M.K.A120")
                .WithReferenceNumber("1205010.12")
                .WithName("Schatting deze aangifte")
                .WithDescription("Schattig deze aangifte")
                .WithBalanceSide(credit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BSch)
                .WithRgsLevel(5)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .WithExcludeRgsNivo5(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(BSchBepBtwSda)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var BSchBepBtwAfo = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BSchBepBtwAfo")
                .WithSortCode("M.K.A130")
                .WithReferenceNumber("1205010.13")
                .WithName("Afgedragen omzetbelasting")
                .WithDescription("Afgedragen omzetbelasting")
                .WithBalanceSide(credit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BSch)
                .WithRgsLevel(5)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsUseWithWoco(true)
                .WithExcludeRgsNivo5(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(BSchBepBtwAfo)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var BSchOpaNto = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BSchOpaNto")
                .WithSortCode("M.Q.A")
                .WithReferenceNumber("1210010")
                .WithName("Nog te ontvangen facturen")
                .WithDescription("Nog te ontvangen facturen overlopende passiva")
                .WithBalanceSide(credit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BSch)
                .WithRgsLevel(4)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(BSchOpaNto)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var BSchOpaVgo = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BSchOpaVgo")
                .WithSortCode("M.Q.Z020")
                .WithReferenceNumber("1210260")
                .WithName("Vooruitgefactureerde omzet")
                .WithDescription("Vooruitgefactureerde omzet")
                .WithBalanceSide(credit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BSch)
                .WithRgsLevel(4)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsUseWithEZ(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(BSchOpaVgo)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var BSchTusTov = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BSchTusTov")
                .WithSortCode("M.R.J")
                .WithReferenceNumber("1229000")
                .WithName("Tussenrekeningen overig")
                .WithDescription("Tussenrekeningen overig")
                .WithBalanceSide(credit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(BSch)
                .WithRgsLevel(4)
                .WithIsRgsUseWithWoco(true)
                .WithExcludeRgsWoco(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(BSchTusTov)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var WBed = new GeneralLedgerAccountTypeBuilder(this.Transaction)
                .WithDescription("Overige bedrijfskosten")
                .Build();

            var WBedVkkOvr = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("WBedVkkOvr")
                .WithSortCode("W.E.W")
                .WithReferenceNumber("4203220")
                .WithName("Overige verkoopkosten")
                .WithDescription("Overige verkoopkosten verkoop gerelateerde kosten")
                .WithBalanceSide(debit)
                .WithBalanceType(profitLoss)
                .WithGeneralLedgerAccountType(WBed)
                .WithRgsLevel(4)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(WBedVkkOvr)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var WBedAdlBet = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("WBedAdlBet")
                .WithSortCode("W.L.H")
                .WithReferenceNumber("4210070")
                .WithName("Betalingsverschillen")
                .WithDescription("Betalingsverschillen administratieve lasten")
                .WithBalanceSide(debit)
                .WithBalanceType(profitLoss)
                .WithGeneralLedgerAccountType(WBed)
                .WithRgsLevel(4)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(WBedAdlBet)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var WNerNewNew = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("WNerNewNew")
                .WithSortCode("2.08.A010")
                .WithReferenceNumber("9999010")
                .WithName("Netto resultaat")
                .WithDescription("Netto resultaat na belastingen")
                .WithBalanceSide(credit)
                .WithBalanceType(profitLoss)
                .WithGeneralLedgerAccountType(WBed)
                .WithRgsLevel(4)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(WNerNewNew)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var BLimKasKas = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BLimKasKas")
                .WithSortCode("I.A010")
                .WithReferenceNumber("1001010")
                .WithName("Kas")
                .WithDescription("Kas kasmiddelen")
                .WithBalanceSide(debit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(WBed)
                .WithRgsLevel(4)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .WithExcludeRgsAfrekSyst(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(BLimKasKas)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var Blim = new GeneralLedgerAccountTypeBuilder(this.Transaction)
                .WithDescription("Liquide middelen")
                .Build();

            var BLimBanRba = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BLimBanRba")
                .WithSortCode("I.B.A")
                .WithReferenceNumber("1002010")
                .WithName("Rekening-courant bank")
                .WithDescription("Rekening-courant bank tegoeden op bankgirorekeningen")
                .WithBalanceSide(debit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(Blim)
                .WithRgsLevel(4)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .WithExcludeRgsAfrekSyst(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(BLimBanRba)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var BLimBanRbaBg1 = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BLimBanRbaBg1")
                .WithSortCode("I.B.A010")
                .WithReferenceNumber("1002010.01")
                .WithName("Rekening-courant bank groep 1")
                .WithDescription("Rekening-courant bank groep 1")
                .WithBalanceSide(debit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(Blim)
                .WithRgsLevel(5)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .WithExcludeRgsNivo5(true)
                .Build();

            var BLimBanRbaBg1Gl = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(BLimBanRbaBg1)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var WOmz = new GeneralLedgerAccountTypeBuilder(this.Transaction)
                .WithDescription("Netto-omzet")
                .Build();

            var WOmzNopOlh = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("WOmzNopOlh")
                .WithSortCode("O.A010")
                .WithReferenceNumber("8001010")
                .WithName("1a. Netto-omzet uit leveringen geproduceerde goederen belast met hoog tarief")
                .WithDescription("1a. Netto-omzet uit leveringen geproduceerde goederen belast met hoog tarief")
                .WithBalanceSide(credit)
                .WithBalanceType(profitLoss)
                .WithGeneralLedgerAccountType(WOmz)
                .WithRgsLevel(4)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithExcludeRgsAfrekSyst(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(WOmzNopOlh)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var WOmzNopOlv = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("WOmzNopOlv")
                .WithSortCode("O.A020")
                .WithReferenceNumber("8001020")
                .WithName("1b. Netto-omzet uit leveringen geproduceerde goederen belast met laagd tarief")
                .WithDescription("1b. Netto-omzet uit leveringen geproduceerde goederen belast met laagd tarief")
                .WithBalanceSide(credit)
                .WithBalanceType(profitLoss)
                .WithGeneralLedgerAccountType(WOmz)
                .WithRgsLevel(4)
                .WithIsRgsUseWithZzp(true)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithExcludeRgsAfrekSyst(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(WOmzNopOlv)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var WOmzNopOlo = new GeneralLedgerAccountBuilder(this.Transaction)
             .WithReferenceCode("WOmzNopOlo")
             .WithSortCode("O.A030")
             .WithReferenceNumber("8001030")
             .WithName("1c. Netto-omzet uit leveringen geproduceerde goederen belast met overige tarieven, behalve 0%")
             .WithDescription("1c. Netto-omzet uit leveringen geproduceerde goederen belast met overige tarieven, behalve 0%")
             .WithBalanceSide(credit)
             .WithBalanceType(profitLoss)
             .WithGeneralLedgerAccountType(WOmz)
             .WithRgsLevel(4)
             .WithIsRgsUseWithZzp(true)
             .WithIsRgsBase(true)
             .WithIsRgsExtended(true)
             .WithIsRgsUseWithEZ(true)
             .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(WOmzNopOlo)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var WOmzNopOpg = new GeneralLedgerAccountBuilder(this.Transaction)
             .WithReferenceCode("WOmzNopOpg")
             .WithSortCode("O.A040")
             .WithReferenceNumber("8001040")
             .WithName("1d. Netto-omzet uit privégebruik geproduceerde goederen")
             .WithDescription("1d.Netto - omzet uit privégebruik geproduceerde goederen")
             .WithBalanceSide(credit)
             .WithBalanceType(profitLoss)
             .WithGeneralLedgerAccountType(WOmz)
             .WithRgsLevel(4)
             .WithIsRgsUseWithZzp(true)
             .WithIsRgsBase(true)
             .WithIsRgsExtended(true)
             .WithIsRgsUseWithEZ(true)
             .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(WOmzNopOpg)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var WOmzNopOlg = new GeneralLedgerAccountBuilder(this.Transaction)
             .WithReferenceCode("WOmzNopOlg")
             .WithSortCode("O.A050")
             .WithReferenceNumber("8001050")
             .WithName("1e. Netto-omzet uit geproduceerde goederen belast met 0% of niet bij u belast")
             .WithDescription("1e. Netto-omzet uit geproduceerde goederen belast met 0% of niet bij u belast")
             .WithBalanceSide(credit)
             .WithBalanceType(profitLoss)
             .WithGeneralLedgerAccountType(WOmz)
             .WithRgsLevel(4)
             .WithIsRgsUseWithZzp(true)
             .WithIsRgsBase(true)
             .WithIsRgsExtended(true)
             .WithIsRgsUseWithEZ(true)
             .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(WOmzNopOlg)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var WOmzNopOll = new GeneralLedgerAccountBuilder(this.Transaction)
             .WithReferenceCode("WOmzNopOll")
             .WithSortCode("O.A060")
             .WithReferenceNumber("8001060")
             .WithName("2a. Netto-omzet uit leveringen geproduceerde goederen waarbij de omzetbelasting naar u  is verlegd")
             .WithDescription("2a. Netto-omzet uit leveringen geproduceerde goederen waarbij de omzetbelasting naar u  is verlegd")
             .WithBalanceSide(credit)
             .WithBalanceType(profitLoss)
             .WithGeneralLedgerAccountType(WOmz)
             .WithRgsLevel(4)
             .WithIsRgsBase(true)
             .WithIsRgsExtended(true)
             .WithIsRgsUseWithEZ(true)
             .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(WOmzNopOll)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var WOmzNopOln = new GeneralLedgerAccountBuilder(this.Transaction)
             .WithReferenceCode("WOmzNopOln")
             .WithSortCode("O.A070")
             .WithReferenceNumber("8001070")
             .WithName("3a. Netto-omzet uit leveringen geproduceerde goederen naar landen buiten EU (uitvoer)")
             .WithDescription("3a. Netto-omzet uit leveringen geproduceerde goederen naar landen buiten EU (uitvoer)")
             .WithBalanceSide(credit)
             .WithBalanceType(profitLoss)
             .WithGeneralLedgerAccountType(WOmz)
             .WithRgsLevel(4)
             .WithIsRgsBase(true)
             .WithIsRgsExtended(true)
             .WithIsRgsUseWithEZ(true)
             .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(WOmzNopOln)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var WOmzNopOli = new GeneralLedgerAccountBuilder(this.Transaction)
             .WithReferenceCode("WOmzNopOli")
             .WithSortCode("O.A080")
             .WithReferenceNumber("8001080")
             .WithName("3b. Netto-omzet uit leveringen geproduceerde goederen naar landen binnen EU")
             .WithDescription("3b. Netto-omzet uit leveringen geproduceerde goederen naar landen binnen EU")
             .WithBalanceSide(credit)
             .WithBalanceType(profitLoss)
             .WithGeneralLedgerAccountType(WOmz)
             .WithRgsLevel(4)
             .WithIsRgsBase(true)
             .WithIsRgsExtended(true)
             .WithIsRgsUseWithEZ(true)
             .WithIsRgsUseWithZzp(true)
             .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(WOmzNopOli)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var BVorOvaNtf = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("BVorOvaNtf")
                .WithSortCode("G.L.C")
                .WithReferenceNumber("1104030")
                .WithName("Nog te factureren of nog te verzenden facturen")
                .WithDescription("Nog te factureren of nog te verzenden facturen overlopende activa")
                .WithBalanceSide(debit)
                .WithBalanceType(balance)
                .WithGeneralLedgerAccountType(Bvor)
                .WithRgsLevel(4)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(BVorOvaNtf)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var WBedAdlBov = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("WBedAdlBov")
                .WithSortCode("W.L.G")
                .WithReferenceNumber("4210060")
                .WithName("Boekingsverschillen")
                .WithDescription("Boekingsverschillen administratieve lasten")
                .WithBalanceSide(debit)
                .WithBalanceType(profitLoss)
                .WithGeneralLedgerAccountType(WBed)
                .WithRgsLevel(4)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(WBedAdlBov)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var WBedAdlVal = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("WBedAdlVal")
                .WithSortCode("W.L.F")
                .WithReferenceNumber("4210050")
                .WithName("Valutaomrekeningsverschillen")
                .WithDescription("Valutaomrekeningsverschillen administratieve lasten")
                .WithBalanceSide(debit)
                .WithBalanceType(profitLoss)
                .WithGeneralLedgerAccountType(WBed)
                .WithRgsLevel(4)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithWoco(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(WBedAdlVal)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var WKpr = new GeneralLedgerAccountTypeBuilder(this.Transaction)
             .WithDescription("Kostprijs van de omzet")
             .Build();

            var WKprBtkBed = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("WKprBtkBed")
                .WithSortCode("Q.K.A")
                .WithReferenceNumber("7008010")
                .WithName("Betalingskorting debiteuren")
                .WithDescription("Betalingskorting debiteuren")
                .WithBalanceSide(debit)
                .WithBalanceType(profitLoss)
                .WithGeneralLedgerAccountType(WKpr)
                .WithRgsLevel(4)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithZzp(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(WKprBtkBed)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var WKprBtkBec = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("WKprBtkBec")
                .WithSortCode("Q.K.B")
                .WithReferenceNumber("7008020")
                .WithName("Betalingskortingen op inkopen")
                .WithDescription("Betalingskortingen op inkopen inkoopwaarde handels- en productiegoederen")
                .WithBalanceSide(debit)
                .WithBalanceType(profitLoss)
                .WithGeneralLedgerAccountType(WKpr)
                .WithRgsLevel(4)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithZzp(true)
                .WithExcludeRgsAfrekSyst(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(WKprBtkBec)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var WOmzNodOdg = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("WOmzNodOdg")
                .WithSortCode("O.C050")
                .WithReferenceNumber("8003050")
                .WithName("1e. Netto-omzet uit verleende diensten belast met 0% of niet bij u belast")
                .WithDescription("1e. Netto-omzet uit verleende diensten belast met 0% of niet bij u belast")
                .WithBalanceSide(credit)
                .WithBalanceType(profitLoss)
                .WithGeneralLedgerAccountType(WOmz)
                .WithRgsLevel(4)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(WOmzNodOdg)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            var WOmzNopNod = new GeneralLedgerAccountBuilder(this.Transaction)
                .WithReferenceCode("WOmzNopNod")
                .WithSortCode("O.A150")
                .WithReferenceNumber("8001150")
                .WithName("Netto-omzet van onbelaste leveringen geproduceerde goederen")
                .WithDescription("Netto-omzet van onbelaste leveringen geproduceerde goederen")
                .WithBalanceSide(credit)
                .WithBalanceType(profitLoss)
                .WithGeneralLedgerAccountType(WOmz)
                .WithRgsLevel(4)
                .WithIsRgsBase(true)
                .WithIsRgsExtended(true)
                .WithIsRgsUseWithEZ(true)
                .WithIsRgsUseWithZzp(true)
                .Build();

            _ = new OrganisationGlAccountBuilder(this.Transaction)
             .WithGeneralLedgerAccount(WOmzNopNod)
             .WithInternalOrganisation(dutchInternalOrganisation)
             .Build();

            this.Transaction.Derive();

            var discountSettings = new InternalOrganisationInvoiceSettingsBuilder(this.Transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction).Discount)
                .WithSalesGeneralLedgerAccount(WKprBtkBed)
                .WithPurchaseLedgerAccount(WKprBtkBec)
                .Build();

            var dutchStandardTariffSetting = new InternalOrganisationVatRegimeSettingsBuilder(this.Transaction)
                .WithVatRegime(new VatRegimes(this.Transaction).DutchStandardTariff)
                .WithGeneralLedgerAccount(WOmzNopOlh)
                .Build();

            var dutchReducedTariffSetting = new InternalOrganisationVatRegimeSettingsBuilder(this.Transaction)
                .WithVatRegime(new VatRegimes(this.Transaction).DutchReducedTariff)
                .WithGeneralLedgerAccount(WOmzNopOlv)
                .Build();

            var serviceB2BSetting = new InternalOrganisationVatRegimeSettingsBuilder(this.Transaction)
                .WithVatRegime(new VatRegimes(this.Transaction).ServiceB2B)
                .WithGeneralLedgerAccount(WOmzNodOdg)
                .Build();

            var zeroRatedSetting = new InternalOrganisationVatRegimeSettingsBuilder(this.Transaction)
                .WithVatRegime(new VatRegimes(this.Transaction).ZeroRated)
                .WithGeneralLedgerAccount(WOmzNopOlg)
                .Build();

            var exemptSetting = new InternalOrganisationVatRegimeSettingsBuilder(this.Transaction)
                .WithVatRegime(new VatRegimes(this.Transaction).Exempt)
                .WithGeneralLedgerAccount(WOmzNopNod)
                .Build();

            var intraCommunautairSetting = new InternalOrganisationVatRegimeSettingsBuilder(this.Transaction)
                .WithVatRegime(new VatRegimes(this.Transaction).IntraCommunautair)
                .WithGeneralLedgerAccount(WOmzNopOli)
                .Build();

            this.Transaction.Derive();

            var yearPeriod = new AccountingPeriodBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Year)
                .WithPeriodNumber(2022)
                .WithInternalOrganisation(dutchInternalOrganisation)
                .Build();

            var periode1 = new AccountingPeriodBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Month)
                .WithPeriodNumber(1)
                .WithParent(yearPeriod)
                .WithInternalOrganisation(dutchInternalOrganisation)
                .Build();

            // Other 11 periodes
            for (var i = 1; i < 12; i++)
            {
                _ = new AccountingPeriodBuilder(this.Transaction)
                .WithFrequency(new TimeFrequencies(this.Transaction).Month)
                .WithPeriodNumber(i + 1)
                .WithParent(yearPeriod)
                .WithInternalOrganisation(dutchInternalOrganisation)
                .Build();
            }

            this.Transaction.Derive();

            var internalOrganisationAccountingSettings = new InternalOrganisationAccountingSettingsBuilder(this.Transaction)
                .WithAccountsPayable(BVorDebHad)
                .WithAccountsReceivable(BSchCreHac)
                .WithSalesPaymentDifferences(WBedAdlBet)
                .WithPurchasePaymentDifferences(WBedAdlBov)
                .WithExhangeRateDifferences(WBedAdlVal)
                .WithOpeningBalance(BSchTusTov)
                .WithInventory(BVrdHanVoo)
                .WithDeferredExpense(BVorOvaVof)
                .WithDeferredRevenue(BSchOpaVgo)
                .WithAccruedExpense(BSchOpaNto)
                .WithAccruedRevenue(BVorOvaNtf)
                .WithRetainedEarnings(BEivOreRvh)
                .WithEquity(BEivKapOnd)
                .WithSettingsForInvoiceItemType(discountSettings)
                .WithSettingsForVatRegime(dutchStandardTariffSetting)
                .WithSettingsForVatRegime(dutchReducedTariffSetting)
                .WithSettingsForVatRegime(serviceB2BSetting)
                .WithSettingsForVatRegime(zeroRatedSetting)
                .WithSettingsForVatRegime(exemptSetting)
                .WithSettingsForVatRegime(intraCommunautairSetting)
                .WithActualAccountingPeriod(periode1)
                .Build();

            dutchInternalOrganisation.SettingsForAccounting = internalOrganisationAccountingSettings;

            this.Transaction.Derive();

            var journal = new JournalBuilder(this.Transaction)
                .WithJournalType(new JournalTypes(this.Transaction).Bank)
                .WithName("Bank Journal")
                .WithContraAccount(BLimBanRbaBg1Gl)
                .WithCurrency(euro)
                .Build();

            this.Transaction.Derive();
        }
    }
}
