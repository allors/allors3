// <copyright file="DerivationLogTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the ApplicationTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System;

    using Allors;
    using Allors.Database.Domain;

    using Xunit;

    public class ValidationTests : DomainTest, IClassFixture<Fixture>
    {
        public ValidationTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void AssertIsUniqueTest()
        {
            var c1 = new ValidationC1Builder(this.Transaction).Build();
            var c2 = new ValidationC2Builder(this.Transaction).Build();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            c1.UniqueId = Guid.NewGuid();

            Assert.False(this.Transaction.Derive(false).HasErrors);

            c2.UniqueId = c1.UniqueId;

            Assert.True(this.Transaction.Derive(false).HasErrors);
        }
    }
}
