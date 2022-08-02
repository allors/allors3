// <copyright file="Skills.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class Skills
    {
        public static readonly Guid ProjectManagementId = new Guid("A8E4E325-8B5C-4f86-AB8E-A3D16C9B7827");

        private UniquelyIdentifiableCache<Skill> cache;

        public Skill ProjectManagement => this.Cache[ProjectManagementId];

        private UniquelyIdentifiableCache<Skill> Cache => this.cache ??= new UniquelyIdentifiableCache<Skill>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(ProjectManagementId, v =>
            {
                v.Name = "Project Management";
                localisedName.Set(v, dutchLocale, "Project Management");
                v.IsActive = true;
            });
        }
    }
}
