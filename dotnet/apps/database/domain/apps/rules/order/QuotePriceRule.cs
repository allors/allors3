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
    using Derivations.Rules;

    public class QuotePriceRule : Rule
    {
        public QuotePriceRule(MetaPopulation m) : base(m, new Guid("21c53b93-36a1-4d8c-a003-e34ade4feca6")) =>
            this.Patterns = new Pattern[]
            {
                m.Quote.RolePattern(v => v.QuoteState),
                m.Quote.RolePattern(v => v.DerivationTrigger),
                m.Quote.RolePattern(v => v.ValidQuoteItems),
                m.Quote.RolePattern(v => v.QuoteItems),
                m.Quote.RolePattern(v => v.Receiver),
                m.Quote.RolePattern(v => v.OrderAdjustments),
                m.DiscountAdjustment.RolePattern(v => v.Percentage, v => v.QuoteWhereOrderAdjustment),
                m.DiscountAdjustment.RolePattern(v => v.Amount, v => v.QuoteWhereOrderAdjustment),
                m.SurchargeAdjustment.RolePattern(v => v.Percentage, v => v.QuoteWhereOrderAdjustment),
                m.SurchargeAdjustment.RolePattern(v => v.Amount, v => v.QuoteWhereOrderAdjustment),
                m.QuoteItem.RolePattern(v => v.TotalBasePrice, v => v.QuoteWhereQuoteItem),
                m.QuoteItem.RolePattern(v => v.TotalDiscount, v => v.QuoteWhereQuoteItem),
                m.QuoteItem.RolePattern(v => v.TotalSurcharge, v => v.QuoteWhereQuoteItem),
                m.QuoteItem.RolePattern(v => v.TotalPriceAdjustment, v => v.QuoteWhereQuoteItem),
                m.QuoteItem.RolePattern(v => v.TotalDiscountAsPercentage, v => v.QuoteWhereQuoteItem),
                m.QuoteItem.RolePattern(v => v.TotalSurchargeAsPercentage, v => v.QuoteWhereQuoteItem),
                m.QuoteItem.RolePattern(v => v.TotalExVat, v => v.QuoteWhereQuoteItem),
                m.QuoteItem.RolePattern(v => v.TotalVat, v => v.QuoteWhereQuoteItem),
                m.QuoteItem.RolePattern(v => v.TotalIncVat, v => v.QuoteWhereQuoteItem),
                m.QuoteItem.RolePattern(v => v.TotalIrpf, v => v.QuoteWhereQuoteItem),
                m.QuoteItem.RolePattern(v => v.GrandTotal, v => v.QuoteWhereQuoteItem),
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
                                        @this.TotalExVat * orderAdjustment.Percentage.Value / 100 :
                                        orderAdjustment.Amount ?? 0;

                        @this.TotalDiscount += discount;

                        if (@this.ExistDerivedVatRegime)
                        {
                            discountVat = discount * @this.DerivedVatRate.Rate / 100;
                        }

                        if (@this.ExistDerivedIrpfRegime)
                        {
                            discountIrpf = discount * @this.DerivedIrpfRate.Rate / 100;
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                    {
                        surcharge = orderAdjustment.Percentage.HasValue ?
                                            @this.TotalExVat * orderAdjustment.Percentage.Value / 100 :
                                            orderAdjustment.Amount ?? 0;

                        @this.TotalSurcharge += surcharge;

                        if (@this.ExistDerivedVatRegime)
                        {
                            surchargeVat = surcharge * @this.DerivedVatRate.Rate / 100;
                        }

                        if (@this.ExistDerivedIrpfRegime)
                        {
                            surchargeIrpf = surcharge * @this.DerivedIrpfRate.Rate / 100;
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                    {
                        fee = orderAdjustment.Percentage.HasValue ?
                                    @this.TotalExVat * orderAdjustment.Percentage.Value / 100 :
                                    orderAdjustment.Amount ?? 0;

                        @this.TotalFee += fee;

                        if (@this.ExistDerivedVatRegime)
                        {
                            feeVat = fee * @this.DerivedVatRate.Rate / 100;
                        }

                        if (@this.ExistDerivedIrpfRegime)
                        {
                            feeIrpf = fee * @this.DerivedIrpfRate.Rate / 100;
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                    {
                        shipping = orderAdjustment.Percentage.HasValue ?
                                        @this.TotalExVat * orderAdjustment.Percentage.Value / 100 :
                                        orderAdjustment.Amount ?? 0;

                        @this.TotalShippingAndHandling += shipping;

                        if (@this.ExistDerivedVatRegime)
                        {
                            shippingVat = shipping * @this.DerivedVatRate.Rate / 100;
                        }

                        if (@this.ExistDerivedIrpfRegime)
                        {
                            shippingIrpf = shipping * @this.DerivedIrpfRate.Rate / 100;
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                    {
                        miscellaneous = orderAdjustment.Percentage.HasValue ?
                                        @this.TotalExVat * orderAdjustment.Percentage.Value / 100 :
                                        orderAdjustment.Amount ?? 0;

                        @this.TotalExtraCharge += miscellaneous;

                        if (@this.ExistDerivedVatRegime)
                        {
                            miscellaneousVat = miscellaneous * @this.DerivedVatRate.Rate / 100;
                        }

                        if (@this.ExistDerivedIrpfRegime)
                        {
                            miscellaneousIrpf = miscellaneous * @this.DerivedIrpfRate.Rate / 100;
                        }
                    }
                }

                var totalExtraCharge = fee + shipping + miscellaneous;
                var totalExVat = @this.TotalExVat - discount + surcharge + fee + shipping + miscellaneous;
                var totalVat = @this.TotalVat - discountVat + surchargeVat + feeVat + shippingVat + miscellaneousVat;
                var totalIncVat = @this.TotalIncVat - discount - discountVat + surcharge + surchargeVat + fee + feeVat + shipping + shippingVat + miscellaneous + miscellaneousVat;
                var totalIrpf = @this.TotalIrpf + discountIrpf - surchargeIrpf - feeIrpf - shippingIrpf - miscellaneousIrpf;
                var grandTotal = totalIncVat - totalIrpf;

                if (@this.ExistIssueDate && @this.ExistDerivedCurrency && @this.ExistIssuer)
                {
                    @this.TotalBasePriceInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalBasePrice, @this.IssueDate, @this.DerivedCurrency, @this.Issuer.PreferredCurrency), 2);
                    @this.TotalDiscountInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalDiscount, @this.IssueDate, @this.DerivedCurrency, @this.Issuer.PreferredCurrency), 2);
                    @this.TotalSurchargeInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalSurcharge, @this.IssueDate, @this.DerivedCurrency, @this.Issuer.PreferredCurrency), 2);
                    @this.TotalExtraChargeInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(totalExtraCharge, @this.IssueDate, @this.DerivedCurrency, @this.Issuer.PreferredCurrency), 2);
                    @this.TotalFeeInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalFee, @this.IssueDate, @this.DerivedCurrency, @this.Issuer.PreferredCurrency), 2);
                    @this.TotalShippingAndHandlingInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalShippingAndHandling, @this.IssueDate, @this.DerivedCurrency, @this.Issuer.PreferredCurrency), 2);
                    @this.TotalListPriceInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalListPrice, @this.IssueDate, @this.DerivedCurrency, @this.Issuer.PreferredCurrency), 2);
                    @this.TotalExVatInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(totalExVat, @this.IssueDate, @this.DerivedCurrency, @this.Issuer.PreferredCurrency), 2);
                    @this.TotalVatInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(totalVat, @this.IssueDate, @this.DerivedCurrency, @this.Issuer.PreferredCurrency), 2);
                    @this.TotalIncVatInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(totalIncVat, @this.IssueDate, @this.DerivedCurrency, @this.Issuer.PreferredCurrency), 2);
                    @this.TotalIrpfInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(totalIrpf, @this.IssueDate, @this.DerivedCurrency, @this.Issuer.PreferredCurrency), 2);
                    @this.GrandTotalInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(grandTotal, @this.IssueDate, @this.DerivedCurrency, @this.Issuer.PreferredCurrency), 2);
                }

                @this.TotalBasePrice = Rounder.RoundDecimal(@this.TotalBasePrice, 2);
                @this.TotalDiscount = Rounder.RoundDecimal(@this.TotalDiscount, 2);
                @this.TotalSurcharge = Rounder.RoundDecimal(@this.TotalSurcharge, 2);
                @this.TotalExtraCharge = Rounder.RoundDecimal(totalExtraCharge, 2);
                @this.TotalFee = Rounder.RoundDecimal(@this.TotalFee, 2);
                @this.TotalShippingAndHandling = Rounder.RoundDecimal(@this.TotalShippingAndHandling, 2);
                @this.TotalListPrice = Rounder.RoundDecimal(@this.TotalListPrice, 2);
                @this.TotalExVat = Rounder.RoundDecimal(totalExVat, 2);
                @this.TotalVat = Rounder.RoundDecimal(totalVat, 2);
                @this.TotalIncVat = Rounder.RoundDecimal(totalIncVat, 2);
                @this.TotalIrpf = Rounder.RoundDecimal(totalIrpf, 2);
                @this.GrandTotal = Rounder.RoundDecimal(grandTotal, 2);
            }
        }
    }
}
