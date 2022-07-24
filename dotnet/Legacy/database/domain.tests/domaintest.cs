// <copyright file="DomainTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Globalization;
    using Adapters.Memory;
    using Domain;
    using Configuration;
    using Database;
    using Database.Derivations;
    using Meta;
    using User = Domain.User;

    public class DomainTest : IDisposable
    {
        public DomainTest(Fixture fixture, bool populate = true)
        {
            var database = new Database(
                new TestDatabaseServices(fixture.Engine),
                new Configuration
                {
                    ObjectFactory = new ObjectFactory(fixture.M, typeof(User)),
                });

            this.M = database.Services.Get<MetaPopulation>();

            this.Setup(database, populate);
        }

        public MetaPopulation M { get; }

        public virtual Config Config { get; } = new Config { SetupSecurity = false };

        public ITransaction Transaction { get; private set; }

        public ITime Time => this.Transaction.Database.Services.Get<Time>();

        public TimeSpan? TimeShift
        {
            get => this.Time.Shift;

            set => this.Time.Shift = value;
        }

        public IDerivationService DerivationService => this.Transaction.Database.Services.Get<IDerivationService>();

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
                CultureInfo.CurrentCulture = new CultureInfo("en-GB");
                CultureInfo.CurrentUICulture = new CultureInfo("en-GB");

                new Setup(database, this.Config).Apply();
            }

            this.Transaction = database.CreateTransaction();
        }
    }
}
