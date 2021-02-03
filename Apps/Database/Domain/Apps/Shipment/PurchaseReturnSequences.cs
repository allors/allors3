// <copyright file="PurchaseReturnSequences.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class PurchaseReturnSequences
    {
        public static readonly Guid EnforcedSequenceId = new Guid("0807bc02-3b8c-47c3-8e0e-35752a8938ac");
        public static readonly Guid RestartOnFiscalYearId = new Guid("73b7d14b-cb9c-440b-86f1-2f8553e2f555");

        private UniquelyIdentifiableCache<PurchaseReturnSequence> cache;

        public PurchaseReturnSequence EnforcedSequence => this.Cache[EnforcedSequenceId];

        public PurchaseReturnSequence RestartOnFiscalYear => this.Cache[RestartOnFiscalYearId];

        private UniquelyIdentifiableCache<PurchaseReturnSequence> Cache => this.cache ??= new UniquelyIdentifiableCache<PurchaseReturnSequence>(this.Session);

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
