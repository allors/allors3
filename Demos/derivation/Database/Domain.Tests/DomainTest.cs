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
        private readonly Fixture fixture;
        private readonly bool populate;

        protected DomainTest(Fixture fixture, bool populate = true)
        {
            this.fixture = fixture;
            this.populate = populate;
        }

        public static IEnumerable<object[]> TestedDerivationTypes
            => new[]
            {
                new object[] {DerivationTypes.Coarse },
                new object[] {DerivationTypes.Fine },
            };
        
        public MetaPopulation M { get; private set; }

        public virtual Config Config { get; } = new Config { SetupSecurity = false };

        public ITransaction Transaction { get; private set; }

        public ITime Time => this.Transaction.Database.Services().Time;

        public IDerivationFactory DerivationFactory => this.Transaction.Database.Services().DerivationFactory;

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

            this.Transaction = database.CreateTransaction();

            if (populate)
            {
                new Setup(this.Transaction, this.Config).Apply();
                this.Transaction.Commit();
            }
        }

        protected void SelectDerivationType(DerivationTypes derivationType)
        {
            DatabaseServices databaseServices = derivationType switch
            {
                DerivationTypes.Fine => new FineDatabaseServices(),
                _ => new CourseDatabaseServices()
            };

            var database = new Database(
                databaseServices,
                new Configuration
                {
                    ObjectFactory = new ObjectFactory(fixture.MetaPopulation, typeof(User)),
                });

            this.M = database.Services().M;

            this.Setup(database, populate);
        }
    }
}
