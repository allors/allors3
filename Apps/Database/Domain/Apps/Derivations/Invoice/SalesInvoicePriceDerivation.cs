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

    public class SalesInvoicePriceDerivation : DomainDerivation
    {
        public SalesInvoicePriceDerivation(M m) : base(m, new Guid("9f3497f0-b48c-453f-800d-209edc0de7f5")) =>
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
                    v => PriceComponents.AppsIsApplicable(
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
