// <copyright file="DefaultDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Configuration
{
    using System;
    using Bogus;
    using Data;
    using Database.Derivations;
    using Derivations.Default;
    using Domain;
    using Meta;
    using Microsoft.AspNetCore.Http;
    using Ranges;
    using Services;

    public abstract class DatabaseServices : IDatabaseServices
    {
        private readonly IHttpContextAccessor httpContextAccessor;

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

        private ISingletonId singletonId;

        private IPasswordHasher passwordHasher;

        private IMailer mailer;

        private IBarcodeGenerator barcodeGenerator;

        private ITemplateObjectCache templateObjectCache;

        private IDerivationService derivationService;

        private IProcedures procedures;

        private IWorkspaceMask workspaceMask;

        private Faker faker;

        protected DatabaseServices(Engine engine, IHttpContextAccessor httpContextAccessor = null)
        {
            this.Engine = engine;
            this.httpContextAccessor = httpContextAccessor;
        }

        internal IDatabase Database { get; private set; }

        public virtual void OnInit(IDatabase database)
        {
            this.Database = database;
            this.M = (MetaPopulation)this.Database.MetaPopulation;
            this.metaCache = new MetaCache(this.Database);
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
                { } type when type == typeof(IPreparedSelects) => (T)(this.preparedSelects ??= new PreparedSelects(this.Database)),
                { } type when type == typeof(IPreparedExtents) => (T)(this.preparedExtents ??= new PreparedExtents(this.Database)),
                { } type when type == typeof(ITreeCache) => (T)(this.treeCache ??= new TreeCache()),
                { } type when type == typeof(IPermissions) => (T)(this.permissions ??= new Permissions()),
                { } type when type == typeof(ITime) => (T)(this.time ??= new Time()),
                { } type when type == typeof(ICaches) => (T)(this.caches ??= new Caches()),
                { } type when type == typeof(IPasswordHasher) => (T)(this.passwordHasher ??= this.CreatePasswordHasher()),
                { } type when type == typeof(IWorkspaceMask) => (T)(this.workspaceMask ??= new WorkspaceMask(this.M)),
                // Base
                { } type when type == typeof(ISingletonId) => (T)(this.singletonId ??= new SingletonId()),
                { } type when type == typeof(IMailer) => (T)(this.mailer ??= new MailKitMailer()),
                { } type when type == typeof(IBarcodeGenerator) => (T)(this.barcodeGenerator ??= new ZXingBarcodeGenerator()),
                { } type when type == typeof(ITemplateObjectCache) => (T)(this.templateObjectCache ??= new TemplateObjectCache()),
                // Custom
                { } type when type == typeof(Faker) => (T)(object)(this.faker ??= new Faker()),
                _ => throw new NotSupportedException($"Service {typeof(T)} not supported")
            };

        protected abstract IPasswordHasher CreatePasswordHasher();

        protected abstract IDerivationService CreateDerivationFactory();

        protected Engine Engine { get; }

        public void Dispose() { }
    }
}
