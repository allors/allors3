// <copyright file="DomainTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Collections.Generic;
    using Allors.Database.Adapters.Memory;
    using Allors.Database.Domain;
    using Configuration;
    using Meta;
    using User = Domain.User;

    public abstract class DomainTest : IDisposable
    {
        protected DomainTest(Fixture fixture, bool populate = true)
        {
            DatabaseServices databaseServices = new DefaultDatabaseServices();

            var database = new Database(
                databaseServices,
                new Configuration
                {
                    ObjectFactory = new ObjectFactory(fixture.MetaPopulation, typeof(User)),
                });

            this.M = database.Services.Get<MetaPopulation>();

            this.Setup(database, populate);
        }

        public MetaPopulation M { get; private set; }

        public virtual Config Config { get; } = new Config { SetupSecurity = false };

        public ITransaction Transaction { get; private set; }

        public ITime Time => this.Transaction.Database.Services.Get<ITime>();

        public TimeSpan? TimeShift
        {
            get => this.Time.Shift;

            set => this.Time.Shift = value;
        }

        public void Dispose()
        {
            this.Transaction.Rollback();
            this.Transaction = null;
        }

        protected void Setup(IDatabase database, bool populate)
        {
            database.Init();

            if (populate)
            {
                new Setup(database, this.Config).Apply();
            }

            this.Transaction = database.CreateTransaction();
        }
    }
}
