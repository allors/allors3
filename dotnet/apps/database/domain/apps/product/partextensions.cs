// <copyright file="PartExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;
    using Meta;

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
            var m = @this.Strategy.Transaction.Database.Services.Get<MetaPopulation>();
            var settings = @this.Strategy.Transaction.GetSingleton().Settings;

            var part = new ProductIdentificationTypes(@this.Strategy.Transaction).Part;
            var partIdentification = @this.ProductIdentifications.FirstOrDefault(v => Equals(part, v.ProductIdentificationType));

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
            if (!@this.ProductIdentifications.Any())
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

        public static void DeriveRelationships(this Part @this)
        {
            var now = @this.Transaction().Now();

            var zz = new SupplierOfferings(@this.Strategy.Transaction).Extent().ToArray();
            @this.CurrentSupplierOfferings = new SupplierOfferings(@this.Strategy.Transaction).Extent()
                .Where(v => v.FromDate <= now
                            && (!v.ExistThroughDate || v.ThroughDate >= now)
                            && v.ExistPart
                            && v.Part.Equals(@this)
                            && v.ExistSupplier
                            && v.Supplier.ExistInternalOrganisationsWhereActiveSupplier);
        }
    }
}
