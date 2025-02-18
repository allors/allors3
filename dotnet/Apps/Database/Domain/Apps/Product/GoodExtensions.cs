
// <copyright file="GoodExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;
    using Meta;

    public static partial class GoodExtensions
    {
        public static void AppsOnInit(this Good @this, ObjectOnInit method)
        {
            var m = @this.Strategy.Transaction.Database.Services.Get<MetaPopulation>();
            var settings = @this.Strategy.Transaction.GetSingleton().Settings;

            var good = new ProductIdentificationTypes(@this.Strategy.Transaction).Good;
            var goodIdentification = @this.ProductIdentifications.FirstOrDefault(v => good.Equals(v.ProductIdentificationType));

            if (goodIdentification == null && settings.UseProductNumberCounter)
            {
                goodIdentification = new ProductNumberBuilder(@this.Strategy.Transaction)
                    .WithIdentification(settings.NextProductNumber())
                    .WithProductIdentificationType(new ProductIdentificationTypes(@this.Strategy.Transaction).Good).Build();

                @this.AddProductIdentification(goodIdentification);
            }
        }

        public static void AppsOnPostDerive(this Good @this, ObjectOnPostDerive method)
        {
            var m = @this.Strategy.Transaction.Database.Services.Get<MetaPopulation>();

            if (!@this.ExistProductIdentifications)
            {
                method.Derivation.Validation.AssertExists(@this, m.Good.ProductIdentifications);
            }
        }
    }
}
