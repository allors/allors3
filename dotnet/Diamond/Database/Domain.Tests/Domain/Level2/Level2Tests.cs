// <copyright file="Level2Tests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class Level2Tests : DomainTest, IClassFixture<Fixture>
    {
        public Level2Tests(Fixture fixture) : base(fixture) { }

        // Level2 adds a role to the class of Level1, next to the roles Level1 gave it.
        [Fact]
        public void ALevel1ItemHasTheRoleOfLevel2()
        {
            var item = new Level1ItemBuilder(this.Transaction).WithLevel1Name("Item one").WithLevel2Note("A note of Level2").Build();

            this.Transaction.Derive();

            Assert.Equal("A note of Level2", item.Level2Note);
            Assert.Equal("ITEM ONE", item.Level1NormalizedName);
        }

        [Fact]
        public void TheNormalizedNameIsDerivedFromTheName()
        {
            var item = new Level2ItemBuilder(this.Transaction).WithLevel2Name("Item two").Build();

            this.Transaction.Derive();

            Assert.Equal("ITEM TWO", item.Level2NormalizedName);
        }

        [Fact]
        public void WithoutANameThereIsNoNormalizedName()
        {
            var item = new Level2ItemBuilder(this.Transaction).WithLevel2Name("Item two").Build();
            this.Transaction.Derive();

            item.RemoveLevel2Name();
            this.Transaction.Derive();

            Assert.False(item.ExistLevel2NormalizedName);
        }
    }
}
