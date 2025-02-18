// <copyright file="DerivationLogTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the ApplicationTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class UniqueTests : DomainTest, IClassFixture<Fixture>
    {
        public UniqueTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void IsUniqueMultipleRole()
        {
            var valiData1 = new ValiDataBuilder(this.Transaction)
                .WithRequiredPerson(new People(this.Transaction).Extent().First)
                .Build();

            valiData1.ValueA = 1;
            valiData1.ValueB = 2;

            Assert.False(this.Transaction.Derive(false).HasErrors);

            var valiData2 = new ValiDataBuilder(this.Transaction)
                .WithRequiredPerson(new People(this.Transaction).Extent().First)
                .Build();

            valiData2.ValueA = 1;
            valiData2.ValueB = 2;

            Assert.True(this.Transaction.Derive(false).HasErrors);

            valiData2.ValueB = 1;

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }
    }
}
