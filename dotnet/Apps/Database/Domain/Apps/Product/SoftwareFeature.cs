// <copyright file="SoftwareFeature.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class SoftwareFeature
    {
        public void AddToBasePrice(BasePrice basePrice) => this.AddBasePrice(basePrice);

        public void RemoveFromBasePrices(BasePrice basePrice) => this.RemoveBasePrice(basePrice);
    }
}
