// <copyright file="DerivationLogTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the ApplicationTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class ValidationTests : DomainTest, IClassFixture<Fixture>
    {
        public ValidationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void AssertIsUniqueTest()
        {
            var valiData = new ValiDataBuilder(this.Transaction).Build();

            Assert.True(this.Transaction.Derive(false).HasErrors);

            valiData.RequiredPerson = new People(this.Transaction).Extent().First;

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }
    }
}
