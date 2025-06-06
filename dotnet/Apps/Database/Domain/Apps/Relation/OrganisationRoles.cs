// <copyright file="OrganisationRoles.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class OrganisationRoles
    {
        private static readonly Guid CustomerId = new Guid("8B5E0CEE-4C98-42F1-8F18-3638FBA943A0");
        private static readonly Guid SupplierId = new Guid("8C6D629B-1E27-4520-AA8C-E8ADF93A5095");
        private static readonly Guid ManufacturerId = new Guid("32E74BEF-2D79-4427-8902-B093AFA81661");

        private UniquelyIdentifiableCache<OrganisationRole> cache;

        public OrganisationRole Customer => this.Cache[CustomerId];

        public OrganisationRole Supplier => this.Cache[SupplierId];

        public OrganisationRole Manufacturer => this.Cache[ManufacturerId];

        private UniquelyIdentifiableCache<OrganisationRole> Cache => this.cache ??= new UniquelyIdentifiableCache<OrganisationRole>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).LocaleByName["nl"];

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(CustomerId, v =>
            {
                v.Name = "Customer";
                localisedName.Set(v, dutchLocale, "Klant");
                v.IsActive = true;
            });

            merge(SupplierId, v =>
            {
                v.Name = "Supplier";
                localisedName.Set(v, dutchLocale, "Leverancier");
                v.IsActive = true;
            });

            merge(ManufacturerId, v =>
            {
                v.Name = "Manufacturer";
                localisedName.Set(v, dutchLocale, "Fabrikant");
                v.IsActive = true;
            });
        }
    }
}
