// <copyright file="DefaultDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Domain
{
    using Data;
    
    using Meta;
    using Microsoft.AspNetCore.Http;

    public abstract class DatabaseState : IDatabaseState
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        protected DatabaseState(IHttpContextAccessor httpContextAccessor = null) => this.httpContextAccessor = httpContextAccessor;

        public virtual void OnInit(IDatabase database)
        {
            this.Database = database;

            this.MetaPopulation = (MetaPopulation)database.ObjectFactory.MetaPopulation;
            this.M = new M(this.MetaPopulation);
            this.MetaCache = new MetaCache(this);
            this.WorkspaceMetaCache = new WorkspaceMetaCache(this);

            this.PrefetchPolicyCache = new PrefetchPolicyCache(this);
            this.PreparedFetches = new PreparedFetches(this);
            this.PreparedExtents = new PreparedExtents(this);
            this.TreeCache = new TreeCache();

            this.PermissionsCache = new PermissionsCache(this);
            this.EffectivePermissionCache = new EffectivePermissionCache();
            this.WorkspaceEffectivePermissionCache = new WorkspaceEffectivePermissionCache();

            this.Time = new Time();
            this.Caches = new Caches();
            this.SingletonId = new SingletonId();

            this.PasswordHasher = new PasswordHasher();
            this.Mailer = new Mailer();
            this.BarcodeGenerator = new BarcodeGenerator();
            this.TemplateObjectCache = new TemplateObjectCache();
        }

        public IDatabase Database { get; private set; }

        public MetaPopulation MetaPopulation { get; private set; }

        public M M { get; private set; }

        public IMetaCache MetaCache { get; private set; }

        public IWorkspaceMetaCache WorkspaceMetaCache { get; set; }

        public IPrefetchPolicyCache PrefetchPolicyCache { get; set; }

        public IPreparedFetches PreparedFetches { get; private set; }

        public IPreparedExtents PreparedExtents { get; private set; }

        public ITreeCache TreeCache { get; private set; }

        public IPermissionsCache PermissionsCache { get; set; }

        public IEffectivePermissionCache EffectivePermissionCache { get; private set; }

        public IWorkspaceEffectivePermissionCache WorkspaceEffectivePermissionCache { get; private set; }

        public ITime Time { get; private set; }

        public ICaches Caches { get; private set; }

        public ISingletonId SingletonId { get; private set; }

        public IPasswordHasher PasswordHasher { get; private set; }

        public IMailer Mailer { get; private set; }

        public IBarcodeGenerator BarcodeGenerator { get; private set; }

        public ITemplateObjectCache TemplateObjectCache { get; private set; }

        public IDerivationFactory DerivationFactory { get; protected set; }

        public ISessionStateLifecycle CreateSessionInstance() => new DefaultSessionState(this.httpContextAccessor);

        public void Dispose()
        {
        }
    }
}
