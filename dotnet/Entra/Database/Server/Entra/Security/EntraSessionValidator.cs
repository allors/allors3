// <copyright file="EntraSessionValidator.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Security
{
    using System.Threading.Tasks;
    using Allors.Server;
    using Allors.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;

    internal sealed class EntraSessionValidator : IAllorsSessionValidator
    {
        private readonly IDatabaseService databaseService;
        private readonly IUserResolver userResolver;

        public EntraSessionValidator(IDatabaseService databaseService, IUserResolver userResolver)
        {
            this.databaseService = databaseService;
            this.userResolver = userResolver;
        }

        public async Task ValidateAsync(CookieValidatePrincipalContext context)
        {
            // Authentication runs before Core's request transaction. Check existence in a fresh
            // transaction, without admission: only a new sign-in or bearer request may create a user.
            using (var transaction = this.databaseService.Database.CreateTransaction())
            {
                if (this.userResolver.Resolve(context.Principal, transaction) != null)
                {
                    return;
                }
            }

            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(context.Scheme.Name);
        }
    }
}
