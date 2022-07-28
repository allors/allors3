// <copyright file="PartCategories.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class PartCategories
    {
        private UniquelyIdentifiableCache<PartCategory> cache;

        public Extent<PartCategory> RootCategories
        {
            get
            {
                var extent = this.Transaction.Extent(this.ObjectType);
                extent.Filter.AddNot().AddExists(this.Meta.PrimaryParent);
                return extent;
            }
        }

        private UniquelyIdentifiableCache<PartCategory> Cache => this.cache ??= new UniquelyIdentifiableCache<PartCategory>(this.Transaction);
    }
}
