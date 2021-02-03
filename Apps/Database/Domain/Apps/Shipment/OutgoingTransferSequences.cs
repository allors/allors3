// <copyright file="OutgoingTransferSequences.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class OutgoingTransferSequences
    {
        public static readonly Guid EnforcedSequenceId = new Guid("0ffa2406-c73f-4ac4-88a4-3511440d3ce2");
        public static readonly Guid RestartOnFiscalYearId = new Guid("778d20fb-4679-4ddd-8aae-3616fc3f6ebf");

        private UniquelyIdentifiableCache<OutgoingTransferSequence> cache;

        public OutgoingTransferSequence EnforcedSequence => this.Cache[EnforcedSequenceId];

        public OutgoingTransferSequence RestartOnFiscalYear => this.Cache[RestartOnFiscalYearId];

        private UniquelyIdentifiableCache<OutgoingTransferSequence> Cache => this.cache ??= new UniquelyIdentifiableCache<OutgoingTransferSequence>(this.Session);

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
