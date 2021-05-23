// <copyright file="PriceComponentExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public static partial class PriceComponentExtensions
    {
        public static void AppsOnInit(this PriceComponent @this, ObjectOnInit method)
        {
            var internalOrganisations = new Organisations(@this.Strategy.Transaction).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

            if (!@this.ExistPricedBy && internalOrganisations.Count() == 1)
            {
                @this.PricedBy = internalOrganisations.First();
            }
        }
    }
}
