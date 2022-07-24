// <copyright file="Products.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;
    using Meta;
    using DateTime = System.DateTime;

    public partial class Products
    {
        private static decimal AppsCatalogPrice(
            SalesOrder salesOrder,
            SalesInvoice salesInvoice,
            Product product,
            DateTime date)
        {
            var m = salesOrder.Strategy.Transaction.Database.Services.Get<MetaPopulation>();

            var productBasePrice = 0M;
            var productDiscount = 0M;
            var productSurcharge = 0M;

            var baseprices = Array.Empty<PriceComponent>();
            if (product.ExistBasePrices)
            {
                baseprices = product.BasePrices.ToArray();
            }

            var party = salesOrder != null ? salesOrder.ShipToCustomer : salesInvoice?.BillToCustomer;

            foreach (BasePrice priceComponent in baseprices)
            {
                if (priceComponent.FromDate <= date &&
                    (!priceComponent.ExistThroughDate || priceComponent.ThroughDate >= date) &&
                    PriceComponents.AppsIsApplicable(new PriceComponents.IsApplicable
                    {
                        PriceComponent = priceComponent,
                        Customer = party,
                        Product = product,
                        SalesOrder = salesOrder,
                        SalesInvoice = salesInvoice,
                    }) &&
                    priceComponent.ExistPrice)
                {
                    if (productBasePrice == 0 || priceComponent.Price < productBasePrice)
                    {
                        productBasePrice = priceComponent.Price ?? 0;
                    }
                }
            }

            var currentPriceComponents = new PriceComponents(product.Strategy.Transaction).CurrentPriceComponents(date);

            foreach (var priceComponent in product.GetPriceComponents(currentPriceComponents))
            {
                if (priceComponent.Strategy.Class.Equals(m.DiscountComponent) || priceComponent.Strategy.Class.Equals(m.SurchargeComponent))
                {
                    if (PriceComponents.AppsIsApplicable(new PriceComponents.IsApplicable
                    {
                        PriceComponent = priceComponent,
                        Customer = party,
                        Product = product,
                        SalesOrder = salesOrder,
                        SalesInvoice = salesInvoice,
                    }))
                    {
                        if (priceComponent.Strategy.Class.Equals(m.DiscountComponent))
                        {
                            var discountComponent = (DiscountComponent)priceComponent;
                            decimal discount;

                            if (discountComponent.Price.HasValue)
                            {
                                discount = discountComponent.Price.Value;
                                productDiscount += discount;
                            }
                            else
                            {
                                var percentage = discountComponent.Percentage ?? 0;
                                discount = Rounder.RoundDecimal(productBasePrice * percentage / 100, 2);
                                productDiscount += discount;
                            }
                        }

                        if (priceComponent.Strategy.Class.Equals(m.SurchargeComponent))
                        {
                            var surchargeComponent = (SurchargeComponent)priceComponent;
                            decimal surcharge;

                            if (surchargeComponent.Price.HasValue)
                            {
                                surcharge = surchargeComponent.Price.Value;
                                productSurcharge += surcharge;
                            }
                            else
                            {
                                var percentage = surchargeComponent.Percentage ?? 0;
                                surcharge = Rounder.RoundDecimal(productBasePrice * percentage / 100, 2);
                                productSurcharge += surcharge;
                            }
                        }
                    }
                }
            }

            return productBasePrice - productDiscount + productSurcharge;
        }
    }
}
