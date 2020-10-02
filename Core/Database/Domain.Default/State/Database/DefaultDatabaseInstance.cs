// <copyright file="DefaultDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors
{
    using Domain;
    using Meta;
    using Microsoft.AspNetCore.Http;
    using Services;

    public class DefaultDatabaseInstance : IDatabaseInstance
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public DefaultDatabaseInstance(IHttpContextAccessor httpContextAccessor = null) => this.httpContextAccessor = httpContextAccessor;

        public void OnInit(IDatabase database)
        {
            this.Database = database;
            this.MetaPopulation = (MetaPopulation)database.ObjectFactory.MetaPopulation;
            this.M = new M(this.MetaPopulation);
            this.MetaService = new MetaCache(this);
            this.PrefetchPolicyCache = new PrefetchPolicyCache(this);
            this.PreparedExtentCache = new PreparedExtentCache(this);
            this.TreeCache = new TreeCache();

            this.PermissionsCache = new PermissionsCache(this);
            this.EffectivePermissionCache = new EffectivePermissionCache();
            this.WorkspaceEffectivePermissionCache = new WorkspaceEffectivePermissionCache();

            this.TemplateObjectCache = new TemplateObjectCache();
            this.BarcodeGenerator = new BarcodeGenerator();

            this.DerivationService = new DerivationService();
            this.FetchService = new FetchService(this);
            this.MailService = new MailService();
            this.PasswordService = new PasswordService();
            this.SingletonService = new SingletonService();
            this.StickyService = new StickyService();
            this.TimeService = new TimeService();
        }

        public IDatabase Database { get; private set; }

        public MetaPopulation MetaPopulation { get; private set; }

        public M M { get; private set; }

        public IPrefetchPolicyCache PrefetchPolicyCache { get; set; }

        public IPermissionsCache PermissionsCache { get; set; }

        public IEffectivePermissionCache EffectivePermissionCache { get; private set; }

        public IWorkspaceEffectivePermissionCache WorkspaceEffectivePermissionCache { get; private set; }

        public ITemplateObjectCache TemplateObjectCache { get; private set; }

        public IBarcodeGenerator BarcodeGenerator { get; private set; }
        public IDerivationService DerivationService { get; private set; }
        public IPreparedExtentCache PreparedExtentCache { get; private set; }
        public IFetchService FetchService { get; private set; }
        public IMailService MailService { get; private set; }
        public IMetaCache MetaService { get; private set; }
        public IPasswordService PasswordService { get; private set; }
        public ISingletonService SingletonService { get; private set; }
        public IStickyService StickyService { get; private set; }
        public ITimeService TimeService { get; private set; }
        public ITreeCache TreeCache { get; private set; }

        public ISessionInstanceLifecycle CreateSessionInstance() => new DefaultSessionInstance(this.httpContextAccessor);

        public void Dispose()
        {
        }
    }
}
