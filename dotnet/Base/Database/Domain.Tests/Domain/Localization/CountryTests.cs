// <copyright file="CountryTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Domain;
    using Xunit;

    public class CountryTests : DomainTest, IClassFixture<Fixture>
    {
        public CountryTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenCountryWhenValidatingThenRequiredRelationsMustExist()
        {
            var builder = new CountryBuilder(this.Transaction);
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            builder.WithIsoCode("XX").Build();
            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder.WithName("X Country");

            builder.Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            this.Transaction.Rollback();

            builder = new CountryBuilder(this.Transaction);
            builder.WithName("X Country");

            builder.Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);
        }
    }
}
