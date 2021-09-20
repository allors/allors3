// <copyright file="RateTypes.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class RateTypes
    {
        private static readonly Guid InternalRateId = new Guid("4fb41198-766d-4645-8c6b-cb1c95448460");
        private static readonly Guid StandardRateId = new Guid("FE2C3012-7FBC-4c10-B76E-F0DA4754020A");
        private static readonly Guid OvertimeRateId = new Guid("DE4D0A4C-EDDC-460c-BF78-A45A9B881F48");
        private static readonly Guid WeekendRateId = new Guid("2AA92139-E634-444e-9997-89B5F598812F");

        private UniquelyIdentifiableCache<RateType> cache;

        public RateType InternalRate => this.Cache[InternalRateId];

        public RateType StandardRate => this.Cache[StandardRateId];

        public RateType OvertimeRate => this.Cache[OvertimeRateId];

        public RateType WeekendRate => this.Cache[WeekendRateId];

        private UniquelyIdentifiableCache<RateType> Cache => this.cache ??= new UniquelyIdentifiableCache<RateType>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(InternalRateId, v =>
            {
                v.Name = "Internal Rate";
                localisedName.Set(v, dutchLocale, "Internal tarief");
                v.IsActive = true;
            });

            merge(StandardRateId, v =>
            {
                v.Name = "Standard Rate";
                localisedName.Set(v, dutchLocale, "Standaard tarief");
                v.IsActive = true;
            });

            merge(OvertimeRateId, v =>
            {
                v.Name = "Overtime Rate";
                localisedName.Set(v, dutchLocale, "Overuren tarief");
                v.IsActive = true;
            });

            merge(WeekendRateId, v =>
            {
                v.Name = "Weekend Rate";
                localisedName.Set(v, dutchLocale, "Weekend tarief");
                v.IsActive = true;
            });
        }
    }
}
