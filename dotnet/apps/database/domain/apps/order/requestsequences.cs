// <copyright file="RequestSequences.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class RequestSequences
    {
        public static readonly Guid EnforcedSequenceId = new Guid("053a8ea8-c384-49de-96ae-f7b5d76da514");
        public static readonly Guid RestartOnFiscalYearId = new Guid("53bb4bfd-7092-4790-9c50-3d95e7460d70");

        private UniquelyIdentifiableCache<RequestSequence> cache;

        public RequestSequence EnforcedSequence => this.Cache[EnforcedSequenceId];

        public RequestSequence RestartOnFiscalYear => this.Cache[RestartOnFiscalYearId];

        private UniquelyIdentifiableCache<RequestSequence> Cache => this.cache ??= new UniquelyIdentifiableCache<RequestSequence>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

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
