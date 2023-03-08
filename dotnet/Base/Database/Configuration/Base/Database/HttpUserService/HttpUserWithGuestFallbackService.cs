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

    public class HttpUserWithGuestFallbackService : IUserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpUserWithGuestFallbackService(IHttpContextAccessor httpContextAccessor) => this.httpContextAccessor = httpContextAccessor;

        public User User { get; set; }

        public virtual void OnInit(ITransaction transaction)
        {
            var nameIdentifier = this.httpContextAccessor?.HttpContext.User.Claims
                .FirstOrDefault(v => v.Type == ClaimTypes.NameIdentifier)
                ?.Value;

            if (long.TryParse(nameIdentifier, out var userId))
            {
                this.User = (User)transaction.Instantiate(userId);
            }
            else
            {
                this.User = new AutomatedAgents(transaction).Guest;
            }
        }
    }
}
