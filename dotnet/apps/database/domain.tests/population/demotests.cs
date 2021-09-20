// <copyright file="DemoTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class DemoTests : DomainTest, IClassFixture<Fixture>
    {
        public DemoTests(Fixture fixture) : base(fixture, false) { }

        [Fact]
        public void TestPopulate()
        {
            var transaction = this.Transaction;
            var database = transaction.Database;

            var config = new Config();
            new Setup(database, config).Apply();

            transaction.Rollback();

            new Upgrade(transaction, null).Execute();

            transaction.Derive();
            transaction.Commit();

            this.Transaction.GetSingleton().Full(config.DataPath, transaction.Faker());

            transaction.Derive();
            transaction.Commit();
        }
    }
}
