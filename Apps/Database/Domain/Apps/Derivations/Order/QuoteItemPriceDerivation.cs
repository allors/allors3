// <copyright file="QuoteItemPriceDerivation.cs" company="Allors bvba">
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
    using Resources;

    public class QuoteItemPriceDerivation : DomainDerivation
    {
        public QuoteItemPriceDerivation(M m) : base(m, new Guid("01b4ac5d-fbd9-4f94-a1cf-2f5b5875063f")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.QuoteItem.Product),
                new ChangedPattern(this.M.QuoteItem.ProductFeature),
                new ChangedPattern(this.M.QuoteItem.QuotedWithFeatures),
                new ChangedPattern(this.M.QuoteItem.Quantity),
                new ChangedPattern(this.M.QuoteItem.AssignedUnitPrice),
                new ChangedPattern(this.M.QuoteItem.DiscountAdjustments),
                new ChangedPattern(this.M.QuoteItem.SurchargeAdjustments),
                new ChangedPattern(this.M.QuoteItem.VatRate),
                new ChangedPattern(this.M.QuoteItem.IrpfRate),
                new ChangedPattern(this.M.DiscountAdjustment.Percentage) { Steps =  new IPropertyType[] {m.DiscountAdjustment.PriceableWhereDiscountAdjustment}, OfType = m.QuoteItem.Class },
                new ChangedPattern(this.M.DiscountAdjustment.Amount) { Steps =  new IPropertyType[] {m.DiscountAdjustment.PriceableWhereDiscountAdjustment}, OfType = m.QuoteItem.Class },
                new ChangedPattern(this.M.SurchargeAdjustment.Percentage) { Steps =  new IPropertyType[] {m.SurchargeAdjustment.PriceableWhereSurchargeAdjustment}, OfType = m.QuoteItem.Class },
                new ChangedPattern(this.M.SurchargeAdjustment.Amount) { Steps =  new IPropertyType[] {m.SurchargeAdjustment.PriceableWhereSurchargeAdjustment}, OfType = m.QuoteItem.Class },
                new ChangedPattern(this.M.Quote.Receiver) { Steps =  new IPropertyType[] {m.Quote.QuoteItems } },
                new ChangedPattern(this.M.Quote.IssueDate) { Steps =  new IPropertyType[] {m.Quote.QuoteItems } },
                new ChangedPattern(this.M.Quote.DerivationTrigger) { Steps =  new IPropertyType[] {m.Quote.QuoteItems } },
                new ChangedPattern(this.M.ProductQuoteItemByProduct.Product) { Steps =  new IPropertyType[] {m.ProductQuoteItemByProduct.ProductQuoteWhereProductQuoteItemsByProduct, m.Quote.QuoteItems } },
                new ChangedPattern(this.M.ProductQuoteItemByProduct.QuantityOrdered) { Steps =  new IPropertyType[] {m.ProductQuoteItemByProduct.ProductQuoteWhereProductQuoteItemsByProduct, m.Quote.QuoteItems } },
                new ChangedPattern(this.M.ProductQuoteItemByProduct.ValueOrdered) { Steps =  new IPropertyType[] {m.ProductQuoteItemByProduct.ProductQuoteWhereProductQuoteItemsByProduct, m.Quote.QuoteItems } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<QuoteItem>())
            {
                var quote = @this.QuoteWhereQuoteItem?? @this.QuoteItemWhereQuotedWithFeature?.QuoteWhereQuoteItem;

                if (quote != null)
                {
                    var itemByProduct = quote is ProductQuote productQuote ? productQuote.ProductQuoteItemsByProduct.FirstOrDefault(v => @this.ExistProduct && v.Product.Equals(@this.Product)) : null;

                    var quantityOrdered = itemByProduct != null ? itemByProduct.QuantityOrdered : 0;
                    var valueOrdered = itemByProduct != null ? itemByProduct.ValueOrdered : 0;

                    var quotePriceComponents = quote?.Issuer?.PriceComponentsWherePricedBy
                    .Where(v => v.FromDate <= quote.IssueDate && (!v.ExistThroughDate || v.ThroughDate >= quote.IssueDate))
                    .ToArray();

                    var quoteItemPriceComponents = Array.Empty<PriceComponent>();
                    if (@this.ExistProduct)
                    {
                        quoteItemPriceComponents = @this.Product?.GetPriceComponents(quotePriceComponents);
                    }
                    else if (@this.ExistProductFeature)
                    {
                        quoteItemPriceComponents = @this.ProductFeature?.GetPriceComponents(@this.QuoteItemWhereQuotedWithFeature?.Product, quotePriceComponents);
                    }

                    var priceComponents = quoteItemPriceComponents.Where(
                        v => PriceComponents.AppsIsApplicable(
                            new PriceComponents.IsApplicable
                            {
                                PriceComponent = v,
                                Customer = quote.Receiver,
                                Product = @this.Product,
                                QuantityOrdered = quantityOrdered,
                                ValueOrdered = valueOrdered,
                            })).ToArray();

                    var unitBasePrice = priceComponents.OfType<BasePrice>()
                        .Where(v => @this.ExistProduct
                                    && @this.QuotedWithFeatures.Count > 0
                                    && v.ExistProduct
                                    && v.ExistProductFeature
                                    && v.Product.Equals(@this.Product)
                                    && @this.QuotedWithFeatures.Contains(v.ProductFeature))
                        .Min(v => v.Price);

                    if (unitBasePrice == null)
                    {
                        unitBasePrice = priceComponents.OfType<BasePrice>().Min(v => v.Price);
                    }

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
                            validation.AddError($"{@this}, {this.M.QuoteItem.UnitBasePrice} No BasePrice with a Price");
                            return;
                        }

                        @this.UnitBasePrice = unitBasePrice ?? 0;

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
                            @this.UnitDiscount += orderAdjustment.Percentage.HasValue ?
                                Math.Round(@this.UnitPrice * orderAdjustment.Percentage.Value / 100, 2) :
                                orderAdjustment.Amount ?? 0;
                        }

                        foreach (OrderAdjustment orderAdjustment in @this.SurchargeAdjustments)
                        {
                            @this.UnitSurcharge += orderAdjustment.Percentage.HasValue ?
                                Math.Round(@this.UnitPrice * orderAdjustment.Percentage.Value / 100, 2) :
                                orderAdjustment.Amount ?? 0;
                        }

                        @this.UnitPrice = @this.UnitBasePrice - @this.UnitDiscount + @this.UnitSurcharge;
                    }

                    if (!@this.ExistUnitPrice)
                    {
                        validation.AddError($"{@this} {@this.Meta.UnitPrice} {ErrorMessages.UnitPriceRequired}");
                    }

                    foreach (QuoteItem featureItem in @this.QuotedWithFeatures)
                    {
                        @this.UnitBasePrice += featureItem.UnitBasePrice;
                        @this.UnitPrice += featureItem.UnitPrice;
                        @this.UnitDiscount += featureItem.UnitDiscount;
                        @this.UnitSurcharge += featureItem.UnitSurcharge;
                    }

                    @this.UnitVat = @this.ExistVatRate ? @this.UnitPrice * @this.VatRate.Rate / 100 : 0;
                    @this.UnitIrpf = @this.ExistIrpfRate ? @this.UnitPrice * @this.IrpfRate.Rate / 100 : 0;

                    // Calculate Totals
                    var totalBasePrice = @this.UnitBasePrice * @this.Quantity;
                    if (@this.TotalBasePrice != totalBasePrice)
                    {
                        @this.TotalBasePrice = totalBasePrice;
                    }

                    @this.TotalDiscount = @this.UnitDiscount * @this.Quantity;
                    @this.TotalSurcharge = @this.UnitSurcharge * @this.Quantity;
                    @this.TotalPriceAdjustment = @this.TotalSurcharge - @this.TotalDiscount;

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

                    @this.TotalExVat = @this.UnitPrice * @this.Quantity;
                    @this.TotalVat = @this.UnitVat * @this.Quantity;
                    @this.TotalIncVat = @this.TotalExVat + @this.TotalVat;
                    @this.TotalIrpf = @this.UnitIrpf * @this.Quantity;
                    @this.GrandTotal = @this.TotalIncVat - @this.TotalIrpf;
                }
            }
        }
    }
}
