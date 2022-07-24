// <copyright file="HttpContext.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Configuration
{
    using System.Linq;
    using System.Security.Claims;
    using Domain;
    using Microsoft.AspNetCore.Http;

    public class HttpUserService : IUserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private ITransaction transaction;
        private User user;

        public HttpUserService(IHttpContextAccessor httpContextAccessor) => this.httpContextAccessor = httpContextAccessor;

        public User User
        {
            get
            {
                var user = this.user;

                if (user == null)
                {
                    var nameIdentifier = this.httpContextAccessor?.HttpContext.User.Claims
                        .FirstOrDefault(v => v.Type == ClaimTypes.NameIdentifier)
                        ?.Value;

                    if (long.TryParse(nameIdentifier, out var userId))
                    {
                        user = (User)this.transaction.Instantiate(userId);
                    }
                }

                return user;
            }

            set => this.user = value;
        }

        public virtual void OnInit(ITransaction transaction) => this.transaction = transaction;
    }
}
