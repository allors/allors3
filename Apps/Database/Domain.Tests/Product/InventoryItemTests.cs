// <copyright file="InventoryItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class InventoryItemTests : DomainTest, IClassFixture<Fixture>
    {
        public InventoryItemTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartDeriveFacility()
        {
            var facility = new FacilityBuilder(this.Session).Build();
            var part = new NonUnifiedPartBuilder(this.Session).WithDefaultFacility(facility).Build();

            var inventoryItem = new NonSerialisedInventoryItemBuilder(this.Session).Build();
            this.Session.Derive(false);

            inventoryItem.Part = part;
            this.Session.Derive(false);

            Assert.Equal(facility, inventoryItem.Facility);
        }
    }
}
