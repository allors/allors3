// <copyright file="GoodExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public static partial class GoodExtensions
    {
        public static void AppsOnInit(this Good @this, ObjectOnInit method)
        {
            var m = @this.Strategy.Session.Database.Context().M;
            var settings = @this.Strategy.Session.GetSingleton().Settings;

            var identifications = @this.ProductIdentifications;
            identifications.Filter.AddEquals(m.ProductIdentification.ProductIdentificationType, new ProductIdentificationTypes(@this.Strategy.Session).Good);
            var goodIdentification = identifications.FirstOrDefault();

            if (goodIdentification == null && settings.UseProductNumberCounter)
            {
                goodIdentification = new ProductNumberBuilder(@this.Strategy.Session)
                    .WithIdentification(settings.NextProductNumber())
                    .WithProductIdentificationType(new ProductIdentificationTypes(@this.Strategy.Session).Good).Build();

                @this.AddProductIdentification(goodIdentification);
            }
        }

        public static void AppsOnPostDerive(this Good @this, ObjectOnPostDerive method)
        {
            var m = @this.Strategy.Session.Database.Context().M;

            if (!@this.ExistProductIdentifications)
            {
                method.Derivation.Validation.AssertExists(@this, m.Good.ProductIdentifications);
            }
        }
    }
}
