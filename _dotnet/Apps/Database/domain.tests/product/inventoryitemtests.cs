// <copyright file="InventoryItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class InventoryItemRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public InventoryItemRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartDeriveFacility()
        {
            var facility = new FacilityBuilder(this.Transaction).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction).WithDefaultFacility(facility).Build();

            var inventoryItem = new NonSerialisedInventoryItemBuilder(this.Transaction).Build();
            this.Derive();

            inventoryItem.Part = part;
            this.Derive();

            Assert.Equal(facility, inventoryItem.Facility);
        }
    }

    public class InventoryItemSearchStringRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public InventoryItemSearchStringRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartDeriveSearchString()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithSearchString("partsearch").Build();

            var inventoryItem = new NonSerialisedInventoryItemBuilder(this.Transaction).Build();
            this.Derive();

            inventoryItem.Part = part;
            this.Derive();

            Assert.Contains(part.SearchString, inventoryItem.SearchString);
        }
    }

    public class InventoryItemPartDisplayNameRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public InventoryItemPartDisplayNameRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartDerivePartDisplayName()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Derive();

            var inventoryItem = new NonSerialisedInventoryItemBuilder(this.Transaction).Build();
            this.Derive();

            inventoryItem.Part = part;
            this.Derive();

            Assert.Contains(part.DisplayName, inventoryItem.PartDisplayName);
        }
    }
}
