// <copyright file="PurchaseOrderPriceDerivation.cs" company="Allors bvba">
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

    public class PurchaseOrderPriceRule : Rule
    {
        public PurchaseOrderPriceRule(MetaPopulation m) : base(m, new Guid("b553564c-45e1-495c-975a-90eb6fc67d5d")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrder.RolePattern(v => v.DerivationTrigger),
                m.PurchaseOrder.RolePattern(v => v.ValidOrderItems),
                m.PurchaseOrder.RolePattern(v => v.TakenViaSupplier),
                m.PurchaseOrder.RolePattern(v => v.OrderAdjustments),
                m.PurchaseOrderItem.RolePattern(v => v.Part, v => v.PurchaseOrderWherePurchaseOrderItem),
                m.PurchaseOrderItem.RolePattern(v => v.QuantityOrdered, v => v.PurchaseOrderWherePurchaseOrderItem),
                m.PurchaseOrderItem.RolePattern(v => v.AssignedUnitPrice, v => v.PurchaseOrderWherePurchaseOrderItem),
                m.PurchaseOrderItem.RolePattern(v => v.DiscountAdjustments, v => v.PurchaseOrderWherePurchaseOrderItem),
                m.PurchaseOrderItem.RolePattern(v => v.SurchargeAdjustments, v => v.PurchaseOrderWherePurchaseOrderItem),
                m.DiscountAdjustment.RolePattern(v => v.Percentage, v => v.PriceableWhereDiscountAdjustment.Priceable.AsPurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem, m.PurchaseOrder),
                m.DiscountAdjustment.RolePattern(v => v.Percentage, v => v.OrderWhereOrderAdjustment, m.PurchaseOrder),
                m.DiscountAdjustment.RolePattern(v => v.Amount, v => v.PriceableWhereDiscountAdjustment.Priceable.AsPurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem, m.PurchaseOrder),
                m.DiscountAdjustment.RolePattern(v => v.Amount, v => v.OrderWhereOrderAdjustment, m.PurchaseOrder),
                m.SurchargeAdjustment.RolePattern(v => v.Percentage, v => v.PriceableWhereSurchargeAdjustment.Priceable.AsPurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem, m.PurchaseOrder),
                m.SurchargeAdjustment.RolePattern(v => v.Percentage, v => v.OrderWhereOrderAdjustment, m.PurchaseOrder),
                m.SurchargeAdjustment.RolePattern(v => v.Amount, v => v.PriceableWhereSurchargeAdjustment.Priceable.AsPurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem, m.PurchaseOrder),
                m.SurchargeAdjustment.RolePattern(v => v.Amount, v => v.OrderWhereOrderAdjustment, m.PurchaseOrder),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseOrder>())
            {
                foreach (PurchaseOrderItem purchaseOrderItem in @this.ValidOrderItems)
                {
                    purchaseOrderItem.UnitBasePrice = 0;
                    purchaseOrderItem.UnitDiscount = 0;
                    purchaseOrderItem.UnitSurcharge = 0;

                    if (purchaseOrderItem.AssignedUnitPrice.HasValue)
                    {
                        purchaseOrderItem.UnitBasePrice = purchaseOrderItem.AssignedUnitPrice.Value;
                        purchaseOrderItem.UnitPrice = purchaseOrderItem.AssignedUnitPrice.Value;
                    }
                    else
                    {
                        purchaseOrderItem.UnitBasePrice = new SupplierOfferings(@this.Strategy.Transaction).PurchasePrice(@this.TakenViaSupplier, @this.OrderDate, purchaseOrderItem.Part);
                    }

                    if (purchaseOrderItem.ExistUnitBasePrice)
                    {
                        foreach (OrderAdjustment orderAdjustment in purchaseOrderItem.DiscountAdjustments)
                        {
                            purchaseOrderItem.UnitDiscount += orderAdjustment.Percentage.HasValue ?
                                purchaseOrderItem.UnitBasePrice * orderAdjustment.Percentage.Value / 100 :
                                orderAdjustment.Amount ?? 0;
                        }

                        foreach (OrderAdjustment orderAdjustment in purchaseOrderItem.SurchargeAdjustments)
                        {
                            purchaseOrderItem.UnitSurcharge += orderAdjustment.Percentage.HasValue ?
                                purchaseOrderItem.UnitBasePrice * orderAdjustment.Percentage.Value / 100 :
                                orderAdjustment.Amount ?? 0;
                        }

                        purchaseOrderItem.TotalBasePrice = purchaseOrderItem.UnitBasePrice * purchaseOrderItem.QuantityOrdered;
                        purchaseOrderItem.TotalDiscount = purchaseOrderItem.UnitDiscount * purchaseOrderItem.QuantityOrdered;
                        purchaseOrderItem.TotalSurcharge = purchaseOrderItem.UnitSurcharge * purchaseOrderItem.QuantityOrdered;
                        purchaseOrderItem.UnitPrice = purchaseOrderItem.UnitBasePrice - purchaseOrderItem.UnitDiscount + purchaseOrderItem.UnitSurcharge;

                        purchaseOrderItem.UnitVat = purchaseOrderItem.ExistVatRate ? purchaseOrderItem.UnitPrice * purchaseOrderItem.VatRate.Rate / 100 : 0;
                        purchaseOrderItem.UnitIrpf = purchaseOrderItem.ExistIrpfRate ? purchaseOrderItem.UnitPrice * purchaseOrderItem.IrpfRate.Rate / 100 : 0;
                        purchaseOrderItem.TotalVat = purchaseOrderItem.UnitVat * purchaseOrderItem.QuantityOrdered;
                        purchaseOrderItem.TotalExVat = purchaseOrderItem.UnitPrice * purchaseOrderItem.QuantityOrdered;
                        purchaseOrderItem.TotalIrpf = purchaseOrderItem.UnitIrpf * purchaseOrderItem.QuantityOrdered;
                        purchaseOrderItem.TotalIncVat = purchaseOrderItem.TotalExVat + @this.TotalVat;
                        purchaseOrderItem.GrandTotal = purchaseOrderItem.TotalIncVat - @this.TotalIrpf;
                    }
                }

                @this.TotalBasePrice = 0;
                @this.TotalDiscount = 0;
                @this.TotalSurcharge = 0;
                @this.TotalVat = 0;
                @this.TotalIrpf = 0;
                @this.TotalExVat = 0;
                @this.TotalExtraCharge = 0;
                @this.TotalIncVat = 0;
                @this.GrandTotal = 0;

                foreach (PurchaseOrderItem orderItem in @this.ValidOrderItems)
                {
                    @this.TotalBasePrice += orderItem.TotalBasePrice;
                    @this.TotalDiscount += orderItem.TotalDiscount;
                    @this.TotalSurcharge += orderItem.TotalSurcharge;
                    @this.TotalVat += orderItem.TotalVat;
                    @this.TotalIrpf += orderItem.TotalIrpf;
                    @this.TotalExVat += orderItem.TotalExVat;
                    @this.TotalIncVat += orderItem.TotalIncVat;
                    @this.GrandTotal += orderItem.GrandTotal;
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

                @this.TotalBasePriceInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalBasePrice, @this.OrderDate, @this.DerivedCurrency, @this.OrderedBy.PreferredCurrency), 2);
                @this.TotalDiscountInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalDiscount, @this.OrderDate, @this.DerivedCurrency, @this.OrderedBy.PreferredCurrency), 2);
                @this.TotalSurchargeInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalSurcharge, @this.OrderDate, @this.DerivedCurrency, @this.OrderedBy.PreferredCurrency), 2);
                @this.TotalExVatInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalExVat, @this.OrderDate, @this.DerivedCurrency, @this.OrderedBy.PreferredCurrency), 2);
                @this.TotalVatInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalVat, @this.OrderDate, @this.DerivedCurrency, @this.OrderedBy.PreferredCurrency), 2);
                @this.TotalIncVatInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalIncVat, @this.OrderDate, @this.DerivedCurrency, @this.OrderedBy.PreferredCurrency), 2);
                @this.TotalIrpfInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.TotalIrpf, @this.OrderDate, @this.DerivedCurrency, @this.OrderedBy.PreferredCurrency), 2);
                @this.GrandTotalInPreferredCurrency = Rounder.RoundDecimal(Currencies.ConvertCurrency(@this.GrandTotal, @this.OrderDate, @this.DerivedCurrency, @this.OrderedBy.PreferredCurrency), 2);

                @this.TotalBasePrice = Rounder.RoundDecimal(@this.TotalBasePrice, 2);
                @this.TotalDiscount = Rounder.RoundDecimal(@this.TotalDiscount, 2);
                @this.TotalSurcharge = Rounder.RoundDecimal(@this.TotalSurcharge, 2);
                @this.TotalExtraCharge = Rounder.RoundDecimal(fee + shipping + miscellaneous, 2);
                @this.TotalExVat = Rounder.RoundDecimal(@this.TotalExVat - discount + surcharge + fee + shipping + miscellaneous, 2);
                @this.TotalVat = Rounder.RoundDecimal(@this.TotalVat - discountVat + surchargeVat + feeVat + shippingVat + miscellaneousVat, 2);
                @this.TotalIncVat = Rounder.RoundDecimal(@this.TotalIncVat - discount - discountVat + surcharge + surchargeVat + fee + feeVat + shipping + shippingVat + miscellaneous + miscellaneousVat, 2);
                @this.TotalIrpf = Rounder.RoundDecimal(@this.TotalIrpf + discountIrpf - surchargeIrpf - feeIrpf - shippingIrpf - miscellaneousIrpf, 2);
                @this.GrandTotal = Rounder.RoundDecimal(@this.TotalIncVat - @this.TotalIrpf, 2);
            }
        }
    }
}
