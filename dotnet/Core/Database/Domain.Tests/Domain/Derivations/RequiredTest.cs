// <copyright file="RequiredTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using Xunit;

    public class RequiredTest : DomainTest, IClassFixture<Fixture>
    {
        public RequiredTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void OnPostBuild()
        {
            var before = this.Transaction.Now();

            var units = new UnitSampleBuilder(this.Transaction).Build();

            this.Transaction.Derive(false);

            var after = this.Transaction.Now();

            Assert.False(units.ExistRequiredBinary);
            Assert.False(units.ExistRequiredString);

            Assert.True(units.ExistRequiredBoolean);
            Assert.True(units.ExistRequiredDateTime);
            Assert.True(units.ExistRequiredDecimal);
            Assert.True(units.ExistRequiredDouble);
            Assert.True(units.ExistRequiredInteger);
            Assert.True(units.ExistRequiredUnique);

            Assert.False(units.RequiredBoolean);
            Assert.True(units.RequiredDateTime > before && units.RequiredDateTime < after);
            Assert.Equal(0m, units.RequiredDecimal);
            Assert.Equal(0d, units.RequiredDouble);
            Assert.Equal(0, units.RequiredInteger);
            Assert.NotEqual(Guid.Empty, units.RequiredUnique);
        }

        [Fact]
        public void AssertRequired()
        {
            var valiData = new ValiDataBuilder(this.Transaction).Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            valiData.RequiredPerson = new People(this.Transaction).Extent().First;

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

    }
}
