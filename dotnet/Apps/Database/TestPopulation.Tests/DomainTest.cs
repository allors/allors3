// <copyright file="DomainTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System;
    using Adapters.Memory;
    using Configuration;
    using Database.Derivations;
    using Meta;
    using Allors;
    using Database.Services;
    using Organisation = Domain.Organisation;
    using Person = Domain.Person;
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

        public virtual Config Config { get; } = new Config { SetupSecurity = false, };

        public ITransaction Transaction { get; private set; }

        public ITime Time => this.Transaction.Database.Services.Get<ITime>();

        public ISecurity Security => this.Transaction.Database.Services.Get<ISecurity>();

        public TimeSpan? TimeShift
        {
            get => this.Time.Shift;

            set => this.Time.Shift = value;
        }

        protected Organisation InternalOrganisation { get; set; }

        public void Dispose()
        {
            this.Transaction.Rollback();
            this.Transaction = null;
        }

        protected IValidation Derive() => this.Transaction.Derive(false, true);

        protected void Setup(IDatabase database, bool populate)
        {
            database.Init();

            this.Transaction ??= database.CreateTransaction();

            if (populate)
            {
                this.Transaction.Services.Get<IUserService>().User = new AutomatedAgents(this.Transaction).System;
                this.Populate(database);
            }
        }

        private void Populate(IDatabase database) => new TestPopulation(this.Transaction, this.Config).Populate(database);
    }
}
