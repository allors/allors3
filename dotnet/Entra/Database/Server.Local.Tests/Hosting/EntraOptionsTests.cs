// <copyright file="EntraOptionsTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using Allors.Security;
    using Allors.Server;
    using Allors.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Options;
    using Xunit;

    // What AddAllorsEntra registers: Microsoft.Identity.Web with the plug-in's names and defaults, the
    // names Core needs, and the plug-in's users.
    public class EntraOptionsTests
    {
        private const string TenantId = "cd3598ab-ef18-4774-bfce-c1b3cebe5a42";
        private const string ClientId = "9c7d6e5f-4a3b-4c2d-8e1f-0a9b8c7d6e5f";

        // How Entra connects to Core: its session, its bearer scheme, the scheme Core challenges for a
        // browser without a session, and a lifetime of the session, as Entra cannot end it.
        [Fact]
        public void TheSchemesAreNamedToCore()
        {
            using var provider = Provider();

            var authentication = provider.GetRequiredService<IOptions<AllorsAuthenticationOptions>>().Value;

            Assert.Equal(EntraDefaults.SessionScheme, authentication.SessionScheme);
            Assert.Equal(EntraDefaults.BearerScheme, authentication.BearerScheme);
            Assert.Equal(EntraDefaults.OpenIdConnectScheme, authentication.ChallengeScheme);
            Assert.Equal(TimeSpan.FromHours(12), authentication.SessionLifetime);
        }

        [Fact]
        public void SessionLifetimeFollowsConfiguration()
        {
            using var provider = Provider(new Dictionary<string, string> { ["Entra:SessionLifetime"] = "02:00:00" });

            Assert.Equal(TimeSpan.FromHours(2), provider.GetRequiredService<IOptions<AllorsAuthenticationOptions>>().Value.SessionLifetime);
        }

        // Core's selecting scheme is the default: it sends a request with a bearer token to the bearer
        // scheme and every other request to the session.
        [Fact]
        public void TheSelectingSchemeIsTheDefault()
        {
            using var provider = Provider();

            Assert.Equal(AllorsAuthenticationDefaults.AuthenticationScheme, provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value.DefaultScheme);
        }

        // Microsoft.Identity.Web registers the session cookie; Core gives it the rules of the session.
        [Fact]
        public void TheSessionCookieIsCores()
        {
            using var provider = Provider();

            var cookie = provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(EntraDefaults.SessionScheme);

            Assert.Equal("Allors.Auth", cookie.Cookie.Name);
            Assert.Equal(TimeSpan.FromHours(8), cookie.ExpireTimeSpan);
        }

        // The authorization code flow with PKCE against the tenant's v2.0 authority, with the code in the
        // query of the redirect back, so that the correlation and nonce cookies can stay Lax.
        [Fact]
        public void SignInUsesTheCodeFlowAgainstTheTenant()
        {
            using var provider = Provider();

            var openIdConnect = OpenIdConnect(provider);

            Assert.Equal("code", openIdConnect.ResponseType);
            Assert.Equal("query", openIdConnect.ResponseMode);
            Assert.True(openIdConnect.UsePkce);
            Assert.Equal($"https://login.microsoftonline.com/{TenantId}/v2.0", openIdConnect.Authority);
            Assert.Equal(ClientId, openIdConnect.ClientId);
            Assert.Equal(EntraDefaults.SessionScheme, openIdConnect.SignInScheme);
            Assert.Equal(SameSiteMode.Lax, openIdConnect.CorrelationCookie.SameSite);
            Assert.Equal(SameSiteMode.Lax, openIdConnect.NonceCookie.SameSite);
        }

        // Development runs over plain http; everywhere else the cookies of the sign-in are Secure.
        [Theory]
        [InlineData("Development", CookieSecurePolicy.SameAsRequest)]
        [InlineData("Production", CookieSecurePolicy.Always)]
        public void TheCookiesOfTheSignInFollowTheEnvironment(string environment, CookieSecurePolicy securePolicy)
        {
            using var provider = Provider(environmentName: environment);

            var openIdConnect = OpenIdConnect(provider);

            Assert.Equal(securePolicy, openIdConnect.CorrelationCookie.SecurePolicy);
            Assert.Equal(securePolicy, openIdConnect.NonceCookie.SecurePolicy);
        }

        [Fact]
        public void TheSectionOverridesTheInstance()
        {
            using var provider = Provider(new Dictionary<string, string> { ["Entra:Instance"] = "https://login.microsoftonline.us/" });

            Assert.Equal($"https://login.microsoftonline.us/{TenantId}/v2.0", OpenIdConnect(provider).Authority);
        }

        // Core's API reads the user name from the identity's name, which an access token carries in
        // preferred_username; the plug-in's user step runs on both schemes.
        [Fact]
        public void TheBearerSchemeNamesThePersonAndAdmitsThePrincipal()
        {
            using var provider = Provider();

            var bearer = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(EntraDefaults.BearerScheme);

            Assert.Equal(EntraClaims.PreferredUserNameClaim, bearer.TokenValidationParameters.NameClaimType);
            Assert.NotNull(bearer.Events?.OnTokenValidated);
            Assert.NotNull(OpenIdConnect(provider).Events.OnTokenValidated);
            Assert.IsType<EntraUserResolver>(provider.GetRequiredService<IUserResolver>());
            Assert.NotNull(provider.GetRequiredService<EntraAdmission>());
        }

        // One tenant: the plug-in signs in the members and guests of the application's tenant, so it
        // refuses to start without a tenant id, or with one of the shared authorities.
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("common")]
        [InlineData("organizations")]
        [InlineData("consumers")]
        [InlineData("contoso.onmicrosoft.com")]
        public void StartUpRefusesAMissingOrSharedTenant(string tenantId)
        {
            var exception = Assert.Throws<InvalidOperationException>(() => Provider(new Dictionary<string, string> { ["Entra:TenantId"] = tenantId }));

            Assert.Contains("Entra:TenantId", exception.Message, StringComparison.Ordinal);
            Assert.Contains("GUID", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void StartUpRefusesAMissingClientId()
        {
            var exception = Assert.Throws<InvalidOperationException>(() => Provider(new Dictionary<string, string> { ["Entra:ClientId"] = "" }));

            Assert.Contains("Entra:ClientId", exception.Message, StringComparison.Ordinal);
        }

        // The code flow redeems its code with a client credential: a secret, or a certificate the way
        // Microsoft.Identity.Web takes one.
        [Fact]
        public void StartUpRefusesAMissingClientCredential()
        {
            var exception = Assert.Throws<InvalidOperationException>(() => Provider(new Dictionary<string, string> { ["Entra:ClientSecret"] = "" }));
            Assert.Contains("Entra:ClientSecret", exception.Message, StringComparison.Ordinal);

            using var provider = Provider(new Dictionary<string, string>
            {
                ["Entra:ClientSecret"] = "",
                ["Entra:ClientCredentials:0:SourceType"] = "StoreWithThumbprint",
                ["Entra:ClientCredentials:0:CertificateThumbprint"] = "ab",
            });
            Assert.NotNull(provider.GetRequiredService<IOptions<AllorsAuthenticationOptions>>().Value.SessionScheme);
        }

        private static OpenIdConnectOptions OpenIdConnect(IServiceProvider provider) =>
            provider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>().Get(EntraDefaults.OpenIdConnectScheme);

        // The services of a server that selects the Entra plug-in, configured for a tenant.
        private static ServiceProvider Provider(IDictionary<string, string> configurationValues = null, string environmentName = "Development")
        {
            var values = new Dictionary<string, string>
            {
                ["Entra:TenantId"] = TenantId,
                ["Entra:ClientId"] = ClientId,
                ["Entra:ClientSecret"] = "a-secret",
            };

            foreach (var (key, value) in configurationValues ?? new Dictionary<string, string>())
            {
                values[key] = value;
            }

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
            var environment = new StubWebHostEnvironment { EnvironmentName = environmentName };

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAllorsServer(configuration, environment, new AllorsServerOptions
            {
                ApplicationName = "Allors.Tests",
            });
            services.AddAllorsEntra(configuration, environment);

            // Registered last: MVC would otherwise look for an assembly with the application's name.
            services.AddSingleton<IWebHostEnvironment>(environment);

            return services.BuildServiceProvider();
        }

        private sealed class StubWebHostEnvironment : IWebHostEnvironment
        {
            public string WebRootPath { get; set; }

            public IFileProvider WebRootFileProvider { get; set; }

            public string ApplicationName { get; set; } = "Allors.Tests";

            public IFileProvider ContentRootFileProvider { get; set; }

            public string ContentRootPath { get; set; } = System.IO.Path.GetTempPath();

            public string EnvironmentName { get; set; } = "Development";
        }
    }
}
