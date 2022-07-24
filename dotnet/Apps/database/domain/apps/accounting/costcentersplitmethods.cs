// <copyright file="CostCenterSplitMethods.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class CostCenterSplitMethods
    {
        private static readonly Guid Topid = new Guid("844444D0-5D69-4A3B-9CB6-0D98747839D8");
        private static readonly Guid BottomId = new Guid("13AE2935-F926-426F-BB1C-979BE7F3DF0B");

        private UniquelyIdentifiableCache<CostCenterSplitMethod> cache;

        public CostCenterSplitMethod Top => this.Cache[Topid];

        public CostCenterSplitMethod Bottom => this.Cache[BottomId];

        private UniquelyIdentifiableCache<CostCenterSplitMethod> Cache => this.cache ??= new UniquelyIdentifiableCache<CostCenterSplitMethod>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(Topid, v =>
            {
                v.Name = "Use top level´s cost center GL-account";
                localisedName.Set(v, dutchLocale, "Gebruik grootboekrekening van kostenplaats van hoogste niveau");
                v.IsActive = true;
            });

            merge(BottomId, v =>
            {
                v.Name = "Use bottom level´s cost center GL-account";
                localisedName.Set(v, dutchLocale, "Gebruik grootboekrekening van kostenplaats van laagste niveau");
                v.IsActive = true;
            });
        }
    }
}
