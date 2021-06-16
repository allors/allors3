// <copyright file="AddendumTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class AddendumTest : DomainTest, IClassFixture<Fixture>
    {
        public AddendumTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenAddendum_WhenDeriving_ThenDescriptionIsRequired()
        {
            var builder = new AddendumBuilder(this.Transaction);
            var addendum = builder.Build();

            Assert.True(this.Derive().HasErrors);

            this.Transaction.Rollback();

            builder.WithDescription("addendum");
            addendum = builder.Build();

            Assert.False(this.Derive().HasErrors);
        }
    }
}
