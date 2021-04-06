// <copyright file="DefaultDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Configuration
{
    using Database;
    using Data;
    using Domain;
    using Domain.Derivations.Default;
    using Meta;
    using Microsoft.AspNetCore.Http;

    public abstract class DatabaseContext : IDatabaseContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        protected DatabaseContext(Engine engine, IHttpContextAccessor httpContextAccessor = null)
        {
            this.Engine = engine;
            this.httpContextAccessor = httpContextAccessor;
        }

        public virtual void OnInit(IDatabase database)
        {
            this.Database = database;

            this.MetaCache = new MetaCache(this);
            this.ClassById = new ClassById();
            this.VersionedIdByStrategy = new VersionedIdByStrategy();

            this.PrefetchPolicyCache = new PrefetchPolicyCache(this);
            this.PreparedSelects = new PreparedSelects(this.M);
            this.PreparedExtents = new PreparedExtents(this.M);
            this.TreeCache = new TreeCache();

            this.PermissionsCache = new PermissionsCache(this);
            this.AccessControlCache = new AccessControlCache();

            this.Time = new Time();
            this.Caches = new Caches();
            this.PasswordHasher = new PasswordHasher();
        }

        public IDatabase Database { get; private set; }

        public MetaPopulation M => (MetaPopulation)this.Database.MetaPopulation;

        public IMetaCache MetaCache { get; private set; }

        public IClassById ClassById { get; private set; }

        public IVersionedIdByStrategy VersionedIdByStrategy { get; private set; }

        public IPrefetchPolicyCache PrefetchPolicyCache { get; set; }

        public IPreparedSelects PreparedSelects { get; private set; }

        public IPreparedExtents PreparedExtents { get; private set; }

        public ITreeCache TreeCache { get; private set; }

        public IPermissionsCache PermissionsCache { get; set; }

        public IAccessControlCache AccessControlCache { get; private set; }

        public ITime Time { get; private set; }

        public ICaches Caches { get; private set; }

        public IPasswordHasher PasswordHasher { get; private set; }

        public IDerivationFactory DerivationFactory { get; protected set; }

        public ITransactionLifecycle CreateTransactionInstance() => new DefaultTransactionContext(this.httpContextAccessor);

        protected Engine Engine { get; }
        
        public void Dispose()
        {
        }
    }
}
