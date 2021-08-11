// <copyright file="DefaultDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

using Allors.Ranges;

namespace Allors.Database.Configuration
{
    using System;
    using Data;
    using Domain;
    using Domain.Derivations.Rules.Default;
    using Meta;
    using Microsoft.AspNetCore.Http;
    using Services;

    public abstract class DatabaseServices : IDatabaseServices
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        private IDatabase database;

        private IRanges ranges;

        private IMetaCache metaCache;

        private IClassById classById;

        private IVersionedIdByStrategy versionedIdByStrategy;

        private IPrefetchPolicyCache prefetchPolicyCache;

        private IPreparedSelects preparedSelects;

        private IPreparedExtents preparedExtents;

        private ITreeCache treeCache;

        private IPermissionsCache permissionsCache;

        private IAccessControlCache accessControlCache;

        private ITime time;

        private ICaches caches;

        private ISingletonId singletonId;

        private IPasswordHasher passwordHasher;

        private IMailer mailer;

        private IBarcodeGenerator barcodeGenerator;

        private ITemplateObjectCache templateObjectCache;

        private IDerivationService derivationFactory;

        protected DatabaseServices(IHttpContextAccessor httpContextAccessor = null)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public virtual void OnInit(IDatabase database) => this.database = database;

        public MetaPopulation M => (MetaPopulation)this.database.MetaPopulation;

        public ITransactionServices CreateTransactionServices() => new TransactionServices(this.httpContextAccessor);

        public T Get<T>() =>
            typeof(T) switch
            {
                // Core
                { } type when type == typeof(MetaPopulation) => (T)(object)this.M,
                { } type when type == typeof(IRanges) => (T)(this.ranges ??= new DefaultRanges()),
                { } type when type == typeof(IMetaCache) => (T)(this.metaCache ??= new MetaCache(this.database)),
                { } type when type == typeof(IClassById) => (T)(this.classById ??= new ClassById()),
                { } type when type == typeof(IVersionedIdByStrategy) => (T)(this.versionedIdByStrategy ??= new VersionedIdByStrategy()),
                { } type when type == typeof(IPrefetchPolicyCache) => (T)(this.prefetchPolicyCache ??= new PrefetchPolicyCache(this.database)),
                { } type when type == typeof(IPreparedSelects) => (T)(this.preparedSelects ??= new PreparedSelects(this.database)),
                { } type when type == typeof(IPreparedExtents) => (T)(this.preparedExtents ??= new PreparedExtents(this.database)),
                { } type when type == typeof(ITreeCache) => (T)(this.treeCache ??= new TreeCache()),
                { } type when type == typeof(IPermissionsCache) => (T)(this.permissionsCache ??= new PermissionsCache(this.database)),
                { } type when type == typeof(IAccessControlCache) => (T)(this.accessControlCache ??= new AccessControlCache()),
                { } type when type == typeof(ITime) => (T)(this.time ??= new Time()),
                { } type when type == typeof(ICaches) => (T)(this.caches ??= new Caches()),
                { } type when type == typeof(IPasswordHasher) => (T)(this.passwordHasher ??= this.CreatePasswordHasher()),
                { } type when type == typeof(IDerivationService) => (T)(this.derivationFactory ??= this.CreateDerivationFactory()),
                // Base
                { } type when type == typeof(ISingletonId) => (T)(this.singletonId ??= new SingletonId()),
                { } type when type == typeof(IMailer) => (T)(this.mailer ??= new MailKitMailer()),
                { } type when type == typeof(IBarcodeGenerator) => (T)(this.barcodeGenerator ??= new ZXingBarcodeGenerator()),
                { } type when type == typeof(ITemplateObjectCache) => (T)(this.templateObjectCache ??= new TemplateObjectCache()),
                _ => throw new NotSupportedException($"Service {typeof(T)} not supported")
            };

        protected IPasswordHasher CreatePasswordHasher() => new PasswordHasher();

        protected IDerivationService CreateDerivationFactory() => new DefaultDerivationService(this.Engine);

        protected Engine Engine { get; set; }

        public void Dispose() { }
    }
}
