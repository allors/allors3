// <copyright file="RatingTypes.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class RatingTypes
    {
        private static readonly Guid PoorId = new Guid("1DC90B94-1CDA-428e-B5D6-C2970C57D142");
        private static readonly Guid FairId = new Guid("90E42936-FAC5-4673-BE94-01D16A8ADC3A");
        private static readonly Guid GoodId = new Guid("5F5C4F82-22A3-44a3-B175-0CFAE502574C");
        private static readonly Guid OutstandingId = new Guid("44E7BC25-5AEC-44ab-905E-E5BAA5415F72");

        private UniquelyIdentifiableCache<RatingType> cache;

        public RatingType Poor => this.Cache[PoorId];

        public RatingType Fair => this.Cache[FairId];

        public RatingType Good => this.Cache[GoodId];

        public RatingType Outstanding => this.Cache[OutstandingId];

        private UniquelyIdentifiableCache<RatingType> Cache => this.cache ??= new UniquelyIdentifiableCache<RatingType>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(PoorId, v =>
            {
                v.Name = "Poor";
                localisedName.Set(v, dutchLocale, "Laag");
                v.IsActive = true;
            });

            merge(FairId, v =>
            {
                v.Name = "Fair";
                localisedName.Set(v, dutchLocale, "Redelijk");
                v.IsActive = true;
            });

            merge(GoodId, v =>
            {
                v.Name = "Good";
                localisedName.Set(v, dutchLocale, "Goed");
                v.IsActive = true;
            });

            merge(OutstandingId, v =>
            {
                v.Name = "Outstanding";
                localisedName.Set(v, dutchLocale, "Uitstekend");
                v.IsActive = true;
            });
        }
    }
}
