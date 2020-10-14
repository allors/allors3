// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;
    using Resources;

    public class SalesInvoiceDerivation : DomainDerivation
    {
        public SalesInvoiceDerivation(M m) : base(m, new Guid("5F9E688C-1805-4982-87EC-CE45100BDD30")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(this.M.SalesInvoice.Class),
            new ChangedRolePattern(this.M.SalesInvoice.SalesInvoiceItems),
            new ChangedRolePattern(this.M.SalesInvoiceItem.AssignedUnitPrice) { Steps =  new IPropertyType[] {m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem} },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var salesInvoice in matches.Cast<SalesInvoice>())
            {
                var internalOrganisations = new Organisations(session).InternalOrganisations();

                if (!salesInvoice.ExistBilledFrom && internalOrganisations.Count() == 1)
                {
                    salesInvoice.BilledFrom = internalOrganisations.First();
                }

                if (!salesInvoice.ExistStore && salesInvoice.ExistBilledFrom)
                {
                    var stores = new Stores(session).Extent();
                    stores.Filter.AddEquals(this.M.Store.InternalOrganisation, salesInvoice.BilledFrom);
                    salesInvoice.Store = stores.FirstOrDefault();
                }

                if (!salesInvoice.ExistInvoiceNumber && salesInvoice.ExistStore)
                {
                    salesInvoice.InvoiceNumber = salesInvoice.Store.NextTemporaryInvoiceNumber();
                    salesInvoice.SortableInvoiceNumber = salesInvoice.Session().GetSingleton().SortableNumber(null, salesInvoice.InvoiceNumber, salesInvoice.InvoiceDate.Year.ToString());
                }

                if (!salesInvoice.ExistBilledFromContactMechanism && salesInvoice.ExistBilledFrom)
                {
                    salesInvoice.BilledFromContactMechanism = salesInvoice.BilledFrom.ExistBillingAddress ? salesInvoice.BilledFrom.BillingAddress : salesInvoice.BilledFrom.GeneralCorrespondence;
                }

                if (!salesInvoice.ExistBillToContactMechanism && salesInvoice.ExistBillToCustomer)
                {
                    salesInvoice.BillToContactMechanism = salesInvoice.BillToCustomer.BillingAddress;
                }

                if (!salesInvoice.ExistBillToEndCustomerContactMechanism && salesInvoice.ExistBillToEndCustomer)
                {
                    salesInvoice.BillToEndCustomerContactMechanism = salesInvoice.BillToEndCustomer.BillingAddress;
                }

                if (!salesInvoice.ExistShipToEndCustomerAddress && salesInvoice.ExistShipToEndCustomer)
                {
                    salesInvoice.ShipToEndCustomerAddress = salesInvoice.ShipToEndCustomer.ShippingAddress;
                }

                if (!salesInvoice.ExistShipToAddress && salesInvoice.ExistShipToCustomer)
                {
                    salesInvoice.ShipToAddress = salesInvoice.ShipToCustomer.ShippingAddress;
                }

                if (!salesInvoice.ExistCurrency && salesInvoice.ExistBilledFrom)
                {
                    if (salesInvoice.ExistBillToCustomer && (salesInvoice.BillToCustomer.ExistPreferredCurrency || salesInvoice.BillToCustomer.ExistLocale))
                    {
                        salesInvoice.Currency = salesInvoice.BillToCustomer.ExistPreferredCurrency ? salesInvoice.BillToCustomer.PreferredCurrency : salesInvoice.BillToCustomer.Locale.Country.Currency;
                    }
                    else
                    {
                        salesInvoice.Currency = salesInvoice.BilledFrom.ExistPreferredCurrency ?
                            salesInvoice.BilledFrom.PreferredCurrency :
                            session.GetSingleton().DefaultLocale.Country.Currency;
                    }
                }

                salesInvoice.VatRegime ??= salesInvoice.BillToCustomer?.VatRegime;
                salesInvoice.IrpfRegime ??= salesInvoice.BillToCustomer?.IrpfRegime;
                salesInvoice.IsRepeating = salesInvoice.ExistRepeatingSalesInvoiceWhereSource;

                foreach (SalesInvoiceItem salesInvoiceItem in salesInvoice.SalesInvoiceItems)
                {
                    foreach (OrderItemBilling orderItemBilling in salesInvoiceItem.OrderItemBillingsWhereInvoiceItem)
                    {
                        if (orderItemBilling.OrderItem is SalesOrderItem salesOrderItem && !salesInvoice.SalesOrders.Contains(salesOrderItem.SalesOrderWhereSalesOrderItem))
                        {
                            salesInvoice.AddSalesOrder(salesOrderItem.SalesOrderWhereSalesOrderItem);
                        }
                    }

                    foreach (WorkEffortBilling workEffortBilling in salesInvoiceItem.WorkEffortBillingsWhereInvoiceItem)
                    {
                        if (!salesInvoice.WorkEfforts.Contains(workEffortBilling.WorkEffort))
                        {
                            salesInvoice.AddWorkEffort(workEffortBilling.WorkEffort);
                        }
                    }

                    foreach (TimeEntryBilling timeEntryBilling in salesInvoiceItem.TimeEntryBillingsWhereInvoiceItem)
                    {
                        if (!salesInvoice.WorkEfforts.Contains(timeEntryBilling.TimeEntry.WorkEffort))
                        {
                            salesInvoice.AddWorkEffort(timeEntryBilling.TimeEntry.WorkEffort);
                        }
                    }
                }

                salesInvoice.IsRepeatingInvoice = salesInvoice.ExistRepeatingSalesInvoiceWhereSource && (!salesInvoice.RepeatingSalesInvoiceWhereSource.ExistFinalExecutionDate || salesInvoice.RepeatingSalesInvoiceWhereSource.FinalExecutionDate.Value.Date >= salesInvoice.Strategy.Session.Now().Date);

                salesInvoice.SalesInvoiceItems = salesInvoice.SalesInvoiceItems.ToArray();

                if (salesInvoice.ExistBillToCustomer && salesInvoice.BillToCustomer.ExistLocale)
                {
                    salesInvoice.Locale = salesInvoice.BillToCustomer.Locale;
                }
                else
                {
                    salesInvoice.Locale = session.GetSingleton().DefaultLocale;
                }

                salesInvoice.PaymentDays = salesInvoice.PaymentNetDays;

                if (!salesInvoice.ExistPaymentDays)
                {
                    salesInvoice.PaymentDays = 0;
                }

                if (salesInvoice.ExistInvoiceDate)
                {
                    salesInvoice.DueDate = salesInvoice.InvoiceDate.AddDays(salesInvoice.PaymentNetDays);
                }

                var validInvoiceItems = salesInvoice.SalesInvoiceItems.Where(v => v.IsValid).ToArray();
                salesInvoice.ValidInvoiceItems = validInvoiceItems;

                var currentPriceComponents = new PriceComponents(salesInvoice.Strategy.Session).CurrentPriceComponents(salesInvoice.InvoiceDate);

                var quantityByProduct = validInvoiceItems
                    .Where(v => v.ExistProduct)
                    .GroupBy(v => v.Product)
                    .ToDictionary(v => v.Key, v => v.Sum(w => w.Quantity));

                // First run to calculate price
                foreach (var salesInvoiceItem in validInvoiceItems)
                {
                    decimal quantityOrdered = 0;

                    if (salesInvoiceItem.ExistProduct)
                    {
                        quantityByProduct.TryGetValue(salesInvoiceItem.Product, out quantityOrdered);
                    }

                    CalculatePrices(salesInvoice, salesInvoiceItem, currentPriceComponents, quantityOrdered, 0);
                }

                var totalBasePriceByProduct = validInvoiceItems
                    .Where(v => v.ExistProduct)
                    .GroupBy(v => v.Product)
                    .ToDictionary(v => v.Key, v => v.Sum(w => w.TotalBasePrice));

                // Second run to calculate price (because of order value break)
                foreach (var salesInvoiceItem in validInvoiceItems)
                {
                    decimal quantityOrdered = 0;
                    decimal totalBasePrice = 0;

                    if (salesInvoiceItem.ExistProduct)
                    {
                        quantityByProduct.TryGetValue(salesInvoiceItem.Product, out quantityOrdered);
                        totalBasePriceByProduct.TryGetValue(salesInvoiceItem.Product, out totalBasePrice);
                    }

                    CalculatePrices(salesInvoice, salesInvoiceItem, currentPriceComponents, quantityOrdered, totalBasePrice);
                }

                // Calculate Totals
                salesInvoice.TotalBasePrice = 0;
                salesInvoice.TotalDiscount = 0;
                salesInvoice.TotalSurcharge = 0;
                salesInvoice.TotalExVat = 0;
                salesInvoice.TotalFee = 0;
                salesInvoice.TotalShippingAndHandling = 0;
                salesInvoice.TotalExtraCharge = 0;
                salesInvoice.TotalVat = 0;
                salesInvoice.TotalIrpf = 0;
                salesInvoice.TotalIncVat = 0;
                salesInvoice.TotalListPrice = 0;
                salesInvoice.TotalIrpf = 0;
                salesInvoice.GrandTotal = 0;

                foreach (var item in validInvoiceItems)
                {
                    salesInvoice.TotalBasePrice += item.TotalBasePrice;
                    salesInvoice.TotalDiscount += item.TotalDiscount;
                    salesInvoice.TotalSurcharge += item.TotalSurcharge;
                    salesInvoice.TotalExVat += item.TotalExVat;
                    salesInvoice.TotalVat += item.TotalVat;
                    salesInvoice.TotalIrpf += item.TotalIrpf;
                    salesInvoice.TotalIncVat += item.TotalIncVat;
                    salesInvoice.TotalListPrice += item.TotalExVat;
                    salesInvoice.GrandTotal += item.GrandTotal;
                }

                var discount = 0M;
                var discountVat = 0M;
                var discountIrpf = 0M;
                var surcharge = 0M;
                var surchargeVat = 0M;
                var surchargeIrpf = 0M;
                var fee = 0M;
                var feeVat = 0M;
                var feeIrpf = 0M;
                var shipping = 0M;
                var shippingVat = 0M;
                var shippingIrpf = 0M;
                var miscellaneous = 0M;
                var miscellaneousVat = 0M;
                var miscellaneousIrpf = 0M;

                foreach (OrderAdjustment orderAdjustment in salesInvoice.OrderAdjustments)
                {
                    if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
                    {
                        discount = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(salesInvoice.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        salesInvoice.TotalDiscount += discount;

                        if (salesInvoice.ExistVatRegime)
                        {
                            discountVat = Math.Round(discount * salesInvoice.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (salesInvoice.ExistIrpfRegime)
                        {
                            discountIrpf = Math.Round(discount * salesInvoice.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                    {
                        surcharge = orderAdjustment.Percentage.HasValue ?
                                            Math.Round(salesInvoice.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                            orderAdjustment.Amount ?? 0;

                        salesInvoice.TotalSurcharge += surcharge;

                        if (salesInvoice.ExistVatRegime)
                        {
                            surchargeVat = Math.Round(surcharge * salesInvoice.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (salesInvoice.ExistIrpfRegime)
                        {
                            surchargeIrpf = Math.Round(surcharge * salesInvoice.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                    {
                        fee = orderAdjustment.Percentage.HasValue ?
                                    Math.Round(salesInvoice.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                    orderAdjustment.Amount ?? 0;

                        salesInvoice.TotalFee += fee;

                        if (salesInvoice.ExistVatRegime)
                        {
                            feeVat = Math.Round(fee * salesInvoice.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (salesInvoice.ExistIrpfRegime)
                        {
                            feeIrpf = Math.Round(fee * salesInvoice.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                    {
                        shipping = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(salesInvoice.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        salesInvoice.TotalShippingAndHandling += shipping;

                        if (salesInvoice.ExistVatRegime)
                        {
                            shippingVat = Math.Round(shipping * salesInvoice.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (salesInvoice.ExistIrpfRegime)
                        {
                            shippingIrpf = Math.Round(shipping * salesInvoice.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                    {
                        miscellaneous = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(salesInvoice.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        salesInvoice.TotalExtraCharge += miscellaneous;

                        if (salesInvoice.ExistVatRegime)
                        {
                            miscellaneousVat = Math.Round(miscellaneous * salesInvoice.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (salesInvoice.ExistIrpfRegime)
                        {
                            miscellaneousIrpf = Math.Round(miscellaneous * salesInvoice.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }
                }

                salesInvoice.TotalExtraCharge = fee + shipping + miscellaneous;

                salesInvoice.TotalExVat = salesInvoice.TotalExVat - discount + surcharge + fee + shipping + miscellaneous;
                salesInvoice.TotalVat = salesInvoice.TotalVat - discountVat + surchargeVat + feeVat + shippingVat + miscellaneousVat;
                salesInvoice.TotalIncVat = salesInvoice.TotalIncVat - discount - discountVat + surcharge + surchargeVat + fee + feeVat + shipping + shippingVat + miscellaneous + miscellaneousVat;
                salesInvoice.TotalIrpf = salesInvoice.TotalIrpf + discountIrpf - surchargeIrpf - feeIrpf - shippingIrpf - miscellaneousIrpf;
                salesInvoice.GrandTotal = salesInvoice.TotalIncVat - salesInvoice.TotalIrpf;

                //// Only take into account items for which there is data at the item level.
                //// Skip negative sales.
                decimal totalUnitBasePrice = 0;
                decimal totalListPrice = 0;

                foreach (var item1 in validInvoiceItems)
                {
                    if (item1.TotalExVat > 0)
                    {
                        totalUnitBasePrice += item1.UnitBasePrice;
                        totalListPrice += item1.UnitPrice;
                    }
                }

                var salesInvoiceItemStates = new SalesInvoiceItemStates(session);
                var salesInvoiceStates = new SalesInvoiceStates(session);

                foreach (var invoiceItem in validInvoiceItems)
                {
                    if (!invoiceItem.SalesInvoiceItemState.Equals(salesInvoiceItemStates.ReadyForPosting))
                    {
                        if (invoiceItem.AmountPaid == 0)
                        {
                            invoiceItem.SalesInvoiceItemState = salesInvoiceItemStates.NotPaid;
                        }
                        else if (invoiceItem.ExistAmountPaid && invoiceItem.AmountPaid > 0 && invoiceItem.AmountPaid >= invoiceItem.TotalIncVat)
                        {
                            invoiceItem.SalesInvoiceItemState = salesInvoiceItemStates.Paid;
                        }
                        else
                        {
                            invoiceItem.SalesInvoiceItemState = salesInvoiceItemStates.PartiallyPaid;
                        }
                    }
                }

                if (validInvoiceItems.Any() && !salesInvoice.SalesInvoiceState.Equals(salesInvoiceStates.ReadyForPosting))
                {
                    if (salesInvoice.SalesInvoiceItems.All(v => v.SalesInvoiceItemState.IsPaid))
                    {
                        salesInvoice.SalesInvoiceState = salesInvoiceStates.Paid;
                    }
                    else if (salesInvoice.SalesInvoiceItems.All(v => v.SalesInvoiceItemState.IsNotPaid))
                    {
                        salesInvoice.SalesInvoiceState = salesInvoiceStates.NotPaid;
                    }
                    else
                    {
                        salesInvoice.SalesInvoiceState = salesInvoiceStates.PartiallyPaid;
                    }
                }

                salesInvoice.AmountPaid = salesInvoice.AdvancePayment;
                salesInvoice.AmountPaid += salesInvoice.PaymentApplicationsWhereInvoice.Sum(v => v.AmountApplied);

                //// Perhaps payments are recorded at the item level.
                if (salesInvoice.AmountPaid == 0)
                {
                    salesInvoice.AmountPaid = salesInvoice.InvoiceItems.Sum(v => v.AmountPaid);
                }

                // If receipts are not matched at invoice level
                // if only advancedPayment is received do not set to partially paid
                // this would disable the invoice for editing and adding new items
                if (salesInvoice.AmountPaid - salesInvoice.AdvancePayment > 0)
                {
                    if (salesInvoice.AmountPaid >= salesInvoice.TotalIncVat)
                    {
                        salesInvoice.SalesInvoiceState = salesInvoiceStates.Paid;
                    }
                    else
                    {
                        if (salesInvoice.AmountPaid > 0)
                        {
                            salesInvoice.SalesInvoiceState = salesInvoiceStates.PartiallyPaid;
                        }
                    }

                    foreach (var invoiceItem in validInvoiceItems)
                    {
                        if (!invoiceItem.SalesInvoiceItemState.Equals(salesInvoiceItemStates.CancelledByInvoice) &&
                            !invoiceItem.SalesInvoiceItemState.Equals(salesInvoiceItemStates.WrittenOff))
                        {
                            if (salesInvoice.AmountPaid >= salesInvoice.TotalIncVat)
                            {
                                invoiceItem.SalesInvoiceItemState = salesInvoiceItemStates.Paid;
                            }
                            else
                            {
                                invoiceItem.SalesInvoiceItemState = salesInvoiceItemStates.PartiallyPaid;
                            }
                        }
                    }
                }

                if (salesInvoice.ExistVatRegime && salesInvoice.VatRegime.ExistVatClause)
                {
                    salesInvoice.DerivedVatClause = salesInvoice.VatRegime.VatClause;
                }
                else
                {
                    if (Equals(salesInvoice.VatRegime, new VatRegimes(session).ServiceB2B))
                    {
                        salesInvoice.DerivedVatClause = new VatClauses(session).ServiceB2B;
                    }
                    else if (Equals(salesInvoice.VatRegime, new VatRegimes(session).IntraCommunautair))
                    {
                        salesInvoice.DerivedVatClause = new VatClauses(session).Intracommunautair;
                    }
                }

                salesInvoice.DerivedVatClause = salesInvoice.ExistAssignedVatClause ? salesInvoice.AssignedVatClause : salesInvoice.DerivedVatClause;

                salesInvoice.RemoveCustomers();
                if (salesInvoice.ExistBillToCustomer && !salesInvoice.Customers.Contains(salesInvoice.BillToCustomer))
                {
                    salesInvoice.AddCustomer(salesInvoice.BillToCustomer);
                }

                if (salesInvoice.ExistShipToCustomer && !salesInvoice.Customers.Contains(salesInvoice.ShipToCustomer))
                {
                    salesInvoice.AddCustomer(salesInvoice.ShipToCustomer);
                }

                if (salesInvoice.ExistBillToCustomer && !salesInvoice.BillToCustomer.BaseIsActiveCustomer(salesInvoice.BilledFrom, salesInvoice.InvoiceDate))
                {
                    validation.AddError($"{this}  {this.M.SalesInvoice.BillToCustomer} {ErrorMessages.PartyIsNotACustomer}");
                }

                if (salesInvoice.ExistShipToCustomer && !salesInvoice.ShipToCustomer.BaseIsActiveCustomer(salesInvoice.BilledFrom, salesInvoice.InvoiceDate))
                {
                    validation.AddError($"{this} {this.M.SalesInvoice.ShipToCustomer} {ErrorMessages.PartyIsNotACustomer}");
                }

                salesInvoice.PreviousBillToCustomer = salesInvoice.BillToCustomer;
                salesInvoice.PreviousShipToCustomer = salesInvoice.ShipToCustomer;

                // this.BaseOnDeriveRevenues(derivation);
                var singleton = salesInvoice.Session().GetSingleton();

                salesInvoice.AddSecurityToken(new SecurityTokens(salesInvoice.Session()).DefaultSecurityToken);

                foreach (SalesInvoiceItem invoiceItem in salesInvoice.SalesInvoiceItems)
                {
                    invoiceItem.Sync(salesInvoice);
                }

                salesInvoice.ResetPrintDocument();

                var deletePermission = new Permissions(salesInvoice.Strategy.Session).Get(salesInvoice.Meta.ObjectType, salesInvoice.Meta.Delete);
                if (salesInvoice.SalesInvoiceState.Equals(new SalesInvoiceStates(salesInvoice.Strategy.Session).ReadyForPosting) &&
                    salesInvoice.SalesInvoiceItems.All(v => v.IsDeletable) &&
                    !salesInvoice.ExistSalesOrders &&
                    !salesInvoice.ExistPurchaseInvoice &&
                    !salesInvoice.ExistRepeatingSalesInvoiceWhereSource &&
                    !salesInvoice.IsRepeatingInvoice)
                {
                    salesInvoice.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    salesInvoice.AddDeniedPermission(deletePermission);
                }
            }

            void CalculatePrices(
                SalesInvoice salesInvoice,
                SalesInvoiceItem salesInvoiceItem,
                PriceComponent[] currentPriceComponents,
                decimal quantityOrdered,
                decimal totalBasePrice)
            {
                var salesInvoiceItemDerivedRoles = salesInvoiceItem;

                var currentGenericOrProductOrFeaturePriceComponents = new List<PriceComponent>();
                if (salesInvoiceItem.ExistProduct)
                {
                    currentGenericOrProductOrFeaturePriceComponents.AddRange(salesInvoiceItem.Product.GetPriceComponents(currentPriceComponents));
                }

                foreach (ProductFeature productFeature in salesInvoiceItem.ProductFeatures)
                {
                    currentGenericOrProductOrFeaturePriceComponents.AddRange(productFeature.GetPriceComponents(salesInvoiceItem.Product, currentPriceComponents));
                }

                var priceComponents = currentGenericOrProductOrFeaturePriceComponents.Where(
                    v => PriceComponents.BaseIsApplicable(
                        new PriceComponents.IsApplicable
                        {
                            PriceComponent = v,
                            Customer = salesInvoice.BillToCustomer,
                            Product = salesInvoiceItem.Product,
                            SalesInvoice = salesInvoice,
                            QuantityOrdered = quantityOrdered,
                            ValueOrdered = totalBasePrice,
                        })).ToArray();

                var unitBasePrice = priceComponents.OfType<BasePrice>().Min(v => v.Price);

                // Calculate Unit Price (with Discounts and Surcharges)
                if (salesInvoiceItem.AssignedUnitPrice.HasValue)
                {
                    salesInvoiceItemDerivedRoles.UnitBasePrice = unitBasePrice ?? salesInvoiceItem.AssignedUnitPrice.Value;
                    salesInvoiceItemDerivedRoles.UnitDiscount = 0;
                    salesInvoiceItemDerivedRoles.UnitSurcharge = 0;
                    salesInvoiceItemDerivedRoles.UnitPrice = salesInvoiceItem.AssignedUnitPrice.Value;
                }
                else
                {
                    if (!unitBasePrice.HasValue)
                    {
                        validation.AddError($"{salesInvoiceItem}, {this.M.SalesOrderItem.UnitBasePrice} No BasePrice with a Price");
                        return;
                    }

                    salesInvoiceItemDerivedRoles.UnitBasePrice = unitBasePrice.Value;

                    salesInvoiceItemDerivedRoles.UnitDiscount = priceComponents.OfType<DiscountComponent>().Sum(
                        v => v.Percentage.HasValue
                                 ? Math.Round(salesInvoiceItem.UnitBasePrice * v.Percentage.Value / 100, 2)
                                 : v.Price ?? 0);

                    salesInvoiceItemDerivedRoles.UnitSurcharge = priceComponents.OfType<SurchargeComponent>().Sum(
                        v => v.Percentage.HasValue
                                 ? Math.Round(salesInvoiceItem.UnitBasePrice * v.Percentage.Value / 100, 2)
                                 : v.Price ?? 0);

                    salesInvoiceItemDerivedRoles.UnitPrice = salesInvoiceItem.UnitBasePrice - salesInvoiceItem.UnitDiscount + salesInvoiceItem.UnitSurcharge;

                    foreach (OrderAdjustment orderAdjustment in salesInvoiceItem.DiscountAdjustments)
                    {
                        salesInvoiceItemDerivedRoles.UnitDiscount += orderAdjustment.Percentage.HasValue ?
                            Math.Round(salesInvoiceItem.UnitPrice * orderAdjustment.Percentage.Value / 100, 2) :
                            orderAdjustment.Amount ?? 0;
                    }

                    foreach (OrderAdjustment orderAdjustment in salesInvoiceItem.SurchargeAdjustments)
                    {
                        salesInvoiceItemDerivedRoles.UnitSurcharge += orderAdjustment.Percentage.HasValue ?
                            Math.Round(salesInvoiceItem.UnitPrice * orderAdjustment.Percentage.Value / 100, 2) :
                            orderAdjustment.Amount ?? 0;
                    }

                    salesInvoiceItemDerivedRoles.UnitPrice = salesInvoiceItem.UnitBasePrice - salesInvoiceItem.UnitDiscount + salesInvoiceItem.UnitSurcharge;
                }

                salesInvoiceItemDerivedRoles.UnitVat = salesInvoiceItem.ExistVatRate ? salesInvoiceItem.UnitPrice * salesInvoiceItem.VatRate.Rate / 100 : 0;
                salesInvoiceItemDerivedRoles.UnitIrpf = salesInvoiceItem.ExistIrpfRate ? salesInvoiceItem.UnitPrice * salesInvoiceItem.IrpfRate.Rate / 100 : 0;

                // Calculate Totals
                salesInvoiceItemDerivedRoles.TotalBasePrice = salesInvoiceItem.UnitBasePrice * salesInvoiceItem.Quantity;
                salesInvoiceItemDerivedRoles.TotalDiscount = salesInvoiceItem.UnitDiscount * salesInvoiceItem.Quantity;
                salesInvoiceItemDerivedRoles.TotalSurcharge = salesInvoiceItem.UnitSurcharge * salesInvoiceItem.Quantity;

                if (salesInvoiceItem.TotalBasePrice > 0)
                {
                    salesInvoiceItemDerivedRoles.TotalDiscountAsPercentage = Math.Round(salesInvoiceItem.TotalDiscount / salesInvoiceItem.TotalBasePrice * 100, 2);
                    salesInvoiceItemDerivedRoles.TotalSurchargeAsPercentage = Math.Round(salesInvoiceItem.TotalSurcharge / salesInvoiceItem.TotalBasePrice * 100, 2);
                }
                else
                {
                    salesInvoiceItemDerivedRoles.TotalDiscountAsPercentage = 0;
                    salesInvoiceItemDerivedRoles.TotalSurchargeAsPercentage = 0;
                }

                salesInvoiceItemDerivedRoles.TotalExVat = salesInvoiceItem.UnitPrice * salesInvoiceItem.Quantity;
                salesInvoiceItemDerivedRoles.TotalVat = salesInvoiceItem.UnitVat * salesInvoiceItem.Quantity;
                salesInvoiceItemDerivedRoles.TotalIncVat = salesInvoiceItem.TotalExVat + salesInvoiceItem.TotalVat;
                salesInvoiceItemDerivedRoles.TotalIrpf = salesInvoiceItem.UnitIrpf * salesInvoiceItem.Quantity;
                salesInvoiceItemDerivedRoles.GrandTotal = salesInvoiceItem.TotalIncVat - salesInvoiceItem.TotalIrpf;
            }
        }
    }
}
