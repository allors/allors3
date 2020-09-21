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

    using Allors;
    using Allors.Meta;
    using Domain;
    using Memory;
    using ObjectFactory = Allors.ObjectFactory;

    public abstract class Profile : IDisposable
    {
        private ISession session;
        private ISession session2;

        protected Profile()
        {
            this.MetaPopulation = new MetaBuilder().Build();
            this.M = new M(this.MetaPopulation);
        }

        public MetaPopulation MetaPopulation { get; }

        public M M { get; set; }
        
        public IObject[] CreateArray(ObjectType objectType, int count)
        {
            var type = objectType.ClrType;
            return (IObject[])Array.CreateInstance(type, count);
        }

        public IDatabase CreateMemoryDatabase() =>
            new Database(new DatabaseScope(), new Memory.Configuration
            {
                ObjectFactory = new ObjectFactory(this.MetaPopulation, typeof(C1)),
            });

        public ISession CreateSession() => this.GetDatabase().CreateSession();

        public ISession CreateSession2() => this.GetDatabase2().CreateSession();

        public virtual void Dispose()
        {
            if (this.session != null)
            {
                this.session.Rollback();
                this.session = null;
            }

            if (this.session2 != null)
            {
                this.session2.Rollback();
                this.session2 = null;
            }
        }

        public abstract IDatabase GetDatabase();

        public abstract IDatabase GetDatabase2();

        public ISession GetSession() => this.session ??= this.GetDatabase().CreateSession();

        public ISession GetSession2() => this.session2 ??= this.GetDatabase2().CreateSession();

        public abstract void Init();

        public abstract bool IsRollbackSupported();
    }
}
