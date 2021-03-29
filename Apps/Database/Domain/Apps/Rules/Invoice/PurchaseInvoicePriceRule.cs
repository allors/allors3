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
    using Resources;

    public class PurchaseInvoicePriceRule : Rule
    {
        public PurchaseInvoicePriceRule(M m) : base(m, new Guid("cde03f3a-9151-4bb7-a3a5-5bc238f3cc54")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PurchaseInvoice, m.PurchaseInvoice.PurchaseInvoiceState),
                new RolePattern(m.PurchaseInvoice, m.PurchaseInvoice.DerivationTrigger),
                new RolePattern(m.PurchaseInvoice, m.PurchaseInvoice.ValidInvoiceItems),
                new RolePattern(m.PurchaseInvoice, m.PurchaseInvoice.BilledFrom),
                new RolePattern(m.PurchaseInvoice, m.PurchaseInvoice.OrderAdjustments),
                new RolePattern(m.PurchaseInvoice, m.PurchaseInvoice.InvoiceDate),
                new RolePattern(m.PurchaseInvoiceItem, m.PurchaseInvoiceItem.Part) { Steps =  new IPropertyType[] {m.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem } },
                new RolePattern(m.PurchaseInvoiceItem, m.PurchaseInvoiceItem.Quantity) { Steps =  new IPropertyType[] {m.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem } },
                new RolePattern(m.PurchaseInvoiceItem, m.PurchaseInvoiceItem.AssignedUnitPrice) { Steps =  new IPropertyType[] {m.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem } },
                new RolePattern(m.PurchaseInvoiceItem, m.PurchaseInvoiceItem.DiscountAdjustments) { Steps =  new IPropertyType[] {m.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem } },
                new RolePattern(m.PurchaseInvoiceItem, m.PurchaseInvoiceItem.SurchargeAdjustments) { Steps =  new IPropertyType[] {m.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem } },
                new RolePattern(m.DiscountAdjustment, m.DiscountAdjustment.Percentage) { Steps =  new IPropertyType[] {m.DiscountAdjustment.PriceableWhereDiscountAdjustment, m.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem}, OfType = m.PurchaseInvoice.Class },
                new RolePattern(m.DiscountAdjustment, m.DiscountAdjustment.Percentage) { Steps =  new IPropertyType[] {m.OrderAdjustment.InvoiceWhereOrderAdjustment}, OfType = m.PurchaseInvoice.Class },
                new RolePattern(m.DiscountAdjustment, m.DiscountAdjustment.Amount) { Steps =  new IPropertyType[] {m.DiscountAdjustment.PriceableWhereDiscountAdjustment, m.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem}, OfType = m.PurchaseInvoice.Class },
                new RolePattern(m.DiscountAdjustment, m.DiscountAdjustment.Amount) { Steps =  new IPropertyType[] {m.OrderAdjustment.InvoiceWhereOrderAdjustment}, OfType = m.PurchaseInvoice.Class },
                new RolePattern(m.SurchargeAdjustment, m.SurchargeAdjustment.Percentage) { Steps =  new IPropertyType[] {m.SurchargeAdjustment.PriceableWhereSurchargeAdjustment, m.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem}, OfType = m.PurchaseInvoice.Class },
                new RolePattern(m.SurchargeAdjustment, m.SurchargeAdjustment.Percentage) { Steps =  new IPropertyType[] {m.OrderAdjustment.InvoiceWhereOrderAdjustment}, OfType = m.PurchaseInvoice.Class },
                new RolePattern(m.SurchargeAdjustment, m.SurchargeAdjustment.Amount) { Steps =  new IPropertyType[] {m.SurchargeAdjustment.PriceableWhereSurchargeAdjustment, m.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem}, OfType = m.PurchaseInvoice.Class },
                new RolePattern(m.SurchargeAdjustment, m.SurchargeAdjustment.Amount) { Steps =  new IPropertyType[] {m.OrderAdjustment.InvoiceWhereOrderAdjustment}, OfType = m.PurchaseInvoice.Class },
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
                                Math.Round(purchaseInvoiceItem.UnitBasePrice * orderAdjustment.Percentage.Value / 100, 2) :
                                orderAdjustment.Amount ?? 0;
                        }

                        foreach (OrderAdjustment orderAdjustment in purchaseInvoiceItem.SurchargeAdjustments)
                        {
                            purchaseInvoiceItem.UnitSurcharge += orderAdjustment.Percentage.HasValue ?
                                Math.Round(purchaseInvoiceItem.UnitBasePrice * orderAdjustment.Percentage.Value / 100, 2) :
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

                //AppsOnDeriveInvoiceTotals
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
        }
    }
}
