// <copyright file="GenderTypes.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class GenderTypes
    {
        private static readonly Guid MaleId = new Guid("DAB59C10-0D45-4478-A802-3ABE54308CCD");
        private static readonly Guid FemaleId = new Guid("B68704AD-82F1-4d5d-BBAF-A54635B5034F");
        private static readonly Guid OtherId = new Guid("09210D7C-804B-4E76-AD91-0E150D36E86E");
        private static readonly Guid PreferNotToSayId = new Guid("AEE7F928-B36B-47AE-BB17-747F1D0A9D23");

        private UniquelyIdentifiableCache<GenderType> cache;

        public GenderType Male => this.Cache[MaleId];

        public GenderType Female => this.Cache[FemaleId];

        public GenderType Other => this.Cache[OtherId];

        public GenderType PreferNotToSay => this.Cache[PreferNotToSayId];

        private UniquelyIdentifiableCache<GenderType> Cache => this.cache ??= new UniquelyIdentifiableCache<GenderType>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).LocaleByName["nl"];

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(MaleId, v =>
            {
                v.Name = "Male";
                localisedName.Set(v, dutchLocale, "Mannelijk");
                v.IsActive = true;
            });

            merge(FemaleId, v =>
            {
                v.Name = "Female";
                localisedName.Set(v, dutchLocale, "Vrouwelijk");
                v.IsActive = true;
            });

            merge(OtherId, v =>
            {
                v.Name = "Other";
                localisedName.Set(v, dutchLocale, "Anders");
                v.IsActive = true;
            });

            merge(OtherId, v =>
            {
                v.Name = "Prefer not to say";
                localisedName.Set(v, dutchLocale, "Zeg liever niet");
                v.IsActive = true;
            });
        }
    }
}
