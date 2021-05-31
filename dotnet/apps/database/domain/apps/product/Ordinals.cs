// <copyright file="Ordinals.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class Ordinals
    {
        private static readonly Guid FirstId = new Guid("B95AB69E-2DDA-4439-BFD6-626723B98876"); // TODO: old guid ("E312891F-7744-43ba-A69F-13878B1FC66B")
        private static readonly Guid SecondId = new Guid("41F8AC7C-A676-4A63-8F39-A023E223B544"); // TODO: old guid ("6593FE82-A00F-4de6-9516-D652FE28A3EA")
        private static readonly Guid ThirdId = new Guid("C207121C-B534-4764-9724-3E829E9C9F21");

        private UniquelyIdentifiableCache<Ordinal> cache;

        public Ordinal First => this.Cache[FirstId];

        public Ordinal Second => this.Cache[SecondId];

        public Ordinal Third => this.Cache[ThirdId];

        private UniquelyIdentifiableCache<Ordinal> Cache => this.cache ??= new UniquelyIdentifiableCache<Ordinal>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(FirstId, v =>
            {
                v.Name = "First";
                localisedName.Set(v, dutchLocale, "Eerste");
                v.IsActive = true;
            });

            merge(SecondId, v =>
            {
                v.Name = "Second";
                localisedName.Set(v, dutchLocale, "Tweede");
                v.IsActive = true;
            });

            merge(ThirdId, v =>
            {
                v.Name = "Third";
                localisedName.Set(v, dutchLocale, "Derde");
                v.IsActive = true;
            });
        }
    }
}
