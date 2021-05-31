// <copyright file="DefaultTransactionContext.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

using Allors.Database.Derivations;
using Allors.Database.Domain.Derivations.Rules.Default;

namespace Allors.Database.Configuration
{
    using Database;
    using Domain;
    using Microsoft.AspNetCore.Http;

    public class DefaultTransactionServices : IDomainTransactionServices
    {
        private readonly HttpContext httpContext;

        public DefaultTransactionServices(IHttpContextAccessor httpContextAccessor) => this.httpContext = new HttpContext(httpContextAccessor);

        public virtual void OnInit(ITransaction transaction)
        {
            this.Derive = new DefaultDerive(transaction);
            this.httpContext.OnInit(transaction);
        } 

        public IDerive Derive { get; private set; }

        public void Dispose()
        {
        }

        public User User
        {
            get => this.httpContext.User;
            set => this.httpContext.User = value;
        }
    }
}
