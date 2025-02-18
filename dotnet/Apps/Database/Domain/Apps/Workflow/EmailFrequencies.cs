// <copyright file="GenderTypes.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class EmailFrequencies
    {
        public static readonly Guid NoEmailId = new Guid("9815bd5c-cfc0-438f-8a80-e0181134a014");
        public static readonly Guid ImmediateId = new Guid("80ae43d6-c4b0-4a33-91f3-86df31405f85");
        public static readonly Guid DailyId = new Guid("1f5dbf92-26e9-403b-a812-72a27d99c243");
        public static readonly Guid WeeklyId = new Guid("00f6eeae-df4a-403a-a285-16d6368aa92e");

        private UniquelyIdentifiableCache<EmailFrequency> cache;

        public EmailFrequency NoEmail => this.Cache[NoEmailId];

        public EmailFrequency Immediate => this.Cache[ImmediateId];

        public EmailFrequency Daily => this.Cache[DailyId];

        public EmailFrequency Weekly => this.Cache[WeeklyId];

        private UniquelyIdentifiableCache<EmailFrequency> Cache => this.cache ??= new UniquelyIdentifiableCache<EmailFrequency>(this.Transaction);

        protected override void AppsSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).LocaleByName["nl"];

            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(NoEmailId, v =>
            {
                v.Name = "No mail";
                localisedName.Set(v, dutchLocale, "Geen mail");
                v.IsActive = true;
            });

            merge(ImmediateId, v =>
            {
                v.Name = "Immediately";
                localisedName.Set(v, dutchLocale, "Direct");
                v.IsActive = true;
            });

            merge(DailyId, v =>
            {
                v.Name = "Daily";
                localisedName.Set(v, dutchLocale, "Dagelijks");
                v.IsActive = true;
            });

            merge(WeeklyId, v =>
            {
                v.Name = "Weekly";
                localisedName.Set(v, dutchLocale, "Wekelijks");
                v.IsActive = true;
            });
        }
    }
}
