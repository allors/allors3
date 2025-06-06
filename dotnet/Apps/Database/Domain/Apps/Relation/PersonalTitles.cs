// <copyright file="PersonalTitles.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class PersonalTitles
    {
        private static readonly Guid MisterId = new Guid("510D5267-4E69-45F7-B99E-CABAF7E42EB2");
        private static readonly Guid MissId = new Guid("93275216-70B9-4DBB-A824-AFBAC7A2B32E");

        private UniquelyIdentifiableCache<PersonalTitle> cache;

        public PersonalTitle Mister => this.Cache[MisterId];

        public PersonalTitle Miss => this.Cache[MissId];

        private UniquelyIdentifiableCache<PersonalTitle> Cache => this.cache ??= new UniquelyIdentifiableCache<PersonalTitle>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).LocaleByName["nl"];

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(MisterId, v =>
            {
                v.Name = "Mister";
                localisedName.Set(v, dutchLocale, "Mijnheer");
                v.IsActive = true;
            });

            merge(MissId, v =>
            {
                v.Name = "Miss";
                localisedName.Set(v, dutchLocale, "Mevrouw");
                v.IsActive = true;
            });
        }
    }
}
