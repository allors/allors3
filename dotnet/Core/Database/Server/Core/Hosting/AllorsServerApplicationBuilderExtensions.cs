// <copyright file="AllorsServerApplicationBuilderExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.Net;
    using Allors.Services;
    using Database.Adapters;
    using Database.Configuration;
    using Database.Configuration.Derivations.Default;
    using Database.Domain;
    using Database.Meta;
    using JSNLog;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpOverrides;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using ObjectFactory = Database.ObjectFactory;
    using User = Database.Domain.User;

    public static class AllorsServerApplicationBuilderExtensions
    {
        public static void UseAllorsServer(this IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            // Allors
            var metaPopulation = new MetaBuilder().Build();
            var engine = new Engine(Rules.Create(metaPopulation));
            var objectFactory = new ObjectFactory(metaPopulation, typeof(User));
            var databaseScope = new DefaultDatabaseServices(engine);
            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var databaseBuilder = new DatabaseBuilder(databaseScope, configuration, objectFactory, null, 60);
            var databaseService = app.ApplicationServices.GetRequiredService<IDatabaseService>();
            databaseService.Build = () => databaseBuilder.Build();
            databaseService.Database = databaseService.Build();

            app.UseForwardedHeaders(CreateForwardedHeadersOptions(configuration));
            app.UseMiddleware<SecurityHeadersMiddleware>();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseCors();

            var jsnlogConfiguration = new JsnlogConfiguration
            {
                corsAllowedOriginsRegex = configuration["Logging:JSNLog:CorsAllowedOriginsRegex"] ?? "^https?://localhost(:[0-9]+)?$",
                serverSideMessageFormat = env.IsDevelopment() ?
                                            "%requestId | %url | %message" :
                                            "%requestId | %url | %userHostAddress | %userAgent | %message",
            };

            app.UseJSNLog(new LoggingAdapter(loggerFactory), jsnlogConfiguration);

            // Serves the Identity UI's static web assets (/Identity/lib/*) and any app static files.
            app.UseStaticFiles();

            app.UseRouting();
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();

            app.ConfigureExceptionHandler(env);
            app.UseResponseCaching();

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
