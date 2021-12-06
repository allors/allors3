// <copyright file="Genders.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    
    public partial class Genders
    {
        private static readonly Guid MaleId = new Guid("DAB59C10-0D45-4478-A802-3ABE54308CCD");
        private static readonly Guid FemaleId = new Guid("B68704AD-82F1-4d5d-BBAF-A54635B5034F");
        private static readonly Guid OtherId = new Guid("09210D7C-804B-4E76-AD91-0E150D36E86E");
        private static readonly Guid PreferNotToSayId = new Guid("AEE7F928-B36B-47AE-BB17-747F1D0A9D23");

        private UniquelyIdentifiableCache<Gender> cache;

        public Gender Male => this.Cache[MaleId];

        public Gender Female => this.Cache[FemaleId];

        public Gender Other => this.Cache[OtherId];

        public Gender PreferNotToSay => this.Cache[PreferNotToSayId];

        private UniquelyIdentifiableCache<Gender> Cache => this.cache ??= new UniquelyIdentifiableCache<Gender>(this.Transaction);

        protected override void CustomSetup(Setup setup)
        {
            base.CustomSetup(setup);

            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            new GenderBuilder(this.Transaction)
                .WithName("Male")
                .WithUniqueId(MaleId)
                .WithIsActive(true)
                .Build();

            new GenderBuilder(this.Transaction)
                .WithName("Female")
                .WithUniqueId(FemaleId)
                .WithIsActive(true)
                .Build();

            new GenderBuilder(this.Transaction)
                .WithName("Other")
                .WithUniqueId(OtherId)
                .WithIsActive(true)
                .Build();

            new GenderBuilder(this.Transaction)
                .WithName("Prefer not to say")
                .WithUniqueId(PreferNotToSayId)
                .WithIsActive(true)
                .Build();
        }
    }
}
