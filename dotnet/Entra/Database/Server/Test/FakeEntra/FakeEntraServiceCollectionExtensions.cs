// <copyright file="FakeEntraServiceCollectionExtensions.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Hosting.Server.Features;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Identity.Web;

    public static class FakeEntraServiceCollectionExtensions
    {
        // Routes every request the plug-in's handlers and Microsoft.Identity.Web make to Microsoft's
        // host to the fake on this server, through two public seams: the handlers' backchannel, and the
        // HttpClient that Microsoft.Identity.Web's issuer validator fetches the tenant's metadata with.
        // The addresses stay Microsoft's and https, so every check on them passes; only the connection
        // goes elsewhere. Registered after AddAllorsEntra, and with Configure rather than PostConfigure:
        // the framework builds the backchannel from the handler in its own post-configuration.
        public static IServiceCollection AddFakeEntra(this IServiceCollection services)
        {
            services.TryAddSingleton<FakeEntra>();

            services.AddOptions<OpenIdConnectOptions>(EntraDefaults.OpenIdConnectScheme)
                .Configure<IServiceProvider>((options, provider) => options.BackchannelHttpHandler = new FakeEntraRewriteHandler(provider));
            services.AddOptions<JwtBearerOptions>(EntraDefaults.BearerScheme)
                .Configure<IServiceProvider>((options, provider) => options.BackchannelHttpHandler = new FakeEntraRewriteHandler(provider));

            services.AddHttpClient(FakeEntra.HttpClientName).ConfigurePrimaryHttpMessageHandler(provider => new FakeEntraRewriteHandler(provider));
            services.Configure<AadIssuerValidatorOptions>(options => options.HttpClientName = FakeEntra.HttpClientName);

            return services;
        }

        // Sends a request for Microsoft's host to the fake on this server, and every other request
        // where it was going.
        private sealed class FakeEntraRewriteHandler : DelegatingHandler
        {
            private readonly IServiceProvider provider;
            private string origin;

            public FakeEntraRewriteHandler(IServiceProvider provider) : base(new HttpClientHandler()) => this.provider = provider;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (request.RequestUri != null && string.Equals(request.RequestUri.Host, FakeEntra.MicrosoftHost, StringComparison.OrdinalIgnoreCase))
                {
                    request.RequestUri = new Uri(this.Origin + FakeEntra.PathPrefix + request.RequestUri.PathAndQuery);
                }

                return base.SendAsync(request, cancellationToken);
            }

            // Where this server listens, known once it has started; the loopback address of a wildcard.
            private string Origin => this.origin ??= this.ResolveOrigin();

            private string ResolveOrigin()
            {
                var addresses = this.provider.GetService<IServer>()?.Features.Get<IServerAddressesFeature>()?.Addresses;
                var address = addresses?.FirstOrDefault(v => v.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) ?? addresses?.FirstOrDefault() ?? "http://localhost:5000";
                return address
                    .Replace("://+", "://localhost", StringComparison.Ordinal)
                    .Replace("://*", "://localhost", StringComparison.Ordinal)
                    .Replace("://0.0.0.0", "://localhost", StringComparison.Ordinal)
                    .Replace("://[::]", "://localhost", StringComparison.Ordinal)
                    .TrimEnd('/');
            }
        }
    }
}
