// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuoteItemModel.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain.Print.SalesInvoiceModel
{
    using System.Globalization;

    public class OrderAdjustmentModel
    {
        public OrderAdjustmentModel(OrderAdjustment orderAdjustment)
        {
            var invoice = orderAdjustment.InvoiceWhereOrderAdjustment;

            this.AdjustmentTypeName = orderAdjustment.GetType().Name;
            this.Description = orderAdjustment.Description;

            if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
            {
                this.Amount = invoice.TotalDiscount.ToString("N2", new CultureInfo("nl-BE"));
            }

            if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
            {
                this.Amount = invoice.TotalSurcharge.ToString("N2", new CultureInfo("nl-BE"));
            }

            if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
            {
                this.Amount = invoice.TotalFee.ToString("N2", new CultureInfo("nl-BE"));
            }

            if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
            {
                this.Amount = invoice.TotalShippingAndHandling.ToString("N2", new CultureInfo("nl-BE"));
            }

            if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
            {
                var miscCharge = invoice.TotalExtraCharge - invoice.TotalFee - invoice.TotalShippingAndHandling;
                this.Amount = miscCharge.ToString("N2", new CultureInfo("nl-BE"));
            }
        }

        public string AdjustmentTypeName { get; set; }

        public string Amount { get; set; }

        public string Description { get; }
    }
}
