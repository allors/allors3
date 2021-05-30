// <copyright file="DefaultDomainTransactionServices.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Configuration
{
    using Database;
    using Derivations;
    using Domain;
    using Domain.Derivations.Rules.Default;
    using Microsoft.AspNetCore.Http;

    public class DefaultDomainTransactionServices : IDomainTransactionServices
    {
        private readonly HttpContext httpContext;

        public DefaultDomainTransactionServices(IHttpContextAccessor httpContextAccessor) => this.httpContext = new HttpContext(httpContextAccessor);

        public virtual void OnInit(ITransaction transaction)
        {
            this.httpContext.OnInit(transaction);
            this.Derive = new DefaultDerive(transaction);
        }

        public IDerive Derive { get; private set; }

        public User User
        {
            get => this.httpContext.User;
            set => this.httpContext.User = value;
        }
        public void Dispose()
        {
        }
    }
}
