// <copyright file="ProductCategory.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class ProductCategory
    {
        public override string ToString() => this.Name;

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistCatScope)
            {
                this.CatScope = new Scopes(this.Strategy.Transaction).Public;
            }
        }

        public void AppsOnInit(ObjectOnInit method) => this.CategoryImage ??= this.Transaction().GetSingleton().Settings.NoImageAvailableImage;
    }
}
