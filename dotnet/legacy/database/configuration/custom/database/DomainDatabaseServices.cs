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
    using Domain.Derivations.Rules.Default;
    using Meta;
    using Microsoft.AspNetCore.Http;

    public abstract class DomainDatabaseServices : IDomainDatabaseServices
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        protected DomainDatabaseServices(Engine engine, IHttpContextAccessor httpContextAccessor = null)
        {
            this.Engine = engine;
            this.httpContextAccessor = httpContextAccessor;
        }

        public virtual void OnInit(IDatabase database)
        {
            this.Database = database;

            this.MetaCache ??= new MetaCache(this);
            this.ClassById ??= new ClassById();
            this.VersionedIdByStrategy ??= new VersionedIdByStrategy();

            this.PrefetchPolicyCache ??= new PrefetchPolicyCache(this);
            this.PreparedSelects ??= new PreparedSelects(this.M);
            this.PreparedExtents ??= new PreparedExtents(this.M);
            this.TreeCache ??= new TreeCache();

            this.PermissionsCache ??= new PermissionsCache(this);
            this.AccessControlCache ??= new AccessControlCache();

            this.Time ??= new Time();
            this.Caches ??= new Caches();
            this.PasswordHasher ??= new PasswordHasher();
        }

        public IDatabase Database { get; private set; }

        public MetaPopulation M => (MetaPopulation)this.Database.MetaPopulation;

        public IMetaCache MetaCache { get; protected set; }

        public IClassById ClassById { get; protected set; }

        public IVersionedIdByStrategy VersionedIdByStrategy { get; protected set; }

        public IPrefetchPolicyCache PrefetchPolicyCache { get; set; }

        public IPreparedSelects PreparedSelects { get; protected set; }

        public IPreparedExtents PreparedExtents { get; protected set; }

        public ITreeCache TreeCache { get; protected set; }

        public IPermissionsCache PermissionsCache { get; set; }

        public IAccessControlCache AccessControlCache { get; protected set; }

        public ITime Time { get; protected set; }

        public ICaches Caches { get; protected set; }

        public IPasswordHasher PasswordHasher { get; protected set; }

        public IDerivationFactory DerivationFactory { get; protected set; }

        public ITransactionServices CreateTransactionServices() => new DefaultDomainTransactionServices(this.httpContextAccessor);

        protected Engine Engine { get; }
        
        public void Dispose()
        {
        }
    }
}
