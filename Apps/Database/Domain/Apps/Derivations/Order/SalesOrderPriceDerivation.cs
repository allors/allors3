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

    public class SalesOrderPriceDerivation : DomainDerivation
    {
        public SalesOrderPriceDerivation(M m) : base(m, new Guid("aa490071-772f-400d-a799-3f015e877c05")) =>
            this.Patterns = new Pattern[]
            {
            new ChangedPattern(this.M.SalesOrder.DerivationTrigger),
            new ChangedPattern(this.M.SalesOrder.ValidOrderItems),
            new ChangedPattern(this.M.SalesOrder.BillToCustomer),
            new ChangedPattern(this.M.SalesOrder.OrderAdjustments),
            new ChangedPattern(this.M.SalesOrderItem.Product) { Steps =  new IPropertyType[] {m.SalesOrderItem.SalesOrderWhereSalesOrderItem} },
            new ChangedPattern(this.M.SalesOrderItem.ProductFeature) { Steps =  new IPropertyType[] {m.SalesOrderItem.SalesOrderWhereSalesOrderItem } },
            new ChangedPattern(this.M.SalesOrderItem.QuantityOrdered) { Steps =  new IPropertyType[] {m.SalesOrderItem.SalesOrderWhereSalesOrderItem } },
            new ChangedPattern(this.M.SalesOrderItem.AssignedUnitPrice) { Steps =  new IPropertyType[] {m.SalesOrderItem.SalesOrderWhereSalesOrderItem } },
            new ChangedPattern(this.M.SalesOrderItem.DiscountAdjustments) { Steps =  new IPropertyType[] {m.SalesOrderItem.SalesOrderWhereSalesOrderItem } },
            new ChangedPattern(this.M.DiscountAdjustment.Percentage) { Steps =  new IPropertyType[] {m.DiscountAdjustment.PriceableWhereDiscountAdjustment, m.SalesOrderItem.SalesOrderWhereSalesOrderItem}, OfType = m.SalesOrder.Class },
            new ChangedPattern(this.M.DiscountAdjustment.Percentage) { Steps =  new IPropertyType[] {m.OrderAdjustment.OrderWhereOrderAdjustment}, OfType = m.SalesOrder.Class },
            new ChangedPattern(this.M.DiscountAdjustment.Amount) { Steps =  new IPropertyType[] {m.DiscountAdjustment.PriceableWhereDiscountAdjustment, m.SalesOrderItem.SalesOrderWhereSalesOrderItem}, OfType = m.SalesOrder.Class },
            new ChangedPattern(this.M.DiscountAdjustment.Amount) { Steps =  new IPropertyType[] {m.OrderAdjustment.OrderWhereOrderAdjustment}, OfType = m.SalesOrder.Class },
            new ChangedPattern(this.M.SalesOrderItem.SurchargeAdjustments) { Steps =  new IPropertyType[] {m.SalesOrderItem.SalesOrderWhereSalesOrderItem } },
            new ChangedPattern(this.M.SurchargeAdjustment.Percentage) { Steps =  new IPropertyType[] {m.SurchargeAdjustment.PriceableWhereSurchargeAdjustment, m.SalesOrderItem.SalesOrderWhereSalesOrderItem}, OfType = m.SalesOrder.Class },
            new ChangedPattern(this.M.SurchargeAdjustment.Percentage) { Steps =  new IPropertyType[] {m.OrderAdjustment.OrderWhereOrderAdjustment}, OfType = m.SalesOrder.Class },
            new ChangedPattern(this.M.SurchargeAdjustment.Amount) { Steps =  new IPropertyType[] {m.SurchargeAdjustment.PriceableWhereSurchargeAdjustment, m.SalesOrderItem.SalesOrderWhereSalesOrderItem}, OfType = m.SalesOrder.Class },
            new ChangedPattern(this.M.SurchargeAdjustment.Amount) { Steps =  new IPropertyType[] {m.OrderAdjustment.OrderWhereOrderAdjustment}, OfType = m.SalesOrder.Class },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<SalesOrder>())
            {
                var validOrderItems = @this.SalesOrderItems.Where(v => v.IsValid).ToArray();
                Sync(validOrderItems, @this);
            }

            void Sync(SalesOrderItem[] validOrderItems, SalesOrder salesOrder)
            {
                // Second run to calculate price (because of order value break)
                foreach (var salesOrderItem in validOrderItems)
                {
                    foreach (SalesOrderItem featureItem in salesOrderItem.OrderedWithFeatures)
                    {
                        SyncPrices(featureItem, salesOrder, true);
                    }

                    salesOrderItem.Sync(salesOrder);
                    SyncPrices(salesOrderItem, salesOrder, true);
                }

                // Calculate Totals
                salesOrder.TotalBasePrice = 0;
                salesOrder.TotalDiscount = 0;
                salesOrder.TotalSurcharge = 0;
                salesOrder.TotalExVat = 0;
                salesOrder.TotalFee = 0;
                salesOrder.TotalShippingAndHandling = 0;
                salesOrder.TotalExtraCharge = 0;
                salesOrder.TotalVat = 0;
                salesOrder.TotalIrpf = 0;
                salesOrder.TotalIncVat = 0;
                salesOrder.TotalListPrice = 0;
                salesOrder.GrandTotal = 0;

                foreach (var item in validOrderItems)
                {
                    if (!item.ExistSalesOrderItemWhereOrderedWithFeature)
                    {
                        salesOrder.TotalBasePrice += item.TotalBasePrice;
                        salesOrder.TotalDiscount += item.TotalDiscount;
                        salesOrder.TotalSurcharge += item.TotalSurcharge;
                        salesOrder.TotalExVat += item.TotalExVat;
                        salesOrder.TotalVat += item.TotalVat;
                        salesOrder.TotalIrpf += item.TotalIrpf;
                        salesOrder.TotalIncVat += item.TotalIncVat;
                        salesOrder.TotalListPrice += item.TotalExVat;
                        salesOrder.GrandTotal += item.GrandTotal;
                    }
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

                foreach (OrderAdjustment orderAdjustment in salesOrder.OrderAdjustments)
                {
                    if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
                    {
                        discount = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(salesOrder.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        salesOrder.TotalDiscount += discount;

                        if (salesOrder.ExistDerivedVatRegime)
                        {
                            discountVat = Math.Round(discount * salesOrder.DerivedVatRegime.VatRate.Rate / 100, 2);
                        }

                        if (salesOrder.ExistDerivedIrpfRegime)
                        {
                            discountIrpf = Math.Round(discount * salesOrder.DerivedIrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                    {
                        surcharge = orderAdjustment.Percentage.HasValue ?
                                            Math.Round(salesOrder.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                            orderAdjustment.Amount ?? 0;

                        salesOrder.TotalSurcharge += surcharge;

                        if (salesOrder.ExistDerivedVatRegime)
                        {
                            surchargeVat = Math.Round(surcharge * salesOrder.DerivedVatRegime.VatRate.Rate / 100, 2);
                        }

                        if (salesOrder.ExistDerivedIrpfRegime)
                        {
                            surchargeIrpf = Math.Round(surcharge * salesOrder.DerivedIrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                    {
                        fee = orderAdjustment.Percentage.HasValue ?
                                    Math.Round(salesOrder.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                    orderAdjustment.Amount ?? 0;

                        salesOrder.TotalFee += fee;

                        if (salesOrder.ExistDerivedVatRegime)
                        {
                            feeVat = Math.Round(fee * salesOrder.DerivedVatRegime.VatRate.Rate / 100, 2);
                        }

                        if (salesOrder.ExistDerivedIrpfRegime)
                        {
                            feeIrpf = Math.Round(fee * salesOrder.DerivedIrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                    {
                        shipping = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(salesOrder.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        salesOrder.TotalShippingAndHandling += shipping;

                        if (salesOrder.ExistDerivedVatRegime)
                        {
                            shippingVat = Math.Round(shipping * salesOrder.DerivedVatRegime.VatRate.Rate / 100, 2);
                        }

                        if (salesOrder.ExistDerivedIrpfRegime)
                        {
                            shippingIrpf = Math.Round(shipping * salesOrder.DerivedIrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                    {
                        miscellaneous = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(salesOrder.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        salesOrder.TotalExtraCharge += miscellaneous;

                        if (salesOrder.ExistDerivedVatRegime)
                        {
                            miscellaneousVat = Math.Round(miscellaneous * salesOrder.DerivedVatRegime.VatRate.Rate / 100, 2);
                        }

                        if (salesOrder.ExistDerivedIrpfRegime)
                        {
                            miscellaneousIrpf = Math.Round(miscellaneous * salesOrder.DerivedIrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }
                }

                salesOrder.TotalExtraCharge = fee + shipping + miscellaneous;

                salesOrder.TotalExVat = salesOrder.TotalExVat - discount + surcharge + fee + shipping + miscellaneous;
                salesOrder.TotalVat = salesOrder.TotalVat - discountVat + surchargeVat + feeVat + shippingVat + miscellaneousVat;
                salesOrder.TotalIncVat = salesOrder.TotalIncVat - discount - discountVat + surcharge + surchargeVat + fee + feeVat + shipping + shippingVat + miscellaneous + miscellaneousVat;
                salesOrder.TotalIrpf = salesOrder.TotalIrpf + discountIrpf - surchargeIrpf - feeIrpf - shippingIrpf - miscellaneousIrpf;
                salesOrder.GrandTotal = salesOrder.TotalIncVat - salesOrder.TotalIrpf;

                //// Only take into account items for which there is data at the item level.
                //// Skip negative sales.
                decimal totalUnitBasePrice = 0;
                decimal totalListPrice = 0;

                foreach (var item1 in validOrderItems)
                {
                    if (item1.TotalExVat > 0)
                    {
                        totalUnitBasePrice += item1.UnitBasePrice;
                        totalListPrice += item1.UnitPrice;
                    }
                }
            }

            void SyncPrices(SalesOrderItem salesOrderItem, SalesOrder salesOrder, bool useValueOrdered = false)
            {
                var sameProductItems = salesOrder.SalesOrderItems
                    .Where(v => v.IsValid && v.ExistProduct && v.Product.Equals(salesOrderItem.Product))
                    .ToArray();

                var quantityOrdered = sameProductItems.Sum(w => w.QuantityOrdered);
                var valueOrdered = useValueOrdered ? sameProductItems.Sum(w => w.TotalBasePrice) : 0;

                var orderPriceComponents = salesOrder.TakenBy?.PriceComponentsWherePricedBy
                    .Where(v => v.FromDate <= salesOrder.OrderDate && (!v.ExistThroughDate || v.ThroughDate >= salesOrder.OrderDate))
                    .ToArray();

                var orderItemPriceComponents = Array.Empty<PriceComponent>();
                if (salesOrderItem.ExistProduct)
                {
                    orderItemPriceComponents = salesOrderItem.Product.GetPriceComponents(orderPriceComponents);
                }
                else if (salesOrderItem.ExistProductFeature)
                {
                    orderItemPriceComponents = salesOrderItem.ProductFeature.GetPriceComponents(salesOrderItem.SalesOrderItemWhereOrderedWithFeature.Product, orderPriceComponents);
                }

                var priceComponents = orderItemPriceComponents.Where(
                    v => PriceComponents.AppsIsApplicable(
                        new PriceComponents.IsApplicable
                        {
                            PriceComponent = v,
                            Customer = salesOrder.BillToCustomer,
                            Product = salesOrderItem.Product,
                            SalesOrder = salesOrder,
                            QuantityOrdered = quantityOrdered,
                            ValueOrdered = valueOrdered,
                        })).ToArray();

                var unitBasePrice = priceComponents.OfType<BasePrice>()
                    .Where(v => salesOrderItem.ExistProduct
                                && salesOrderItem.OrderedWithFeatures.Count > 0
                                && v.ExistProduct
                                && v.ExistProductFeature
                                && v.Product.Equals(salesOrderItem.Product)
                                && salesOrderItem.OrderedWithFeatures.Contains(v.ProductFeature))
                    .Min(v => v.Price);

                if (unitBasePrice == null)
                {
                    unitBasePrice = priceComponents.OfType<BasePrice>().Min(v => v.Price);
                }


                // Calculate Unit Price (with Discounts and Surcharges)
                if (salesOrderItem.AssignedUnitPrice.HasValue)
                {
                    salesOrderItem.UnitBasePrice = unitBasePrice ?? salesOrderItem.AssignedUnitPrice.Value;
                    salesOrderItem.UnitDiscount = 0;
                    salesOrderItem.UnitSurcharge = 0;
                    salesOrderItem.UnitPrice = salesOrderItem.AssignedUnitPrice.Value;
                }
                else
                {
                    if (!unitBasePrice.HasValue)
                    {
                        validation.AddError($"{salesOrderItem} {this.M.SalesOrderItem.UnitBasePrice} No BasePrice with a Price");
                        return;
                    }

                    salesOrderItem.UnitBasePrice = unitBasePrice.Value;

                    salesOrderItem.UnitDiscount = priceComponents.OfType<DiscountComponent>().Sum(
                        v => v.Percentage.HasValue
                            ? Math.Round(salesOrderItem.UnitBasePrice * v.Percentage.Value / 100, 2)
                            : v.Price ?? 0);

                    salesOrderItem.UnitSurcharge = priceComponents.OfType<SurchargeComponent>().Sum(
                        v => v.Percentage.HasValue
                            ? Math.Round(salesOrderItem.UnitBasePrice * v.Percentage.Value / 100, 2)
                            : v.Price ?? 0);

                    salesOrderItem.UnitPrice = salesOrderItem.UnitBasePrice - salesOrderItem.UnitDiscount + salesOrderItem.UnitSurcharge;

                    foreach (OrderAdjustment orderAdjustment in salesOrderItem.DiscountAdjustments)
                    {
                        salesOrderItem.UnitDiscount += orderAdjustment.Percentage.HasValue
                            ? Math.Round(salesOrderItem.UnitPrice * orderAdjustment.Percentage.Value / 100, 2)
                            : orderAdjustment.Amount ?? 0;
                    }

                    foreach (OrderAdjustment orderAdjustment in salesOrderItem.SurchargeAdjustments)
                    {
                        salesOrderItem.UnitSurcharge += orderAdjustment.Percentage.HasValue
                            ? Math.Round(salesOrderItem.UnitPrice * orderAdjustment.Percentage.Value / 100, 2)
                            : orderAdjustment.Amount ?? 0;
                    }

                    salesOrderItem.UnitPrice = salesOrderItem.UnitBasePrice - salesOrderItem.UnitDiscount + salesOrderItem.UnitSurcharge;
                }

                foreach (SalesOrderItem featureItem in salesOrderItem.OrderedWithFeatures)
                {
                    salesOrderItem.UnitBasePrice += featureItem.UnitBasePrice;
                    salesOrderItem.UnitPrice += featureItem.UnitPrice;
                    salesOrderItem.UnitDiscount += featureItem.UnitDiscount;
                    salesOrderItem.UnitSurcharge += featureItem.UnitSurcharge;
                }

                salesOrderItem.UnitVat = salesOrderItem.ExistVatRate ? salesOrderItem.UnitPrice * salesOrderItem.VatRate.Rate / 100 : 0;
                salesOrderItem.UnitIrpf = salesOrderItem.ExistIrpfRate ? salesOrderItem.UnitPrice * salesOrderItem.IrpfRate.Rate / 100 : 0;

                // Calculate Totals
                salesOrderItem.TotalBasePrice = salesOrderItem.UnitBasePrice * salesOrderItem.QuantityOrdered;
                salesOrderItem.TotalDiscount = salesOrderItem.UnitDiscount * salesOrderItem.QuantityOrdered;
                salesOrderItem.TotalSurcharge = salesOrderItem.UnitSurcharge * salesOrderItem.QuantityOrdered;
                salesOrderItem.TotalOrderAdjustment = salesOrderItem.TotalSurcharge - salesOrderItem.TotalDiscount;

                if (salesOrderItem.TotalBasePrice > 0)
                {
                    salesOrderItem.TotalDiscountAsPercentage = Math.Round(salesOrderItem.TotalDiscount / salesOrderItem.TotalBasePrice * 100, 2);
                    salesOrderItem.TotalSurchargeAsPercentage = Math.Round(salesOrderItem.TotalSurcharge / salesOrderItem.TotalBasePrice * 100, 2);
                }
                else
                {
                    salesOrderItem.TotalDiscountAsPercentage = 0;
                    salesOrderItem.TotalSurchargeAsPercentage = 0;
                }

                salesOrderItem.TotalExVat = salesOrderItem.UnitPrice * salesOrderItem.QuantityOrdered;
                salesOrderItem.TotalVat = salesOrderItem.UnitVat * salesOrderItem.QuantityOrdered;
                salesOrderItem.TotalIncVat = salesOrderItem.TotalExVat + salesOrderItem.TotalVat;
                salesOrderItem.TotalIrpf = salesOrderItem.UnitIrpf * salesOrderItem.QuantityOrdered;
                salesOrderItem.GrandTotal = salesOrderItem.TotalIncVat - salesOrderItem.TotalIrpf;
            }
        }
    }
}
