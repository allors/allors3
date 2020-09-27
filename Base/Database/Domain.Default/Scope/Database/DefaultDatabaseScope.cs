// <copyright file="DefaultDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors
{
    using Meta;
    using Microsoft.AspNetCore.Http;
    using Services;

    public class DefaultDatabaseScope : IDatabaseScope 
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public DefaultDatabaseScope(IHttpContextAccessor httpContextAccessor = null) => this.httpContextAccessor = httpContextAccessor;

        public virtual void OnInit(IDatabase database)
        {
            var metaPopulation = (MetaPopulation)database.MetaPopulation;

            this.M = new M((MetaPopulation)database.ObjectFactory.MetaPopulation);

            this.BarcodeService = new BarcodeService();
            this.CacheService = new CacheService();
            this.DerivationService = new DerivationService();
            this.ExtentService = new ExtentService(database);
            this.FetchService = new FetchService(database);
            this.MailService = new MailService();
            this.MetaService = new Services.MetaService(metaPopulation, database.ObjectFactory.Assembly);
            this.PasswordService = new PasswordService();
            this.SingletonService = new SingletonService();
            this.StickyService = new StickyService();
            this.TimeService = new TimeService();
            this.TreeService = new TreeService();
        }

        public M M { get; private set; }
        public IBarcodeService BarcodeService { get; private set; }
        public ICacheService CacheService { get; private set; }
        public IDerivationService DerivationService { get; private set; }
        public IExtentService ExtentService { get; private set; }
        public IFetchService FetchService { get; private set; }
        public IMailService MailService { get; private set; }
        public IMetaService MetaService { get; private set; }
        public IPasswordService PasswordService { get; private set; }
        public ISingletonService SingletonService { get; private set; }
        public IStickyService StickyService { get; private set; }
        public ITimeService TimeService { get; private set; }
        public ITreeService TreeService { get; private set; }

        public ISessionLifecycle CreateSessionScope() => new DefaultSessionScope(this.httpContextAccessor);

        public void Dispose()
        {
        }
    }
}
