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

    public class SalesOrderItemPriceDerivation : DomainDerivation
    {
        public SalesOrderItemPriceDerivation(M m) : base(m, new Guid("f8c7f18b-7df0-4ed4-ba1b-30e4c32ee414")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.SalesOrderItem.Product),
                new ChangedPattern(this.M.SalesOrderItem.ProductFeature),
                new ChangedPattern(this.M.SalesOrderItem.OrderedWithFeatures),
                new ChangedPattern(this.M.SalesOrderItem.QuantityOrdered),
                new ChangedPattern(this.M.SalesOrderItem.AssignedUnitPrice),
                new ChangedPattern(this.M.SalesOrderItem.DiscountAdjustments),
                new ChangedPattern(this.M.SalesOrderItem.SurchargeAdjustments),
                new ChangedPattern(this.M.SalesOrderItem.VatRate),
                new ChangedPattern(this.M.SalesOrderItem.IrpfRate),
                new ChangedPattern(this.M.DiscountAdjustment.Percentage) { Steps =  new IPropertyType[] {m.DiscountAdjustment.PriceableWhereDiscountAdjustment}, OfType = m.SalesOrderItem.Class },
                new ChangedPattern(this.M.DiscountAdjustment.Amount) { Steps =  new IPropertyType[] {m.DiscountAdjustment.PriceableWhereDiscountAdjustment}, OfType = m.SalesOrderItem.Class },
                new ChangedPattern(this.M.SurchargeAdjustment.Percentage) { Steps =  new IPropertyType[] {m.SurchargeAdjustment.PriceableWhereSurchargeAdjustment}, OfType = m.SalesOrderItem.Class },
                new ChangedPattern(this.M.SurchargeAdjustment.Amount) { Steps =  new IPropertyType[] {m.SurchargeAdjustment.PriceableWhereSurchargeAdjustment}, OfType = m.SalesOrderItem.Class },
                new ChangedPattern(this.M.SalesOrder.BillToCustomer) { Steps =  new IPropertyType[] {m.SalesOrder.SalesOrderItems } },
                new ChangedPattern(this.M.SalesOrder.OrderDate) { Steps =  new IPropertyType[] {m.SalesOrder.SalesOrderItems } },
                new ChangedPattern(this.M.SalesOrder.DerivationTrigger) { Steps =  new IPropertyType[] {m.SalesOrder.SalesOrderItems } },
                new ChangedPattern(this.M.SalesOrderItemByProduct.Product) { Steps =  new IPropertyType[] {m.SalesOrderItemByProduct.SalesOrderWhereSalesOrderItemsByProduct, m.SalesOrder.SalesOrderItems } },
                new ChangedPattern(this.M.SalesOrderItemByProduct.QuantityOrdered) { Steps =  new IPropertyType[] {m.SalesOrderItemByProduct.SalesOrderWhereSalesOrderItemsByProduct, m.SalesOrder.SalesOrderItems } },
                new ChangedPattern(this.M.SalesOrderItemByProduct.ValueOrdered) { Steps =  new IPropertyType[] {m.SalesOrderItemByProduct.SalesOrderWhereSalesOrderItemsByProduct, m.SalesOrder.SalesOrderItems } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<SalesOrderItem>())
            {
                var salesOrder = @this.SalesOrderWhereSalesOrderItem ?? @this.SalesOrderItemWhereOrderedWithFeature?.SalesOrderWhereSalesOrderItem;

                if (salesOrder != null)
                {
                    var itemByProduct = salesOrder.SalesOrderItemsByProduct.FirstOrDefault(v => @this.ExistProduct && v.Product.Equals(@this.Product));

                    var quantityOrdered = itemByProduct != null ? itemByProduct.QuantityOrdered : 0;
                    var valueOrdered = itemByProduct != null ? itemByProduct.ValueOrdered : 0;

                    var orderPriceComponents = salesOrder?.TakenBy?.PriceComponentsWherePricedBy
                        .Where(v => v.FromDate <= salesOrder.OrderDate && (!v.ExistThroughDate || v.ThroughDate >= salesOrder.OrderDate))
                        .ToArray();

                    var orderItemPriceComponents = Array.Empty<PriceComponent>();
                    if (@this.ExistProduct)
                    {
                        orderItemPriceComponents = @this.Product.GetPriceComponents(orderPriceComponents);
                    }
                    else if (@this.ExistProductFeature)
                    {
                        orderItemPriceComponents = @this.ProductFeature.GetPriceComponents(@this.SalesOrderItemWhereOrderedWithFeature.Product, orderPriceComponents);
                    }

                    var priceComponents = orderItemPriceComponents.Where(
                        v => PriceComponents.AppsIsApplicable(
                            new PriceComponents.IsApplicable
                            {
                                PriceComponent = v,
                                Customer = salesOrder?.BillToCustomer,
                                Product = @this.Product,
                                SalesOrder = salesOrder,
                                QuantityOrdered = quantityOrdered,
                                ValueOrdered = valueOrdered,
                            })).ToArray();

                    var unitBasePrice = priceComponents.OfType<BasePrice>().Min(v => v.Price);

                    // Calculate Unit Price (with Discounts and Surcharges)
                    if (@this.AssignedUnitPrice.HasValue)
                    {
                        @this.UnitBasePrice = unitBasePrice ?? @this.AssignedUnitPrice.Value;
                        @this.UnitDiscount = 0;
                        @this.UnitSurcharge = 0;
                        @this.UnitPrice = @this.AssignedUnitPrice.Value;
                    }
                    else
                    {
                        if (!unitBasePrice.HasValue)
                        {
                            validation.AddError($"{@this}, {this.M.SalesOrderItem.UnitBasePrice} No BasePrice with a Price");
                            return;
                        }

                        @this.UnitBasePrice = unitBasePrice.Value;

                        @this.UnitDiscount = priceComponents.OfType<DiscountComponent>().Sum(
                            v => v.Percentage.HasValue
                                ? Math.Round(@this.UnitBasePrice * v.Percentage.Value / 100, 2)
                                : v.Price ?? 0);

                        @this.UnitSurcharge = priceComponents.OfType<SurchargeComponent>().Sum(
                            v => v.Percentage.HasValue
                                ? Math.Round(@this.UnitBasePrice * v.Percentage.Value / 100, 2)
                                : v.Price ?? 0);

                        @this.UnitPrice = @this.UnitBasePrice - @this.UnitDiscount + @this.UnitSurcharge;

                        foreach (OrderAdjustment orderAdjustment in @this.DiscountAdjustments)
                        {
                            @this.UnitDiscount += orderAdjustment.Percentage.HasValue
                                ? Math.Round(@this.UnitPrice * orderAdjustment.Percentage.Value / 100, 2)
                                : orderAdjustment.Amount ?? 0;
                        }

                        foreach (OrderAdjustment orderAdjustment in @this.SurchargeAdjustments)
                        {
                            @this.UnitSurcharge += orderAdjustment.Percentage.HasValue
                                ? Math.Round(@this.UnitPrice * orderAdjustment.Percentage.Value / 100, 2)
                                : orderAdjustment.Amount ?? 0;
                        }

                        @this.UnitPrice = @this.UnitBasePrice - @this.UnitDiscount + @this.UnitSurcharge;
                    }

                    foreach (SalesOrderItem featureItem in @this.OrderedWithFeatures)
                    {
                        @this.UnitBasePrice += featureItem.UnitBasePrice;
                        @this.UnitPrice += featureItem.UnitPrice;
                        @this.UnitDiscount += featureItem.UnitDiscount;
                        @this.UnitSurcharge += featureItem.UnitSurcharge;
                    }

                    @this.UnitVat = @this.ExistVatRate ? @this.UnitPrice * @this.VatRate.Rate / 100 : 0;
                    @this.UnitIrpf = @this.ExistIrpfRate ? @this.UnitPrice * @this.IrpfRate.Rate / 100 : 0;

                    // Calculate Totals
                    var totalBasePrice = @this.UnitBasePrice * @this.QuantityOrdered;
                    if (@this.TotalBasePrice != totalBasePrice)
                    {
                        @this.TotalBasePrice = totalBasePrice;
                    }

                    @this.TotalDiscount = @this.UnitDiscount * @this.QuantityOrdered;
                    @this.TotalSurcharge = @this.UnitSurcharge * @this.QuantityOrdered;
                    @this.TotalOrderAdjustment = @this.TotalSurcharge - @this.TotalDiscount;

                    if (@this.TotalBasePrice > 0)
                    {
                        @this.TotalDiscountAsPercentage = Math.Round(@this.TotalDiscount / @this.TotalBasePrice * 100, 2);
                        @this.TotalSurchargeAsPercentage = Math.Round(@this.TotalSurcharge / @this.TotalBasePrice * 100, 2);
                    }
                    else
                    {
                        @this.TotalDiscountAsPercentage = 0;
                        @this.TotalSurchargeAsPercentage = 0;
                    }

                    @this.TotalExVat = @this.UnitPrice * @this.QuantityOrdered;
                    @this.TotalVat = @this.UnitVat * @this.QuantityOrdered;
                    @this.TotalIncVat = @this.TotalExVat + @this.TotalVat;
                    @this.TotalIrpf = @this.UnitIrpf * @this.QuantityOrdered;
                    @this.GrandTotal = @this.TotalIncVat - @this.TotalIrpf;
                }
            }
        }
    }
}
