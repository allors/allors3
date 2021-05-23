// <copyright file="PartExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public static partial class PartExtensions
    {
        public static void AppsOnBuild(this Part @this, ObjectOnBuild method)
        {
            if (!@this.ExistPartWeightedAverage)
            {
                @this.PartWeightedAverage = new PartWeightedAverageBuilder(@this.Transaction()).Build();
            }
        }
        public static void AppsOnInit(this Part @this, ObjectOnInit method)
        {
            var m = @this.Strategy.Transaction.Database.Context().M;
            var settings = @this.Strategy.Transaction.GetSingleton().Settings;

            var identifications = @this.ProductIdentifications;
            identifications.Filter.AddEquals(m.ProductIdentification.ProductIdentificationType, new ProductIdentificationTypes(@this.Strategy.Transaction).Part);
            var partIdentification = identifications.FirstOrDefault();

            if (partIdentification == null && settings.UsePartNumberCounter)
            {
                partIdentification = new PartNumberBuilder(@this.Strategy.Transaction)
                    .WithIdentification(settings.NextPartNumber())
                    .WithProductIdentificationType(new ProductIdentificationTypes(@this.Strategy.Transaction).Part).Build();

                @this.AddProductIdentification(partIdentification);
            }
        }

        public static string PartIdentification(this Part @this)
        {
            if (@this.ProductIdentifications.Count == 0)
            {
                return null;
            }

            var partId = @this.ProductIdentifications.FirstOrDefault(g => g.ExistProductIdentificationType
                                                                         && g.ProductIdentificationType.Equals(new ProductIdentificationTypes(@this.Strategy.Transaction).Part));

            var goodId = @this.ProductIdentifications.FirstOrDefault(g => g.ExistProductIdentificationType
                                                                          && g.ProductIdentificationType.Equals(new ProductIdentificationTypes(@this.Strategy.Transaction).Good));

            var id = partId ?? goodId;
            return id?.Identification;
        }

        public static PriceComponent[] GetPriceComponents(this Part @this, PriceComponent[] currentPriceComponents)
        {
            var genericPriceComponents = currentPriceComponents.Where(priceComponent => !priceComponent.ExistPart && !priceComponent.ExistProduct && !priceComponent.ExistProductFeature).ToArray();

            var exclusivePartPriceComponents = currentPriceComponents.Where(priceComponent => priceComponent.Part?.Equals(@this) == true).ToArray();

            if (exclusivePartPriceComponents.Length > 0)
            {
                return exclusivePartPriceComponents.Union(genericPriceComponents).ToArray();
            }

            return genericPriceComponents;
        }

        public static void AppsSetDisplayName(this Part @this, PartSetDisplayName method)
        {
            @this.DisplayName = @this.Name;
            method.StopPropagation = true;
        }
    }
}
