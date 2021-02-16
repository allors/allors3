// <copyright file="CustomerReturnSequences.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class CustomerReturnSequences
    {
        public static readonly Guid EnforcedSequenceId = new Guid("45e708cb-4cdd-4771-9f82-fec524791217");
        public static readonly Guid RestartOnFiscalYearId = new Guid("36e74a2d-0723-4ce1-841e-a7e9a2640660");

        private UniquelyIdentifiableCache<CustomerReturnSequence> cache;

        public CustomerReturnSequence EnforcedSequence => this.Cache[EnforcedSequenceId];

        public CustomerReturnSequence RestartOnFiscalYear => this.Cache[RestartOnFiscalYearId];

        private UniquelyIdentifiableCache<CustomerReturnSequence> Cache => this.cache ??= new UniquelyIdentifiableCache<CustomerReturnSequence>(this.Transaction);

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
