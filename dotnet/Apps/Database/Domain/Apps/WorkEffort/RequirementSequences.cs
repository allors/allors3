// <copyright file="RequirementSequences.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class RequirementSequences
    {
        public static readonly Guid EnforcedSequenceId = new Guid("868ec23c-c388-43a0-b061-9398f7c8804a");
        public static readonly Guid RestartOnFiscalYearId = new Guid("3a784980-2381-445e-bc1a-b5c550c049d6");

        private UniquelyIdentifiableCache<RequirementSequence> cache;

        public RequirementSequence EnforcedSequence => this.Cache[EnforcedSequenceId];

        public RequirementSequence RestartOnFiscalYear => this.Cache[RestartOnFiscalYearId];

        private UniquelyIdentifiableCache<RequirementSequence> Cache => this.cache ??= new UniquelyIdentifiableCache<RequirementSequence>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).LocaleByName["nl"];

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
