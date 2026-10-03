// <copyright file="DomainTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System;
    using Adapters.Memory;
    using Configuration;
    using Meta;
    using User = Domain.User;

    public class DomainTest : IDisposable
    {
        public DomainTest(Fixture fixture)
        {
            var database = new Database(
                new TestDatabaseServices(fixture.Engine),
                new Configuration
                {
                    ObjectFactory = new ObjectFactory(fixture.MetaPopulation, typeof(User)),
                });

            this.M = ((IDatabase)database).Services.Get<MetaPopulation>();

            database.Init();

            new Setup(database, this.Config).Apply();

            this.Transaction = database.CreateTransaction();
        }

        public MetaPopulation M { get; }

        public virtual Config Config { get; } = new Config { SetupSecurity = false };

        public ITransaction Transaction { get; private set; }

        public void Dispose()
        {
            this.Transaction.Rollback();
            this.Transaction = null;
        }
    }
}
