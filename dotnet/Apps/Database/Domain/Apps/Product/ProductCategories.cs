// <copyright file="ProductCategories.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class ProductCategories
    {
        private UniquelyIdentifiableCache<ProductCategory> cache;

        public Extent<ProductCategory> RootCategories
        {
            get
            {
                var extent = this.Transaction.Extent(this.ObjectType);
                extent.Filter.AddNot().AddExists(this.Meta.PrimaryParent);
                return extent;
            }
        }

        private UniquelyIdentifiableCache<ProductCategory> Cache => this.cache ??= new UniquelyIdentifiableCache<ProductCategory>(this.Transaction);
    }
}
