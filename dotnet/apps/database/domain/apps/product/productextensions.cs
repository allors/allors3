// <copyright file="ProductExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public static partial class ProductExtensions
    {
        public static PriceComponent[] GetPriceComponents(this Product @this, PriceComponent[] currentPriceComponents)
        {
            if (currentPriceComponents == null)
            {
                return new PriceComponent[] { };
            }

            var genericPriceComponents = currentPriceComponents.Where(priceComponent => !priceComponent.ExistProduct && !priceComponent.ExistPart && !priceComponent.ExistProductFeature).ToArray();

            var exclusiveProductPriceComponents = currentPriceComponents.Where(priceComponent => priceComponent.Product?.Equals(@this) == true && !priceComponent.ExistProductFeature).ToArray();

            if (exclusiveProductPriceComponents.Length > 0)
            {
                return exclusiveProductPriceComponents.Union(genericPriceComponents).ToArray();
            }

            if (@this.ExistProductWhereVariant)
            {
                return currentPriceComponents.Where(priceComponent => priceComponent.Product?.Equals(@this.ProductWhereVariant) == true && !priceComponent.ExistProductFeature).Union(genericPriceComponents).ToArray();
            }

            return genericPriceComponents;
        }
    }
}
