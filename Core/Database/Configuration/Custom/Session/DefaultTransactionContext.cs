// <copyright file="DefaultTransactionContext.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Linq;
    using System.Security.Claims;
    using Database;
    using Domain;
    using Microsoft.AspNetCore.Http;

    public class DefaultTransactionContext : ITransactionContext
    {
        private readonly HttpContext httpContext;

        public DefaultTransactionContext(IHttpContextAccessor httpContextAccessor) => this.httpContext = new HttpContext(httpContextAccessor);

        public virtual void OnInit(ITransaction transaction) => this.httpContext.OnInit(transaction);

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
