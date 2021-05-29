// <copyright file="SingletonExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>

namespace Allors
{
    using System.IO;
    using System.Linq;
    using Database.Domain;
    using Database.Domain.TestPopulation;
    using Bogus;

    public static class SingletonExtensions
    {
        public static void Full(this Singleton @this, DirectoryInfo dataPath, Faker faker)
        {
            var m = @this.Strategy.Transaction.Database.Services().M;

            var dutchLocale = new Locales(@this.Transaction()).DutchNetherlands;
            @this.AddAdditionalLocale(dutchLocale);

            var administrator = new PersonBuilder(@this.Transaction()).WithUserName("administrator").Build();
            new UserGroups(@this.Transaction()).Administrators.AddMember(administrator);
            new UserGroups(@this.Transaction()).Creators.AddMember(administrator);

            @this.Transaction().Derive();

            var euro = new Currencies(@this.Transaction()).FindBy(m.Currency.IsoCode, "EUR");

            var be = new Countries(@this.Transaction()).FindBy(m.Country.IsoCode, "BE");
            var us = new Countries(@this.Transaction()).FindBy(m.Country.IsoCode, "US");

            var allorsLogo = dataPath + @"\www\admin\images\logo.png";

            var serialisedItemSoldOns = new SerialisedItemSoldOn[] { new SerialisedItemSoldOns(@this.Transaction()).SalesInvoiceSend, new SerialisedItemSoldOns(@this.Transaction()).PurchaseInvoiceConfirm };

            var allors = Organisations.CreateInternalOrganisation(
                transaction: @this.Transaction(),
                name: "Allors BVBA",
                address: "Kleine Nieuwedijkstraat 4",
                postalCode: "2800",
                locality: "Mechelen",
                country: be,
                phone1CountryCode: "+32",
                phone1: "2 335 2335",
                phone1Purpose: new ContactMechanismPurposes(@this.Transaction()).GeneralPhoneNumber,
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
                billingProcess: new BillingProcesses(@this.Transaction()).BillingForOrderItems,
                customerShipmentNumberPrefix: "a-CS",
                salesInvoiceNumberPrefix: "a-SI-{year}-",
                salesOrderNumberPrefix: "a-SO",
                purchaseOrderNumberPrefix: "a-PO",
                purchaseInvoiceNumberPrefix: "a-PI",
                requestNumberPrefix: "a-RFQ",
                quoteNumberPrefix: "a-Q",
                productNumberPrefix: "A-",
                workEffortPrefix: "a-WO-",
                creditNoteNumberPrefix: "a-CN-",
                isImmediatelyPicked: true,
                autoGenerateShipmentPackage: true,
                isImmediatelyPacked: true,
                isAutomaticallyShipped: true,
                autoGenerateCustomerShipment: true,
                isAutomaticallyReceived: false,
                autoGeneratePurchaseShipment: false,
                useCreditNoteSequence: true,
                requestCounterValue: 0,
                quoteCounterValue: 0,
                orderCounterValue: 0,
                purchaseOrderCounterValue: 0,
                invoiceCounterValue: 100,
                purchaseInvoiceCounterValue: 0,
                purchaseOrderNeedsApproval: true,
                purchaseOrderApprovalThresholdLevel1: 1000M,
                purchaseOrderApprovalThresholdLevel2: 5000M,
                serialisedItemSoldOns: serialisedItemSoldOns,
                collectiveWorkEffortInvoice: true,
                invoiceSequence: new InvoiceSequences(@this.Transaction()).EnforcedSequence,
                requestSequence: new RequestSequences(@this.Transaction()).EnforcedSequence,
                quoteSequence: new QuoteSequences(@this.Transaction()).EnforcedSequence,
                customerShipmentSequence: new CustomerShipmentSequences(@this.Transaction()).EnforcedSequence,
                purchaseShipmentSequence: new PurchaseShipmentSequences(@this.Transaction()).EnforcedSequence,
                workEffortSequence: new WorkEffortSequences(@this.Transaction()).EnforcedSequence);

            var dipu = Organisations.CreateInternalOrganisation(
                transaction: @this.Transaction(),
                name: "Dipu BVBA",
                address: "Kleine Nieuwedijkstraat 2",
                postalCode: "2800",
                locality: "Mechelen",
                country: be,
                phone1CountryCode: "+32",
                phone1: "2 15 49 49 49",
                phone1Purpose: new ContactMechanismPurposes(@this.Transaction()).GeneralPhoneNumber,
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
                billingProcess: new BillingProcesses(@this.Transaction()).BillingForOrderItems,
                customerShipmentNumberPrefix: "d-CS",
                salesInvoiceNumberPrefix: "d-SI",
                salesOrderNumberPrefix: "d-SO",
                purchaseOrderNumberPrefix: "d-PO",
                purchaseInvoiceNumberPrefix: "d-PI",
                requestNumberPrefix: "d-RFQ",
                quoteNumberPrefix: "d-Q",
                productNumberPrefix: "D-",
                workEffortPrefix: "a-WO-",
                creditNoteNumberPrefix: "d-CN-",
                isImmediatelyPicked: true,
                autoGenerateShipmentPackage: true,
                isImmediatelyPacked: true,
                isAutomaticallyShipped: true,
                autoGenerateCustomerShipment: true,
                isAutomaticallyReceived: false,
                autoGeneratePurchaseShipment: false,
                useCreditNoteSequence: true,
                requestCounterValue: 0,
                quoteCounterValue: 0,
                orderCounterValue: 0,
                purchaseOrderCounterValue: 0,
                purchaseInvoiceCounterValue: 0,
                invoiceCounterValue: 0,
                purchaseOrderNeedsApproval: false,
                purchaseOrderApprovalThresholdLevel1: null,
                purchaseOrderApprovalThresholdLevel2: null,
                serialisedItemSoldOns: serialisedItemSoldOns,
                collectiveWorkEffortInvoice: true,
                invoiceSequence: new InvoiceSequences(@this.Transaction()).EnforcedSequence,
                requestSequence: new RequestSequences(@this.Transaction()).EnforcedSequence,
                quoteSequence: new QuoteSequences(@this.Transaction()).EnforcedSequence,
                customerShipmentSequence: new CustomerShipmentSequences(@this.Transaction()).EnforcedSequence,
                purchaseShipmentSequence: new PurchaseShipmentSequences(@this.Transaction()).EnforcedSequence,
                workEffortSequence: new WorkEffortSequences(@this.Transaction()).EnforcedSequence);

            // Give Administrator access
            new EmploymentBuilder(@this.Transaction()).WithEmployee(administrator).WithEmployer(allors).Build();

            @this.Settings.DefaultFacility = allors.FacilitiesWhereOwner.First;

            var allorsEmployee1 = allors.CreateEmployee("letmein", faker);
            var allorsEmployee2 = allors.CreateEmployee("letmein", faker);
            var allorsProductQuoteApprover = allors.CreateEmployee("letmein", faker);
            var allorsPurchaseInvoiceApprover = allors.CreateEmployee("letmein", faker);
            var allorsPurchaseOrderApproverLevel1 = allors.CreateEmployee("letmein", faker);
            var allorsPurchaseOrderApproverLevel2 = allors.CreateEmployee("letmein", faker);

            var dipuEmployee = dipu.CreateEmployee("letmein", faker);
            var dipuProductQuoteApprover = dipu.CreateEmployee("letmein", faker);
            var dipuPurchaseInvoiceApprover = dipu.CreateEmployee("letmein", faker);

            new FacilityBuilder(@this.Transaction())
                .WithName("Allors warehouse 2")
                .WithFacilityType(new FacilityTypes(@this.Transaction()).Warehouse)
                .WithOwner(allors)
                .Build();

            var vatRate = new VatRateBuilder(@this.Transaction()).WithRate(21).Build();
            var manufacturer = new OrganisationBuilder(@this.Transaction()).WithManufacturerDefaults(faker).Build();

            allors.CreateB2BCustomer(faker);
            allors.CreateB2CCustomer(faker);

            allors.CreateSupplier(faker);
            allors.CreateSupplier(faker);

            allors.CreateSubContractor(faker);
            allors.CreateSubContractor(faker);

            @this.Transaction().Derive();

            var nonSerialisedPart1 = allors.CreateNonSerialisedNonUnifiedPart(faker);
            var nonSerialisedPart2 = allors.CreateNonSerialisedNonUnifiedPart(faker);
            var serialisedPart1 = allors.CreateSerialisedNonUnifiedPart(faker);
            var serialisedPart2 = allors.CreateSerialisedNonUnifiedPart(faker);

            var good1 = new NonUnifiedGoodBuilder(@this.Transaction()).WithNonSerialisedDefaults(allors).Build();

            var good2 = new NonUnifiedGoodBuilder(@this.Transaction()).WithSerialisedDefaults(allors).Build();

            var serialisedItem = new SerialisedItemBuilder(@this.Transaction()).WithDefaults(allors).Build();
            serialisedPart1.AddSerialisedItem(serialisedItem);

            new InventoryItemTransactionBuilder(@this.Transaction())
                .WithSerialisedItem(serialisedItem)
                .WithFacility(allors.FacilitiesWhereOwner.First)
                .WithQuantity(1)
                .WithReason(new InventoryTransactionReasons(@this.Transaction()).IncomingShipment)
                .WithSerialisedInventoryItemState(new SerialisedInventoryItemStates(@this.Transaction()).Good)
                .Build();

            @this.Transaction().Derive();

            var good3 = new NonUnifiedGoodBuilder(@this.Transaction()).WithNonSerialisedDefaults(allors).Build();

            var good4 = new NonUnifiedGoodBuilder(@this.Transaction()).WithSerialisedDefaults(allors).Build();

            var productCategory1 = new ProductCategoryBuilder(@this.Transaction())
                .WithInternalOrganisation(allors)
                .WithName("Best selling gizmo's")
                .WithLocalisedName(new LocalisedTextBuilder(@this.Transaction()).WithText("Meest verkochte gizmo's").WithLocale(dutchLocale).Build())
                .Build();

            var productCategory2 = new ProductCategoryBuilder(@this.Transaction())
                .WithInternalOrganisation(allors)
                .WithName("Big Gizmo's")
                .WithLocalisedName(new LocalisedTextBuilder(@this.Transaction()).WithText("Grote Gizmo's").WithLocale(dutchLocale).Build())
                .Build();

            var productCategory3 = new ProductCategoryBuilder(@this.Transaction())
                .WithInternalOrganisation(allors)
                .WithName("Small gizmo's")
                .WithLocalisedName(new LocalisedTextBuilder(@this.Transaction()).WithText("Kleine gizmo's").WithLocale(dutchLocale).Build())
                .WithProduct(good1)
                .WithProduct(good2)
                .WithProduct(good3)
                .WithProduct(good4)
                .Build();

            new CatalogueBuilder(@this.Transaction())
                .WithInternalOrganisation(allors)
                .WithName("New gizmo's")
                .WithLocalisedName(new LocalisedTextBuilder(@this.Transaction()).WithText("Nieuwe gizmo's").WithLocale(dutchLocale).Build())
                .WithDescription("Latest in the world of Gizmo's")
                .WithLocalisedDescription(new LocalisedTextBuilder(@this.Transaction()).WithText("Laatste in de wereld van Gizmo's").WithLocale(dutchLocale).Build())
                .WithProductCategory(productCategory1)
                .Build();

            @this.Transaction().Derive();

            var email2 = new EmailAddressBuilder(@this.Transaction())
                .WithElectronicAddressString("recipient@acme.com")
                .Build();

            for (var i = 0; i < 10; i++)
            {
                var b2BCustomer = allors.CreateB2BCustomer(faker);

                @this.Transaction().Derive();

                new FaceToFaceCommunicationBuilder(@this.Transaction())
                    .WithDescription($"Meeting {i}")
                    .WithSubject($"meeting {i}")
                    .WithEventPurpose(new CommunicationEventPurposes(@this.Transaction()).Meeting)
                    .WithFromParty(allors.CurrentContacts.First)
                    .WithToParty(b2BCustomer.CurrentContacts.First)
                    .WithOwner(administrator)
                    .WithActualStart(@this.Transaction().Now())
                    .Build();

                new EmailCommunicationBuilder(@this.Transaction())
                    .WithDescription($"Email {i}")
                    .WithSubject($"email {i}")
                    .WithFromParty(allors.CurrentContacts.First)
                    .WithToParty(b2BCustomer.CurrentContacts.First)
                    .WithFromEmail(allors.GeneralEmail)
                    .WithToEmail(email2)
                    .WithEventPurpose(new CommunicationEventPurposes(@this.Transaction()).Meeting)
                    .WithOwner(administrator)
                    .WithActualStart(@this.Transaction().Now())
                    .Build();

                new LetterCorrespondenceBuilder(@this.Transaction())
                    .WithDescription($"Letter {i}")
                    .WithSubject($"letter {i}")
                    .WithFromParty(administrator)
                    .WithToParty(b2BCustomer.CurrentContacts.First)
                    .WithEventPurpose(new CommunicationEventPurposes(@this.Transaction()).Meeting)
                    .WithOwner(administrator)
                    .WithActualStart(@this.Transaction().Now())
                    .Build();

                new PhoneCommunicationBuilder(@this.Transaction())
                    .WithDescription($"Phone {i}")
                    .WithSubject($"phone {i}")
                    .WithFromParty(administrator)
                    .WithToParty(b2BCustomer.CurrentContacts.First)
                    .WithEventPurpose(new CommunicationEventPurposes(@this.Transaction()).Meeting)
                    .WithOwner(administrator)
                    .WithActualStart(@this.Transaction().Now())
                    .Build();

                var requestForQuote = new RequestForQuoteBuilder(@this.Transaction())
                    .WithEmailAddress($"customer{i}@acme.com")
                    .WithTelephoneNumber("+1 234 56789")
                    .WithRecipient(allors)
                    .Build();

                var requestItem = new RequestItemBuilder(@this.Transaction())
                    .WithSerialisedItem(serialisedItem)
                    .WithProduct(serialisedItem.PartWhereSerialisedItem.NonUnifiedGoodsWherePart.FirstOrDefault())
                    .WithComment($"Comment {i}")
                    .WithQuantity(1)
                    .Build();

                requestForQuote.AddRequestItem(requestItem);

                var quote = new ProductQuoteBuilder(@this.Transaction()).WithDefaults(allors).Build();

                var salesOrderItem1 = new SalesOrderItemBuilder(@this.Transaction())
                    .WithDescription("first item")
                    .WithProduct(good1)
                    .WithAssignedUnitPrice(3000)
                    .WithQuantityOrdered(1)
                    .WithMessage(@"line1
line2")
                    .WithInvoiceItemType(new InvoiceItemTypes(@this.Transaction()).ProductItem)
                    .Build();

                var salesOrderItem2 = new SalesOrderItemBuilder(@this.Transaction())
                    .WithDescription("second item")
                    .WithAssignedUnitPrice(2000)
                    .WithQuantityOrdered(2)
                    .WithInvoiceItemType(new InvoiceItemTypes(@this.Transaction()).ProductItem)
                    .Build();

                var salesOrderItem3 = new SalesOrderItemBuilder(@this.Transaction())
                    .WithDescription("Service")
                    .WithAssignedUnitPrice(100)
                    .WithQuantityOrdered(1)
                    .WithInvoiceItemType(new InvoiceItemTypes(@this.Transaction()).Service)
                    .Build();

                var order = new SalesOrderBuilder(@this.Transaction())
                    .WithTakenBy(allors)
                    .WithBillToCustomer(b2BCustomer)
                    .WithAssignedBillToEndCustomerContactMechanism(b2BCustomer.BillingAddress)
                    .WithSalesOrderItem(salesOrderItem1)
                    .WithSalesOrderItem(salesOrderItem2)
                    .WithSalesOrderItem(salesOrderItem3)
                    .WithCustomerReference("a reference number")
                    .WithDescription("Sale of 1 used Aircraft Towbar")
                    .WithAssignedVatRegime(new VatRegimes(@this.Transaction()).BelgiumStandard)
                    .Build();

                var salesInvoiceItem1 = new SalesInvoiceItemBuilder(@this.Transaction())
                    .WithDescription("first item")
                    .WithProduct(good1)
                    .WithAssignedUnitPrice(3000)
                    .WithQuantity(1)
                    .WithMessage(@"line1
line2")
                    .WithInvoiceItemType(new InvoiceItemTypes(@this.Transaction()).ProductItem)
                    .Build();

                var salesInvoiceItem2 = new SalesInvoiceItemBuilder(@this.Transaction())
                    .WithDescription("second item")
                    .WithAssignedUnitPrice(2000)
                    .WithQuantity(2)
                    .WithInvoiceItemType(new InvoiceItemTypes(@this.Transaction()).ProductItem)
                    .Build();

                var salesInvoiceItem3 = new SalesInvoiceItemBuilder(@this.Transaction())
                    .WithDescription("Service")
                    .WithAssignedUnitPrice(100)
                    .WithQuantity(1)
                    .WithInvoiceItemType(new InvoiceItemTypes(@this.Transaction()).Service)
                    .Build();

                var exw = new IncoTermTypes(@this.Transaction()).Exw;
                var incoTerm = new IncoTermBuilder(@this.Transaction()).WithTermType(exw).WithTermValue("XW").Build();

                var salesInvoice = new SalesInvoiceBuilder(@this.Transaction())
                    .WithBilledFrom(allors)
                    .WithBillToCustomer(b2BCustomer)
                    .WithBillToContactPerson(b2BCustomer.CurrentContacts.First)
                    .WithAssignedBillToContactMechanism(b2BCustomer.BillingAddress)
                    .WithSalesInvoiceItem(salesInvoiceItem1)
                    .WithSalesInvoiceItem(salesInvoiceItem2)
                    .WithSalesInvoiceItem(salesInvoiceItem3)
                    .WithCustomerReference("a reference number")
                    .WithDescription("Sale of 1 used Aircraft Towbar")
                    .WithSalesInvoiceType(new SalesInvoiceTypes(@this.Transaction()).SalesInvoice)
                    .WithSalesTerm(incoTerm)
                    .WithAssignedVatRegime(new VatRegimes(@this.Transaction()).BelgiumStandard)
                    .Build();

                for (var j = 0; j < 3; j++)
                {
                    var salesInvoiceItem = new SalesInvoiceItemBuilder(@this.Transaction())
                        .WithDescription("Some service being delivered")
                        .WithAssignedUnitPrice(100 + j)
                        .WithQuantity(1)
                        .WithInvoiceItemType(new InvoiceItemTypes(@this.Transaction()).Service)
                        .Build();

                    salesInvoice.AddSalesInvoiceItem(salesInvoiceItem);
                }
            }

            @this.Transaction().Derive();

            for (var i = 0; i < 4; i++)
            {
                var supplier = faker.Random.ListItem(allors.ActiveSuppliers);

                var purchaseInvoiceItem1 = new PurchaseInvoiceItemBuilder(@this.Transaction())
                    .WithDescription("first item")
                    .WithPart(nonSerialisedPart1)
                    .WithAssignedUnitPrice(3000)
                    .WithQuantity(1)
                    .WithMessage(@"line1
line2")
                    .WithInvoiceItemType(new InvoiceItemTypes(@this.Transaction()).PartItem)
                    .Build();

                var purchaseInvoiceItem2 = new PurchaseInvoiceItemBuilder(@this.Transaction())
                    .WithDescription("second item")
                    .WithAssignedUnitPrice(2000)
                    .WithQuantity(2)
                    .WithPart(nonSerialisedPart2)
                    .WithInvoiceItemType(new InvoiceItemTypes(@this.Transaction()).PartItem)
                    .Build();

                var purchaseInvoiceItem3 = new PurchaseInvoiceItemBuilder(@this.Transaction())
                    .WithDescription("Service")
                    .WithAssignedUnitPrice(100)
                    .WithQuantity(1)
                    .WithInvoiceItemType(new InvoiceItemTypes(@this.Transaction()).Service)
                    .Build();

                var purchaseInvoice = new PurchaseInvoiceBuilder(@this.Transaction())
                    .WithBilledTo(allors)
                    .WithBilledFrom(supplier)
                    .WithPurchaseInvoiceItem(purchaseInvoiceItem1)
                    .WithPurchaseInvoiceItem(purchaseInvoiceItem2)
                    .WithPurchaseInvoiceItem(purchaseInvoiceItem3)
                    .WithCustomerReference("a reference number")
                    .WithDescription("Purchase of 1 used Aircraft Towbar")
                    .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(@this.Transaction()).PurchaseInvoice)
                    .WithAssignedVatRegime(new VatRegimes(@this.Transaction()).BelgiumStandard)
                    .Build();

                var purchaseOrderItem1 = new PurchaseOrderItemBuilder(@this.Transaction())
                    .WithDescription("first purchase order item")
                    .WithPart(nonSerialisedPart1)
                    .WithStoredInFacility(allors.FacilitiesWhereOwner.First)
                    .WithQuantityOrdered(1)
                    .Build();

                var purchaseOrder = new PurchaseOrderBuilder(@this.Transaction())
                    .WithOrderedBy(allors)
                    .WithTakenViaSupplier(supplier)
                    .WithPurchaseOrderItem(purchaseOrderItem1)
                    .WithCustomerReference("reference " + i)
                    .Build();
            }

            var anOrganisation = new Organisations(@this.Transaction()).FindBy(m.Organisation.IsInternalOrganisation, false);

            var item = new SerialisedItemBuilder(@this.Transaction())
                .WithName("name")
                .WithSerialNumber("112")
                .WithSerialisedItemAvailability(new SerialisedItemAvailabilities(@this.Transaction()).Sold)
                .WithAvailableForSale(false)
                .WithOwnedBy(anOrganisation)
                .Build();

            nonSerialisedPart2.AddSerialisedItem(item);

            var workTask = new WorkTaskBuilder(@this.Transaction())
                .WithTakenBy(allors)
                .WithCustomer(anOrganisation)
                .WithName("maintenance")
                .Build();

            new WorkEffortFixedAssetAssignmentBuilder(@this.Transaction())
                .WithFixedAsset(item)
                .WithAssignment(workTask)
                .Build();

            var workOrderPart1 = allors.CreateNonSerialisedNonUnifiedPart(faker);
            var workOrderPart2 = allors.CreateNonSerialisedNonUnifiedPart(faker);
            var workOrderPart3 = allors.CreateNonSerialisedNonUnifiedPart(faker);

            @this.Transaction().Derive();

            var workOrder = new WorkTaskBuilder(@this.Transaction())
                .WithName("Task")
                .WithTakenBy(allors)
                .WithFacility(new Facilities(@this.Transaction()).Extent().First)
                .WithCustomer(anOrganisation)
                .WithWorkEffortPurpose(new WorkEffortPurposes(@this.Transaction()).Maintenance)
                .WithSpecialTerms("Net 45 Days")
                .Build();

            new WorkEffortFixedAssetAssignmentBuilder(@this.Transaction())
                .WithFixedAsset(item)
                .WithAssignment(workOrder)
                .WithComment("Busted tailpipe")
                .Build();

            workOrder.CreateInventoryAssignment(workOrderPart1, 11);
            workOrder.CreateInventoryAssignment(workOrderPart2, 12);
            workOrder.CreateInventoryAssignment(workOrderPart3, 13);

            //// Work Effort Time Entries
            var yesterday = DateTimeFactory.CreateDateTime(@this.Transaction().Now().AddDays(-1));
            var laterYesterday = DateTimeFactory.CreateDateTime(yesterday.AddHours(3));

            var today = DateTimeFactory.CreateDateTime(@this.Transaction().Now());
            var laterToday = DateTimeFactory.CreateDateTime(today.AddHours(4));

            var tomorrow = DateTimeFactory.CreateDateTime(@this.Transaction().Now().AddDays(1));
            var laterTomorrow = DateTimeFactory.CreateDateTime(tomorrow.AddHours(6));

            var standardRate = new RateTypes(@this.Transaction()).StandardRate;
            var overtimeRate = new RateTypes(@this.Transaction()).OvertimeRate;

            var frequencies = new TimeFrequencies(@this.Transaction());

            var timeEntryYesterday1 = workOrder.CreateTimeEntry(yesterday, laterYesterday, frequencies.Day, standardRate);
            var timeEntryToday1 = workOrder.CreateTimeEntry(today, laterToday, frequencies.Hour, standardRate);
            var timeEntryTomorrow1 = workOrder.CreateTimeEntry(tomorrow, laterTomorrow, frequencies.Minute, overtimeRate);

            allorsEmployee1.TimeSheetWhereWorker.AddTimeEntry(timeEntryYesterday1);
            allorsEmployee1.TimeSheetWhereWorker.AddTimeEntry(timeEntryToday1);
            allorsEmployee1.TimeSheetWhereWorker.AddTimeEntry(timeEntryTomorrow1);

            var timeEntryYesterday2 = workOrder.CreateTimeEntry(yesterday, laterYesterday, frequencies.Day, standardRate);
            var timeEntryToday2 = workOrder.CreateTimeEntry(today, laterToday, frequencies.Hour, standardRate);
            var timeEntryTomorrow2 = workOrder.CreateTimeEntry(tomorrow, laterTomorrow, frequencies.Minute, overtimeRate);

            allorsEmployee2.TimeSheetWhereWorker.AddTimeEntry(timeEntryYesterday2);
            allorsEmployee2.TimeSheetWhereWorker.AddTimeEntry(timeEntryToday2);
            allorsEmployee2.TimeSheetWhereWorker.AddTimeEntry(timeEntryTomorrow2);

            var po = new PurchaseOrders(@this.Transaction()).Extent().First;
            foreach (PurchaseOrderItem purchaseOrderItem in po.PurchaseOrderItems)
            {
                new WorkEffortPurchaseOrderItemAssignmentBuilder(@this.Transaction())
                    .WithPurchaseOrderItem(purchaseOrderItem)
                    .WithAssignment(workOrder)
                    .WithQuantity(1)
                    .Build();
            }

            @this.Transaction().Derive();
        }
    }
}
