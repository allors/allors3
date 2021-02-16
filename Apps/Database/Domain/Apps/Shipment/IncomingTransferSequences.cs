// <copyright file="IncomingTransferSequences.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class IncomingTransferSequences
    {
        public static readonly Guid EnforcedSequenceId = new Guid("bcdf5519-74ef-4d7c-a35d-41d849364391");
        public static readonly Guid RestartOnFiscalYearId = new Guid("49dec7a9-de60-4956-9e4d-375f83c0a866");

        private UniquelyIdentifiableCache<IncomingTransferSequence> cache;

        public IncomingTransferSequence EnforcedSequence => this.Cache[EnforcedSequenceId];

        public IncomingTransferSequence RestartOnFiscalYear => this.Cache[RestartOnFiscalYearId];

        private UniquelyIdentifiableCache<IncomingTransferSequence> Cache => this.cache ??= new UniquelyIdentifiableCache<IncomingTransferSequence>(this.Transaction);

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
