// <copyright file="ContactMechanismTypes.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class ContactMechanismTypes
    {
        private static readonly Guid PhoneId = new Guid("443EE069-2975-4FA3-88DF-C243171379FD");
        private static readonly Guid MobilePhoneId = new Guid("CFA76E30-42A9-46A6-8D0F-A3D1D7907743");
        private static readonly Guid FaxId = new Guid("7AC12B75-061D-4249-862B-49C38CD233DF");

        private UniquelyIdentifiableCache<ContactMechanismType> cache;

        public ContactMechanismType Phone => this.Cache[PhoneId];

        public ContactMechanismType MobilePhone => this.Cache[MobilePhoneId];

        public ContactMechanismType Fax => this.Cache[FaxId];

        private UniquelyIdentifiableCache<ContactMechanismType> Cache => this.cache ??= new UniquelyIdentifiableCache<ContactMechanismType>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).LocaleByName["nl"];

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(PhoneId, v =>
            {
                v.Name = "Phone";
                localisedName.Set(v, dutchLocale, "Telefoon");
                v.IsActive = true;
            });

            merge(MobilePhoneId, v =>
            {
                v.Name = "Mobile Phone";
                localisedName.Set(v, dutchLocale, "Mobiel");
                v.IsActive = true;
            });

            merge(FaxId, v =>
            {
                v.Name = "FaxId";
                localisedName.Set(v, dutchLocale, "Fax");
                v.IsActive = true;
            });
        }
    }
}
