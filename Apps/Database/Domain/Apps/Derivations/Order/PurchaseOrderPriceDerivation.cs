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

    public class PurchaseOrderPriceDerivation : DomainDerivation
    {
        public PurchaseOrderPriceDerivation(M m) : base(m, new Guid("b553564c-45e1-495c-975a-90eb6fc67d5d")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(this.M.PurchaseOrder.DerivationTrigger),
                new AssociationPattern(this.M.PurchaseOrder.ValidOrderItems),
                new AssociationPattern(this.M.PurchaseOrder.TakenViaSupplier),
                new AssociationPattern(this.M.PurchaseOrder.OrderAdjustments),
                new AssociationPattern(this.M.PurchaseOrderItem.Part) { Steps =  new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem } },
                new AssociationPattern(this.M.PurchaseOrderItem.QuantityOrdered) { Steps =  new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem } },
                new AssociationPattern(this.M.PurchaseOrderItem.AssignedUnitPrice) { Steps =  new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem } },
                new AssociationPattern(this.M.PurchaseOrderItem.DiscountAdjustments) { Steps =  new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem } },
                new AssociationPattern(this.M.PurchaseOrderItem.SurchargeAdjustments) { Steps =  new IPropertyType[] {m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem } },
                new AssociationPattern(this.M.DiscountAdjustment.Percentage) { Steps =  new IPropertyType[] {m.DiscountAdjustment.PriceableWhereDiscountAdjustment, m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem}, OfType = m.PurchaseOrder.Class },
                new AssociationPattern(this.M.DiscountAdjustment.Percentage) { Steps =  new IPropertyType[] {m.OrderAdjustment.OrderWhereOrderAdjustment}, OfType = m.PurchaseOrder.Class },
                new AssociationPattern(this.M.DiscountAdjustment.Amount) { Steps =  new IPropertyType[] {m.DiscountAdjustment.PriceableWhereDiscountAdjustment, m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem}, OfType = m.PurchaseOrder.Class },
                new AssociationPattern(this.M.DiscountAdjustment.Amount) { Steps =  new IPropertyType[] {m.OrderAdjustment.OrderWhereOrderAdjustment}, OfType = m.PurchaseOrder.Class },
                new AssociationPattern(this.M.SurchargeAdjustment.Percentage) { Steps =  new IPropertyType[] {m.SurchargeAdjustment.PriceableWhereSurchargeAdjustment, m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem}, OfType = m.PurchaseOrder.Class },
                new AssociationPattern(this.M.SurchargeAdjustment.Percentage) { Steps =  new IPropertyType[] {m.OrderAdjustment.OrderWhereOrderAdjustment}, OfType = m.PurchaseOrder.Class },
                new AssociationPattern(this.M.SurchargeAdjustment.Amount) { Steps =  new IPropertyType[] {m.SurchargeAdjustment.PriceableWhereSurchargeAdjustment, m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem}, OfType = m.PurchaseOrder.Class },
                new AssociationPattern(this.M.SurchargeAdjustment.Amount) { Steps =  new IPropertyType[] {m.OrderAdjustment.OrderWhereOrderAdjustment}, OfType = m.PurchaseOrder.Class },
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
                        var order = purchaseOrderItem.PurchaseOrderWherePurchaseOrderItem;
                        if (order != null)
                        {
                            purchaseOrderItem.UnitBasePrice = new SupplierOfferings(@this.Strategy.Transaction).PurchasePrice(order.TakenViaSupplier, order.OrderDate, purchaseOrderItem.Part);
                        }
                    }
                    if (purchaseOrderItem.ExistUnitBasePrice)
                    {
                        purchaseOrderItem.DerivedVatRegime = purchaseOrderItem.AssignedVatRegime ?? purchaseOrderItem.PurchaseOrderWherePurchaseOrderItem.DerivedVatRegime;
                        purchaseOrderItem.VatRate = purchaseOrderItem.DerivedVatRegime?.VatRate;

                        purchaseOrderItem.DerivedIrpfRegime = purchaseOrderItem.AssignedIrpfRegime ?? purchaseOrderItem.PurchaseOrderWherePurchaseOrderItem.DerivedIrpfRegime;
                        purchaseOrderItem.IrpfRate = purchaseOrderItem.DerivedIrpfRegime?.IrpfRate;

                        foreach (OrderAdjustment orderAdjustment in purchaseOrderItem.DiscountAdjustments)
                        {
                            purchaseOrderItem.UnitDiscount += orderAdjustment.Percentage.HasValue ?
                                Math.Round(purchaseOrderItem.UnitBasePrice * orderAdjustment.Percentage.Value / 100, 2) :
                                orderAdjustment.Amount ?? 0;
                        }

                        foreach (OrderAdjustment orderAdjustment in purchaseOrderItem.SurchargeAdjustments)
                        {
                            purchaseOrderItem.UnitSurcharge += orderAdjustment.Percentage.HasValue ?
                                Math.Round(purchaseOrderItem.UnitBasePrice * orderAdjustment.Percentage.Value / 100, 2) :
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
