// <copyright file="DefaultSessionContext.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Configuration
{
    using System.Linq;
    using System.Security.Claims;
    using Database;
    using Domain;
    using Microsoft.AspNetCore.Http;
    using ISession = Database.ISession;

    public class DefaultSessionContext : ISessionContext
    {
        private readonly HttpContext httpContext;

        public DefaultSessionContext(IHttpContextAccessor httpContextAccessor) => this.httpContext = new HttpContext(httpContextAccessor);

        public virtual void OnInit(ISession session) => this.httpContext.OnInit(session);

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
