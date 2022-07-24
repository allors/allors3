// <copyright file="InvoiceSequences.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class InvoiceSequences
    {
        public static readonly Guid EnforcedSequenceId = new Guid("54FF2FC1-9A4F-4d46-8BEA-866F4FBB448C");
        public static readonly Guid RestartOnFiscalYearId = new Guid("2A2027B5-30D2-42a1-BE8B-FEF343C742D1");

        private UniquelyIdentifiableCache<InvoiceSequence> cache;

        public InvoiceSequence EnforcedSequence => this.Cache[EnforcedSequenceId];

        public InvoiceSequence RestartOnFiscalYear => this.Cache[RestartOnFiscalYearId];

        private UniquelyIdentifiableCache<InvoiceSequence> Cache => this.cache ??= new UniquelyIdentifiableCache<InvoiceSequence>(this.Transaction);

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
                v.Name = "Restart each fiscal year (no gaps, reset to '1' each year";
                localisedName.Set(v, dutchLocale, "Herstart elk fiscaal jaar");
                v.IsActive = true;
            });
        }
    }
}
