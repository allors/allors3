// <copyright file="SerialisedItemCharacteristicTypeTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class SerialisedItemCharacteristicTypeTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemCharacteristicTypeTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedLocalisedNamesDeriveName()
        {
            var defaultLocale = this.Transaction.GetSingleton().DefaultLocale;

            var characteristicType = new SerialisedItemCharacteristicTypeBuilder(this.Transaction).Build();
            this.Derive();

            characteristicType.AddLocalisedName(new LocalisedTextBuilder(this.Transaction).WithLocale(defaultLocale).WithText("defaultname").Build());
            this.Derive();

            Assert.Equal("defaultname", characteristicType.Name);
        }
    }
}
