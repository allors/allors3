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
    using Derivations.Rules;

    public class PurchaseInvoicePriceRule : Rule
    {
        public PurchaseInvoicePriceRule(MetaPopulation m) : base(m, new Guid("cde03f3a-9151-4bb7-a3a5-5bc238f3cc54")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseInvoice.RolePattern(v => v.PurchaseInvoiceState),
                m.PurchaseInvoice.RolePattern(v => v.DerivationTrigger),
                m.PurchaseInvoice.RolePattern(v => v.ValidInvoiceItems),
                m.PurchaseInvoice.RolePattern(v => v.BilledFrom),
                m.PurchaseInvoice.RolePattern(v => v.OrderAdjustments),
                m.PurchaseInvoice.RolePattern(v => v.InvoiceDate),
                m.PurchaseInvoiceItem.RolePattern(v => v.Part, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseInvoiceItem.RolePattern(v => v.Quantity, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseInvoiceItem.RolePattern(v => v.AssignedUnitPrice, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseInvoiceItem.RolePattern(v => v.DiscountAdjustments, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.PurchaseInvoiceItem.RolePattern(v => v.SurchargeAdjustments, v => v.PurchaseInvoiceWherePurchaseInvoiceItem),
                m.DiscountAdjustment.RolePattern(v => v.Percentage, v => v.PriceableWhereDiscountAdjustment.Priceable.AsPurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem, m.PurchaseInvoice),
                m.DiscountAdjustment.RolePattern(v => v.Percentage, v => v.InvoiceWhereOrderAdjustment, m.PurchaseInvoice),
                m.DiscountAdjustment.RolePattern(v => v.Amount, v => v.PriceableWhereDiscountAdjustment.Priceable.AsPurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem, m.PurchaseInvoice),
                m.DiscountAdjustment.RolePattern(v => v.Amount, v => v.InvoiceVersionWhereOrderAdjustment, m.PurchaseInvoice),
                m.DiscountAdjustment.RolePattern(v => v.Amount, v => v.InvoiceWhereOrderAdjustment, m.PurchaseInvoice),
                m.SurchargeAdjustment.RolePattern(v => v.Percentage, v => v.PriceableWhereSurchargeAdjustment.Priceable.AsPurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem, m.PurchaseInvoice),
                m.SurchargeAdjustment.RolePattern(v => v.Percentage, v => v.InvoiceWhereOrderAdjustment, m.PurchaseInvoice),
                m.SurchargeAdjustment.RolePattern(v => v.Amount, v => v.PriceableWhereSurchargeAdjustment.Priceable.AsPurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem, m.PurchaseInvoice),
                m.SurchargeAdjustment.RolePattern(v => v.Amount, v => v.InvoiceWhereOrderAdjustment, m.PurchaseInvoice),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseInvoice>().Where(v => v.ExistPurchaseInvoiceState && (v.PurchaseInvoiceState.IsCreated || v.PurchaseInvoiceState.IsRevising)))
            {
                foreach (PurchaseInvoiceItem purchaseInvoiceItem in @this.ValidInvoiceItems)
                {
                    purchaseInvoiceItem.UnitBasePrice = 0;
                    purchaseInvoiceItem.UnitDiscount = 0;
                    purchaseInvoiceItem.UnitSurcharge = 0;

                    purchaseInvoiceItem.DerivedVatRegime = purchaseInvoiceItem.AssignedVatRegime ?? @this.DerivedVatRegime;
                    purchaseInvoiceItem.VatRate = purchaseInvoiceItem.DerivedVatRegime?.VatRates.First(v => v.FromDate <= @this.InvoiceDate && (!v.ExistThroughDate || v.ThroughDate >= @this.InvoiceDate));

                    purchaseInvoiceItem.DerivedIrpfRegime = purchaseInvoiceItem.AssignedIrpfRegime ?? @this.DerivedIrpfRegime;
                    purchaseInvoiceItem.IrpfRate = purchaseInvoiceItem.DerivedIrpfRegime?.IrpfRates.First(v => v.FromDate <= @this.InvoiceDate && (!v.ExistThroughDate || v.ThroughDate >= @this.InvoiceDate));

                    if (purchaseInvoiceItem.AssignedUnitPrice.HasValue)
                    {
                        purchaseInvoiceItem.UnitBasePrice = purchaseInvoiceItem.AssignedUnitPrice.Value;
                        purchaseInvoiceItem.UnitPrice = purchaseInvoiceItem.AssignedUnitPrice.Value;
                    }
                    else
                    {
                        if (purchaseInvoiceItem.ExistPart)
                        {
                            purchaseInvoiceItem.UnitBasePrice = new SupplierOfferings(purchaseInvoiceItem.Strategy.Transaction).PurchasePrice(@this.BilledFrom, @this.InvoiceDate, purchaseInvoiceItem.Part);
                        }
                    }

                    if (purchaseInvoiceItem.ExistUnitBasePrice)
                    {
                        foreach (OrderAdjustment orderAdjustment in purchaseInvoiceItem.DiscountAdjustments)
                        {
                            purchaseInvoiceItem.UnitDiscount += orderAdjustment.Percentage.HasValue ?
                                purchaseInvoiceItem.UnitBasePrice * orderAdjustment.Percentage.Value / 100 :
                                orderAdjustment.Amount ?? 0;
                        }

                        foreach (OrderAdjustment orderAdjustment in purchaseInvoiceItem.SurchargeAdjustments)
                        {
                            purchaseInvoiceItem.UnitSurcharge += orderAdjustment.Percentage.HasValue ?
                                purchaseInvoiceItem.UnitBasePrice * orderAdjustment.Percentage.Value / 100 :
                                orderAdjustment.Amount ?? 0;
                        }

                        purchaseInvoiceItem.TotalBasePrice = purchaseInvoiceItem.UnitBasePrice * purchaseInvoiceItem.Quantity;
                        purchaseInvoiceItem.TotalDiscount = purchaseInvoiceItem.UnitDiscount * purchaseInvoiceItem.Quantity;
                        purchaseInvoiceItem.TotalSurcharge = purchaseInvoiceItem.UnitSurcharge * purchaseInvoiceItem.Quantity;
                        purchaseInvoiceItem.UnitPrice = purchaseInvoiceItem.UnitBasePrice - purchaseInvoiceItem.UnitDiscount + purchaseInvoiceItem.UnitSurcharge;

                        purchaseInvoiceItem.UnitVat = purchaseInvoiceItem.ExistVatRate ? purchaseInvoiceItem.UnitPrice * purchaseInvoiceItem.VatRate.Rate / 100 : 0;
                        purchaseInvoiceItem.UnitIrpf = purchaseInvoiceItem.ExistIrpfRate ? purchaseInvoiceItem.UnitPrice * purchaseInvoiceItem.IrpfRate.Rate / 100 : 0;
                        purchaseInvoiceItem.TotalVat = purchaseInvoiceItem.UnitVat * purchaseInvoiceItem.Quantity;
                        purchaseInvoiceItem.TotalExVat = purchaseInvoiceItem.UnitPrice * purchaseInvoiceItem.Quantity;
                        purchaseInvoiceItem.TotalIrpf = purchaseInvoiceItem.UnitIrpf * purchaseInvoiceItem.Quantity;
                        purchaseInvoiceItem.TotalIncVat = purchaseInvoiceItem.TotalExVat + purchaseInvoiceItem.TotalVat;
                        purchaseInvoiceItem.GrandTotal = purchaseInvoiceItem.TotalIncVat - purchaseInvoiceItem.TotalIrpf;
                    }
                }

                @this.TotalBasePrice = 0;
                @this.TotalDiscount = 0;
                @this.TotalSurcharge = 0;
                @this.TotalVat = 0;
                @this.TotalExVat = 0;
                @this.TotalIncVat = 0;
                @this.TotalIrpf = 0;
                @this.TotalExtraCharge = 0;
                @this.GrandTotal = 0;

                foreach (PurchaseInvoiceItem item in @this.ValidInvoiceItems)
                {
                    @this.TotalBasePrice += item.TotalBasePrice;
                    @this.TotalSurcharge += item.TotalSurcharge;
                    @this.TotalIrpf += item.TotalIrpf;
                    @this.TotalVat += item.TotalVat;
                    @this.TotalExVat += item.TotalExVat;
                    @this.TotalIncVat += item.TotalIncVat;
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

                foreach (var orderAdjustment in @this.OrderAdjustments)
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

                if (@this.ExistInvoiceDate && @this.ExistDerivedCurrency && @this.ExistBilledTo)
                {
                    @this.TotalBasePriceInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalBasePrice, @this.InvoiceDate, @this.DerivedCurrency, @this.BilledTo.PreferredCurrency), 2);
                    @this.TotalDiscountInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalDiscount, @this.InvoiceDate, @this.DerivedCurrency, @this.BilledTo.PreferredCurrency), 2);
                    @this.TotalSurchargeInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalSurcharge, @this.InvoiceDate, @this.DerivedCurrency, @this.BilledTo.PreferredCurrency), 2);
                    @this.TotalExtraChargeInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(totalExtraCharge, @this.InvoiceDate, @this.DerivedCurrency, @this.BilledTo.PreferredCurrency), 2);
                    @this.TotalFeeInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalFee, @this.InvoiceDate, @this.DerivedCurrency, @this.BilledTo.PreferredCurrency), 2);
                    @this.TotalShippingAndHandlingInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalShippingAndHandling, @this.InvoiceDate, @this.DerivedCurrency, @this.BilledTo.PreferredCurrency), 2);
                    @this.TotalExVatInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(totalExVat, @this.InvoiceDate, @this.DerivedCurrency, @this.BilledTo.PreferredCurrency), 2);
                    @this.TotalVatInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(totalVat, @this.InvoiceDate, @this.DerivedCurrency, @this.BilledTo.PreferredCurrency), 2);
                    @this.TotalIncVatInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(totalIncVat, @this.InvoiceDate, @this.DerivedCurrency, @this.BilledTo.PreferredCurrency), 2);
                    @this.TotalIrpfInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(totalIrpf, @this.InvoiceDate, @this.DerivedCurrency, @this.BilledTo.PreferredCurrency), 2);
                    @this.GrandTotalInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(grandTotal, @this.InvoiceDate, @this.DerivedCurrency, @this.BilledTo.PreferredCurrency), 2);
                }

                @this.TotalBasePrice = Rounder.RoundDecimal(@this.TotalBasePrice, 2);
                @this.TotalDiscount = Rounder.RoundDecimal(@this.TotalDiscount, 2);
                @this.TotalSurcharge = Rounder.RoundDecimal(@this.TotalSurcharge, 2);
                @this.TotalExtraCharge = Rounder.RoundDecimal(totalExtraCharge, 2);
                @this.TotalFee = Rounder.RoundDecimal(@this.TotalFee, 2);
                @this.TotalShippingAndHandling = Rounder.RoundDecimal(@this.TotalShippingAndHandling, 2);
                @this.TotalExVat = Rounder.RoundDecimal(totalExVat, 2);
                @this.TotalVat = Rounder.RoundDecimal(totalVat, 2);
                @this.TotalIncVat = Rounder.RoundDecimal(totalIncVat, 2);
                @this.TotalIrpf = Rounder.RoundDecimal(totalIrpf, 2);
                @this.GrandTotal = Rounder.RoundDecimal(grandTotal, 2);
            }
        }
    }
}
