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
            var locales = this.Transaction.GetSingleton().AdditionalLocales;

            var characteristic = new SerialisedItemCharacteristicBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            characteristic.SerialisedItemCharacteristicType = new SerialisedItemCharacteristicTypeBuilder(this.Transaction).WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Kilogram).Build();
            this.Transaction.Derive(false);

            foreach(Locale locale in locales)
            {
                Assert.Contains(characteristic.LocalisedValues, v => v.Locale.Equals(locale));
            }
        }

        [Fact]
        public void ChangedSerialisedItemCharacteristicTypeUnitOfMeasureDeriveLocalisedValue()
        {
            var locales = this.Transaction.GetSingleton().AdditionalLocales;

            var characteristicType = new SerialisedItemCharacteristicTypeBuilder(this.Transaction).Build();
            var characteristic = new SerialisedItemCharacteristicBuilder(this.Transaction).WithSerialisedItemCharacteristicType(characteristicType).Build();
            this.Transaction.Derive(false);

            characteristicType.UnitOfMeasure = new UnitsOfMeasure(this.Transaction).Kilogram;
            this.Transaction.Derive(false);

            foreach (Locale locale in locales)
            {
                Assert.Contains(characteristic.LocalisedValues, v => v.Locale.Equals(locale));
            }
        }

        [Fact]
        public void ChangedValueDeriveLocalisedValue()
        {
            var locales = this.Transaction.GetSingleton().AdditionalLocales;

            var characteristic = new SerialisedItemCharacteristicBuilder(this.Transaction)
                .WithSerialisedItemCharacteristicType(new SerialisedItemCharacteristicTypeBuilder(this.Transaction).WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Kilogram).Build())
                .Build();
            this.Transaction.Derive(false);

            characteristic.Value = "value";
            this.Transaction.Derive(false);

            foreach (Locale locale in locales)
            {
                Assert.Contains(characteristic.LocalisedValues, v => v.Text.Equals("value"));
            }
        }
    }
}
