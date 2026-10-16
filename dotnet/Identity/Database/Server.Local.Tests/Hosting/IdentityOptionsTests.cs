// <copyright file="IdentityOptionsTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Allors.Server;
    using Allors.Database.Domain;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Options;
    using Xunit;

    public class IdentityOptionsTests
    {
        [Fact]
        public void LockoutFollowsBestPracticeDefaults()
        {
            var identityOptions = Resolve();

            Assert.True(identityOptions.Lockout.AllowedForNewUsers);
            Assert.Equal(10, identityOptions.Lockout.MaxFailedAccessAttempts);
            Assert.Equal(TimeSpan.FromMinutes(15), identityOptions.Lockout.DefaultLockoutTimeSpan);
        }

        [Fact]
        public void PasswordPolicyFavorsLengthOverComposition()
        {
            var identityOptions = Resolve();

            Assert.Equal(12, identityOptions.Password.RequiredLength);
            Assert.False(identityOptions.Password.RequireDigit);
            Assert.False(identityOptions.Password.RequireUppercase);
            Assert.False(identityOptions.Password.RequireLowercase);
            Assert.False(identityOptions.Password.RequireNonAlphanumeric);
            Assert.Equal(4, identityOptions.Password.RequiredUniqueChars);
        }

        [Fact]
        public void ConfigurationOverridesBind()
        {
            var identityOptions = Resolve(new Dictionary<string, string>
            {
                ["Identity:Lockout:MaxFailedAccessAttempts"] = "3",
                ["Identity:Password:RequiredLength"] = "20",
            });

            Assert.Equal(3, identityOptions.Lockout.MaxFailedAccessAttempts);
            Assert.Equal(20, identityOptions.Password.RequiredLength);
        }

        // The application cookie of ASP.NET Core Identity is the browser session of this plug-in. The
        // plug-in names it, and Core applies the rules of the session to it: the cookie of every
        // plug-in is hardened in one place.
        [Fact]
        public void TheApplicationCookieIsTheSession()
        {
            using var provider = Provider();

            Assert.Equal(IdentityConstants.ApplicationScheme, provider.GetRequiredService<IOptions<AllorsAuthenticationOptions>>().Value.SessionScheme);
            Assert.Equal("Allors.Auth", ApplicationCookie(provider).Cookie.Name);
        }

        // Identity registers its cookie after Core and sets the events of the cookie itself. Core
        // wraps them, so Identity keeps revalidating the security stamp of a signed-in user.
        [Fact]
        public void TheSessionKeepsTheSecurityStampValidatorOfIdentity()
        {
            using var provider = Provider();

            var validatePrincipal = ApplicationCookie(provider).Events.OnValidatePrincipal;

            Assert.Equal(typeof(SecurityStampValidator), validatePrincipal.Method.DeclaringType);
        }

        // The same holds the other way round: Identity replaces the events of the cookie after Core
        // registered, and Core's rule for the Allors API still applies.
        [Fact]
        public async Task TheSessionAnswersTheApiWith401UnderIdentity()
        {
            using var provider = Provider();
            var cookie = ApplicationCookie(provider);
            var context = new DefaultHttpContext { RequestServices = provider };
            context.Request.Path = "/allors/pull";
            var scheme = new AuthenticationScheme(IdentityConstants.ApplicationScheme, null, typeof(CookieAuthenticationHandler));

            await cookie.Events.RedirectToLogin(new RedirectContext<CookieAuthenticationOptions>(context, scheme, cookie, new AuthenticationProperties(), "/Identity/Account/Login"));

            Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
            Assert.False(context.Response.Headers.ContainsKey("Location"));
        }

        [Fact]
        public void SessionLifetimeFollowsConfiguration()
        {
            using var provider = Provider(new Dictionary<string, string>
            {
                ["Identity:Cookie:ExpireTimeSpan"] = "01:30:00",
            });

            Assert.Equal(TimeSpan.FromMinutes(90), ApplicationCookie(provider).ExpireTimeSpan);
        }

        [Fact]
        public void SessionLifetimeIsCoresDefaultWithoutConfiguration()
        {
            using var provider = Provider();

            Assert.Equal(TimeSpan.FromHours(8), ApplicationCookie(provider).ExpireTimeSpan);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ReadRouteRequiresAnAuthenticatedIdentitySession(bool authenticated)
        {
            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                EnvironmentName = "Development", ContentRootPath = System.IO.Path.GetTempPath(),
            });
            builder.Logging.ClearProviders();
            builder.Services.AddAllorsServer(builder.Configuration, builder.Environment, new AllorsServerOptions { ApplicationName = "Allors.ReadRouteTests" })
                .AddApplicationPart(typeof(Allors.Database.Protocol.Json.ReadController).Assembly);
            builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
            builder.Services.AddAllorsIdentity(builder.Configuration, builder.Environment);
            var meta = new Allors.Database.Meta.MetaBuilder().Build();
            var database = new Allors.Database.Adapters.Memory.Database(
                new Allors.Database.Configuration.DefaultDatabaseServices(new Allors.Database.Configuration.Derivations.Default.Engine(Allors.Database.Domain.Rules.Create(meta))),
                new Allors.Database.Adapters.Memory.Configuration { ObjectFactory = new Allors.Database.ObjectFactory(meta, typeof(Allors.Database.Domain.User)) });
            database.Init();
            new Setup(database, new Config { SetupSecurity = false }).Apply();
            using (var transaction = database.CreateTransaction())
            {
                new PersonBuilder(transaction).WithUserName("read-route-user").Build();
                transaction.Derive();
                transaction.Commit();
            }

            await using var app = builder.Build();
            app.Services.GetRequiredService<Allors.Services.IDatabaseService>().Database = database;
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAllorsServer();
            app.Urls.Add("http://127.0.0.1:0");
            await app.StartAsync();
            using var handler = new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = new CookieContainer() };
            var origin = new Uri(app.Urls.Single());
            using var client = new HttpClient(handler) { BaseAddress = origin };
            if (authenticated)
            {
                using var signIn = await client.PostAsync("/allors/TestAuthentication/SignIn",
                    new StringContent("{\"l\":\"read-route-user\"}", System.Text.Encoding.UTF8, "application/json"));
                Assert.Equal(HttpStatusCode.OK, signIn.StatusCode);
                using var ready = await client.GetAsync("/allors/UserInfo");
                Assert.Equal(HttpStatusCode.OK, ready.StatusCode);
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, "/allors/read") { Content = new StringContent("{}") };
            var xsrf = handler.CookieContainer.GetCookies(origin)["XSRF-TOKEN"]?.Value;
            if (xsrf != null)
            {
                request.Headers.Add("X-XSRF-TOKEN", xsrf);
            }

            using var response = await client.SendAsync(request);
            Assert.Equal(authenticated ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.Unauthorized, response.StatusCode);
        }

        private static CookieAuthenticationOptions ApplicationCookie(IServiceProvider provider) =>
            provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(IdentityConstants.ApplicationScheme);

        // The services of a server that selects the Identity plug-in, with the environment the web
        // host registers and the Identity UI asks for.
        private static ServiceProvider Provider(IDictionary<string, string> configurationValues = null)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationValues ?? new Dictionary<string, string>())
                .Build();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAllorsServer(configuration, new StubWebHostEnvironment(), new AllorsServerOptions
            {
                ApplicationName = "Allors.Tests",
            });
            services.AddAllorsIdentity(configuration, new StubWebHostEnvironment());

            // Registered last: MVC would otherwise look for an assembly with the application's name.
            services.AddSingleton<IWebHostEnvironment>(new StubWebHostEnvironment());

            return services.BuildServiceProvider();
        }

        private static IdentityOptions Resolve(IDictionary<string, string> configurationValues = null)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationValues ?? new Dictionary<string, string>())
                .Build();

            var services = new ServiceCollection();
            services.AddAllorsServer(configuration, new StubWebHostEnvironment(), new AllorsServerOptions
            {
                ApplicationName = "Allors.Tests",
            });
            services.AddAllorsIdentity(configuration, new StubWebHostEnvironment());

            using var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<IOptions<IdentityOptions>>().Value;
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
