// <copyright file="DemoTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    [Trait("Category", "Security")]
    public class SetupTests : DomainTest, IClassFixture<Fixture>
    {
        public SetupTests(Fixture fixture) : base(fixture, false) { }

        [Fact]
        public void Twice()
        {
            var transaction = this.Transaction;
            var database = transaction.Database;

            var config = new Config();
            new Setup(database, config).Apply();

            transaction.Rollback();

            var objects1 = new Objects(transaction).Extent().ToArray();

            new Setup(database, config).Apply();

            transaction.Rollback();

            var objects2 = new Objects(transaction).Extent().ToArray();

            var diff = objects2.Except(objects1).ToArray();

            Assert.Empty(diff);
        }
    }
}
