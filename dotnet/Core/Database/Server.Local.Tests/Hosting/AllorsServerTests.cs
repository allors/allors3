// <copyright file="AllorsServerTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Allors.Server;
    using Allors.Services;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Authorization.Infrastructure;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection.KeyManagement;
    using Microsoft.AspNetCore.DataProtection.Repositories;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.RateLimiting;
    using Microsoft.AspNetCore.Routing;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Xunit;
    using MemoryConfiguration = Allors.Database.Adapters.Memory.Configuration;
    using MemoryDatabase = Allors.Database.Adapters.Memory.Database;
    using ObjectFactory = Allors.Database.ObjectFactory;
    using User = Allors.Database.Domain.User;

    // AddAllorsServer registers what Core decides: the services behind the Allors API. Everything an
    // application may want around it is a building block it switches on itself.
    public class AllorsServerTests
    {
        [Fact]
        public void AddAllorsServerSetsNoFallbackPolicy()
        {
            using var provider = Services().BuildServiceProvider();

            Assert.Null(provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value.FallbackPolicy);
        }

        [Fact]
        public void AddAllorsDefaultDenyRequiresAnAuthenticatedUser()
        {
            var services = Services();
            services.AddAllorsDefaultDeny();
            using var provider = services.BuildServiceProvider();

            var fallbackPolicy = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value.FallbackPolicy;

            Assert.NotNull(fallbackPolicy);
            Assert.Contains(fallbackPolicy.Requirements, v => v is DenyAnonymousAuthorizationRequirement);
        }

        [Fact]
        public void AddAllorsServerAddsNoRateLimiter()
        {
            using var provider = Services().BuildServiceProvider();

            Assert.Null(provider.GetRequiredService<IOptions<RateLimiterOptions>>().Value.GlobalLimiter);
        }

        [Fact]
        public void AddAllorsRateLimitingLimitsTheGivenPaths()
        {
            var configuration = Configuration(new Dictionary<string, string>
            {
                ["Security:AuthenticationRateLimit:PermitLimit"] = "1",
            });

            var services = Services();
            services.AddAllorsRateLimiting(configuration, "/sign-in");
            using var provider = services.BuildServiceProvider();

            var limiter = provider.GetRequiredService<IOptions<RateLimiterOptions>>().Value.GlobalLimiter;

            Assert.NotNull(limiter);
            Assert.True(limiter.AttemptAcquire(Context("/sign-in")).IsAcquired);
            Assert.False(limiter.AttemptAcquire(Context("/sign-in")).IsAcquired);
            Assert.True(limiter.AttemptAcquire(Context("/allors/pull")).IsAcquired);
            Assert.True(limiter.AttemptAcquire(Context("/allors/pull")).IsAcquired);
        }

        [Fact]
        public void AddAllorsServerPersistsNoDataProtectionKeys()
        {
            using var provider = Services().BuildServiceProvider();

            Assert.Null(provider.GetRequiredService<IOptions<KeyManagementOptions>>().Value.XmlRepository);
        }

        [Fact]
        public void AddAllorsDataProtectionPersistsKeysToTheConfiguredDirectory()
        {
            var keysDirectory = Path.Combine(Path.GetTempPath(), "allors-dataprotection-" + Guid.NewGuid());
            var configuration = Configuration(new Dictionary<string, string>
            {
                ["DataProtection:KeysDirectory"] = keysDirectory,
            });

            var services = Services();
            services.AddAllorsDataProtection(configuration, new StubWebHostEnvironment());
            using var provider = services.BuildServiceProvider();

            var repository = Assert.IsType<FileSystemXmlRepository>(provider.GetRequiredService<IOptions<KeyManagementOptions>>().Value.XmlRepository);
            Assert.Equal(new DirectoryInfo(keysDirectory).FullName, repository.Directory.FullName);
        }

        [Fact]
        public void UseAllorsServerWithoutAUserResolverFails()
        {
            using var provider = Services().BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() => new ApplicationBuilder(provider).UseAllorsServer());

            Assert.Contains(nameof(IUserResolver), exception.Message);
            Assert.Contains("AddAllorsIdentity", exception.Message);
        }

        [Fact]
        public void UseAllorsServerWithTwoUserResolversFails()
        {
            var services = Services();
            services.AddSingleton<IUserResolver, FirstUserResolver>();
            services.AddSingleton<IUserResolver, SecondUserResolver>();
            using var provider = services.BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() => new ApplicationBuilder(provider).UseAllorsServer());

            Assert.Contains(nameof(FirstUserResolver), exception.Message);
            Assert.Contains(nameof(SecondUserResolver), exception.Message);
        }

        [Fact]
        public void UseAllorsServerWithoutADatabaseFails()
        {
            var services = Services();
            services.AddSingleton<IUserResolver, FirstUserResolver>();
            using var provider = services.BuildServiceProvider();

            var exception = Assert.Throws<InvalidOperationException>(() => new ApplicationBuilder(provider).UseAllorsServer());

            Assert.Contains(nameof(IDatabaseService), exception.Message);
        }

        // Core maps the Allors API. An application maps its own endpoints, such as Razor Pages, in the
        // endpoints callback of UseAllorsServer, so a server without Razor Pages needs none of its
        // services.
        [Fact]
        public void UseAllorsServerNeedsNoRazorPages()
        {
            using var provider = ProviderForUseAllorsServer();
            var app = new ApplicationBuilder(provider);
            app.UseRouting();

            app.UseAllorsServer();
        }

        [Fact]
        public void UseAllorsServerMapsTheApplicationsEndpoints()
        {
            using var provider = ProviderForUseAllorsServer();
            var app = new ApplicationBuilder(provider);
            app.UseRouting();

            IEndpointRouteBuilder mapped = null;
            app.UseAllorsServer(endpoints =>
            {
                endpoints.MapGet("/sign-in", _ => Task.CompletedTask);
                mapped = endpoints;
            });

            Assert.Contains(mapped.DataSources.SelectMany(v => v.Endpoints).OfType<RouteEndpoint>(), v => v.RoutePattern.RawText == "/sign-in");
        }

        // A request with an invalid model is logged with its errors, not only with the title of the
        // problem details.
        [Fact]
        public void InvalidModelStateIsLoggedWithItsErrors()
        {
            var loggerProvider = new RecordingLoggerProvider();
            var services = Services();
            services.AddLogging(builder => builder.AddProvider(loggerProvider));
            using var provider = services.BuildServiceProvider();

            var actionContext = new ActionContext(new DefaultHttpContext { RequestServices = provider }, new RouteData(), new ActionDescriptor());
            actionContext.ModelState.AddModelError("name", "The name field is required.");

            provider.GetRequiredService<IOptions<ApiBehaviorOptions>>().Value.InvalidModelStateResponseFactory(actionContext);

            Assert.Contains(loggerProvider.Messages, v => v.Contains("The name field is required.", StringComparison.Ordinal));
        }

        private static ServiceCollection Services()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAllorsServer(Configuration(), new StubWebHostEnvironment(), new AllorsServerOptions
            {
                ApplicationName = "Allors.Tests",
            });

            return services;
        }

        // What UseAllorsServer requires: routing (UseRouting comes from the application), one user
        // resolver, an environment, a built database, and the diagnostic listener the web host
        // registers, which the controller endpoints need.
        private static ServiceProvider ProviderForUseAllorsServer()
        {
            var services = Services();
            services.AddSingleton<IWebHostEnvironment>(new StubWebHostEnvironment());
            services.AddSingleton<IUserResolver, FirstUserResolver>();
            services.AddSingleton(new DiagnosticListener("Allors.Tests"));
            services.AddSingleton<DiagnosticSource>(provider => provider.GetRequiredService<DiagnosticListener>());

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<IDatabaseService>().Database = NewDatabase();
            return provider;
        }

        private static MemoryDatabase NewDatabase()
        {
            var metaPopulation = new MetaBuilder().Build();
            var database = new MemoryDatabase(
                new DefaultDatabaseServices(new Engine(Rules.Create(metaPopulation))),
                new MemoryConfiguration
                {
                    ObjectFactory = new ObjectFactory(metaPopulation, typeof(User)),
                });

            database.Init();
            return database;
        }

        private static IConfiguration Configuration(IDictionary<string, string> configurationValues = null) =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(configurationValues ?? new Dictionary<string, string>())
                .Build();

        private static HttpContext Context(string path)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;
            context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.7");
            return context;
        }

        private sealed class RecordingLoggerProvider : ILoggerProvider
        {
            public ConcurrentQueue<string> Messages { get; } = new();

            public ILogger CreateLogger(string categoryName) => new RecordingLogger(this.Messages);

            public void Dispose()
            {
            }

            private sealed class RecordingLogger : ILogger
            {
                private readonly ConcurrentQueue<string> messages;

                public RecordingLogger(ConcurrentQueue<string> messages) => this.messages = messages;

                public IDisposable BeginScope<TState>(TState state) => null;

                public bool IsEnabled(LogLevel logLevel) => true;

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
                    this.messages.Enqueue(formatter(state, exception));
            }
        }

        private sealed class FirstUserResolver : IUserResolver
        {
            public User Resolve(ClaimsPrincipal principal, ITransaction transaction) => null;
        }

        private sealed class SecondUserResolver : IUserResolver
        {
            public User Resolve(ClaimsPrincipal principal, ITransaction transaction) => null;
        }

        private sealed class StubWebHostEnvironment : IWebHostEnvironment
        {
            public string WebRootPath { get; set; }

            public IFileProvider WebRootFileProvider { get; set; }

            public string ApplicationName { get; set; } = "Allors.Tests";

            public IFileProvider ContentRootFileProvider { get; set; }

            public string ContentRootPath { get; set; } = Path.GetTempPath();

            public string EnvironmentName { get; set; } = "Development";
        }
    }
}
