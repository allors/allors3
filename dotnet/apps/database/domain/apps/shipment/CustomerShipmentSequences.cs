// <copyright file="CustomerShipmentSequences.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class CustomerShipmentSequences
    {
        public static readonly Guid EnforcedSequenceId = new Guid("6ef7385c-d31c-4dff-80b9-5c9b5904d32b");
        public static readonly Guid RestartOnFiscalYearId = new Guid("e8534cca-df20-428e-9376-ee9385b918b0");

        private UniquelyIdentifiableCache<CustomerShipmentSequence> cache;

        public CustomerShipmentSequence EnforcedSequence => this.Cache[EnforcedSequenceId];

        public CustomerShipmentSequence RestartOnFiscalYear => this.Cache[RestartOnFiscalYearId];

        private UniquelyIdentifiableCache<CustomerShipmentSequence> Cache => this.cache ??= new UniquelyIdentifiableCache<CustomerShipmentSequence>(this.Transaction);

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
