// <copyright file="Plugin1Tests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    // The plug-in adds a key to the users of its host and derives its normalized form, for the
    // user class the concrete domain declares.
    public class Plugin1Tests : DomainTest, IClassFixture<Fixture>
    {
        public Plugin1Tests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void TheNormalizedKeyIsDerivedFromTheKey()
        {
            var person = new PersonBuilder(this.Transaction).WithPlugin1Key("key-one").Build();

            this.Transaction.Derive();

            Assert.Equal("KEY-ONE", person.Plugin1NormalizedKey);
        }

        [Fact]
        public void WithoutAKeyThereIsNoNormalizedKey()
        {
            var person = new PersonBuilder(this.Transaction).WithPlugin1Key("key-one").Build();
            this.Transaction.Derive();

            person.RemovePlugin1Key();
            this.Transaction.Derive();

            Assert.False(person.ExistPlugin1NormalizedKey);
        }
    }
}
