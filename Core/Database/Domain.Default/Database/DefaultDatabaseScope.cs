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

    public partial class DefaultDatabaseScope : DatabaseScope 
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public DefaultDatabaseScope(IHttpContextAccessor httpContextAccessor = null) => this.httpContextAccessor = httpContextAccessor;

        public override void OnInit(IDatabase database)
        {
            this.M = new M((MetaPopulation)database.ObjectFactory.MetaPopulation);

            this.BarcodeService = new BarcodeService();
            this.CacheService = new CacheService();
            this.DerivationService = new DerivationService();
            this.ExtentService = new ExtentService(database);
            this.FetchService = new FetchService(database);
            this.MailService = new MailService();
            this.PasswordService = new PasswordService();
            this.SingletonService = new SingletonService();
            this.StickyService = new StickyService();
            this.TimeService = new TimeService();
            this.TreeService = new TreeService();
        }

        public override ISessionScope CreateSessionScope() => new DefaultSessionScope(this.httpContextAccessor);
    }
}
