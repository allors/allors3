// <copyright file="BasePrice.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class BasePrice
    {
        public void AppsOnPostDerive(ObjectOnPostDerive method) => method.Derivation.Validation.AssertAtLeastOne(this, this.M.BasePrice.Product, this.M.BasePrice.ProductFeature);

        public void AppsDelete(DeletableDelete method)
        {
            this.Product.RemoveBasePrice(this);

            this.ProductFeature.RemoveFromBasePrices(this);
        }
    }
}
