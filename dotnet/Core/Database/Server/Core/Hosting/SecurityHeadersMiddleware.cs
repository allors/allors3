// <copyright file="SecurityHeadersMiddleware.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;

    public class SecurityHeadersMiddleware
    {
        public const string DefaultContentSecurityPolicy = "frame-ancestors 'none'";

        private readonly RequestDelegate next;
        private readonly string contentSecurityPolicy;

        public SecurityHeadersMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            this.next = next;
            this.contentSecurityPolicy = configuration["Security:ContentSecurityPolicy"] ?? DefaultContentSecurityPolicy;
        }

        public Task InvokeAsync(HttpContext context)
        {
            // OnStarting so the headers survive downstream Response.Clear() (e.g. the exception handler).
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers.XContentTypeOptions = "nosniff";
                headers["Referrer-Policy"] = "no-referrer";
                headers["Permissions-Policy"] = "camera=(), geolocation=(), microphone=()";
                headers.ContentSecurityPolicy = this.contentSecurityPolicy;
                return Task.CompletedTask;
            });

            return this.next(context);
        }
    }
}
