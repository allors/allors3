// <copyright file="PolicyService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System.Threading.Tasks;
    using System;
    using Microsoft.AspNetCore.Http;

    public class ClaimsPrincipalServiceMiddleware
    {
        private readonly RequestDelegate next;

        public ClaimsPrincipalServiceMiddleware(RequestDelegate next)
        {
            this.next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context, IClaimsPrincipalService service)
        {
            service.User = context.User;
            await next(context);
        }
    }
}
