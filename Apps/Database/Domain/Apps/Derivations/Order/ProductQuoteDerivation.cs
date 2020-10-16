// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class ProductQuoteDerivation : DomainDerivation
    {
        public ProductQuoteDerivation(M m) : base(m, new Guid("6F421122-37A0-4F8E-A08A-996F16CC0218")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(this.M.ProductQuote.Class)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var productQuote in matches.Cast<ProductQuote>())
            {
                productQuote.ValidQuoteItems = productQuote.QuoteItems.Where(v => v.IsValid).ToArray();

                var currentPriceComponents = new PriceComponents(cycle.Session).CurrentPriceComponents(productQuote.IssueDate);

                var quantityOrderedByProduct = productQuote.ValidQuoteItems
                    .Where(v => v.ExistProduct)
                    .GroupBy(v => v.Product)
                    .ToDictionary(v => v.Key, v => v.Sum(w => w.Quantity));

                // First run to calculate price
                foreach (QuoteItem quoteItem in productQuote.ValidQuoteItems)
                {
                    decimal quantityOrdered = 0;

                    if (quoteItem.ExistProduct)
                    {
                        quantityOrderedByProduct.TryGetValue(quoteItem.Product, out quantityOrdered);
                    }

                    foreach (QuoteItem featureItem in quoteItem.QuotedWithFeatures)
                    {
                        featureItem.Quantity = quoteItem.Quantity;
                        CalculatePrices(productQuote, featureItem, currentPriceComponents, quantityOrdered, 0);
                    }

                    CalculatePrices(productQuote, quoteItem, currentPriceComponents, quantityOrdered, 0);
                }

                var totalBasePriceByProduct = productQuote.QuoteItems
                    .Where(v => v.ExistProduct)
                    .GroupBy(v => v.Product)
                    .ToDictionary(v => v.Key, v => v.Sum(w => w.TotalBasePrice));

                // Second run to calculate price (because of order value break)
                foreach (QuoteItem quoteItem in productQuote.ValidQuoteItems)
                {
                    decimal quantityOrdered = 0;
                    decimal totalBasePrice = 0;

                    if (quoteItem.ExistProduct)
                    {
                        quantityOrderedByProduct.TryGetValue(quoteItem.Product, out quantityOrdered);
                        totalBasePriceByProduct.TryGetValue(quoteItem.Product, out totalBasePrice);
                    }

                    foreach (QuoteItem featureItem in quoteItem.QuotedWithFeatures)
                    {
                        CalculatePrices(productQuote, featureItem, currentPriceComponents, quantityOrdered, totalBasePrice);
                    }

                    CalculatePrices(productQuote, quoteItem, currentPriceComponents, quantityOrdered, totalBasePrice);
                }

                // SalesOrderItem Derivations and Validations
                foreach (QuoteItem quoteItem in productQuote.ValidQuoteItems)
                {
                    var isSubTotalItem = quoteItem.ExistInvoiceItemType && (quoteItem.InvoiceItemType.IsProductItem || quoteItem.InvoiceItemType.IsPartItem);
                    if (isSubTotalItem)
                    {
                        if (quoteItem.Quantity == 0)
                        {
                            cycle.Validation.AddError($"{quoteItem} {this.M.QuoteItem.Quantity} Quantity is Required");
                        }
                    }
                    else
                    {
                        if (quoteItem.UnitPrice == 0)
                        {
                            cycle.Validation.AddError($"{quoteItem} {this.M.QuoteItem.UnitPrice} Price is Required");
                        }
                    }
                }

                // Calculate Totals
                productQuote.TotalBasePrice = 0;
                productQuote.TotalDiscount = 0;
                productQuote.TotalSurcharge = 0;
                productQuote.TotalExVat = 0;
                productQuote.TotalFee = 0;
                productQuote.TotalShippingAndHandling = 0;
                productQuote.TotalExtraCharge = 0;
                productQuote.TotalVat = 0;
                productQuote.TotalIrpf = 0;
                productQuote.TotalIncVat = 0;
                productQuote.TotalListPrice = 0;
                productQuote.GrandTotal = 0;

                foreach (QuoteItem quoteItem in productQuote.ValidQuoteItems)
                {
                    if (!quoteItem.ExistQuoteItemWhereQuotedWithFeature)
                    {
                        productQuote.TotalBasePrice += quoteItem.TotalBasePrice;
                        productQuote.TotalDiscount += quoteItem.TotalDiscount;
                        productQuote.TotalSurcharge += quoteItem.TotalSurcharge;
                        productQuote.TotalExVat += quoteItem.TotalExVat;
                        productQuote.TotalVat += quoteItem.TotalVat;
                        productQuote.TotalIrpf += quoteItem.TotalIrpf;
                        productQuote.TotalIncVat += quoteItem.TotalIncVat;
                        productQuote.TotalListPrice += quoteItem.TotalExVat;
                        productQuote.GrandTotal += quoteItem.GrandTotal;
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

                foreach (OrderAdjustment orderAdjustment in productQuote.OrderAdjustments)
                {
                    if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
                    {
                        discount = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(productQuote.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        productQuote.TotalDiscount += discount;

                        if (productQuote.ExistVatRegime)
                        {
                            discountVat = Math.Round(discount * productQuote.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (productQuote.ExistIrpfRegime)
                        {
                            discountIrpf = Math.Round(discount * productQuote.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                    {
                        surcharge = orderAdjustment.Percentage.HasValue ?
                                            Math.Round(productQuote.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                            orderAdjustment.Amount ?? 0;

                        productQuote.TotalSurcharge += surcharge;

                        if (productQuote.ExistVatRegime)
                        {
                            surchargeVat = Math.Round(surcharge * productQuote.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (productQuote.ExistIrpfRegime)
                        {
                            surchargeIrpf = Math.Round(surcharge * productQuote.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                    {
                        fee = orderAdjustment.Percentage.HasValue ?
                                    Math.Round(productQuote.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                    orderAdjustment.Amount ?? 0;

                        productQuote.TotalFee += fee;

                        if (productQuote.ExistVatRegime)
                        {
                            feeVat = Math.Round(fee * productQuote.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (productQuote.ExistIrpfRegime)
                        {
                            feeIrpf = Math.Round(fee * productQuote.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                    {
                        shipping = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(productQuote.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        productQuote.TotalShippingAndHandling += shipping;

                        if (productQuote.ExistVatRegime)
                        {
                            shippingVat = Math.Round(shipping * productQuote.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (productQuote.ExistIrpfRegime)
                        {
                            shippingIrpf = Math.Round(shipping * productQuote.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                    {
                        miscellaneous = orderAdjustment.Percentage.HasValue ?
                                        Math.Round(productQuote.TotalExVat * orderAdjustment.Percentage.Value / 100, 2) :
                                        orderAdjustment.Amount ?? 0;

                        productQuote.TotalExtraCharge += miscellaneous;

                        if (productQuote.ExistVatRegime)
                        {
                            miscellaneousVat = Math.Round(miscellaneous * productQuote.VatRegime.VatRate.Rate / 100, 2);
                        }

                        if (productQuote.ExistIrpfRegime)
                        {
                            miscellaneousIrpf = Math.Round(miscellaneous * productQuote.IrpfRegime.IrpfRate.Rate / 100, 2);
                        }
                    }
                }

                productQuote.TotalExtraCharge = fee + shipping + miscellaneous;

                productQuote.TotalExVat = productQuote.TotalExVat - discount + surcharge + fee + shipping + miscellaneous;
                productQuote.TotalVat = productQuote.TotalVat - discountVat + surchargeVat + feeVat + shippingVat + miscellaneousVat;
                productQuote.TotalIncVat = productQuote.TotalIncVat - discount - discountVat + surcharge + surchargeVat + fee + feeVat + shipping + shippingVat + miscellaneous + miscellaneousVat;
                productQuote.TotalIrpf = productQuote.TotalIrpf + discountIrpf - surchargeIrpf - feeIrpf - shippingIrpf - miscellaneousIrpf;
                productQuote.GrandTotal = productQuote.TotalIncVat - productQuote.TotalIrpf;

                //// Only take into account items for which there is data at the item level.
                //// Skip negative sales.
                decimal totalUnitBasePrice = 0;
                decimal totalListPrice = 0;

                foreach (QuoteItem item1 in productQuote.ValidQuoteItems)
                {
                    if (item1.TotalExVat > 0)
                    {
                        totalUnitBasePrice += item1.UnitBasePrice;
                        totalListPrice += item1.UnitPrice;
                    }
                }

                DeriveWorkflow(productQuote);

                Sync(productQuote);

                productQuote.ResetPrintDocument();

                var SetReadyPermission = new Permissions(productQuote.Strategy.Session).Get(productQuote.Meta.ObjectType, productQuote.Meta.SetReadyForProcessing);

                if (productQuote.QuoteState.IsCreated)
                {
                    if (productQuote.ExistValidQuoteItems)
                    {
                        productQuote.RemoveDeniedPermission(SetReadyPermission);
                    }
                    else
                    {
                        productQuote.AddDeniedPermission(SetReadyPermission);
                    }
                }

                var deletePermission = new Permissions(productQuote.Strategy.Session).Get(productQuote.Meta.ObjectType, productQuote.Meta.Delete);
                if (productQuote.IsDeletable)
                {
                    productQuote.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    productQuote.AddDeniedPermission(deletePermission);
                }
            }

            void CalculatePrices(
                ProductQuote productQuote,
                QuoteItem quoteItem,
                PriceComponent[] currentPriceComponents,
                decimal quantityOrdered,
                decimal totalBasePrice)
            {
                var quoteItemDeriveRoles = quoteItem;

                var currentGenericOrProductOrFeaturePriceComponents = Array.Empty<PriceComponent>();
                if (quoteItem.ExistProduct)
                {
                    currentGenericOrProductOrFeaturePriceComponents = quoteItem.Product.GetPriceComponents(currentPriceComponents);
                }
                else if (quoteItem.ExistProductFeature)
                {
                    currentGenericOrProductOrFeaturePriceComponents = quoteItem.ProductFeature.GetPriceComponents(quoteItem.QuoteItemWhereQuotedWithFeature.Product, currentPriceComponents);
                }

                var priceComponents = currentGenericOrProductOrFeaturePriceComponents.Where(
                    v => PriceComponents.AppsIsApplicable(
                        new PriceComponents.IsApplicable
                        {
                            PriceComponent = v,
                            Customer = productQuote.Receiver,
                            Product = quoteItem.Product,
                            QuantityOrdered = quantityOrdered,
                            ValueOrdered = totalBasePrice,
                        })).ToArray();

                var unitBasePrice = priceComponents.OfType<BasePrice>().Min(v => v.Price);

                // Calculate Unit Price (with Discounts and Surcharges)
                if (quoteItem.AssignedUnitPrice.HasValue)
                {
                    quoteItemDeriveRoles.UnitBasePrice = unitBasePrice ?? quoteItem.AssignedUnitPrice.Value;
                    quoteItemDeriveRoles.UnitDiscount = 0;
                    quoteItemDeriveRoles.UnitSurcharge = 0;
                    quoteItemDeriveRoles.UnitPrice = quoteItem.AssignedUnitPrice.Value;
                }
                else
                {
                    quoteItemDeriveRoles.UnitBasePrice = unitBasePrice.Value;

                    quoteItemDeriveRoles.UnitDiscount = priceComponents.OfType<DiscountComponent>().Sum(
                        v => v.Percentage.HasValue
                                 ? Math.Round(quoteItem.UnitBasePrice * v.Percentage.Value / 100, 2)
                                 : v.Price ?? 0);

                    quoteItemDeriveRoles.UnitSurcharge = priceComponents.OfType<SurchargeComponent>().Sum(
                        v => v.Percentage.HasValue
                                 ? Math.Round(quoteItem.UnitBasePrice * v.Percentage.Value / 100, 2)
                                 : v.Price ?? 0);

                    quoteItemDeriveRoles.UnitPrice = quoteItem.UnitBasePrice - quoteItem.UnitDiscount + quoteItem.UnitSurcharge;

                    foreach (OrderAdjustment orderAdjustment in quoteItem.DiscountAdjustments)
                    {
                        quoteItemDeriveRoles.UnitDiscount += orderAdjustment.Percentage.HasValue ?
                            Math.Round(quoteItem.UnitPrice * orderAdjustment.Percentage.Value / 100, 2) :
                            orderAdjustment.Amount ?? 0;
                    }

                    foreach (OrderAdjustment orderAdjustment in quoteItem.SurchargeAdjustments)
                    {
                        quoteItemDeriveRoles.UnitSurcharge += orderAdjustment.Percentage.HasValue ?
                            Math.Round(quoteItem.UnitPrice * orderAdjustment.Percentage.Value / 100, 2) :
                            orderAdjustment.Amount ?? 0;
                    }

                    quoteItemDeriveRoles.UnitPrice = quoteItem.UnitBasePrice - quoteItem.UnitDiscount + quoteItem.UnitSurcharge;
                }

                foreach (QuoteItem featureItem in quoteItem.QuotedWithFeatures)
                {
                    quoteItemDeriveRoles.UnitBasePrice += featureItem.UnitBasePrice;
                    quoteItemDeriveRoles.UnitPrice += featureItem.UnitPrice;
                    quoteItemDeriveRoles.UnitDiscount += featureItem.UnitDiscount;
                    quoteItemDeriveRoles.UnitSurcharge += featureItem.UnitSurcharge;
                }

                quoteItemDeriveRoles.UnitVat = quoteItem.ExistVatRate ? quoteItem.UnitPrice * quoteItem.VatRate.Rate / 100 : 0;
                quoteItemDeriveRoles.UnitIrpf = quoteItem.ExistIrpfRate ? quoteItem.UnitPrice * quoteItem.IrpfRate.Rate / 100 : 0;

                // Calculate Totals
                quoteItemDeriveRoles.TotalBasePrice = quoteItem.UnitBasePrice * quoteItem.Quantity;
                quoteItemDeriveRoles.TotalDiscount = quoteItem.UnitDiscount * quoteItem.Quantity;
                quoteItemDeriveRoles.TotalSurcharge = quoteItem.UnitSurcharge * quoteItem.Quantity;
                quoteItemDeriveRoles.TotalPriceAdjustment = quoteItem.TotalSurcharge - quoteItem.TotalDiscount;

                if (quoteItem.TotalBasePrice > 0)
                {
                    quoteItemDeriveRoles.TotalDiscountAsPercentage = Math.Round(quoteItem.TotalDiscount / quoteItem.TotalBasePrice * 100, 2);
                    quoteItemDeriveRoles.TotalSurchargeAsPercentage = Math.Round(quoteItem.TotalSurcharge / quoteItem.TotalBasePrice * 100, 2);
                }
                else
                {
                    quoteItemDeriveRoles.TotalDiscountAsPercentage = 0;
                    quoteItemDeriveRoles.TotalSurchargeAsPercentage = 0;
                }

                quoteItemDeriveRoles.TotalExVat = quoteItem.UnitPrice * quoteItem.Quantity;
                quoteItemDeriveRoles.TotalVat = quoteItem.UnitVat * quoteItem.Quantity;
                quoteItemDeriveRoles.TotalIncVat = quoteItem.TotalExVat + quoteItem.TotalVat;
                quoteItemDeriveRoles.TotalIrpf = quoteItem.UnitIrpf * quoteItem.Quantity;
                quoteItemDeriveRoles.GrandTotal = quoteItem.TotalIncVat - quoteItem.TotalIrpf;
            }

            void Sync(ProductQuote productQuote)
            {
                // session.Prefetch(this.SyncPrefetch, this);
                foreach (QuoteItem quoteItem in productQuote.QuoteItems)
                {
                    quoteItem.Sync(productQuote);
                }
            }

            void DeriveWorkflow(ProductQuote productQuote)
            {
                productQuote.WorkItemDescription = $"ProductQuote: {productQuote.QuoteNumber} [{productQuote.Issuer?.PartyName}]";

                var openTasks = productQuote.TasksWhereWorkItem.Where(v => !v.ExistDateClosed).ToArray();

                if (productQuote.QuoteState.IsAwaitingApproval)
                {
                    if (!openTasks.OfType<ProductQuoteApproval>().Any())
                    {
                        new ProductQuoteApprovalBuilder(productQuote.Session()).WithProductQuote(productQuote).Build();
                    }
                }
            }
        }
    }
}
