// <copyright file="DiscountComponent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class DiscountComponent
    {
        public void AppsOnDerive(ObjectOnDerive method)
        {
            //var derivation = method.Derivation;

            //derivation.Validation.AssertAtLeastOne(this, M.DiscountComponent.Price, M.DiscountComponent.Percentage);
            //derivation.Validation.AssertExistsAtMostOne(this, M.DiscountComponent.Price, M.DiscountComponent.Percentage);

            //if (this.ExistPrice)
            //{
            //    if (!this.ExistCurrency)
            //    {
            //        this.Currency = this.PricedBy.PreferredCurrency;
            //    }

            //    derivation.Validation.AssertExists(this, M.BasePrice.Currency);
            //}

            //this.AppsOnDeriveVirtualProductPriceComponent();
        }

        //public void AppsOnDeriveVirtualProductPriceComponent()
        //{
        //    if (this.ExistProduct)
        //    {
        //        this.Product.AppsOnDeriveVirtualProductPriceComponent();
        //    }
        //}
    }
}
