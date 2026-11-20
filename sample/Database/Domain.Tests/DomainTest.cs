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
            var services = new DefaultDatabaseServices(fixture.Engine);
            var objectFactory = new ObjectFactory(fixture.MetaPopulation, typeof(User));
            var database = new Database(services, new Configuration { ObjectFactory = objectFactory });

            this.M = database.Services.Get<MetaPopulation>();

            database.Init();

            // The setup of Core, with its security, through the hooks of this application.
            new Setup(database, new Config()).Apply();

            this.Transaction = database.CreateTransaction();
        }

        public MetaPopulation M { get; }

        public ITransaction Transaction { get; private set; }

        public void Dispose()
        {
            this.Transaction.Rollback();
            this.Transaction = null;
        }
    }
}
