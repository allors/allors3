// <copyright file="DeductionTypes.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class DeductionTypes
    {
        private static readonly Guid RetirementId = new Guid("2D0F0788-EB2B-4ef3-A09A-A7285DAD72CF");
        private static readonly Guid InsuranceId = new Guid("D82A5A9F-068F-4e30-88F5-5E6C81D03BAF");

        private UniquelyIdentifiableCache<DeductionType> cache;

        public DeductionType Retirement => this.Cache[RetirementId];

        public DeductionType Insurance => this.Cache[InsuranceId];

        private UniquelyIdentifiableCache<DeductionType> Cache => this.cache ??= new UniquelyIdentifiableCache<DeductionType>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).LocaleByName["nl"];

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(RetirementId, v =>
            {
                v.Name = "Retirement";
                localisedName.Set(v, dutchLocale, "Pensioen");
                v.IsActive = true;
            });

            merge(InsuranceId, v =>
            {
                v.Name = "Insurance";
                localisedName.Set(v, dutchLocale, "Verzekering");
                v.IsActive = true;
            });
        }
    }
}
