// <copyright file="InventoryItemKinds.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class InventoryItemKinds
    {
        public static readonly Guid SerialisedId = new Guid("2596E2DD-3F5D-4588-A4A2-167D6FBE3FAE");
        public static readonly Guid NonSerialisedId = new Guid("EAA6C331-0DD9-4bb1-8245-12A673304468");

        private UniquelyIdentifiableCache<InventoryItemKind> cache;

        public InventoryItemKind Serialised => this.Cache[SerialisedId];

        public InventoryItemKind NonSerialised => this.Cache[NonSerialisedId];

        private UniquelyIdentifiableCache<InventoryItemKind> Cache => this.cache ??= new UniquelyIdentifiableCache<InventoryItemKind>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(SerialisedId, v =>
            {
                v.Name = "Serialised";
                localisedName.Set(v, dutchLocale, "Op serienummer");
                v.IsActive = true;
            });

            merge(NonSerialisedId, v =>
            {
                v.Name = "Non serialised";
                localisedName.Set(v, dutchLocale, "Zonder serienummer");
                v.IsActive = true;
            });
        }
    }
}
