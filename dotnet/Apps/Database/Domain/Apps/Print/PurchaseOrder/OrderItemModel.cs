// <copyright file="OrderItemModel.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.PurchaseOrderModel
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public class OrderItemModel
    {
        public OrderItemModel(PurchaseOrderItem item, Dictionary<string, byte[]> imageByImageName)
        {
            this.Part = item.Part?.Name;
            var description = item.Description;

            if (string.IsNullOrEmpty(this.Part) && description == null)
            {
                description = item.InvoiceItemType.Name;
            }

            this.Description = description?.Split('\n');

            this.Quantity = item.QuantityOrdered;
            this.Price = Rounder.RoundDecimal(item.UnitPrice, 2).ToString("N2", new CultureInfo("nl-BE"));
            this.Amount = Rounder.RoundDecimal(item.TotalExVat, 2).ToString("N2", new CultureInfo("nl-BE"));
            this.Comment = item.Comment?.Split('\n');
            this.SupplierProductId = item.Part?.SupplierOfferingsWherePart?.FirstOrDefault(v => v.Supplier.Equals(item.PurchaseOrderWherePurchaseOrderItem.TakenViaSupplier))?.SupplierProductId;
        }

        public string Part { get; }

        public string[] Description { get; }

        public decimal Quantity { get; }

        public string Price { get; }

        public string Amount { get; }

        public string[] Comment { get; }

        public string SupplierProductId { get; }
    }
}
