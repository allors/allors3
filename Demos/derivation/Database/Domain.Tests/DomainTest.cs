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
    using Allors.Database.Meta;
    using Configuration;

    public abstract class DomainTest : IDisposable
    {
        protected DomainTest(Fixture fixture, bool populate = true)
        {
            var database = new Database(
                new DefaultDatabaseContext(),
                new Configuration
                {
                    ObjectFactory = new ObjectFactory(fixture.MetaPopulation, typeof(User)),
                });

            this.M = database.Context().M;

            this.Setup(database, populate);
        }

        public static IEnumerable<object[]> TestedDerivationTypes
            => new object[][] {
                new object[] {DerivationTypes.Coarse },
                new object[] {DerivationTypes.Fine },
            };
        
        public M M { get; }

        public virtual Config Config { get; } = new Config { SetupSecurity = false };

        public ITransaction Transaction { get; private set; }

        public ITime Time => this.Transaction.Database.Context().Time;

        public IDerivationFactory DerivationFactory => this.Transaction.Database.Context().DerivationFactory;

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

            database.RegisterDerivations();

            this.Transaction = database.CreateTransaction();

            if (populate)
            {
                new Setup(this.Transaction, this.Config).Apply();
                this.Transaction.Commit();
            }
        }

        protected void RegisterAdditionalDerivations(DerivationTypes derivationType)
        {
            if (derivationType == DerivationTypes.Fine)
            {
                this.Transaction.Database.RegisterFineDerivations();
            }
            else
            {
                this.Transaction.Database.RegisterCoarseDerivations();
            }
        }
    }
}
