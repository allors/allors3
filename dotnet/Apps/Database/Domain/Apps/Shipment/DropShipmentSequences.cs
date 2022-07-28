// <copyright file="DropShipmentSequences.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class DropShipmentSequences
    {
        public static readonly Guid EnforcedSequenceId = new Guid("28f63578-4232-4f46-b720-d4ca33536b62");
        public static readonly Guid RestartOnFiscalYearId = new Guid("45f32a1f-c22d-462e-a5c1-9f35bfb6828e");

        private UniquelyIdentifiableCache<DropShipmentSequence> cache;

        public DropShipmentSequence EnforcedSequence => this.Cache[EnforcedSequenceId];

        public DropShipmentSequence RestartOnFiscalYear => this.Cache[RestartOnFiscalYearId];

        private UniquelyIdentifiableCache<DropShipmentSequence> Cache => this.cache ??= new UniquelyIdentifiableCache<DropShipmentSequence>(this.Transaction);

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
