// <copyright file="DatabaseServices.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Configuration
{
    using System;
    using Database;
    using Data;
    using Database.Derivations;
    using Derivations.Default;
    using Domain;
    using Meta;
    using Ranges;
    using Services;

    public abstract class DatabaseServices : IDatabaseServices
    {
        private IRanges<long> ranges;

        private IMetaCache metaCache;

        private ISecurity security;

        private IClassById classById;

        private IVersionedIdByStrategy versionedIdByStrategy;

        private IPrefetchPolicyCache prefetchPolicyCache;

        private IPreparedSelects preparedSelects;

        private IPreparedExtents preparedExtents;

        private ITreeCache treeCache;

        private IPermissions permissions;

        private ITime time;

        private ICaches caches;

        private IPasswordHasher passwordHasher;

        private IDerivationService derivationService;

        private IProcedures procedures;

        private IWorkspaceMask workspaceMask;

        protected DatabaseServices(Engine engine) => this.Engine = engine;

        internal IDatabase Database { get; private set; }

        public virtual void OnInit(IDatabase database)
        {
            this.Database = database;
            this.M = (MetaPopulation)this.Database.MetaPopulation;
            this.metaCache = new MetaCache(this.Database);

            // Init() recreates the schema and restarts object-id allocation, so any service that caches
            // domain identity -> database object-id/version becomes stale and must be discarded. Otherwise
            // a second Init/Setup in the same process reuses dead ids -- e.g. the security Setup merger
            // resolves a Grant to a wiped id and never re-links its subjects, failing the
            // "Grant.Subjects, Grant.SubjectGroups at least one!" derivation.
            this.caches = null;
            this.classById = null;
            this.versionedIdByStrategy = null;
            this.security = null;
            this.permissions = null;
        }

        public MetaPopulation M { get; private set; }

        public ITransactionServices CreateTransactionServices() => new TransactionServices();

        public T Get<T>() =>
            typeof(T) switch
            {
                // System
                { } type when type == typeof(IMetaCache) => (T)this.metaCache,
                { } type when type == typeof(IDerivationService) => (T)(this.derivationService ??= this.CreateDerivationFactory()),
                { } type when type == typeof(IProcedures) => (T)(this.procedures ??= new Procedures(this.Database.ObjectFactory.Assembly)),
                { } type when type == typeof(ISecurity) => (T)(this.security ??= new Security(this)),
                { } type when type == typeof(IPrefetchPolicyCache) => (T)(this.prefetchPolicyCache ??= new PrefetchPolicyCache(this.Database, this.metaCache)),
                // Core
                { } type when type == typeof(MetaPopulation) => (T)(object)this.M,
                { } type when type == typeof(IRanges<long>) => (T)(this.ranges ??= new DefaultStructRanges<long>()),
                { } type when type == typeof(IClassById) => (T)(this.classById ??= new ClassById()),
                { } type when type == typeof(IVersionedIdByStrategy) => (T)(this.versionedIdByStrategy ??= new VersionedIdByStrategy()),
                { } type when type == typeof(IPreparedSelects) => (T)(this.preparedSelects ??= new PreparedSelects(this.M)),
                { } type when type == typeof(IPreparedExtents) => (T)(this.preparedExtents ??= new PreparedExtents(this.M)),
                { } type when type == typeof(ITreeCache) => (T)(this.treeCache ??= new TreeCache()),
                { } type when type == typeof(IPermissions) => (T)(this.permissions ??= new Permissions()),
                { } type when type == typeof(ITime) => (T)(this.time ??= new Time()),
                { } type when type == typeof(ICaches) => (T)(this.caches ??= new Caches()),
                { } type when type == typeof(IPasswordHasher) => (T)(this.passwordHasher ??= this.CreatePasswordHasher()),
                { } type when type == typeof(IWorkspaceMask) => (T)(this.workspaceMask ??= new WorkspaceMask(this.M)),
                _ => throw new NotSupportedException($"Service {typeof(T)} not supported")
            };

        protected abstract IPasswordHasher CreatePasswordHasher();

        protected abstract IDerivationService CreateDerivationFactory();

        protected Engine Engine { get; }

        public void Dispose() { }
    }
}
