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
    using Resources;

    public class SalesInvoiceInvoiceNumberRule : Rule
    {
        public SalesInvoiceInvoiceNumberRule(MetaPopulation m) : base(m, new Guid("b9e4fb13-5b4e-4c69-80c3-907cc8edc5db")) =>
            this.Patterns = new Pattern[]
        {
            m.SalesInvoice.RolePattern(v => v.PreviousSalesInvoiceState),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SalesInvoice>()
                .Where(v => v.SalesInvoiceState.IsNotPaid
                        && v.ExistPreviousSalesInvoiceState
                        && v.PreviousSalesInvoiceState.IsReadyForPosting))
            {
                var singleton = transaction.GetSingleton();
                var year = @this.InvoiceDate.Year;

                if (Equals(@this.SalesInvoiceType, new SalesInvoiceTypes(transaction).SalesInvoice))
                {
                    var lastInvoiceNumber = @this.Store.LastSalesInvoiceNumber(@this.InvoiceDate.Year);
                    var lastInvoice = new SalesInvoices(transaction).FindBy(this.M.SalesInvoice.InvoiceNumber, lastInvoiceNumber);
                    if (@this.InvoiceDate < lastInvoice?.InvoiceDate)
                    {
                        validation.AddError(@this, @this.Meta.InvoiceDate, ErrorMessages.InvalidInvoiceDate);
                    }

                    @this.InvoiceNumber = @this.Store.NextSalesInvoiceNumber(@this.InvoiceDate.Year);

                    var fiscalYearStoreSequenceNumbers = @this.Store.FiscalYearsStoreSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = @this.BilledFrom.InvoiceSequence.IsEnforcedSequence ? @this.Store.SalesInvoiceNumberPrefix : fiscalYearStoreSequenceNumbers.SalesInvoiceNumberPrefix;
                    @this.SortableInvoiceNumber = singleton.SortableNumber(prefix, @this.InvoiceNumber, year.ToString());

                    @this.FinalInvoiceNumber = @this.InvoiceNumber;
                }

                if (Equals(@this.SalesInvoiceType, new SalesInvoiceTypes(transaction).CreditNote))
                {
                    // After reopen/revise of a credit note, use the number that was issued on initial Send action
                    if (@this.ExistFinalInvoiceNumber)
                    {
                        @this.InvoiceNumber = @this.FinalInvoiceNumber;
                    }
                    else
                    {
                        @this.InvoiceNumber = @this.Store.NextCreditNoteNumber(@this.InvoiceDate.Year);

                        var fiscalYearStoreSequenceNumbers = @this.Store.FiscalYearsStoreSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                        var prefix = @this.BilledFrom.InvoiceSequence.IsEnforcedSequence ? @this.Store.CreditNoteNumberPrefix : fiscalYearStoreSequenceNumbers.CreditNoteNumberPrefix;
                        @this.SortableInvoiceNumber = singleton.SortableNumber(prefix, @this.InvoiceNumber, year.ToString());

                        @this.FinalInvoiceNumber = @this.InvoiceNumber;
                    }
                }

                foreach (var salesInvoiceItem in @this.SalesInvoiceItems)
                {
                    salesInvoiceItem.SalesInvoiceItemState = new SalesInvoiceItemStates(transaction).NotPaid;
                    salesInvoiceItem.DerivationTrigger = Guid.NewGuid();

                    if (@this.SalesInvoiceType.Equals(new SalesInvoiceTypes(transaction).SalesInvoice)
                        && salesInvoiceItem.ExistSerialisedItem
                        && ((@this.BillToCustomer as InternalOrganisation)?.IsInternalOrganisation == false
                            || @this.BillToCustomer.GetType().Name.Equals(typeof(Person).Name))
                        && @this.BilledFrom.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(transaction).SalesInvoiceSend))
                    {
                        if (salesInvoiceItem.NextSerialisedItemAvailability?.Equals(new SerialisedItemAvailabilities(transaction).Sold) == true)
                        {
                            salesInvoiceItem.SerialisedItemVersionBeforeSale = salesInvoiceItem.SerialisedItem.CurrentVersion;

                            salesInvoiceItem.SerialisedItem.Seller = @this.BilledFrom;
                            salesInvoiceItem.SerialisedItem.OwnedBy = @this.BillToCustomer;
                            salesInvoiceItem.SerialisedItem.Ownership = new Ownerships(transaction).ThirdParty;
                            salesInvoiceItem.SerialisedItem.SerialisedItemAvailability = salesInvoiceItem.NextSerialisedItemAvailability;
                            salesInvoiceItem.SerialisedItem.AvailableForSale = false;
                        }

                        if (salesInvoiceItem.NextSerialisedItemAvailability?.Equals(new SerialisedItemAvailabilities(transaction).InRent) == true)
                        {
                            salesInvoiceItem.SerialisedItem.RentedBy = @this.BillToCustomer;
                            salesInvoiceItem.SerialisedItem.SerialisedItemAvailability = salesInvoiceItem.NextSerialisedItemAvailability;
                            salesInvoiceItem.SerialisedItem.AvailableForSale = false;
                        }
                    }

                    if (@this.SalesInvoiceType.Equals(new SalesInvoiceTypes(transaction).CreditNote)
                        && salesInvoiceItem.ExistSerialisedItem
                        && salesInvoiceItem.ExistSerialisedItemVersionBeforeSale
                        && ((@this.BillToCustomer as InternalOrganisation)?.IsInternalOrganisation == false
                            || @this.BillToCustomer.GetType().Name.Equals(typeof(Person).Name))
                        && @this.BilledFrom.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(transaction).SalesInvoiceSend))
                    {
                        salesInvoiceItem.SerialisedItem.Seller = salesInvoiceItem.SerialisedItemVersionBeforeSale.Seller;
                        salesInvoiceItem.SerialisedItem.OwnedBy = salesInvoiceItem.SerialisedItemVersionBeforeSale.OwnedBy;
                        salesInvoiceItem.SerialisedItem.Ownership = salesInvoiceItem.SerialisedItemVersionBeforeSale.Ownership;
                        salesInvoiceItem.SerialisedItem.SerialisedItemAvailability = salesInvoiceItem.SerialisedItemVersionBeforeSale.SerialisedItemAvailability;
                        salesInvoiceItem.SerialisedItem.AvailableForSale = salesInvoiceItem.SerialisedItemVersionBeforeSale.AvailableForSale;
                    }
                }

                if (@this.BillToCustomer is Organisation organisation && organisation.IsInternalOrganisation)
                {
                    var purchaseInvoice = new PurchaseInvoiceBuilder(transaction)
                        .WithBilledFrom((Organisation)@this.BilledFrom)
                        .WithBilledFromContactPerson(@this.BilledFromContactPerson)
                        .WithBilledTo((InternalOrganisation)@this.BillToCustomer)
                        .WithBilledToContactPerson(@this.BillToContactPerson)
                        .WithBillToEndCustomer(@this.BillToEndCustomer)
                        .WithAssignedBillToEndCustomerContactMechanism(@this.DerivedBillToEndCustomerContactMechanism)
                        .WithBillToEndCustomerContactPerson(@this.BillToEndCustomerContactPerson)
                        .WithAssignedBillToCustomerPaymentMethod(@this.DerivedPaymentMethod)
                        .WithShipToCustomer(@this.ShipToCustomer)
                        .WithAssignedShipToCustomerAddress(@this.DerivedShipToAddress)
                        .WithShipToCustomerContactPerson(@this.ShipToContactPerson)
                        .WithShipToEndCustomer(@this.ShipToEndCustomer)
                        .WithAssignedShipToEndCustomerAddress(@this.DerivedShipToEndCustomerAddress)
                        .WithShipToEndCustomerContactPerson(@this.ShipToEndCustomerContactPerson)
                        .WithDescription(@this.Description)
                        .WithInvoiceDate(@this.Transaction().Now())
                        .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(transaction).PurchaseInvoice)
                        .WithCustomerReference(@this.CustomerReference)
                        .WithComment(@this.Comment)
                        .WithInternalComment(@this.InternalComment)
                        .Build();

                    foreach (var orderAdjustment in @this.OrderAdjustments)
                    {
                        OrderAdjustment newAdjustment = null;
                        if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
                        {
                            newAdjustment = new DiscountAdjustmentBuilder(transaction).Build();
                        }

                        if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                        {
                            newAdjustment = new SurchargeAdjustmentBuilder(transaction).Build();
                        }

                        if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                        {
                            newAdjustment = new FeeBuilder(transaction).Build();
                        }

                        if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                        {
                            newAdjustment = new ShippingAndHandlingChargeBuilder(transaction).Build();
                        }

                        if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                        {
                            newAdjustment = new MiscellaneousChargeBuilder(transaction).Build();
                        }

                        newAdjustment.Amount ??= orderAdjustment.Amount;
                        newAdjustment.Percentage ??= orderAdjustment.Percentage;
                        purchaseInvoice.AddOrderAdjustment(newAdjustment);
                    }

                    foreach (var salesInvoiceItem in @this.SalesInvoiceItems)
                    {
                        var nonUnifiedGood = salesInvoiceItem.Product as NonUnifiedGood;
                        var unifiedGood = salesInvoiceItem.Product as UnifiedGood;
                        var nonUnifiedPart = salesInvoiceItem.Product as NonUnifiedPart;
                        var part = unifiedGood ?? nonUnifiedGood?.Part ?? nonUnifiedPart;

                        var invoiceItem = new PurchaseInvoiceItemBuilder(transaction)
                            .WithInvoiceItemType(salesInvoiceItem.InvoiceItemType)
                            .WithAssignedUnitPrice(salesInvoiceItem.AssignedUnitPrice)
                            .WithAssignedVatRegime(salesInvoiceItem.AssignedVatRegime)
                            .WithAssignedIrpfRegime(salesInvoiceItem.AssignedIrpfRegime)
                            .WithPart(part)
                            .WithSerialisedItem(salesInvoiceItem.SerialisedItem)
                            .WithQuantity(salesInvoiceItem.Quantity)
                            .WithDescription(salesInvoiceItem.Description)
                            .WithComment(salesInvoiceItem.Comment)
                            .WithInternalComment(salesInvoiceItem.InternalComment)
                            .Build();

                        purchaseInvoice.AddPurchaseInvoiceItem(invoiceItem);
                    }
                }
            }
        }
    }
}
