// <copyright file="Catalogue.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class Catalogue
    {
        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistCatScope)
            {
                this.CatScope = new Scopes(this.Strategy.Transaction).Public;
            }

            if (!this.ExistCatalogueImage)
            {
                this.CatalogueImage = this.strategy.Transaction.GetSingleton().Settings.NoImageAvailableImage;
            }
        }
    }
}
