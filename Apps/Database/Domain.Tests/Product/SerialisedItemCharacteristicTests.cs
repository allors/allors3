// <copyright file="SerialisedItemCharacteristicTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class SerialisedItemCharacteristicTests : DomainTest, IClassFixture<Fixture>
    {
        public SerialisedItemCharacteristicTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSerialisedItemCharacteristicTypeDeriveLocalisedValue()
        {
            var locales = this.Session.GetSingleton().AdditionalLocales;

            var characteristic = new SerialisedItemCharacteristicBuilder(this.Session).Build();
            this.Session.Derive(false);

            characteristic.SerialisedItemCharacteristicType = new SerialisedItemCharacteristicTypeBuilder(this.Session).WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Kilogram).Build();
            this.Session.Derive(false);

            foreach(Locale locale in locales)
            {
                Assert.Contains(characteristic.LocalisedValues, v => v.Locale.Equals(locale));
            }
        }

        [Fact]
        public void ChangedSerialisedItemCharacteristicTypeUnitOfMeasureDeriveLocalisedValue()
        {
            var locales = this.Session.GetSingleton().AdditionalLocales;

            var characteristicType = new SerialisedItemCharacteristicTypeBuilder(this.Session).Build();
            var characteristic = new SerialisedItemCharacteristicBuilder(this.Session).WithSerialisedItemCharacteristicType(characteristicType).Build();
            this.Session.Derive(false);

            characteristicType.UnitOfMeasure = new UnitsOfMeasure(this.Session).Kilogram;
            this.Session.Derive(false);

            foreach (Locale locale in locales)
            {
                Assert.Contains(characteristic.LocalisedValues, v => v.Locale.Equals(locale));
            }
        }

        [Fact]
        public void ChangedValueDeriveLocalisedValue()
        {
            var locales = this.Session.GetSingleton().AdditionalLocales;

            var characteristic = new SerialisedItemCharacteristicBuilder(this.Session)
                .WithSerialisedItemCharacteristicType(new SerialisedItemCharacteristicTypeBuilder(this.Session).WithUnitOfMeasure(new UnitsOfMeasure(this.Session).Kilogram).Build())
                .Build();
            this.Session.Derive(false);

            characteristic.Value = "value";
            this.Session.Derive(false);

            foreach (Locale locale in locales)
            {
                Assert.Contains(characteristic.LocalisedValues, v => v.Text.Equals("value"));
            }
        }
    }
}
