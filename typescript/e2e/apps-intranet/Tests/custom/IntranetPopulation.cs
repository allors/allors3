// <copyright file="IntranetPopulation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>


using Allors.Database.Meta;

namespace Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Allors;
    using Allors.Database;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;

    public class IntranetPopulation
    {
        private readonly ITransaction Transaction;

        private readonly DirectoryInfo DataPath;

        private readonly MetaPopulation M;

        public IntranetPopulation(ITransaction transaction, DirectoryInfo dataPath, MetaPopulation m)
        {
            this.Transaction = transaction;
            this.DataPath = dataPath;
            this.M = m;
        }

        public void Execute()
        {
            var singleton = this.Transaction.GetSingleton();
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;
            singleton.AddAdditionalLocale(dutchLocale);

            var euro = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "EUR");
            var usd = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "USD");

            new ExchangeRateBuilder(this.Transaction).WithValidFrom(new DateTime(2021, 04, 19, 12, 0, 0, DateTimeKind.Utc)).WithFromCurrency(euro).WithToCurrency(usd).WithRate(1.2027861M).Build();

            var be = new Countries(this.Transaction).FindBy(M.Country.IsoCode, "BE");
            var us = new Countries(this.Transaction).FindBy(M.Country.IsoCode, "US");

            var email2 = new EmailAddressBuilder(this.Transaction).WithDefaults().Build();

            var allorsLogo = this.DataPath + @"\www\admin\images\logo.png";

            var serialisedItemSoldOns = new SerialisedItemSoldOn[] { new SerialisedItemSoldOns(this.Transaction).SalesInvoiceSend, new SerialisedItemSoldOns(this.Transaction).PurchaseInvoiceConfirm };

            var allors = Organisations.CreateInternalOrganisation(
               transaction: this.Transaction,
               name: "Allors BVBA",
               address1: "Kleine Nieuwedijkstraat 4",
               postalCode: "2800",
               locality: "Mechelen",
               country: be,
               phone1CountryCode: "+32",
               phone1: "2 335 2335",
               phone1Purpose: new ContactMechanismPurposes(this.Transaction).GeneralPhoneNumber,
               phone2CountryCode: string.Empty,
               phone2: string.Empty,
               phone2Purpose: null,
               emailAddress: "email@allors.com",
               websiteAddress: "www.allors.com",
               taxNumber: "BE 0476967014",
               bankName: "ING",
               facilityName: "Allors Warehouse 1",
               bic: "BBRUBEBB",
               iban: "BE89 3200 1467 7685",
               currency: euro,
               logo: "allors.png",
               storeName: "Allors Store",
               billingProcess: new BillingProcesses(this.Transaction).BillingForOrderItems,
               customerShipmentNumberPrefix: "a-CS",
               customerReturnNumberPrefix: "a-CR",
               purchaseShipmentNumberPrefix: "a-PS",
               purchaseReturnNumberPrefix: "a-PR",
               salesInvoiceNumberPrefix: "a-SI-{year}-",
               salesOrderNumberPrefix: "a-SO",
               purchaseOrderNumberPrefix: "a-PO",
               purchaseInvoiceNumberPrefix: "a-PI",
               requestNumberPrefix: "a-RFQ",
               productQuoteNumberPrefix: "a-PQ",
               statementOfWorkNumberPrefix: "a-WQ",
               productNumberPrefix: "A-",
               workEffortPrefix: "a-WO-",
               requirementPrefix: "a-REQ-",
               creditNoteNumberPrefix: "a-CN-",
               isImmediatelyPicked: true,
               autoGenerateShipmentPackage: true,
               isImmediatelyPacked: true,
               isAutomaticallyShipped: true,
               autoGenerateCustomerShipment: true,
               isAutomaticallyReceived: false,
               shipmentIsAutomaticallyReturned: true,
               autoGeneratePurchaseShipment: false,
               useCreditNoteSequence: true,
               requestCounterValue: 0,
               productQuoteCounterValue: 0,
               statementOfWorkCounterValue: 0,
               orderCounterValue: 0,
               purchaseOrderCounterValue: 0,
               invoiceCounterValue: 100,
               purchaseInvoiceCounterValue: 0,
               purchaseOrderNeedsApproval: true,
               purchaseOrderApprovalThresholdLevel1: 1000M,
               purchaseOrderApprovalThresholdLevel2: 5000M,
               serialisedItemSoldOns: serialisedItemSoldOns,
               collectiveWorkEffortInvoice: true,
               invoiceSequence: new InvoiceSequences(this.Transaction).EnforcedSequence,
               requestSequence: new RequestSequences(this.Transaction).EnforcedSequence,
               quoteSequence: new QuoteSequences(this.Transaction).EnforcedSequence,
               customerShipmentSequence: new CustomerShipmentSequences(this.Transaction).EnforcedSequence,
               purchaseShipmentSequence: new PurchaseShipmentSequences(this.Transaction).EnforcedSequence,
               workEffortSequence: new WorkEffortSequences(this.Transaction).EnforcedSequence,
               requirementSequence: new RequirementSequences(this.Transaction).EnforcedSequence);

            var dipu = Organisations.CreateInternalOrganisation(
                transaction: this.Transaction,
                name: "Dipu BVBA",
                address1: "Kleine Nieuwedijkstraat 2",
                postalCode: "2800",
                locality: "Mechelen",
                country: be,
                phone1CountryCode: "+32",
                phone1: "2 15 49 49 49",
                phone1Purpose: new ContactMechanismPurposes(this.Transaction).GeneralPhoneNumber,
                phone2CountryCode: string.Empty,
                phone2: string.Empty,
                phone2Purpose: null,
                emailAddress: "email@dipu.com",
                websiteAddress: "www.dipu.com",
                taxNumber: "BE 0445366489",
                bankName: "ING",
                facilityName: "Dipu Facility",
                bic: "BBRUBEBB",
                iban: "BE23 3300 6167 6391",
                currency: euro,
                logo: "allors.png",
                storeName: "Dipu Store",
                billingProcess: new BillingProcesses(this.Transaction).BillingForOrderItems,
                customerShipmentNumberPrefix: "d-CS",
                customerReturnNumberPrefix: "d-CR",
                purchaseShipmentNumberPrefix: "d-PS",
                purchaseReturnNumberPrefix: "d-PR",
                salesInvoiceNumberPrefix: "d-SI",
                salesOrderNumberPrefix: "d-SO",
                purchaseOrderNumberPrefix: "d-PO",
                purchaseInvoiceNumberPrefix: "d-PI",
                requestNumberPrefix: "d-RFQ",
                productQuoteNumberPrefix: "d-PQ",
                statementOfWorkNumberPrefix: "d-WQ",
                productNumberPrefix: "D-",
                workEffortPrefix: "d-WO-",
                requirementPrefix: "d-REQ-",
                creditNoteNumberPrefix: "d-CN-",
                isImmediatelyPicked: true,
                autoGenerateShipmentPackage: true,
                isImmediatelyPacked: true,
                isAutomaticallyShipped: true,
                autoGenerateCustomerShipment: true,
                isAutomaticallyReceived: false,
                shipmentIsAutomaticallyReturned: true,
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


            singleton.Settings.DefaultFacility = allors.FacilitiesWhereOwner.FirstOrDefault();
            var faker = this.Transaction.Faker();

            allors.CreateEmployee("letmein", faker);
            var jane = allors.CreateAdministrator("letmein", faker);
            jane.UserName = "jane@example.com";
            var allorsB2BCustomer = allors.CreateB2BCustomer(this.Transaction.Faker());
            var allorsB2CCustomer = allors.CreateB2CCustomer(this.Transaction.Faker());
            allors.CreateSupplier(this.Transaction.Faker());
            allors.CreateSubContractor(this.Transaction.Faker());

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(allorsB2BCustomer)
                .WithInternalOrganisation(dipu)
                .WithFromDate(this.Transaction.Now())
                .Build();

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(allorsB2CCustomer)
                .WithInternalOrganisation(dipu)
                .WithFromDate(this.Transaction.Now())
                .Build();

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(dipu)
                .WithInternalOrganisation(allors)
                .WithFromDate(this.Transaction.Now())
                .Build();

            dipu.CreateEmployee("letmein", faker);
            dipu.CreateAdministrator("letmein", faker);
            var dipuB2BCustomer = dipu.CreateB2BCustomer(this.Transaction.Faker());
            var dipuB2CCustomer = dipu.CreateB2CCustomer(this.Transaction.Faker());
            dipu.CreateSupplier(this.Transaction.Faker());
            dipu.CreateSubContractor(this.Transaction.Faker());

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(dipuB2BCustomer)
                .WithInternalOrganisation(allors)
                .WithFromDate(this.Transaction.Now())
                .Build();

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(dipuB2CCustomer)
                .WithInternalOrganisation(allors)
                .WithFromDate(this.Transaction.Now())
                .Build();

            new CustomerRelationshipBuilder(this.Transaction)
                .WithCustomer(allors)
                .WithInternalOrganisation(dipu)
                .WithFromDate(this.Transaction.Now())
                .Build();

            this.Transaction.Derive();

            var facility = new FacilityBuilder(this.Transaction)
                .WithName("Allors warehouse 2")
                .WithFacilityType(new FacilityTypes(this.Transaction).Warehouse)
                .WithOwner(allors)
                .Build();

            // TODO: Martien
            //var store = new StoreBuilder(this.Transaction).WithName("store")
            //    .WithInternalOrganisation(allors)
            //    .WithDefaultFacility(facility)
            //    .WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
            //    .WithDefaultCarrier(new Carriers(this.Transaction).Fedex)
            //    .Build();

            var productType = new ProductTypeBuilder(this.Transaction)
                .WithName($"Gizmo Serialised")
                .WithSerialisedItemCharacteristicType(new SerialisedItemCharacteristicTypeBuilder(this.Transaction)
                                            .WithName("Size")
                                            .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Afmeting").WithLocale(dutchLocale).Build())
                                            .Build())
                .WithSerialisedItemCharacteristicType(new SerialisedItemCharacteristicTypeBuilder(this.Transaction)
                                            .WithName("Weight")
                                            .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Gewicht").WithLocale(dutchLocale).Build())
                                            .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Kilogram)
                                            .Build())
                .Build();

            var vatRate = new VatRateBuilder(this.Transaction).WithRate(21).Build();

            var brand = new BrandBuilder(this.Transaction).WithDefaults().Build();

            var good_1 = new UnifiedGoodBuilder(this.Transaction).WithNonSerialisedDefaults(allors).Build();

            this.Transaction.Derive();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(good_1)
                .WithQuantity(100)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .Build();

            var good_2 = new NonUnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(allors).Build();

            var serialisedItem1 = new SerialisedItemBuilder(this.Transaction).WithDefaults(allors).Build();

            good_2.Part.AddSerialisedItem(serialisedItem1);

            this.Transaction.Derive();

            new SerialisedInventoryItemBuilder(this.Transaction)
                .WithPart(good_2.Part)
                .WithSerialisedItem(serialisedItem1)
                .WithFacility(allors.StoresWhereInternalOrganisation.First().DefaultFacility)
                .Build();

            var good_3 = new NonUnifiedGoodBuilder(this.Transaction).WithNonSerialisedDefaults(allors).Build();

            var good_4 = new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(allors).Build();

            var productCategory_1 = new ProductCategoryBuilder(this.Transaction)
                .WithInternalOrganisation(allors)
                .WithName("Best selling gizmo's")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Meest verkochte gizmo's").WithLocale(dutchLocale).Build())
                .Build();

            var productCategory_2 = new ProductCategoryBuilder(this.Transaction)
                .WithInternalOrganisation(allors)
                .WithName("Big Gizmo's")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Grote Gizmo's").WithLocale(dutchLocale).Build())
                .Build();

            var productCategory_3 = new ProductCategoryBuilder(this.Transaction)
                .WithInternalOrganisation(allors)
                .WithName("Small gizmo's")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Kleine gizmo's").WithLocale(dutchLocale).Build())
                .WithProduct(good_1)
                .WithProduct(good_2)
                .WithProduct(good_3)
                .WithProduct(good_4)
                .Build();

            new CatalogueBuilder(this.Transaction)
                .WithInternalOrganisation(allors)
                .WithName("New gizmo's")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Nieuwe gizmo's").WithLocale(dutchLocale).Build())
                .WithDescription("Latest in the world of Gizmo's")
                .WithLocalisedDescription(new LocalisedTextBuilder(this.Transaction).WithText("Laatste in de wereld van Gizmo's").WithLocale(dutchLocale).Build())
                .WithProductCategory(productCategory_1)
                .Build();

            this.Transaction.Derive();

            new FaceToFaceCommunicationBuilder(this.Transaction).WithDefaults(allors).Build();
            new EmailCommunicationBuilder(this.Transaction).WithDefaults(allors).Build();
            new LetterCorrespondenceBuilder(this.Transaction).WithDefaults(allors).Build();
            new PhoneCommunicationBuilder(this.Transaction).WithDefaults(allors).Build();

            var salesOrder_1 = allors.CreateB2BSalesOrder(faker);
            var salesOrder_2 = allors.CreateB2CSalesOrder(faker);
            var salesOrder_3 = allors.CreateInternalSalesOrder(faker);

            new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(allors).Build();
            new SalesInvoiceBuilder(this.Transaction).WithCreditNoteDefaults(allors).Build();

            this.Transaction.Derive();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(good_1)
                .WithSupplier(allors.ActiveSuppliers.FirstOrDefault())
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(7)
                .WithCurrency(euro)
                .Build();

            var purchaseInvoicePartItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithPartItemDefaults().Build();

            var purchaseInvoiceProductItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithSerialisedProductItemDefaults().Build();

            var purchaseInvoiceNotPartOrProductItem = new PurchaseInvoiceItemBuilder(this.Transaction).WithDefaults().Build();

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithExternalB2BInvoiceDefaults(allors).Build();

            allors.CreatePurchaseOrderWithBothItems(faker);

            var workTask = new WorkTaskBuilder(this.Transaction)
                .WithTakenBy(allors)
                .WithCustomer(allors.ActiveCustomers.FirstOrDefault())
                .WithName("maintenance")
                .Build();

            new PositionTypeBuilder(this.Transaction)
                .WithTitle("Mechanic")
                .WithUniqueId(new Guid("E62A8F4B-8045-472E-AB18-E39C51A02696"))
                .Build();

            new PositionTypeRateBuilder(this.Transaction)
                .WithRate(100)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFrequency(new TimeFrequencies(this.Transaction).Hour)
                .Build();

            this.Transaction.Derive();

            // Serialized RFQ with Serialized Unified-Good
            var serializedRFQ = new RequestForQuoteBuilder(this.Transaction).WithSerializedDefaults(allors).Build();

            // NonSerialized RFQ with NonSerialized Unified-Good
            var nonSerializedRFQ = new RequestForQuoteBuilder(this.Transaction).WithNonSerializedDefaults(allors).Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithSerializedDefaults(allors).Build();

            this.Transaction.Derive();

            var salesOrderItem_1 = new SalesOrderItemBuilder(this.Transaction).WithNonSerialisedPartItemDefaults().Build();

            salesOrder_2.AddSalesOrderItem(salesOrderItem_1);

            this.Transaction.Derive();

            new CustomerShipmentBuilder(this.Transaction).WithDefaults(allors).Build();

            this.Transaction.Derive();

            new PurchaseShipmentBuilder(this.Transaction).WithDefaults(allors).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        private byte[] GetResourceBytes(string name)
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            var manifestResourceName = assembly.GetManifestResourceNames().First(v => v.Contains(name));
            var resource = assembly.GetManifestResourceStream(manifestResourceName);
            if (resource != null)
            {
                using var ms = new MemoryStream();
                resource.CopyTo(ms);
                return ms.ToArray();
            }

            return null;
        }

        private Template CreateOpenDocumentTemplate(byte[] content)
        {
            var media = new MediaBuilder(this.Transaction).WithInData(content).Build();
            var templateType = new TemplateTypes(this.Transaction).OpenDocumentType;
            var template = new TemplateBuilder(this.Transaction).WithMedia(media).WithTemplateType(templateType).Build();
            return template;
        }
    }
}
