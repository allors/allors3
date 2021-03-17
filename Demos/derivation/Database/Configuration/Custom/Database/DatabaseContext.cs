// <copyright file="DatabaseContext.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Configuration
{
    using Database.Data;
    using Domain;
    using Meta;
    using Microsoft.AspNetCore.Http;

    public abstract class DatabaseContext : IDatabaseContext
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        protected DatabaseContext(IHttpContextAccessor httpContextAccessor = null) => this.httpContextAccessor = httpContextAccessor;

        public virtual void OnInit(IDatabase database)
        {
            this.Database = database;

            this.MetaPopulation = (MetaPopulation)database.ObjectFactory.MetaPopulation;
            this.M = new M(this.MetaPopulation);
            this.MetaCache = new MetaCache(this);
            this.ClassById = new ClassById();
            this.VersionedIdByStrategy = new VersionedIdByStrategy();

            this.PrefetchPolicyCache = new PrefetchPolicyCache(this);
            this.PreparedSelects = new PreparedSelects(this);
            this.PreparedExtents = new PreparedExtents(this);
            this.TreeCache = new TreeCache();

            this.PermissionsCache = new PermissionsCache(this);
            this.AccessControlCache = new AccessControlCache();

            this.Time = new Time();
            this.Caches = new Caches();
            this.SingletonId = new SingletonId();

            this.PasswordHasher = new PasswordHasher();
            this.Mailer = new MailKitMailer();
            this.BarcodeGenerator = new ZXingBarcodeGenerator();
            this.TemplateObjectCache = new TemplateObjectCache();
        }

        public IDatabase Database { get; private set; }

        public MetaPopulation MetaPopulation { get; private set; }

        public M M { get; private set; }

        public IMetaCache MetaCache { get; private set; }

        public IPrefetchPolicyCache PrefetchPolicyCache { get; set; }

        public IPreparedSelects PreparedSelects { get; private set; }

        public IPreparedExtents PreparedExtents { get; private set; }

        public ITreeCache TreeCache { get; private set; }

        public IClassById ClassById { get; private set; }

        public IVersionedIdByStrategy VersionedIdByStrategy { get; private set; }

        public IPermissionsCache PermissionsCache { get; private set; }

        public IAccessControlCache AccessControlCache { get; private set; }

        public ITime Time { get; private set; }

        public ICaches Caches { get; private set; }

        public ISingletonId SingletonId { get; private set; }

        public IPasswordHasher PasswordHasher { get; private set; }

        public IMailer Mailer { get; private set; }

        public IBarcodeGenerator BarcodeGenerator { get; private set; }

        public ITemplateObjectCache TemplateObjectCache { get; private set; }

        public IDerivationFactory DerivationFactory { get; protected set; }

        public ITransactionLifecycle CreateTransactionInstance() => new DefaultTransactionContext(this.httpContextAccessor);

        public void Dispose()
        {
        }
    }
}
