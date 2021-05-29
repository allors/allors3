// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Profile.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
// Dual Licensed under
//   a) the Lesser General Public Licence v3 (LGPL)
//   b) the Allors License
// The LGPL License is included in the file lgpl.txt.
// The Allors License is an addendum to your contract.
// Allors Platform is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Adapters
{
    using System;
    using Memory;
    using Meta;
    using C1 = Domain.C1;
    using ObjectFactory = ObjectFactory;

    public abstract class Profile : IDisposable
    {
        private ITransaction transaction;
        private ITransaction transaction2;

        protected Profile() => this.M = new MetaBuilder().Build();

        public MetaPopulation M { get; }

        public IObject[] CreateArray(IObjectType objectType, int count)
        {
            var type = objectType.ClrType;
            return (IObject[])Array.CreateInstance(type, count);
        }

        public IDatabase CreateMemoryDatabase() =>
            new Database(new DefaultDomainDatabaseServices(), new Memory.Configuration
            {
                ObjectFactory = new ObjectFactory(this.M, typeof(C1)),
            });

        public ITransaction CreateTransaction() => this.GetDatabase().CreateTransaction();

        public ITransaction CreateTransaction2() => this.GetDatabase2().CreateTransaction();

        public virtual void Dispose()
        {
            if (this.transaction != null)
            {
                this.transaction.Rollback();
                this.transaction = null;
            }

            if (this.transaction2 != null)
            {
                this.transaction2.Rollback();
                this.transaction2 = null;
            }
        }

        public abstract IDatabase GetDatabase();

        public abstract IDatabase GetDatabase2();

        public ITransaction GetTransaction() => this.transaction ??= this.GetDatabase().CreateTransaction();

        public ITransaction GetTransaction2() => this.transaction2 ??= this.GetDatabase2().CreateTransaction();

        public abstract void Init();

        public abstract bool IsRollbackSupported();
    }
}
