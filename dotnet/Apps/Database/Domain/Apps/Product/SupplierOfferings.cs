// <copyright file="SupplierOfferings.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class SupplierOfferings
    {
        public decimal PurchasePrice(Organisation supplier, DateTime orderDate, Part part = null)
        {
            decimal price = 0;

            if (supplier != null)
            {
                foreach (var supplierOffering in supplier.SupplierOfferingsWhereSupplier)
                {
                    if (supplierOffering.ExistPart && supplierOffering.Part.Equals(part))
                    {
                        if (supplierOffering.FromDate.Date <= orderDate.Date && (!supplierOffering.ExistThroughDate || supplierOffering.ThroughDate.Value.Date >= orderDate.Date))
                        {
                            price = supplierOffering.Price;
                            break;
                        }
                    }
                }
            }

            return price;
        }
    }
}
