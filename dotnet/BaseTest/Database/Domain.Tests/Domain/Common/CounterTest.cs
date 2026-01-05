// <copyright file="CounterTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the ApplicationTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using Domain;
    using Xunit;

    public class CounterTest : DomainTest, IClassFixture<Fixture>
    {
        public CounterTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void Meta()
        {
            var counterBuilder = new CounterBuilder(this.Transaction).Build();

            Assert.True(counterBuilder.ExistUniqueId);
            Assert.NotEqual(Guid.Empty, counterBuilder.UniqueId);

            Assert.Equal(0, counterBuilder.Value);

            var secondCounterBuilder = new CounterBuilder(this.Transaction).Build();

            Assert.NotEqual(counterBuilder.UniqueId, secondCounterBuilder.UniqueId);
        }

        [Fact]
        public void NextValue()
        {
            var id = Guid.NewGuid();

            var counter = new CounterBuilder(this.Transaction).WithUniqueId(id).Build();
            this.Transaction.Derive();
            this.Transaction.Commit();

            Assert.Equal(1, counter.NextValue());
            Assert.Equal(2, counter.NextValue());
            Assert.Equal(3, counter.NextValue());
            Assert.Equal(4, counter.NextValue());
        }
    }
}
