// <copyright file="InventoryItemTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class InventoryItemDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public InventoryItemDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartDeriveFacility()
        {
            var facility = new FacilityBuilder(this.Transaction).Build();
            var part = new NonUnifiedPartBuilder(this.Transaction).WithDefaultFacility(facility).Build();

            var inventoryItem = new NonSerialisedInventoryItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            inventoryItem.Part = part;
            this.Transaction.Derive(false);

            Assert.Equal(facility, inventoryItem.Facility);
        }
    }

    public class InventoryItemSearchStringDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public InventoryItemSearchStringDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartDeriveSearchString()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).WithSearchString("partsearch").Build();

            var inventoryItem = new NonSerialisedInventoryItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            inventoryItem.Part = part;
            this.Transaction.Derive(false);

            Assert.Contains(part.SearchString, inventoryItem.SearchString);
        }
    }

    public class InventoryItemPartDisplayNameDerivationTests : DomainTest, IClassFixture<Fixture>
    {
        public InventoryItemPartDisplayNameDerivationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedPartDerivePartDisplayName()
        {
            var part = new NonUnifiedPartBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var inventoryItem = new NonSerialisedInventoryItemBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            inventoryItem.Part = part;
            this.Transaction.Derive(false);

            Assert.Contains(part.DisplayName, inventoryItem.PartDisplayName);
        }
    }
}
