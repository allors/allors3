// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class SalesInvoicePriceRule : Rule
    {
        public SalesInvoicePriceRule(M m) : base(m, new Guid("9f3497f0-b48c-453f-800d-209edc0de7f5")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.SalesInvoice, m.SalesInvoice.SalesInvoiceState),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.DerivationTrigger),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.ValidInvoiceItems),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.BillToCustomer),
            new RolePattern(m.SalesInvoice, m.SalesInvoice.OrderAdjustments),
            new RolePattern(m.SalesInvoiceItem, m.SalesInvoiceItem.Product) { Steps =  new IPropertyType[] {m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem} },
            new RolePattern(m.SalesInvoiceItem, m.SalesInvoiceItem.ProductFeatures) { Steps =  new IPropertyType[] {m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem} },
            new RolePattern(m.SalesInvoiceItem, m.SalesInvoiceItem.Quantity) { Steps =  new IPropertyType[] {m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem} },
            new RolePattern(m.SalesInvoiceItem, m.SalesInvoiceItem.AssignedUnitPrice) { Steps =  new IPropertyType[] {m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem} },
            new RolePattern(m.SalesInvoiceItem, m.SalesInvoiceItem.DiscountAdjustments) { Steps =  new IPropertyType[] {m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem} },
            new RolePattern(m.DiscountAdjustment, m.DiscountAdjustment.Percentage) { Steps =  new IPropertyType[] {m.DiscountAdjustment.PriceableWhereDiscountAdjustment, m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem}, OfType = m.SalesInvoice.Class },
            new RolePattern(m.DiscountAdjustment, m.DiscountAdjustment.Percentage) { Steps =  new IPropertyType[] {m.OrderAdjustment.InvoiceWhereOrderAdjustment}, OfType = m.SalesInvoice.Class },
            new RolePattern(m.DiscountAdjustment, m.DiscountAdjustment.Amount) { Steps =  new IPropertyType[] {m.DiscountAdjustment.PriceableWhereDiscountAdjustment, m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem}, OfType = m.SalesInvoice.Class },
            new RolePattern(m.DiscountAdjustment, m.DiscountAdjustment.Amount) { Steps =  new IPropertyType[] {m.OrderAdjustment.InvoiceWhereOrderAdjustment}, OfType = m.SalesInvoice.Class },
            new RolePattern(m.SalesInvoiceItem, m.SalesInvoiceItem.SurchargeAdjustments) { Steps =  new IPropertyType[] {m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem} },
            new RolePattern(m.SurchargeAdjustment, m.SurchargeAdjustment.Percentage) { Steps =  new IPropertyType[] {m.SurchargeAdjustment.PriceableWhereSurchargeAdjustment, m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem}, OfType = m.SalesInvoice.Class },
            new RolePattern(m.SurchargeAdjustment, m.SurchargeAdjustment.Percentage) { Steps =  new IPropertyType[] {m.OrderAdjustment.InvoiceWhereOrderAdjustment}, OfType = m.SalesInvoice.Class },
            new RolePattern(m.SurchargeAdjustment, m.SurchargeAdjustment.Amount) { Steps =  new IPropertyType[] {m.SurchargeAdjustment.PriceableWhereSurchargeAdjustment, m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem}, OfType = m.SalesInvoice.Class },
            new RolePattern(m.SurchargeAdjustment, m.SurchargeAdjustment.Amount) { Steps =  new IPropertyType[] {m.OrderAdjustment.InvoiceWhereOrderAdjustment}, OfType = m.SalesInvoice.Class },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            // calculate prices only if salesinvoice is not posted yet.
            foreach (var @this in matches.Cast<SalesInvoice>().Where(v => v.ExistSalesInvoiceState && v.SalesInvoiceState.IsReadyForPosting))
            {
                var validInvoiceItems = @this.ValidInvoiceItems.Cast<SalesInvoiceItem>().ToArray();

                var currentPriceComponents = @this.BilledFrom?.PriceComponentsWherePricedBy
                    .Where(v => v.FromDate <= @this.InvoiceDate && (!v.ExistThroughDate || v.ThroughDate >= @this.InvoiceDate))
                    .ToArray();

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

                    CalculatePrices(@this, salesInvoiceItem, currentPriceComponents, quantityOrdered, 0);
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

                    CalculatePrices(@this, salesInvoiceItem, currentPriceComponents, quantityOrdered, totalBasePrice);
                }

                // Calculate Totals
                @this.TotalBasePrice = 0;
                @this.TotalDiscount = 0;
                @this.TotalSurcharge = 0;
                @this.TotalExVat = 0;
                @this.TotalFee = 0;
                @this.TotalShippingAndHandling = 0;
                @this.TotalExtraCharge = 0;
                @this.TotalVat = 0;
                @this.TotalIrpf = 0;
                @this.TotalIncVat = 0;
                @this.TotalListPrice = 0;
                @this.TotalIrpf = 0;
                @this.GrandTotal = 0;

                foreach (var item in validInvoiceItems)
                {
                    @this.TotalBasePrice += item.TotalBasePrice;
                    @this.TotalDiscount += item.TotalDiscount;
                    @this.TotalSurcharge += item.TotalSurcharge;
                    @this.TotalExVat += item.TotalExVat;
                    @this.TotalVat += item.TotalVat;
                    @this.TotalIrpf += item.TotalIrpf;
                    @this.TotalIncVat += item.TotalIncVat;
                    @this.TotalListPrice += item.TotalExVat;
                    @this.GrandTotal += item.GrandTotal;
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

                foreach (OrderAdjustment orderAdjustment in @this.OrderAdjustments)
                {
                    if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
                    {
                        discount = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(@this.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        @this.TotalDiscount += discount;

                        if (@this.ExistDerivedVatRegime)
                        {
                            discountVat = Math.Round(discount * @this.DerivedVatRate.Rate / 100, 2);
                        }

                        if (@this.ExistDerivedIrpfRegime)
                        {
                            discountIrpf = Math.Round(discount * @this.DerivedIrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                    {
                        surcharge = orderAdjustment.Percentage.HasValue ?
                                            Math.Round(@this.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                            orderAdjustment.Amount ?? 0;

                        @this.TotalSurcharge += surcharge;

                        if (@this.ExistDerivedVatRegime)
                        {
                            surchargeVat = Math.Round(surcharge * @this.DerivedVatRate.Rate / 100, 2);
                        }

                        if (@this.ExistDerivedIrpfRegime)
                        {
                            surchargeIrpf = Math.Round(surcharge * @this.DerivedIrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                    {
                        fee = orderAdjustment.Percentage.HasValue ?
                                    Math.Round(@this.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                    orderAdjustment.Amount ?? 0;

                        @this.TotalFee += fee;

                        if (@this.ExistDerivedVatRegime)
                        {
                            feeVat = Math.Round(fee * @this.DerivedVatRate.Rate / 100, 2);
                        }

                        if (@this.ExistDerivedIrpfRegime)
                        {
                            feeIrpf = Math.Round(fee * @this.DerivedIrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                    {
                        shipping = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(@this.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        @this.TotalShippingAndHandling += shipping;

                        if (@this.ExistDerivedVatRegime)
                        {
                            shippingVat = Math.Round(shipping * @this.DerivedVatRate.Rate / 100, 2);
                        }

                        if (@this.ExistDerivedIrpfRegime)
                        {
                            shippingIrpf = Math.Round(shipping * @this.DerivedIrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                    {
                        miscellaneous = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(@this.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        @this.TotalExtraCharge += miscellaneous;

                        if (@this.ExistDerivedVatRegime)
                        {
                            miscellaneousVat = Math.Round(miscellaneous * @this.DerivedVatRate.Rate / 100, 2);
                        }

                        if (@this.ExistDerivedIrpfRegime)
                        {
                            miscellaneousIrpf = Math.Round(miscellaneous * @this.DerivedIrpfRate.Rate / 100, 2);
                        }
                    }
                }

                @this.TotalExtraCharge = fee + shipping + miscellaneous;

                @this.TotalExVat = @this.TotalExVat - discount + surcharge + fee + shipping + miscellaneous;
                @this.TotalVat = @this.TotalVat - discountVat + surchargeVat + feeVat + shippingVat + miscellaneousVat;
                @this.TotalIncVat = @this.TotalIncVat - discount - discountVat + surcharge + surchargeVat + fee + feeVat + shipping + shippingVat + miscellaneous + miscellaneousVat;
                @this.TotalIrpf = @this.TotalIrpf + discountIrpf - surchargeIrpf - feeIrpf - shippingIrpf - miscellaneousIrpf;
                @this.GrandTotal = @this.TotalIncVat - @this.TotalIrpf;
            }

            void CalculatePrices(
                SalesInvoice salesInvoice,
                SalesInvoiceItem salesInvoiceItem,
                PriceComponent[] currentPriceComponents,
                decimal quantityOrdered,
                decimal totalBasePrice)
            {
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

                var unitBasePrice = priceComponents.OfType<BasePrice>()
                    .Where(v => salesInvoiceItem.ExistProduct
                                && salesInvoiceItem.ProductFeatures.Count > 0
                                && v.ExistProduct
                                && v.ExistProductFeature
                                && v.Product.Equals(salesInvoiceItem.Product)
                                && salesInvoiceItem.ProductFeatures.Contains(v.ProductFeature))
                    .Min(v => v.Price);

                if (unitBasePrice == null)
                {
                    unitBasePrice = priceComponents.OfType<BasePrice>().Min(v => v.Price);
                }

                // Calculate Unit Price (with Discounts and Surcharges)
                if (salesInvoiceItem.AssignedUnitPrice.HasValue)
                {
                    salesInvoiceItem.UnitBasePrice = unitBasePrice ?? salesInvoiceItem.AssignedUnitPrice.Value;
                    salesInvoiceItem.UnitDiscount = 0;
                    salesInvoiceItem.UnitSurcharge = 0;
                    salesInvoiceItem.UnitPrice = salesInvoiceItem.AssignedUnitPrice.Value;
                }
                else
                {
                    if (!unitBasePrice.HasValue)
                    {
                        validation.AddError($"{salesInvoiceItem}, {this.M.SalesOrderItem.UnitBasePrice} No BasePrice with a Price");
                        return;
                    }

                    salesInvoiceItem.UnitBasePrice = unitBasePrice.Value;

                    salesInvoiceItem.UnitDiscount = priceComponents.OfType<DiscountComponent>().Sum(
                        v => v.Percentage.HasValue
                                 ? Math.Round(salesInvoiceItem.UnitBasePrice * v.Percentage.Value / 100, 2)
                                 : v.Price ?? 0);

                    salesInvoiceItem.UnitSurcharge = priceComponents.OfType<SurchargeComponent>().Sum(
                        v => v.Percentage.HasValue
                                 ? Math.Round(salesInvoiceItem.UnitBasePrice * v.Percentage.Value / 100, 2)
                                 : v.Price ?? 0);

                    salesInvoiceItem.UnitPrice = salesInvoiceItem.UnitBasePrice - salesInvoiceItem.UnitDiscount + salesInvoiceItem.UnitSurcharge;

                    foreach (OrderAdjustment orderAdjustment in salesInvoiceItem.DiscountAdjustments)
                    {
                        salesInvoiceItem.UnitDiscount += orderAdjustment.Percentage.HasValue ?
                            Math.Round(salesInvoiceItem.UnitPrice * orderAdjustment.Percentage.Value / 100, 2) :
                            orderAdjustment.Amount ?? 0;
                    }

                    foreach (OrderAdjustment orderAdjustment in salesInvoiceItem.SurchargeAdjustments)
                    {
                        salesInvoiceItem.UnitSurcharge += orderAdjustment.Percentage.HasValue ?
                            Math.Round(salesInvoiceItem.UnitPrice * orderAdjustment.Percentage.Value / 100, 2) :
                            orderAdjustment.Amount ?? 0;
                    }

                    salesInvoiceItem.UnitPrice = salesInvoiceItem.UnitBasePrice - salesInvoiceItem.UnitDiscount + salesInvoiceItem.UnitSurcharge;
                }

                salesInvoiceItem.UnitVat = salesInvoiceItem.ExistVatRate ? salesInvoiceItem.UnitPrice * salesInvoiceItem.VatRate.Rate / 100 : 0;
                salesInvoiceItem.UnitIrpf = salesInvoiceItem.ExistIrpfRate ? salesInvoiceItem.UnitPrice * salesInvoiceItem.IrpfRate.Rate / 100 : 0;

                // Calculate Totals
                salesInvoiceItem.TotalBasePrice = salesInvoiceItem.UnitBasePrice * salesInvoiceItem.Quantity;
                salesInvoiceItem.TotalDiscount = salesInvoiceItem.UnitDiscount * salesInvoiceItem.Quantity;
                salesInvoiceItem.TotalSurcharge = salesInvoiceItem.UnitSurcharge * salesInvoiceItem.Quantity;

                if (salesInvoiceItem.TotalBasePrice > 0)
                {
                    salesInvoiceItem.TotalDiscountAsPercentage = Math.Round(salesInvoiceItem.TotalDiscount / salesInvoiceItem.TotalBasePrice * 100, 2);
                    salesInvoiceItem.TotalSurchargeAsPercentage = Math.Round(salesInvoiceItem.TotalSurcharge / salesInvoiceItem.TotalBasePrice * 100, 2);
                }
                else
                {
                    salesInvoiceItem.TotalDiscountAsPercentage = 0;
                    salesInvoiceItem.TotalSurchargeAsPercentage = 0;
                }

                salesInvoiceItem.TotalExVat = salesInvoiceItem.UnitPrice * salesInvoiceItem.Quantity;
                salesInvoiceItem.TotalVat = salesInvoiceItem.UnitVat * salesInvoiceItem.Quantity;
                salesInvoiceItem.TotalIncVat = salesInvoiceItem.TotalExVat + salesInvoiceItem.TotalVat;
                salesInvoiceItem.TotalIrpf = salesInvoiceItem.UnitIrpf * salesInvoiceItem.Quantity;
                salesInvoiceItem.GrandTotal = salesInvoiceItem.TotalIncVat - salesInvoiceItem.TotalIrpf;
            }
        }
    }
}
