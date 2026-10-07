// <copyright file="Level1Tests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class Level1Tests : DomainTest, IClassFixture<Fixture>
    {
        public Level1Tests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void TheNormalizedNameIsDerivedFromTheName()
        {
            var item = new Level1ItemBuilder(this.Transaction).WithLevel1Name("Item one").Build();

            this.Transaction.Derive();

            Assert.Equal("ITEM ONE", item.Level1NormalizedName);
        }

        [Fact]
        public void WithoutANameThereIsNoNormalizedName()
        {
            var item = new Level1ItemBuilder(this.Transaction).WithLevel1Name("Item one").Build();
            this.Transaction.Derive();

            item.RemoveLevel1Name();
            this.Transaction.Derive();

            Assert.False(item.ExistLevel1NormalizedName);
        }
    }
}
