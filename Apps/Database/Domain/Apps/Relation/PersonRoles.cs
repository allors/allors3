// <copyright file="PersonRoles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class PersonRoles
    {
        private static readonly Guid EmployeeId = new Guid("DB06A3E1-6146-4C18-A60D-DD10E19F7243");
        private static readonly Guid ContactId = new Guid("FA2DF11E-7795-4DF7-8B3F-4FD87D0C4D8E");
        private static readonly Guid CustomerId = new Guid("B29444EF-0950-4D6F-AB3E-9C6DC44C050F");

        private UniquelyIdentifiableCache<PersonRole> cache;

        public PersonRole Employee => this.Cache[EmployeeId];

        public PersonRole Contact => this.Cache[ContactId];

        public PersonRole Customer => this.Cache[CustomerId];

        private UniquelyIdentifiableCache<PersonRole> Cache => this.cache ??= new UniquelyIdentifiableCache<PersonRole>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(EmployeeId, v =>
            {
                v.Name = "Employee";
                localisedName.Set(v, dutchLocale, "Werknemer");
                v.IsActive = true;
            });

            merge(EmployeeId, v =>
            {
                v.Name = "Sales Rep";
                localisedName.Set(v, dutchLocale, "Verkoper");
                v.IsActive = true;
            });

            merge(ContactId, v =>
            {
                v.Name = "Organisation Contact";
                localisedName.Set(v, dutchLocale, "Contact");
                v.IsActive = true;
            });

            merge(CustomerId, v =>
            {
                v.Name = "Customer";
                localisedName.Set(v, dutchLocale, "Klant");
                v.IsActive = true;
            });
        }
    }
}
