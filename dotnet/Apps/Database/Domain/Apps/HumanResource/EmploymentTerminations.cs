// <copyright file="EmploymentTerminations.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class EmploymentTerminations
    {
        private static readonly Guid ResignationId = new Guid("93A901E4-5BB6-456c-886E-463D9F60B4F2");
        private static readonly Guid FiredId = new Guid("C1EFC297-20C2-469d-BC93-2AB4A452C512");
        private static readonly Guid RetirenmentId = new Guid("1D567408-2630-4625-A676-D7CB8B19D04B");
        private static readonly Guid DeceasedId = new Guid("BE60EFE5-9790-49f2-886C-1C8DE5DB046C");

        private UniquelyIdentifiableCache<EmploymentTermination> cache;

        public EmploymentTermination Resignation => this.Cache[ResignationId];

        public EmploymentTermination Fired => this.Cache[FiredId];

        public EmploymentTermination Retirenment => this.Cache[RetirenmentId];

        public EmploymentTermination Deceased => this.Cache[DeceasedId];

        private UniquelyIdentifiableCache<EmploymentTermination> Cache => this.cache ??= new UniquelyIdentifiableCache<EmploymentTermination>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).LocaleByName["nl"];

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(ResignationId, v =>
            {
                v.Name = "Resignation";
                localisedName.Set(v, dutchLocale, "Ontslag genomen");
                v.IsActive = true;
            });

            merge(FiredId, v =>
            {
                v.Name = "Fired";
                localisedName.Set(v, dutchLocale, "Ontslagen");
                v.IsActive = true;
            });

            merge(RetirenmentId, v =>
            {
                v.Name = "Retirement";
                localisedName.Set(v, dutchLocale, "Pensioen");
                v.IsActive = true;
            });

            merge(DeceasedId, v =>
            {
                v.Name = "Deceased";
                localisedName.Set(v, dutchLocale, "Overleden");
                v.IsActive = true;
            });
        }
    }
}
