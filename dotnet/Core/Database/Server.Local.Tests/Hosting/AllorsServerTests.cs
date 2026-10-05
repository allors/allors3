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
    using System.Text.Encodings.Web;
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Allors.Server;
    using Allors.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
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

        // An authentication plug-in names the cookie scheme of its browser session once. Core then
        // applies the rules of that session to the scheme, wherever the plug-in registers it.
        [Fact]
        public void SessionCookieGetsCoresDefaults()
        {
            using var provider = SessionProvider();

            var options = SessionOptions(provider);

            Assert.Equal("Allors.Auth", options.Cookie.Name);
            Assert.True(options.Cookie.HttpOnly);
            Assert.Equal(SameSiteMode.Lax, options.Cookie.SameSite);
            Assert.Equal(CookieSecurePolicy.SameAsRequest, options.Cookie.SecurePolicy);
            Assert.True(options.SlidingExpiration);
            Assert.Equal(TimeSpan.FromHours(8), options.ExpireTimeSpan);
        }

        [Fact]
        public void SessionCookieIsSecureAndBoundToTheHostOutsideDevelopment()
        {
            using var provider = SessionProvider(environmentName: "Production");

            var options = SessionOptions(provider);

            Assert.Equal("__Host-Allors.Auth", options.Cookie.Name);
            Assert.Equal(CookieSecurePolicy.Always, options.Cookie.SecurePolicy);
        }

        // Core's values are defaults: what the application configures after them wins.
        [Fact]
        public void ApplicationOverridesASessionCookieDefault()
        {
            using var provider = SessionProvider(configure: services =>
                services.Configure<CookieAuthenticationOptions>(SessionScheme, v => v.ExpireTimeSpan = TimeSpan.FromHours(2)));

            Assert.Equal(TimeSpan.FromHours(2), SessionOptions(provider).ExpireTimeSpan);
        }

        // A caller of the Allors API gets a status code, never a redirect to a sign-in page. Other
        // paths keep the answer the cookie had.
        [Fact]
        public async Task SessionChallengeAnswersTheApiWith401()
        {
            using var provider = SessionProvider();
            var options = SessionOptions(provider);

            var api = Redirect(provider, options, "/allors/pull");
            await options.Events.RedirectToLogin(api);
            var page = Redirect(provider, options, "/account");
            await options.Events.RedirectToLogin(page);

            Assert.Equal(StatusCodes.Status401Unauthorized, api.Response.StatusCode);
            Assert.False(api.Response.Headers.ContainsKey("Location"));
            Assert.Equal(StatusCodes.Status302Found, page.Response.StatusCode);
            Assert.Equal("/sign-in", page.Response.Headers.Location);
        }

        [Fact]
        public async Task SessionForbidAnswersTheApiWith403()
        {
            using var provider = SessionProvider();
            var options = SessionOptions(provider);

            var api = Redirect(provider, options, "/allors/pull");
            await options.Events.RedirectToAccessDenied(api);
            var page = Redirect(provider, options, "/account");
            await options.Events.RedirectToAccessDenied(page);

            Assert.Equal(StatusCodes.Status403Forbidden, api.Response.StatusCode);
            Assert.False(api.Response.Headers.ContainsKey("Location"));
            Assert.Equal(StatusCodes.Status302Found, page.Response.StatusCode);
        }

        // A plug-in registers its cookie after Core and may replace the events of the cookie, as
        // ASP.NET Core Identity does for its security stamp validator. Core wraps what is there, so
        // the plug-in's events stay and the rule for the API still holds.
        [Fact]
        public async Task SessionRulesKeepTheEventsOfThePlugIn()
        {
            var validated = 0;
            var redirected = 0;
            using var provider = SessionProvider(configureCookie: v => v.Events = new CookieAuthenticationEvents
            {
                OnValidatePrincipal = _ =>
                {
                    validated++;
                    return Task.CompletedTask;
                },
                OnRedirectToLogin = _ =>
                {
                    redirected++;
                    return Task.CompletedTask;
                },
            });
            var options = SessionOptions(provider);

            await options.Events.ValidatePrincipal(null);
            var api = Redirect(provider, options, "/allors/pull");
            await options.Events.RedirectToLogin(api);
            await options.Events.RedirectToLogin(Redirect(provider, options, "/account"));

            Assert.Equal(1, validated);
            Assert.Equal(StatusCodes.Status401Unauthorized, api.Response.StatusCode);
            Assert.Equal(1, redirected);
        }

        // An antiforgery token is bound to the user it was issued for. Signing in or out drops the
        // token cookie, and the next safe request to the API issues one for the new state.
        [Fact]
        public async Task SessionSignInAndSignOutDropTheAntiforgeryCookie()
        {
            using var provider = SessionProvider();
            var options = SessionOptions(provider);

            var signedIn = new DefaultHttpContext { RequestServices = provider };
            await options.Events.SignedIn(new CookieSignedInContext(signedIn, CookieScheme(), new ClaimsPrincipal(new ClaimsIdentity()), new AuthenticationProperties(), options));
            var signingOut = new DefaultHttpContext { RequestServices = provider };
            await options.Events.SigningOut(new CookieSigningOutContext(signingOut, CookieScheme(), options, new AuthenticationProperties(), new CookieOptions()));

            Assert.Contains(signedIn.Response.Headers.SetCookie, v => v.StartsWith("XSRF-TOKEN=;", StringComparison.Ordinal));
            Assert.Contains(signingOut.Response.Headers.SetCookie, v => v.StartsWith("XSRF-TOKEN=;", StringComparison.Ordinal));
        }

        [Fact]
        public void ACookieThatIsNotTheSessionIsLeftAlone()
        {
            using var provider = SessionProvider(configure: services => services.AddAuthentication().AddCookie("Tests.Other"));

            var other = provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get("Tests.Other");

            Assert.NotEqual("Allors.Auth", other.Cookie.Name);
            Assert.Equal(TimeSpan.FromDays(14), other.ExpireTimeSpan);
        }

        // Core applies its rules through the events of the cookie. A cookie that takes its events
        // from a type would skip them without a word, so Core refuses it.
        [Fact]
        public void SessionCookieWithAnEventsTypeIsRefused()
        {
            using var provider = SessionProvider(configureCookie: v => v.EventsType = typeof(CookieAuthenticationEvents));

            var exception = Assert.Throws<InvalidOperationException>(() => SessionOptions(provider));

            Assert.Contains(SessionScheme, exception.Message);
            Assert.Contains(nameof(CookieAuthenticationOptions.EventsType), exception.Message);
        }

        [Theory]
        [InlineData(nameof(CookieAuthenticationOptions.Events))]
        [InlineData(nameof(CookieAuthenticationOptions.EventsType))]
        [InlineData(nameof(CookieAuthenticationEvents.OnRedirectToLogin))]
        [InlineData(nameof(CookieAuthenticationEvents.OnRedirectToAccessDenied))]
        [InlineData(nameof(CookieAuthenticationEvents.OnSignedIn))]
        [InlineData(nameof(CookieAuthenticationEvents.OnSigningOut))]
        [InlineData(nameof(CookieAuthenticationEvents.OnSigningIn))]
        [InlineData(nameof(CookieAuthenticationEvents.OnValidatePrincipal))]
        public void SessionRulesCannotBeReplacedAfterPostConfiguration(string member)
        {
            using var provider = SessionProvider(configure: services =>
            {
                services.Configure<AllorsAuthenticationOptions>(v => v.SessionLifetime = TimeSpan.FromHours(12));
                services.PostConfigure<CookieAuthenticationOptions>(SessionScheme, v => ReplaceSessionEvent(v, member));
            });

            var exception = Assert.Throws<OptionsValidationException>(() => SessionOptions(provider));

            Assert.Contains(SessionScheme, exception.Message);
            Assert.Contains(member, exception.Message);
            Assert.Contains("Configure", exception.Message);
        }

        [Theory]
        [InlineData(nameof(CookieAuthenticationOptions.Events))]
        [InlineData(nameof(CookieAuthenticationOptions.EventsType))]
        [InlineData(nameof(CookieAuthenticationEvents.OnSigningIn))]
        public void UseAllorsServerRefusesReplacedSessionRulesAtStartup(string member)
        {
            using var provider = ProviderForUseAllorsServer(services =>
            {
                services.AddAuthentication().AddCookie(SessionScheme);
                services.Configure<AllorsAuthenticationOptions>(v =>
                {
                    v.SessionScheme = SessionScheme;
                    v.SessionLifetime = TimeSpan.FromHours(12);
                });
                services.PostConfigure<CookieAuthenticationOptions>(SessionScheme, v => ReplaceSessionEvent(v, member));
            });
            var app = new ApplicationBuilder(provider);
            app.UseRouting();

            var exception = Assert.Throws<OptionsValidationException>(() => app.UseAllorsServer());

            Assert.Contains(SessionScheme, exception.Message);
            Assert.Contains(member, exception.Message);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SessionRulesAllowLaterCustomizationOfUnprotectedEvents(bool lifetime)
        {
            var callbacks = 0;
            using var provider = SessionProvider(configure: services =>
            {
                if (lifetime)
                {
                    services.Configure<AllorsAuthenticationOptions>(v => v.SessionLifetime = TimeSpan.FromHours(12));
                }

                services.PostConfigure<CookieAuthenticationOptions>(SessionScheme, v =>
                {
                    v.Events.OnRedirectToLogout = _ =>
                    {
                        callbacks++;
                        return Task.CompletedTask;
                    };
                    if (!lifetime)
                    {
                        v.Events.OnSigningIn = _ =>
                        {
                            callbacks++;
                            return Task.CompletedTask;
                        };
                        v.Events.OnValidatePrincipal = _ =>
                        {
                            callbacks++;
                            return Task.CompletedTask;
                        };
                    }
                });
            });
            var options = SessionOptions(provider);

            await options.Events.RedirectToLogout(Redirect(provider, options, "/account"));
            if (!lifetime)
            {
                await options.Events.SigningIn(null);
                await options.Events.ValidatePrincipal(null);
            }

            var api = Redirect(provider, options, "/allors/pull");
            await options.Events.RedirectToLogin(api);

            Assert.Equal(lifetime ? 1 : 3, callbacks);
            Assert.Equal(StatusCodes.Status401Unauthorized, api.Response.StatusCode);
        }

        [Theory]
        [InlineData(nameof(CookieAuthenticationOptions.Events))]
        [InlineData(nameof(CookieAuthenticationOptions.EventsType))]
        public void OtherCookiesAllowEventsReplacedAfterPostConfiguration(string member)
        {
            var events = new CookieAuthenticationEvents();
            using var provider = SessionProvider(configure: services =>
            {
                services.AddAuthentication().AddCookie("Tests.Other");
                services.PostConfigure<CookieAuthenticationOptions>("Tests.Other", v =>
                {
                    v.Events = events;
                    if (member == nameof(CookieAuthenticationOptions.EventsType))
                    {
                        v.EventsType = typeof(CookieAuthenticationEvents);
                    }
                });
            });

            var other = provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get("Tests.Other");

            Assert.Same(events, other.Events);
            Assert.Equal(member == nameof(CookieAuthenticationOptions.EventsType) ? typeof(CookieAuthenticationEvents) : null, other.EventsType);
        }

        [Theory]
        [InlineData(typeof(OverriddenLoginEvents), nameof(CookieAuthenticationEvents.RedirectToLogin))]
        [InlineData(typeof(OverriddenAccessDeniedEvents), nameof(CookieAuthenticationEvents.RedirectToAccessDenied))]
        [InlineData(typeof(OverriddenSignedInEvents), nameof(CookieAuthenticationEvents.SignedIn))]
        [InlineData(typeof(OverriddenSigningOutEvents), nameof(CookieAuthenticationEvents.SigningOut))]
        [InlineData(typeof(OverriddenSigningInEvents), nameof(CookieAuthenticationEvents.SigningIn))]
        [InlineData(typeof(OverriddenValidatePrincipalEvents), nameof(CookieAuthenticationEvents.ValidatePrincipal))]
        public void SessionRulesCannotBeSkippedByOverriddenEvents(Type eventsType, string member)
        {
            using var provider = SessionProvider(
                configureCookie: v => v.Events = (CookieAuthenticationEvents)Activator.CreateInstance(eventsType, true),
                configure: services => services.Configure<AllorsAuthenticationOptions>(v => v.SessionLifetime = TimeSpan.FromHours(12)));

            var exception = Assert.Throws<OptionsValidationException>(() => SessionOptions(provider));

            Assert.Contains(SessionScheme, exception.Message);
            Assert.Contains(member, exception.Message);
            Assert.Contains("Configure", exception.Message);
        }

        [Fact]
        public void UseAllorsServerRefusesOverriddenSessionRulesAtStartup()
        {
            using var provider = ProviderForUseAllorsServer(services =>
            {
                services.AddAuthentication().AddCookie(SessionScheme, v => v.Events = new OverriddenValidatePrincipalEvents());
                services.Configure<AllorsAuthenticationOptions>(v =>
                {
                    v.SessionScheme = SessionScheme;
                    v.SessionLifetime = TimeSpan.FromHours(12);
                });
            });
            var app = new ApplicationBuilder(provider);
            app.UseRouting();

            var exception = Assert.Throws<OptionsValidationException>(() => app.UseAllorsServer());

            Assert.Contains(SessionScheme, exception.Message);
            Assert.Contains(nameof(CookieAuthenticationEvents.ValidatePrincipal), exception.Message);
        }

        [Theory]
        [InlineData(typeof(PlainCookieEvents), true)]
        [InlineData(typeof(OverriddenLogoutEvents), true)]
        [InlineData(typeof(HiddenLoginEvents), true)]
        [InlineData(typeof(OverriddenSigningInEvents), false)]
        [InlineData(typeof(OverriddenValidatePrincipalEvents), false)]
        public async Task SessionRulesKeepHarmlessEventSubclasses(Type eventsType, bool lifetime)
        {
            var events = (CookieAuthenticationEvents)Activator.CreateInstance(eventsType, true);
            using var provider = SessionProvider(configureCookie: v => v.Events = events, configure: services =>
            {
                if (lifetime)
                {
                    services.Configure<AllorsAuthenticationOptions>(v => v.SessionLifetime = TimeSpan.FromHours(12));
                }
            });
            var options = SessionOptions(provider);

            var api = Redirect(provider, options, "/allors/pull");
            await options.Events.RedirectToLogin(api);

            Assert.Same(events, options.Events);
            Assert.Equal(StatusCodes.Status401Unauthorized, api.Response.StatusCode);
        }

        // Core registers one scheme that selects, per request, the session or the bearer scheme a
        // plug-in named. A plug-in makes it the default scheme; Core's own test server keeps its
        // header scheme as the default.
        [Fact]
        public async Task AddAllorsServerRegistersTheSelectingScheme()
        {
            using var provider = Services().BuildServiceProvider();

            var scheme = await provider.GetRequiredService<IAuthenticationSchemeProvider>().GetSchemeAsync(AllorsAuthenticationDefaults.AuthenticationScheme);

            Assert.NotNull(scheme);
            Assert.Null(provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value.DefaultScheme);
        }

        // A request that carries a bearer token is for the bearer scheme, every other request is for
        // the session. The ticket names the scheme that authenticated, which is what antiforgery reads.
        [Fact]
        public async Task TheSelectingSchemeTakesABearerTokenToTheBearerSchemeAndTheRestToTheSession()
        {
            using var provider = SelectingProvider(SessionScheme, BearerScheme);

            Assert.Equal(BearerScheme, await AuthenticatedBy(provider, "Bearer a-token"));
            Assert.Equal(SessionScheme, await AuthenticatedBy(provider, null));
            Assert.Equal(SessionScheme, await AuthenticatedBy(provider, "Basic a-credential"));
        }

        [Fact]
        public async Task WithoutABearerSchemeEveryRequestIsForTheSession()
        {
            using var provider = SelectingProvider(SessionScheme, null);

            Assert.Equal(SessionScheme, await AuthenticatedBy(provider, "Bearer a-token"));
            Assert.Equal(SessionScheme, await AuthenticatedBy(provider, null));
        }

        // A server that takes bearer tokens only.
        [Fact]
        public async Task WithoutASessionSchemeEveryRequestIsForTheBearerScheme()
        {
            using var provider = SelectingProvider(null, BearerScheme);

            Assert.Equal(BearerScheme, await AuthenticatedBy(provider, "Bearer a-token"));
            Assert.Equal(BearerScheme, await AuthenticatedBy(provider, null));
        }

        // The selecting scheme has nothing to select until a plug-in names its schemes; a server
        // that makes it the default without them stops at start-up, not at its first request.
        [Fact]
        public void UseAllorsServerFailsWhenTheSelectingSchemeIsTheDefaultAndNoSchemeIsNamed()
        {
            using var provider = ProviderForUseAllorsServer(services =>
                services.AddAuthentication(AllorsAuthenticationDefaults.AuthenticationScheme));

            var exception = Assert.Throws<InvalidOperationException>(() => new ApplicationBuilder(provider).UseAllorsServer());

            Assert.Contains(AllorsAuthenticationDefaults.AuthenticationScheme, exception.Message);
            Assert.Contains(nameof(AllorsAuthenticationOptions.SessionScheme), exception.Message);
            Assert.Contains(nameof(AllorsAuthenticationOptions.BearerScheme), exception.Message);
        }

        [Fact]
        public void UseAllorsServerFailsWhenANamedSchemeIsNotRegistered()
        {
            using var provider = ProviderForUseAllorsServer(services =>
                services.Configure<AllorsAuthenticationOptions>(v => v.SessionScheme = "Tests.Missing"));

            var exception = Assert.Throws<InvalidOperationException>(() => new ApplicationBuilder(provider).UseAllorsServer());

            Assert.Contains("Tests.Missing", exception.Message);
            Assert.Contains(nameof(AllorsAuthenticationOptions), exception.Message);
        }

        [Fact]
        public void UseAllorsServerAcceptsTheSelectingSchemeAsTheDefaultWhenASchemeIsNamed()
        {
            using var provider = ProviderForUseAllorsServer(services =>
            {
                services.AddAuthentication(AllorsAuthenticationDefaults.AuthenticationScheme)
                    .AddScheme<AuthenticationSchemeOptions, StubAuthenticationHandler>(BearerScheme, null);
                services.Configure<AllorsAuthenticationOptions>(v => v.BearerScheme = BearerScheme);
            });
            var app = new ApplicationBuilder(provider);
            app.UseRouting();

            app.UseAllorsServer();
        }

        // A plug-in that signs a browser in elsewhere, with OpenID Connect for instance, names the
        // scheme to challenge. Outside the API the session's challenge goes there; the API still
        // gets a status code.
        [Fact]
        public async Task SessionChallengeOutsideTheApiGoesToTheNamedScheme()
        {
            using var provider = SessionProvider(configure: services =>
            {
                services.AddAuthentication().AddScheme<AuthenticationSchemeOptions, StubAuthenticationHandler>(SignInScheme, null);
                services.Configure<AllorsAuthenticationOptions>(v => v.ChallengeScheme = SignInScheme);
            });
            using var scope = provider.CreateScope();
            var options = SessionOptions(provider);

            var page = Redirect(scope.ServiceProvider, options, "/account");
            await options.Events.RedirectToLogin(page);
            var api = Redirect(scope.ServiceProvider, options, "/allors/pull");
            await options.Events.RedirectToLogin(api);

            Assert.Equal(SignInScheme, page.Response.Headers[StubAuthenticationHandler.ChallengedHeader]);
            Assert.Equal(StatusCodes.Status401Unauthorized, api.Response.StatusCode);
            Assert.False(api.Response.Headers.ContainsKey(StubAuthenticationHandler.ChallengedHeader));
        }

        // A session may get an absolute lifetime next to its sliding one, for a plug-in whose
        // identity provider cannot end the application's session. Core stamps the start at sign-in
        // and refuses the cookie once the lifetime has passed, however often it was renewed.
        [Fact]
        public async Task SessionLifetimeEndsTheSessionAfterItsLifetime()
        {
            var clock = new Clock();
            using var provider = SessionProvider(configure: services =>
            {
                services.Configure<AllorsAuthenticationOptions>(v => v.SessionLifetime = TimeSpan.FromHours(12));
                services.Configure<CookieAuthenticationOptions>(SessionScheme, v => v.TimeProvider = clock);
            });
            using var scope = provider.CreateScope();
            var options = SessionOptions(provider);
            var principal = new ClaimsPrincipal(new ClaimsIdentity("Tests"));
            var properties = new AuthenticationProperties();

            await options.Events.SigningIn(new CookieSigningInContext(Context(scope.ServiceProvider), CookieScheme(), options, principal, properties, new CookieOptions()));
            clock.Advance(TimeSpan.FromHours(11));
            var withinLifetime = Validate(scope.ServiceProvider, options, principal, properties);
            await options.Events.ValidatePrincipal(withinLifetime);
            clock.Advance(TimeSpan.FromHours(2));
            var pastLifetime = Validate(scope.ServiceProvider, options, principal, properties);
            await options.Events.ValidatePrincipal(pastLifetime);

            Assert.NotNull(withinLifetime.Principal);
            Assert.Null(pastLifetime.Principal);
            Assert.Contains(pastLifetime.HttpContext.Response.Headers.SetCookie, v => v.StartsWith("Allors.Auth=;", StringComparison.Ordinal));
        }

        // A cookie without a start, issued before the lifetime was set, is refused too.
        [Fact]
        public async Task SessionLifetimeRefusesASessionWithoutAStart()
        {
            using var provider = SessionProvider(configure: services =>
                services.Configure<AllorsAuthenticationOptions>(v => v.SessionLifetime = TimeSpan.FromHours(12)));
            using var scope = provider.CreateScope();
            var options = SessionOptions(provider);

            var withoutStart = Validate(scope.ServiceProvider, options, new ClaimsPrincipal(new ClaimsIdentity("Tests")), new AuthenticationProperties());
            await options.Events.ValidatePrincipal(withoutStart);

            Assert.Null(withoutStart.Principal);
        }

        // Without a lifetime the events of sign-in and validation stay the plug-in's: Identity's
        // security stamp validator, for instance, keeps its place.
        [Fact]
        public void WithoutASessionLifetimeTheSignInAndValidationEventsStayThePlugIns()
        {
            using var provider = SessionProvider();

            var options = SessionOptions(provider);

            Assert.DoesNotContain(nameof(AllorsSessionCookie), options.Events.OnSigningIn.Method.DeclaringType.FullName, StringComparison.Ordinal);
            Assert.DoesNotContain(nameof(AllorsSessionCookie), options.Events.OnValidatePrincipal.Method.DeclaringType.FullName, StringComparison.Ordinal);
        }

        private const string SessionScheme = "Tests.Session";

        private const string BearerScheme = "Tests.Bearer";

        private const string SignInScheme = "Tests.SignIn";

        // A server whose plug-in registered the named schemes and made Core's selecting scheme the
        // default, as a plug-in with a browser session and bearer tokens does.
        private static ServiceProvider SelectingProvider(string sessionScheme, string bearerScheme)
        {
            var services = Services();
            var authentication = services.AddAuthentication(AllorsAuthenticationDefaults.AuthenticationScheme);
            foreach (var scheme in new[] { sessionScheme, bearerScheme }.Where(v => v != null))
            {
                authentication.AddScheme<AuthenticationSchemeOptions, StubAuthenticationHandler>(scheme, null);
            }

            services.Configure<AllorsAuthenticationOptions>(v =>
            {
                v.SessionScheme = sessionScheme;
                v.BearerScheme = bearerScheme;
            });

            return services.BuildServiceProvider();
        }

        // The scheme that authenticated a request with the given Authorization header, through the
        // default scheme.
        private static async Task<string> AuthenticatedBy(ServiceProvider provider, string authorization)
        {
            using var scope = provider.CreateScope();
            var context = Context(scope.ServiceProvider);
            if (authorization != null)
            {
                context.Request.Headers.Authorization = authorization;
            }

            var result = await context.AuthenticateAsync();
            return result.Ticket?.AuthenticationScheme;
        }

        private static HttpContext Context(IServiceProvider provider) => new DefaultHttpContext { RequestServices = provider };

        private static CookieValidatePrincipalContext Validate(IServiceProvider provider, CookieAuthenticationOptions options, ClaimsPrincipal principal, AuthenticationProperties properties) =>
            new(Context(provider), CookieScheme(), options, new AuthenticationTicket(principal, properties, SessionScheme));

        // A server whose plug-in registers a cookie scheme after AddAllorsServer, as AddAllorsIdentity
        // does, and names it as its session.
        private static ServiceProvider SessionProvider(string environmentName = "Development", Action<CookieAuthenticationOptions> configureCookie = null, Action<IServiceCollection> configure = null)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAllorsServer(Configuration(), new StubWebHostEnvironment { EnvironmentName = environmentName }, new AllorsServerOptions
            {
                ApplicationName = "Allors.Tests",
            });

            services.AddAuthentication().AddCookie(SessionScheme, configureCookie ?? (_ => { }));
            services.Configure<AllorsAuthenticationOptions>(v => v.SessionScheme = SessionScheme);
            configure?.Invoke(services);

            return services.BuildServiceProvider();
        }

        private sealed class OverriddenLoginEvents : CookieAuthenticationEvents
        {
            public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context) => Task.CompletedTask;
        }

        private sealed class OverriddenAccessDeniedEvents : CookieAuthenticationEvents
        {
            public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context) => Task.CompletedTask;
        }

        private sealed class OverriddenSignedInEvents : CookieAuthenticationEvents
        {
            public override Task SignedIn(CookieSignedInContext context) => Task.CompletedTask;
        }

        private sealed class OverriddenSigningOutEvents : CookieAuthenticationEvents
        {
            public override Task SigningOut(CookieSigningOutContext context) => Task.CompletedTask;
        }

        private sealed class OverriddenSigningInEvents : CookieAuthenticationEvents
        {
            public override Task SigningIn(CookieSigningInContext context) => Task.CompletedTask;
        }

        private sealed class OverriddenValidatePrincipalEvents : CookieAuthenticationEvents
        {
            public override Task ValidatePrincipal(CookieValidatePrincipalContext context) => Task.CompletedTask;
        }

        private sealed class PlainCookieEvents : CookieAuthenticationEvents
        {
        }

        private sealed class OverriddenLogoutEvents : CookieAuthenticationEvents
        {
            public override Task RedirectToLogout(RedirectContext<CookieAuthenticationOptions> context) => Task.CompletedTask;
        }

        private sealed class HiddenLoginEvents : CookieAuthenticationEvents
        {
            public new Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context) => Task.CompletedTask;
        }

        private static void ReplaceSessionEvent(CookieAuthenticationOptions options, string member)
        {
            if (member == nameof(CookieAuthenticationOptions.Events))
            {
                options.Events = new CookieAuthenticationEvents();
            }
            else if (member == nameof(CookieAuthenticationOptions.EventsType))
            {
                options.EventsType = typeof(CookieAuthenticationEvents);
            }
            else
            {
                var property = typeof(CookieAuthenticationEvents).GetProperty(member);
                property.SetValue(options.Events, property.GetValue(new CookieAuthenticationEvents()));
            }
        }

        private static CookieAuthenticationOptions SessionOptions(IServiceProvider provider) =>
            provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(SessionScheme);

        private static RedirectContext<CookieAuthenticationOptions> Redirect(IServiceProvider provider, CookieAuthenticationOptions options, string path)
        {
            var context = new DefaultHttpContext { RequestServices = provider };
            context.Request.Path = path;
            return new RedirectContext<CookieAuthenticationOptions>(context, CookieScheme(), options, new AuthenticationProperties(), "/sign-in");
        }

        private static AuthenticationScheme CookieScheme() => new(SessionScheme, null, typeof(CookieAuthenticationHandler));

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

        // As ProviderForUseAllorsServer, with the plug-in's part of the registration.
        private static ServiceProvider ProviderForUseAllorsServer(Action<IServiceCollection> configure)
        {
            var services = Services();
            services.AddSingleton<IWebHostEnvironment>(new StubWebHostEnvironment());
            services.AddSingleton<IUserResolver, FirstUserResolver>();
            services.AddSingleton(new DiagnosticListener("Allors.Tests"));
            services.AddSingleton<DiagnosticSource>(provider => provider.GetRequiredService<DiagnosticListener>());
            configure(services);

            var provider = services.BuildServiceProvider();
            provider.GetRequiredService<IDatabaseService>().Database = NewDatabase();
            return provider;
        }

        // A clock the test moves, for the cookie handler and for Core's rules of the session.
        private sealed class Clock : TimeProvider
        {
            private DateTimeOffset now = new(2026, 10, 3, 8, 0, 0, TimeSpan.Zero);

            public override DateTimeOffset GetUtcNow() => this.now;

            public void Advance(TimeSpan timeSpan) => this.now += timeSpan;
        }

        // Authenticates every request as an identity of its own scheme, and records a challenge in a
        // response header, so that a test sees which scheme Core selected or challenged.
        private sealed class StubAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public const string ChallengedHeader = "X-Tests-Challenged";

            public StubAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
                : base(options, logger, encoder)
            {
            }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync() =>
                Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(this.Scheme.Name)), this.Scheme.Name)));

            protected override Task HandleChallengeAsync(AuthenticationProperties properties)
            {
                this.Response.Headers[ChallengedHeader] = this.Scheme.Name;
                return Task.CompletedTask;
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
