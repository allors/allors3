// <copyright file="ProductFeature.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial interface ProductFeature
    {
        void AddToBasePrice(BasePrice basePrice);

        void RemoveFromBasePrices(BasePrice basePrice);
    }
}
