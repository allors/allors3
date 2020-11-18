// <copyright file="SettingsTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class SettingsTests : DomainTest, IClassFixture<Fixture>
    {
        public SettingsTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void DeriveCounters()
        {
            var settings = this.Session.GetSingleton().Settings;

            this.Session.Derive();

            Assert.True(settings.ExistPartNumberCounter);
            Assert.True(settings.ExistProductNumberCounter);
            Assert.True(settings.ExistSerialisedItemCounter);
            Assert.True(settings.ExistSkuCounter);
        }
    }
}
