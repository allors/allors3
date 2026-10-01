// <copyright file="AllorsServerApplicationBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.Linq;
    using System.Net;
    using Allors.Services;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public static class AllorsServerApplicationBuilderExtensions
    {
        // What Core decides, after the application's UseRouting(), UseAuthentication() and
        // UseAuthorization(): antiforgery for cookie sign-ins, the current user, and the Allors API.
        // The API controllers carry [Authorize], so a missing UseAuthorization() fails the first API
        // request instead of leaving the API open.
        public static void UseAllorsServer(this IApplicationBuilder app)
        {
            var userResolvers = app.ApplicationServices.GetServices<IUserResolver>().ToArray();
            if (userResolvers.Length == 0)
            {
                throw new InvalidOperationException(
                    $"No {nameof(IUserResolver)} is registered, so the Allors API cannot tell which user a request is for. " +
                    "Select one authentication plug-in, for example with services.AddAllorsIdentity(...), before app.UseAllorsServer().");
            }

            if (userResolvers.Length > 1)
            {
                throw new InvalidOperationException(
                    $"More than one {nameof(IUserResolver)} is registered: {string.Join(", ", userResolvers.Select(v => v.GetType().Name))}. " +
                    "Select exactly one authentication plug-in.");
            }

            var databaseService = app.ApplicationServices.GetRequiredService<IDatabaseService>();
            if (databaseService.Database == null)
            {
                throw new InvalidOperationException(
                    $"The Allors database is not set. Build it and assign {nameof(IDatabaseService)}.{nameof(IDatabaseService.Database)} before app.UseAllorsServer().");
            }

            var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();

            app.UseMiddleware<AllorsAntiforgeryMiddleware>(!environment.IsDevelopment());
            app.UseMiddleware<ClaimsPrincipalServiceMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "allors/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllers();
            });
        }

        // Building block: trusts X-Forwarded-For and X-Forwarded-Proto from loopback, and from the
        // proxies and networks in ForwardedHeaders:KnownProxies and ForwardedHeaders:KnownNetworks.
        public static IApplicationBuilder UseAllorsForwardedHeaders(this IApplicationBuilder app, IConfiguration configuration) =>
            app.UseForwardedHeaders(CreateForwardedHeadersOptions(configuration));

        // Building block: baseline security headers, with the content security policy from
        // Security:ContentSecurityPolicy.
        public static IApplicationBuilder UseAllorsSecurityHeaders(this IApplicationBuilder app) =>
            app.UseMiddleware<SecurityHeadersMiddleware>();

        private static ForwardedHeadersOptions CreateForwardedHeadersOptions(IConfiguration configuration)
        {
            // Trust defaults to loopback only (a same-host reverse proxy); extend via configuration.
            var options = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
            };

            foreach (var section in configuration.GetSection("ForwardedHeaders:KnownProxies").GetChildren())
            {
                if (!IPAddress.TryParse(section.Value, out var address))
                {
                    throw new InvalidOperationException(
                        $"ForwardedHeaders:KnownProxies contains '{section.Value}', which is not a valid IP address. Use e.g. \"10.0.0.5\".");
                }

                options.KnownProxies.Add(address);
            }

            foreach (var section in configuration.GetSection("ForwardedHeaders:KnownNetworks").GetChildren())
            {
                if (!System.Net.IPNetwork.TryParse(section.Value, out var network))
                {
                    throw new InvalidOperationException(
                        $"ForwardedHeaders:KnownNetworks contains '{section.Value}', which is not a valid CIDR network. Use e.g. \"10.0.0.0/8\".");
                }

                options.KnownIPNetworks.Add(network);
            }

            return options;
        }
    }
}
