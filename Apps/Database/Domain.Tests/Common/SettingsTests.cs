// <copyright file="SettingsTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Domain
{
    using Xunit;

    public class SettingsTests : DomainTest, IClassFixture<Fixture>
    {
        public SettingsTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenNewSettings_WhenDeriving_ThenCreatePatternIsMatched()
        {
            var newSettings = new SettingsBuilder(this.Session).Build();

            this.Session.Derive();

            Assert.True(newSettings.ExistPartNumberCounter);
            Assert.True(newSettings.ExistProductNumberCounter);
            Assert.True(newSettings.ExistSerialisedItemCounter);
            Assert.True(newSettings.ExistSkuCounter);
            Assert.Equal(0, newSettings.PartNumberCounter.Value);
            Assert.Equal(0, newSettings.ProductNumberCounter.Value);
            Assert.Equal(0, newSettings.SerialisedItemCounter.Value);
            Assert.Equal(0, newSettings.SkuCounter.Value);
        }
    }
}
