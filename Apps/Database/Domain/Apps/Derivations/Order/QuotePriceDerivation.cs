// <copyright file="ProductQuotePriceDerivation.cs" company="Allors bvba">
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

    public class QuotePriceDerivation : DomainDerivation
    {
        public QuotePriceDerivation(M m) : base(m, new Guid("21c53b93-36a1-4d8c-a003-e34ade4feca6")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.Quote.QuoteState),
                new ChangedPattern(this.M.Quote.DerivationTrigger),
                new ChangedPattern(this.M.Quote.ValidQuoteItems),
                new ChangedPattern(this.M.Quote.QuoteItems),
                new ChangedPattern(this.M.Quote.Receiver),
                new ChangedPattern(this.M.Quote.OrderAdjustments),
                new ChangedPattern(this.M.DiscountAdjustment.Percentage) { Steps =  new IPropertyType[] {m.OrderAdjustment.QuoteWhereOrderAdjustment} },
                new ChangedPattern(this.M.DiscountAdjustment.Amount) { Steps =  new IPropertyType[] {m.OrderAdjustment.QuoteWhereOrderAdjustment} },
                new ChangedPattern(this.M.SurchargeAdjustment.Percentage) { Steps =  new IPropertyType[] {m.OrderAdjustment.QuoteWhereOrderAdjustment} },
                new ChangedPattern(this.M.SurchargeAdjustment.Amount) { Steps =  new IPropertyType[] {m.OrderAdjustment.QuoteWhereOrderAdjustment} },
                new ChangedPattern(this.M.QuoteItem.TotalBasePrice) { Steps =  new IPropertyType[] {m.QuoteItem.QuoteWhereQuoteItem} },
                new ChangedPattern(this.M.QuoteItem.TotalDiscount) { Steps =  new IPropertyType[] {m.QuoteItem.QuoteWhereQuoteItem } },
                new ChangedPattern(this.M.QuoteItem.TotalSurcharge) { Steps =  new IPropertyType[] {m.QuoteItem.QuoteWhereQuoteItem } },
                new ChangedPattern(this.M.QuoteItem.TotalPriceAdjustment) { Steps =  new IPropertyType[] {m.QuoteItem.QuoteWhereQuoteItem } },
                new ChangedPattern(this.M.QuoteItem.TotalDiscountAsPercentage) { Steps =  new IPropertyType[] {m.QuoteItem.QuoteWhereQuoteItem } },
                new ChangedPattern(this.M.QuoteItem.TotalSurchargeAsPercentage) { Steps =  new IPropertyType[] {m.QuoteItem.QuoteWhereQuoteItem } },
                new ChangedPattern(this.M.QuoteItem.TotalExVat) { Steps =  new IPropertyType[] {m.QuoteItem.QuoteWhereQuoteItem } },
                new ChangedPattern(this.M.QuoteItem.TotalVat) { Steps =  new IPropertyType[] {m.QuoteItem.QuoteWhereQuoteItem } },
                new ChangedPattern(this.M.QuoteItem.TotalIncVat) { Steps =  new IPropertyType[] {m.QuoteItem.QuoteWhereQuoteItem } },
                new ChangedPattern(this.M.QuoteItem.TotalIrpf) { Steps =  new IPropertyType[] {m.QuoteItem.QuoteWhereQuoteItem } },
                new ChangedPattern(this.M.QuoteItem.GrandTotal) { Steps =  new IPropertyType[] {m.QuoteItem.QuoteWhereQuoteItem } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Quote>().Where(v => v.ExistQuoteState && v.QuoteState.IsCreated))
            {
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
                @this.GrandTotal = 0;

                foreach (QuoteItem quoteItem in @this.ValidQuoteItems)
                {
                    if (!quoteItem.ExistQuoteItemWhereQuotedWithFeature)
                    {
                        @this.TotalBasePrice += quoteItem.TotalBasePrice;
                        @this.TotalDiscount += quoteItem.TotalDiscount;
                        @this.TotalSurcharge += quoteItem.TotalSurcharge;
                        @this.TotalExVat += quoteItem.TotalExVat;
                        @this.TotalVat += quoteItem.TotalVat;
                        @this.TotalIrpf += quoteItem.TotalIrpf;
                        @this.TotalIncVat += quoteItem.TotalIncVat;
                        @this.TotalListPrice += quoteItem.TotalExVat;
                        @this.GrandTotal += quoteItem.GrandTotal;
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
                            discountVat = Math.Round(discount * @this.DerivedVatRegime.VatRate.Rate / 100, 2);
                        }

                        if (@this.ExistDerivedIrpfRegime)
                        {
                            discountIrpf = Math.Round(discount * @this.DerivedIrpfRegime.IrpfRate.Rate / 100, 2);
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
                            surchargeVat = Math.Round(surcharge * @this.DerivedVatRegime.VatRate.Rate / 100, 2);
                        }

                        if (@this.ExistDerivedIrpfRegime)
                        {
                            surchargeIrpf = Math.Round(surcharge * @this.DerivedIrpfRegime.IrpfRate.Rate / 100, 2);
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
                            feeVat = Math.Round(fee * @this.DerivedVatRegime.VatRate.Rate / 100, 2);
                        }

                        if (@this.ExistDerivedIrpfRegime)
                        {
                            feeIrpf = Math.Round(fee * @this.DerivedIrpfRegime.IrpfRate.Rate / 100, 2);
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
                            shippingVat = Math.Round(shipping * @this.DerivedVatRegime.VatRate.Rate / 100, 2);
                        }

                        if (@this.ExistDerivedIrpfRegime)
                        {
                            shippingIrpf = Math.Round(shipping * @this.DerivedIrpfRegime.IrpfRate.Rate / 100, 2);
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
                            miscellaneousVat = Math.Round(miscellaneous * @this.DerivedVatRegime.VatRate.Rate / 100, 2);
                        }

                        if (@this.ExistDerivedIrpfRegime)
                        {
                            miscellaneousIrpf = Math.Round(miscellaneous * @this.DerivedIrpfRegime.IrpfRate.Rate / 100, 2);
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
        }
    }
}
