// <copyright file="UnifiedProductExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Allors.Database.Domain
{
    public static partial class UnifiedProductExtensions
    {
        public static void CustomOnBuild(this UnifiedProduct @this, ObjectOnBuild method)
        {
            if (!@this.ExistScope)
            {
                @this.Scope = new Scopes(@this.Strategy.Transaction).Public;
            }
        }
        public static PriceComponent[] GetPriceComponents(this UnifiedProduct @this, PriceComponent[] currentPriceComponents)
        {
            if (currentPriceComponents == null)
            {
                return new PriceComponent[] { };
            }

            var genericPriceComponents = currentPriceComponents.Where(priceComponent => !priceComponent.ExistProduct && !priceComponent.ExistProductFeature).ToArray();

            var exclusiveProductPriceComponents = currentPriceComponents.Where(priceComponent => priceComponent.Product?.Equals(@this) == true && !priceComponent.ExistProductFeature).ToArray();

            if (exclusiveProductPriceComponents.Length > 0)
            {
                return exclusiveProductPriceComponents.Union(genericPriceComponents).ToArray();
            }

            if (@this.ExistUnifiedProductWhereVariant)
            {
                return currentPriceComponents.Where(priceComponent => priceComponent.Product?.Equals(@this.UnifiedProductWhereVariant) == true && !priceComponent.ExistProductFeature).Union(genericPriceComponents).ToArray();
            }

            return genericPriceComponents;
        }
    }
}
