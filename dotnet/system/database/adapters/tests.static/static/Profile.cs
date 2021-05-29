// <copyright file="Profile.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters
{
    using System;
    using System.Collections.Generic;
    using Memory;
    using Meta;
    using C1 = Domain.C1;
    using ObjectFactory = ObjectFactory;

    public abstract class Profile : IProfile
    {
        public ITransaction Transaction { get; private set; }

        public IDatabase Database { get; private set; }

        public abstract Action[] Markers { get; }

        public virtual Action[] Inits
        {
            get
            {
                var inits = new List<Action> { this.Init };

                if (Settings.ExtraInits)
                {
                    inits.Add(this.Init);
                }

                return inits.ToArray();
            }
        }

        public void SwitchDatabase()
        {
            this.Transaction.Rollback();
            this.Database = this.CreateDatabase();
            this.Transaction = this.Database.CreateTransaction();
            this.Transaction.Commit();
        }

        public virtual void Dispose()
        {
            this.Transaction?.Rollback();

            this.Transaction = null;
            this.Database = null;
        }

        public IDatabase CreateMemoryDatabase()
        {
            var metaPopulation = new MetaBuilder().Build();
            var scope = new DefaultDomainDatabaseServices();
            return new Database(scope, new Memory.Configuration
            {
                ObjectFactory = new ObjectFactory(metaPopulation, typeof(C1)),
            });
        }

        public abstract IDatabase CreateDatabase();

        internal ITransaction CreateTransaction() => this.Database.CreateTransaction();

        protected internal void Init()
        {
            try
            {
                this.Transaction?.Rollback();

                this.Database = this.CreateDatabase();
                this.Database.Init();
                this.Transaction = this.Database.CreateTransaction();
                this.Transaction.Commit();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
        }
    }
}
