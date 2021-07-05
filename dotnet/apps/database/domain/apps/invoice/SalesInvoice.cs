// <copyright file="SalesInvoice.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class SalesInvoice
    {
        public bool IsDeletable =>
            this.ExistSalesInvoiceState
            && this.SalesInvoiceState.Equals(new SalesInvoiceStates(this.Strategy.Transaction).ReadyForPosting)
            && this.SalesInvoiceItems.All(v => v.IsDeletable)
            && !this.ExistSalesOrders
            && !this.ExistPurchaseInvoice
            && !this.ExistRepeatingSalesInvoiceWhereSource
            && !this.IsRepeatingInvoice;

        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.SalesInvoice, this.M.SalesInvoice.SalesInvoiceState),
        };

        public int PaymentNetDays
        {
            get
            {
                if (this.ExistSalesTerms)
                {
                    foreach (AgreementTerm term in this.SalesTerms)
                    {
                        if (term.TermType.Equals(new InvoiceTermTypes(this.Strategy.Transaction).PaymentNetDays))
                        {
                            if (int.TryParse(term.TermValue, out var netDays))
                            {
                                return netDays;
                            }
                        }
                    }
                }

                var now = this.Transaction().Now();
                var customerRelationship = this.BillToCustomer?.CustomerRelationshipsWhereCustomer
                    .FirstOrDefault(v => Equals(v.InternalOrganisation, this.BilledFrom)
                      && v.FromDate <= now
                      && (!v.ExistThroughDate || v.ThroughDate >= now));

                if (customerRelationship?.PaymentNetDays().HasValue == true)
                {
                    return customerRelationship.PaymentNetDays().Value;
                }

                if (this.ExistStore && this.Store.ExistPaymentNetDays)
                {
                    return this.Store.PaymentNetDays;
                }

                return 0;
            }
        }

        public InvoiceItem[] InvoiceItems => this.SalesInvoiceItems.ToArray();

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistSalesInvoiceState)
            {
                this.SalesInvoiceState = new SalesInvoiceStates(this.Strategy.Transaction).ReadyForPosting;
            }

            if (!this.ExistEntryDate)
            {
                this.EntryDate = this.Transaction().Now();
            }

            if (!this.ExistInvoiceDate)
            {
                this.InvoiceDate = this.Transaction().Now();
            }

            if (this.ExistBillToCustomer)
            {
                this.PreviousBillToCustomer = this.BillToCustomer;
            }

            if (!this.ExistSalesInvoiceType)
            {
                this.SalesInvoiceType = new SalesInvoiceTypes(this.Strategy.Transaction).SalesInvoice;
            }
        }

        public void AppsOnInit(ObjectOnInit method)
        {
            var internalOrganisations = new Organisations(this.Transaction()).InternalOrganisations();

            if (!this.ExistBilledFrom && internalOrganisations.Length == 1)
            {
                this.BilledFrom = internalOrganisations[0];
            }
        }

        public void AppsSend(SalesInvoiceSend method)
        {
            var singleton = this.Transaction().GetSingleton();
            var year = this.InvoiceDate.Year;

            if (object.Equals(this.SalesInvoiceType, new SalesInvoiceTypes(this.Strategy.Transaction).SalesInvoice))
            {
                this.InvoiceNumber = this.Store.NextSalesInvoiceNumber(this.InvoiceDate.Year);

                var fiscalYearStoreSequenceNumbers = this.Store.FiscalYearsStoreSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                var prefix = this.BilledFrom.InvoiceSequence.IsEnforcedSequence ? this.Store.SalesInvoiceNumberPrefix : fiscalYearStoreSequenceNumbers.SalesInvoiceNumberPrefix;
                this.SortableInvoiceNumber = singleton.SortableNumber(prefix, this.InvoiceNumber, year.ToString());
            }

            if (object.Equals(this.SalesInvoiceType, new SalesInvoiceTypes(this.Strategy.Transaction).CreditNote))
            {
                this.InvoiceNumber = this.Store.NextCreditNoteNumber(this.InvoiceDate.Year);

                var fiscalYearStoreSequenceNumbers = this.Store.FiscalYearsStoreSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                var prefix = this.BilledFrom.InvoiceSequence.IsEnforcedSequence ? this.Store.CreditNoteNumberPrefix : fiscalYearStoreSequenceNumbers.CreditNoteNumberPrefix;
                this.SortableInvoiceNumber = singleton.SortableNumber(prefix, this.InvoiceNumber, year.ToString());
            }

            this.SalesInvoiceState = new SalesInvoiceStates(this.Strategy.Transaction).NotPaid;

            foreach (var salesInvoiceItem in this.SalesInvoiceItems)
            {
                salesInvoiceItem.SalesInvoiceItemState = new SalesInvoiceItemStates(this.Strategy.Transaction).NotPaid;
                salesInvoiceItem.DerivationTrigger = Guid.NewGuid();

                if (this.SalesInvoiceType.Equals(new SalesInvoiceTypes(this.Transaction()).SalesInvoice)
                    && salesInvoiceItem.ExistSerialisedItem
                    && (this.BillToCustomer as InternalOrganisation)?.IsInternalOrganisation == false
                    && this.BilledFrom.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(this.Transaction()).SalesInvoiceSend))
                {
                    if (salesInvoiceItem.NextSerialisedItemAvailability?.Equals(new SerialisedItemAvailabilities(this.Transaction()).Sold) == true)
                    {
                        salesInvoiceItem.SerialisedItemVersionBeforeSale = salesInvoiceItem.SerialisedItem.CurrentVersion;

                        salesInvoiceItem.SerialisedItem.Seller = this.BilledFrom;
                        salesInvoiceItem.SerialisedItem.OwnedBy = this.BillToCustomer;
                        salesInvoiceItem.SerialisedItem.Ownership = new Ownerships(this.Transaction()).ThirdParty;
                        salesInvoiceItem.SerialisedItem.SerialisedItemAvailability = salesInvoiceItem.NextSerialisedItemAvailability;
                        salesInvoiceItem.SerialisedItem.AvailableForSale = false;
                    }

                    if (salesInvoiceItem.NextSerialisedItemAvailability?.Equals(new SerialisedItemAvailabilities(this.Transaction()).InRent) == true)
                    {
                        salesInvoiceItem.SerialisedItem.RentedBy = this.BillToCustomer;
                        salesInvoiceItem.SerialisedItem.SerialisedItemAvailability = salesInvoiceItem.NextSerialisedItemAvailability;
                        salesInvoiceItem.SerialisedItem.AvailableForSale = false;
                    }
                }

                if (this.SalesInvoiceType.Equals(new SalesInvoiceTypes(this.Transaction()).CreditNote)
                    && salesInvoiceItem.ExistSerialisedItem
                    && salesInvoiceItem.ExistSerialisedItemVersionBeforeSale
                    && (this.BillToCustomer as InternalOrganisation)?.IsInternalOrganisation == false
                    && this.BilledFrom.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(this.Transaction()).SalesInvoiceSend))
                {
                    salesInvoiceItem.SerialisedItem.Seller = salesInvoiceItem.SerialisedItemVersionBeforeSale.Seller;
                    salesInvoiceItem.SerialisedItem.OwnedBy = salesInvoiceItem.SerialisedItemVersionBeforeSale.OwnedBy;
                    salesInvoiceItem.SerialisedItem.Ownership = salesInvoiceItem.SerialisedItemVersionBeforeSale.Ownership;
                    salesInvoiceItem.SerialisedItem.SerialisedItemAvailability = salesInvoiceItem.SerialisedItemVersionBeforeSale.SerialisedItemAvailability;
                    salesInvoiceItem.SerialisedItem.AvailableForSale = salesInvoiceItem.SerialisedItemVersionBeforeSale.AvailableForSale;
                }
            }

            if (this.BillToCustomer is Organisation organisation && organisation.IsInternalOrganisation)
            {
                var purchaseInvoice = new PurchaseInvoiceBuilder(this.Strategy.Transaction)
                    .WithBilledFrom((Organisation)this.BilledFrom)
                    .WithBilledFromContactPerson(this.BilledFromContactPerson)
                    .WithBilledTo((InternalOrganisation)this.BillToCustomer)
                    .WithBilledToContactPerson(this.BillToContactPerson)
                    .WithBillToEndCustomer(this.BillToEndCustomer)
                    .WithAssignedBillToEndCustomerContactMechanism(this.DerivedBillToEndCustomerContactMechanism)
                    .WithBillToEndCustomerContactPerson(this.BillToEndCustomerContactPerson)
                    .WithAssignedBillToCustomerPaymentMethod(this.DerivedPaymentMethod)
                    .WithShipToCustomer(this.ShipToCustomer)
                    .WithAssignedShipToCustomerAddress(this.DerivedShipToAddress)
                    .WithShipToCustomerContactPerson(this.ShipToContactPerson)
                    .WithShipToEndCustomer(this.ShipToEndCustomer)
                    .WithAssignedShipToEndCustomerAddress(this.DerivedShipToEndCustomerAddress)
                    .WithShipToEndCustomerContactPerson(this.ShipToEndCustomerContactPerson)
                    .WithDescription(this.Description)
                    .WithInvoiceDate(this.Transaction().Now())
                    .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Strategy.Transaction).PurchaseInvoice)
                    .WithCustomerReference(this.CustomerReference)
                    .WithComment(this.Comment)
                    .WithInternalComment(this.InternalComment)
                    .Build();

                foreach (var orderAdjustment in this.OrderAdjustments)
                {
                    OrderAdjustment newAdjustment = null;
                    if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
                    {
                        newAdjustment = new DiscountAdjustmentBuilder(this.Transaction()).Build();
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                    {
                        newAdjustment = new SurchargeAdjustmentBuilder(this.Transaction()).Build();
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                    {
                        newAdjustment = new FeeBuilder(this.Transaction()).Build();
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                    {
                        newAdjustment = new ShippingAndHandlingChargeBuilder(this.Transaction()).Build();
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                    {
                        newAdjustment = new MiscellaneousChargeBuilder(this.Transaction()).Build();
                    }

                    newAdjustment.Amount ??= orderAdjustment.Amount;
                    newAdjustment.Percentage ??= orderAdjustment.Percentage;
                    purchaseInvoice.AddOrderAdjustment(newAdjustment);
                }

                foreach (var salesInvoiceItem in this.SalesInvoiceItems)
                {
                    var invoiceItem = new PurchaseInvoiceItemBuilder(this.Strategy.Transaction)
                        .WithInvoiceItemType(salesInvoiceItem.InvoiceItemType)
                        .WithAssignedUnitPrice(salesInvoiceItem.AssignedUnitPrice)
                        .WithAssignedVatRegime(salesInvoiceItem.AssignedVatRegime)
                        .WithAssignedIrpfRegime(salesInvoiceItem.AssignedIrpfRegime)
                        .WithPart(salesInvoiceItem.Product as UnifiedGood)
                        .WithSerialisedItem(salesInvoiceItem.SerialisedItem)
                        .WithQuantity(salesInvoiceItem.Quantity)
                        .WithDescription(salesInvoiceItem.Description)
                        .WithComment(salesInvoiceItem.Comment)
                        .WithInternalComment(salesInvoiceItem.InternalComment)
                        .Build();

                    purchaseInvoice.AddPurchaseInvoiceItem(invoiceItem);
                }
            }

            method.StopPropagation = true;
        }

        public void AppsWriteOff(SalesInvoiceWriteOff method)
        {
            this.SalesInvoiceState = new SalesInvoiceStates(this.Strategy.Transaction).WrittenOff;
            method.StopPropagation = true;
        }

        public void AppsReopen(SalesInvoiceReopen method)
        {
            this.SalesInvoiceState = this.PreviousSalesInvoiceState;
            method.StopPropagation = true;
        }

        public void AppsCancelInvoice(SalesInvoiceCancelInvoice method)
        {
            this.SalesInvoiceState = new SalesInvoiceStates(this.Strategy.Transaction).Cancelled;
            method.StopPropagation = true;
        }

        public SalesInvoice AppsCopy(SalesInvoiceCopy method)
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Strategy.Transaction)
                .WithPurchaseInvoice(this.PurchaseInvoice)
                .WithBilledFrom(this.BilledFrom)
                .WithAssignedBilledFromContactMechanism(this.DerivedBilledFromContactMechanism)
                .WithBilledFromContactPerson(this.BilledFromContactPerson)
                .WithBillToCustomer(this.BillToCustomer)
                .WithAssignedBillToContactMechanism(this.DerivedBillToContactMechanism)
                .WithBillToContactPerson(this.BillToContactPerson)
                .WithBillToEndCustomer(this.BillToEndCustomer)
                .WithAssignedBillToEndCustomerContactMechanism(this.DerivedBillToEndCustomerContactMechanism)
                .WithBillToEndCustomerContactPerson(this.BillToEndCustomerContactPerson)
                .WithShipToCustomer(this.ShipToCustomer)
                .WithAssignedShipToAddress(this.DerivedShipToAddress)
                .WithShipToContactPerson(this.ShipToContactPerson)
                .WithShipToEndCustomer(this.ShipToEndCustomer)
                .WithAssignedShipToEndCustomerAddress(this.DerivedShipToEndCustomerAddress)
                .WithShipToEndCustomerContactPerson(this.ShipToEndCustomerContactPerson)
                .WithDescription(this.Description)
                .WithStore(this.Store)
                .WithInvoiceDate(this.Transaction().Now())
                .WithSalesChannel(this.SalesChannel)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Strategy.Transaction).SalesInvoice)
                .WithAssignedVatRegime(this.AssignedVatRegime)
                .WithAssignedIrpfRegime(this.AssignedIrpfRegime)
                .WithCustomerReference(this.CustomerReference)
                .WithAssignedPaymentMethod(this.DerivedPaymentMethod)
                .WithComment(this.Comment)
                .WithInternalComment(this.InternalComment)
                .WithMessage(this.Message)
                .WithBillingAccount(this.BillingAccount)
                .Build();

            foreach (var orderAdjustment in this.OrderAdjustments)
            {
                OrderAdjustment newAdjustment = null;
                if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
                {
                    newAdjustment = new DiscountAdjustmentBuilder(this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                {
                    newAdjustment = new SurchargeAdjustmentBuilder(this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                {
                    newAdjustment = new FeeBuilder(this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                {
                    newAdjustment = new ShippingAndHandlingChargeBuilder(this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                {
                    newAdjustment = new MiscellaneousChargeBuilder(this.Transaction()).Build();
                }

                newAdjustment.Amount ??= orderAdjustment.Amount;
                newAdjustment.Percentage ??= orderAdjustment.Percentage;
                salesInvoice.AddOrderAdjustment(newAdjustment);
            }

            foreach (var salesInvoiceItem in this.SalesInvoiceItems)
            {
                var invoiceItem = new SalesInvoiceItemBuilder(this.Strategy.Transaction)
                    .WithInvoiceItemType(salesInvoiceItem.InvoiceItemType)
                    .WithAssignedUnitPrice(salesInvoiceItem.AssignedUnitPrice)
                    .WithAssignedVatRegime(salesInvoiceItem.AssignedVatRegime)
                    .WithAssignedIrpfRegime(salesInvoiceItem.AssignedIrpfRegime)
                    .WithProduct(salesInvoiceItem.Product)
                    .WithQuantity(salesInvoiceItem.Quantity)
                    .WithDescription(salesInvoiceItem.Description)
                    .WithSerialisedItem(salesInvoiceItem.SerialisedItem)
                    .WithNextSerialisedItemAvailability(salesInvoiceItem.NextSerialisedItemAvailability)
                    .WithComment(salesInvoiceItem.Comment)
                    .WithInternalComment(salesInvoiceItem.InternalComment)
                    .WithMessage(salesInvoiceItem.Message)
                    .WithFacility(salesInvoiceItem.Facility)
                    .Build();

                invoiceItem.ProductFeatures = salesInvoiceItem.ProductFeatures;
                salesInvoice.AddSalesInvoiceItem(invoiceItem);

                foreach (var salesTerm in salesInvoiceItem.SalesTerms)
                {
                    if (salesTerm.GetType().Name == typeof(IncoTerm).Name)
                    {
                        salesInvoiceItem.AddSalesTerm(new IncoTermBuilder(this.Strategy.Transaction)
                            .WithTermType(salesTerm.TermType)
                            .WithTermValue(salesTerm.TermValue)
                            .WithDescription(salesTerm.Description)
                            .Build());
                    }

                    if (salesTerm.GetType().Name == typeof(InvoiceTerm).Name)
                    {
                        salesInvoiceItem.AddSalesTerm(new InvoiceTermBuilder(this.Strategy.Transaction)
                            .WithTermType(salesTerm.TermType)
                            .WithTermValue(salesTerm.TermValue)
                            .WithDescription(salesTerm.Description)
                            .Build());
                    }

                    if (salesTerm.GetType().Name == typeof(OrderTerm).Name)
                    {
                        salesInvoiceItem.AddSalesTerm(new OrderTermBuilder(this.Strategy.Transaction)
                            .WithTermType(salesTerm.TermType)
                            .WithTermValue(salesTerm.TermValue)
                            .WithDescription(salesTerm.Description)
                            .Build());
                    }
                }
            }

            foreach (var salesTerm in this.SalesTerms)
            {
                if (salesTerm.GetType().Name == typeof(IncoTerm).Name)
                {
                    salesInvoice.AddSalesTerm(new IncoTermBuilder(this.Strategy.Transaction)
                                                .WithTermType(salesTerm.TermType)
                                                .WithTermValue(salesTerm.TermValue)
                                                .WithDescription(salesTerm.Description)
                                                .Build());
                }

                if (salesTerm.GetType().Name == typeof(InvoiceTerm).Name)
                {
                    salesInvoice.AddSalesTerm(new InvoiceTermBuilder(this.Strategy.Transaction)
                        .WithTermType(salesTerm.TermType)
                        .WithTermValue(salesTerm.TermValue)
                        .WithDescription(salesTerm.Description)
                        .Build());
                }

                if (salesTerm.GetType().Name == typeof(OrderTerm).Name)
                {
                    salesInvoice.AddSalesTerm(new OrderTermBuilder(this.Strategy.Transaction)
                        .WithTermType(salesTerm.TermType)
                        .WithTermValue(salesTerm.TermValue)
                        .WithDescription(salesTerm.Description)
                        .Build());
                }
            }

            return salesInvoice;
        }

        public SalesInvoice AppsCredit(SalesInvoiceCredit method)
        {
            var creditNote = new SalesInvoiceBuilder(this.Strategy.Transaction)
                .WithCreditedFromInvoice(this)
                .WithPurchaseInvoice(this.PurchaseInvoice)
                .WithBilledFrom(this.BilledFrom)
                .WithAssignedBilledFromContactMechanism(this.DerivedBilledFromContactMechanism)
                .WithBilledFromContactPerson(this.BilledFromContactPerson)
                .WithBillToCustomer(this.BillToCustomer)
                .WithAssignedBillToContactMechanism(this.DerivedBillToContactMechanism)
                .WithBillToContactPerson(this.BillToContactPerson)
                .WithBillToEndCustomer(this.BillToEndCustomer)
                .WithAssignedBillToEndCustomerContactMechanism(this.DerivedBillToEndCustomerContactMechanism)
                .WithBillToEndCustomerContactPerson(this.BillToEndCustomerContactPerson)
                .WithShipToCustomer(this.ShipToCustomer)
                .WithAssignedShipToAddress(this.DerivedShipToAddress)
                .WithShipToContactPerson(this.ShipToContactPerson)
                .WithShipToEndCustomer(this.ShipToEndCustomer)
                .WithAssignedShipToEndCustomerAddress(this.DerivedShipToEndCustomerAddress)
                .WithShipToEndCustomerContactPerson(this.ShipToEndCustomerContactPerson)
                .WithDescription(this.Description)
                .WithStore(this.Store)
                .WithInvoiceDate(this.Transaction().Now())
                .WithSalesChannel(this.SalesChannel)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Strategy.Transaction).CreditNote)
                .WithAssignedVatRegime(this.AssignedVatRegime)
                .WithAssignedIrpfRegime(this.AssignedIrpfRegime)
                .WithCustomerReference(this.CustomerReference)
                .WithAssignedPaymentMethod(this.DerivedPaymentMethod)
                .WithBillingAccount(this.BillingAccount)
                .Build();

            foreach (var orderAdjustment in this.OrderAdjustments)
            {
                OrderAdjustment newAdjustment = null;
                if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
                {
                    newAdjustment = new DiscountAdjustmentBuilder(this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                {
                    newAdjustment = new SurchargeAdjustmentBuilder(this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                {
                    newAdjustment = new FeeBuilder(this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                {
                    newAdjustment = new ShippingAndHandlingChargeBuilder(this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                {
                    newAdjustment = new MiscellaneousChargeBuilder(this.Transaction()).Build();
                }

                newAdjustment.Amount ??= orderAdjustment.Amount * -1;
                creditNote.AddOrderAdjustment(newAdjustment);
            }

            foreach (var salesInvoiceItem in this.SalesInvoiceItems)
            {
                var invoiceItem = new SalesInvoiceItemBuilder(this.Strategy.Transaction)
                    .WithInvoiceItemType(salesInvoiceItem.InvoiceItemType)
                    .WithAssignedUnitPrice(salesInvoiceItem.UnitPrice)
                    .WithProduct(salesInvoiceItem.Product)
                    .WithQuantity(salesInvoiceItem.Quantity)
                    .WithAssignedVatRegime(salesInvoiceItem.DerivedVatRegime)
                    .WithAssignedIrpfRegime(salesInvoiceItem.DerivedIrpfRegime)
                    .WithDescription(salesInvoiceItem.Description)
                    .WithSerialisedItem(salesInvoiceItem.SerialisedItem)
                    .WithComment(salesInvoiceItem.Comment)
                    .WithInternalComment(salesInvoiceItem.InternalComment)
                    .WithFacility(salesInvoiceItem.Facility)
                    .WithSerialisedItemVersionBeforeSale(salesInvoiceItem.SerialisedItemVersionBeforeSale)
                    .Build();

                invoiceItem.ProductFeatures = salesInvoiceItem.ProductFeatures;
                creditNote.AddSalesInvoiceItem(invoiceItem);

                foreach (var workEffortBilling in salesInvoiceItem.WorkEffortBillingsWhereInvoiceItem)
                {
                    new WorkEffortBillingBuilder(this.Strategy.Transaction)
                        .WithWorkEffort(workEffortBilling.WorkEffort)
                        .WithInvoiceItem(invoiceItem)
                        .Build();

                    workEffortBilling.WorkEffort.CanInvoice = true;
                }
            }

            method.StopPropagation = true;

            return creditNote;
        }

        public void AppsDelete(DeletableDelete method)
        {
            if (this.IsDeletable)
            {
                foreach (var orderAdjustment in this.OrderAdjustments)
                {
                    orderAdjustment.Delete();
                }

                foreach (var salesInvoiceItem in this.SalesInvoiceItems)
                {
                    salesInvoiceItem.Delete();
                }

                foreach (var salesTerm in this.SalesTerms)
                {
                    salesTerm.Delete();
                }
            }
        }

        public void AppsPrint(PrintablePrint method)
        {
            var singleton = this.Strategy.Transaction.GetSingleton();
            var logo = this.BilledFrom?.ExistLogoImage == true ?
                            this.BilledFrom.LogoImage.MediaContent.Data :
                            singleton.LogoImage.MediaContent.Data;

            var images = new Dictionary<string, byte[]>
                                {
                                    { "Logo", logo },
                                };

            if (this.ExistInvoiceNumber)
            {
                var transaction = this.Strategy.Transaction;
                var barcodeGenerator = transaction.Database.Services().Get<IBarcodeGenerator>();
                var barcode = barcodeGenerator.Generate(this.InvoiceNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                images.Add("Barcode", barcode);
            }

            var printModel = new Print.SalesInvoiceModel.Model(this);
            this.RenderPrintDocument(this.BilledFrom?.SalesInvoiceTemplate, printModel, images);

            this.PrintDocument.Media.InFileName = $"{this.InvoiceNumber}.odt";

            method.StopPropagation = true;
        }
    }
}
