// <copyright file="Ownerships.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class Ownerships
    {
        private static readonly Guid OwnId = new Guid("1cefe3e7-3f9a-43a1-b12c-93e8032d3880");
        private static readonly Guid TradingId = new Guid("3ec25bbf-511c-44f1-a599-4a0330f28c3e");
        private static readonly Guid ThirdPartyId = new Guid("6b613409-bdf4-4a86-815f-6920d2fec8d3");

        private UniquelyIdentifiableCache<Ownership> cache;

        public Ownership Own => this.Cache[OwnId];

        public Ownership Trading => this.Cache[TradingId];

        public Ownership ThirdParty => this.Cache[ThirdPartyId];

        private UniquelyIdentifiableCache<Ownership> Cache => this.cache ??= new UniquelyIdentifiableCache<Ownership>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(OwnId, v =>
            {
                v.Name = "Own";
                localisedName.Set(v, dutchLocale, "Eigen");
                v.IsActive = true;
            });

            merge(TradingId, v =>
            {
                v.Name = "Trading";
                localisedName.Set(v, dutchLocale, "Handel");
                v.IsActive = true;
            });

            merge(ThirdPartyId, v =>
            {
                v.Name = "Third party";
                localisedName.Set(v, dutchLocale, "Derde partij");
                v.IsActive = true;
            });
        }
    }
}
