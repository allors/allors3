// <copyright file="QuoteSequences.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class QuoteSequences
    {
        public static readonly Guid EnforcedSequenceId = new Guid("d8b83e48-4469-4640-a782-c33dbb7fb492");
        public static readonly Guid RestartOnFiscalYearId = new Guid("88def14a-c69a-4873-b68c-865507daf89a");

        private UniquelyIdentifiableCache<QuoteSequence> cache;

        public QuoteSequence EnforcedSequence => this.Cache[EnforcedSequenceId];

        public QuoteSequence RestartOnFiscalYear => this.Cache[RestartOnFiscalYearId];

        private UniquelyIdentifiableCache<QuoteSequence> Cache => this.cache ??= new UniquelyIdentifiableCache<QuoteSequence>(this.Session);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Session).DutchNetherlands;

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(EnforcedSequenceId, v =>
            {
                v.Name = "Enforced Sequence (no gaps)";
                localisedName.Set(v, dutchLocale, "Aansluitend genummerd");
                v.IsActive = true;
            });

            merge(RestartOnFiscalYearId, v =>
            {
                v.Name = "Restart each fiscal year";
                localisedName.Set(v, dutchLocale, "Herstart elk fiscaal jaar");
                v.IsActive = true;
            });
        }
    }
}
