// <copyright file="ProductCategories.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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
                var extent = this.Session.Extent(this.ObjectType);
                extent.Filter.AddNot().AddExists(this.Meta.PrimaryParent);
                return extent;
            }
        }

        private UniquelyIdentifiableCache<ProductCategory> Cache => this.cache ??= new UniquelyIdentifiableCache<ProductCategory>(this.Session);
    }
}
