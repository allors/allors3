// <copyright file="EntraOptionsTests.cs" company="Allors bv">
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
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Security;
    using Allors.Server;
    using Allors.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Identity.Web;
    using Microsoft.IdentityModel.JsonWebTokens;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
    using Xunit;
    using MemoryConfiguration = Allors.Database.Adapters.Memory.Configuration;
    using MemoryDatabase = Allors.Database.Adapters.Memory.Database;
    using MetaBuilder = Allors.Database.Meta.MetaBuilder;
    using Person = Allors.Database.Domain.Person;
    using User = Allors.Database.Domain.User;

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

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("12h")]
        [InlineData("garbage")]
        [InlineData("00:00:00")]
        [InlineData("00:00:00.0000000")]
        [InlineData("-00:00:01")]
        [InlineData("-1.00:00:00")]
        [InlineData("12")]
        [InlineData("24:00:00")]
        [InlineData("12:00")]
        [InlineData("1:00:00")]
        [InlineData("12:0:00")]
        [InlineData("12:00:0")]
        [InlineData("00:60:00")]
        [InlineData("00:00:60")]
        [InlineData("12:00:00.")]
        [InlineData("12:00:00.12345678")]
        [InlineData("10675200.00:00:00")]
        [InlineData(" 12:00:00 ")]
        [InlineData("1:12:00:00")]
        public void StartUpRefusesAnInvalidSessionLifetime(string value)
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
                Provider(new Dictionary<string, string> { ["Entra:SessionLifetime"] = value }));

            Assert.Contains("Entra:SessionLifetime", exception.Message, StringComparison.Ordinal);
            Assert.Contains("positive", exception.Message, StringComparison.Ordinal);
            Assert.Contains("hh:mm:ss", exception.Message, StringComparison.Ordinal);
            Assert.Contains("12:00:00", exception.Message, StringComparison.Ordinal);
            Assert.Contains("1.00:00:00", exception.Message, StringComparison.Ordinal);
            Assert.Contains("omit", exception.Message, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("12:00:00", 12 * TimeSpan.TicksPerHour)]
        [InlineData("1.00:00:00", TimeSpan.TicksPerDay)]
        [InlineData("2.03:04:05", 2 * TimeSpan.TicksPerDay + 3 * TimeSpan.TicksPerHour + 4 * TimeSpan.TicksPerMinute + 5 * TimeSpan.TicksPerSecond)]
        [InlineData("00:00:00.5", TimeSpan.TicksPerSecond / 2)]
        [InlineData("00:00:00.0000001", 1)]
        [InlineData("1.02:03:04.1234567", TimeSpan.TicksPerDay + 2 * TimeSpan.TicksPerHour + 3 * TimeSpan.TicksPerMinute + 4 * TimeSpan.TicksPerSecond + 1234567)]
        [InlineData("0.00:00:01", TimeSpan.TicksPerSecond)]
        [InlineData("10675199.02:48:05.4775807", long.MaxValue)]
        public void SessionLifetimeAcceptsExplicitPositiveDurations(string value, long ticks)
        {
            using var provider = Provider(new Dictionary<string, string> { ["Entra:SessionLifetime"] = value });

            Assert.Equal(TimeSpan.FromTicks(ticks), provider.GetRequiredService<IOptions<AllorsAuthenticationOptions>>().Value.SessionLifetime);
        }

        // The registered options use the value that passed startup validation, even if the
        // configuration provider changes before those options are first requested.
        [Fact]
        public void SessionLifetimeUsesTheValueValidatedAtRegistration()
        {
            using var provider = Provider(new Dictionary<string, string> { ["Entra:SessionLifetime"] = "02:00:00" },
                configure: services =>
                {
                    var configuration = (IConfiguration)services.Single(v => v.ServiceType == typeof(IConfiguration)).ImplementationInstance;
                    configuration["Entra:SessionLifetime"] = "00:00:00";
                });

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

        // A v1 token's upn may already have been mapped by the handler. Both forms name the
        // principal before the application's callback and the admission factory inspect it.
        [Theory]
        [InlineData(EntraClaims.UpnClaim)]
        [InlineData(ClaimTypes.Upn)]
        public async Task AV1UserNameIsAvailableToTheApplicationsTokenHandler(string claimType)
        {
            string observedName = null;
            using var provider = Provider(configure: services =>
                services.Configure<JwtBearerOptions>(EntraDefaults.BearerScheme, options =>
                    options.Events.OnTokenValidated = context =>
                    {
                        observedName = context.Principal.Identity.Name;
                        context.Fail("Stop before admission in this test.");
                        return Task.CompletedTask;
                    }));
            var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(EntraDefaults.BearerScheme);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(claimType, "jane@example.com") }, "Tests", options.TokenValidationParameters.NameClaimType, ClaimTypes.Role));
            var context = new Microsoft.AspNetCore.Authentication.JwtBearer.TokenValidatedContext(
                new DefaultHttpContext { RequestServices = provider },
                new AuthenticationScheme(EntraDefaults.BearerScheme, null, typeof(JwtBearerHandler)), options)
            {
                Principal = principal,
            };

            await options.Events.TokenValidated(context);

            Assert.Equal("jane@example.com", observedName);
        }

        // Choosing a name claim or a retriever belongs to the application. The plug-in does not
        // supply a preferred_username that the application's choice deliberately did not select.
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TheApplicationsBearerNameSelectionIsPreserved(bool useRetriever)
        {
            using var provider = Provider(configure: services =>
                services.Configure<JwtBearerOptions>(EntraDefaults.BearerScheme, options =>
                {
                    if (useRetriever)
                    {
                        options.TokenValidationParameters.NameClaimTypeRetriever = (_, _) => EntraClaims.PreferredUserNameClaim;
                    }
                    else
                    {
                        options.TokenValidationParameters.NameClaimType = "custom_name";
                    }

                    options.Events.OnTokenValidated = context =>
                    {
                        context.Fail("Stop before admission in this test.");
                        return Task.CompletedTask;
                    };
                }));
            var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(EntraDefaults.BearerScheme);
            var nameClaimType = options.TokenValidationParameters.NameClaimTypeRetriever?.Invoke(null, null) ?? options.TokenValidationParameters.NameClaimType;
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Upn, "jane@example.com") }, "Tests", nameClaimType, ClaimTypes.Role));
            var context = new Microsoft.AspNetCore.Authentication.JwtBearer.TokenValidatedContext(
                new DefaultHttpContext { RequestServices = provider },
                new AuthenticationScheme(EntraDefaults.BearerScheme, null, typeof(JwtBearerHandler)), options)
            {
                Principal = principal,
            };

            await options.Events.TokenValidated(context);

            Assert.Null(principal.Identity.Name);
            Assert.False(principal.HasClaim(v => v.Type == EntraClaims.PreferredUserNameClaim));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task TheApplicationsTicketHandlerCanStopAdmission(bool skip)
        {
            using var provider = Provider(configure: services =>
                services.Configure<OpenIdConnectOptions>(EntraDefaults.OpenIdConnectScheme, options =>
                    options.Events.OnTicketReceived = context =>
                    {
                        if (skip)
                        {
                            context.SkipHandler();
                        }
                        else
                        {
                            context.HandleResponse();
                        }

                        return Task.CompletedTask;
                    }));
            var options = OpenIdConnect(provider);
            var principal = new ClaimsPrincipal(new ClaimsIdentity("Tests"));
            var context = new TicketReceivedContext(
                new DefaultHttpContext { RequestServices = provider },
                new AuthenticationScheme(EntraDefaults.OpenIdConnectScheme, null, typeof(OpenIdConnectHandler)), options,
                new AuthenticationTicket(principal, new AuthenticationProperties(), EntraDefaults.OpenIdConnectScheme));

            await options.Events.TicketReceived(context);

            Assert.Same(principal, context.Principal);
            Assert.Equal(skip, context.Result.Skipped);
            Assert.Equal(!skip, context.Result.Handled);
        }

        [Fact]
        public async Task AFailureFromTheApplicationsTicketHandlerStopsSignIn()
        {
            var rejection = new InvalidOperationException("The application refused the ticket.");
            using var provider = Provider(configure: services =>
                services.Configure<OpenIdConnectOptions>(EntraDefaults.OpenIdConnectScheme, options =>
                    options.Events.OnTicketReceived = context =>
                    {
                        context.Fail(rejection);
                        return Task.CompletedTask;
                    }));
            var options = OpenIdConnect(provider);
            var principal = new ClaimsPrincipal(new ClaimsIdentity("Tests"));
            var context = new TicketReceivedContext(
                new DefaultHttpContext { RequestServices = provider },
                new AuthenticationScheme(EntraDefaults.OpenIdConnectScheme, null, typeof(OpenIdConnectHandler)), options,
                new AuthenticationTicket(principal, new AuthenticationProperties(), EntraDefaults.OpenIdConnectScheme));

            var exception = await Assert.ThrowsAsync<AuthenticationFailureException>(() => options.Events.TicketReceived(context));

            Assert.Same(rejection, exception.InnerException);
            Assert.Same(principal, context.Principal);
        }

        // Admission uses the application's current failure callback, including a callback installed
        // by a later PostConfigure. Each outcome must stop cookie issuance on an admission refusal.
        [Theory]
        [InlineData("Handled")]
        [InlineData("Skipped")]
        [InlineData("Failed")]
        public async Task TheApplicationsLaterFailureHandlerControlsAdmissionRefusal(string outcome)
        {
            Exception observedFailure = null;
            var rejection = new InvalidOperationException("The application refused the sign-in.");
            using var provider = Provider(configure: services =>
                services.PostConfigure<OpenIdConnectOptions>(EntraDefaults.OpenIdConnectScheme, options =>
                    options.Events.OnRemoteFailure = context =>
                    {
                        observedFailure = context.Failure;
                        if (outcome == "Handled")
                        {
                            context.Response.StatusCode = StatusCodes.Status418ImATeapot;
                            context.HandleResponse();
                        }
                        else if (outcome == "Skipped")
                        {
                            context.SkipHandler();
                        }
                        else
                        {
                            context.Failure = rejection;
                        }

                        return Task.CompletedTask;
                    }));
            var options = OpenIdConnect(provider);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(EntraClaims.TenantIdClaim, TenantId) }, "Tests"));
            var context = new TicketReceivedContext(
                new DefaultHttpContext { RequestServices = provider },
                new AuthenticationScheme(EntraDefaults.OpenIdConnectScheme, null, typeof(OpenIdConnectHandler)), options,
                new AuthenticationTicket(principal, new AuthenticationProperties(), EntraDefaults.OpenIdConnectScheme));

            if (outcome == "Failed")
            {
                var exception = await Assert.ThrowsAsync<AuthenticationFailureException>(() => options.Events.TicketReceived(context));
                Assert.Same(rejection, exception.InnerException);
            }
            else
            {
                await options.Events.TicketReceived(context);
                Assert.Equal(outcome == "Handled", context.Result.Handled);
                Assert.Equal(outcome == "Skipped", context.Result.Skipped);
                Assert.Equal(outcome == "Handled" ? StatusCodes.Status418ImATeapot : StatusCodes.Status200OK, context.Response.StatusCode);
            }

            Assert.IsType<EntraAdmission.NotAdmittedException>(observedFailure);
            Assert.Same(principal, context.Principal);
        }

        [Fact]
        public void ClaimActionsKeepTheIssuerAndClientForAdmission()
        {
            using var provider = Provider();
            var options = OpenIdConnect(provider);
            var issuer = $"https://login.microsoftonline.com/{TenantId}/v2.0";
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(EntraClaims.IssuerClaim, issuer),
                new Claim(EntraClaims.AuthorizedPartyClaim, ClientId),
                new Claim("nonce", "the-validated-nonce"),
            }, "Tests");
            using var userData = JsonDocument.Parse("{}");

            foreach (var action in options.ClaimActions)
            {
                action.Run(userData.RootElement, identity, issuer);
            }

            var principal = new ClaimsPrincipal(identity);
            Assert.Equal(issuer, principal.IdentityProvider());
            Assert.Equal(ClientId, principal.ClientApplicationId());
            Assert.False(principal.HasClaim(v => v.Type == "nonce"));
        }

        [Theory]
        [InlineData(true, "EventsType")]
        [InlineData(false, "EventsType")]
        [InlineData(true, "LateEventsType")]
        [InlineData(false, "LateEventsType")]
        [InlineData(true, "Events")]
        [InlineData(false, "Events")]
        [InlineData(true, "Admission")]
        [InlineData(false, "Admission")]
        [InlineData(true, "MicrosoftIdentityEventsType")]
        [InlineData(true, "Certificate")]
        [InlineData(true, "AdmissionOverride")]
        [InlineData(false, "AdmissionOverride")]
        [InlineData(true, "CertificateOverride")]
        public void EntraRefusesConfigurationThatReplacesItsEvents(bool browser, string replacement)
        {
            using var provider = Provider(configure: services => ReplaceEntraEvents(services, browser, replacement));

            var exception = Assert.Throws<OptionsValidationException>(() =>
            {
                if (browser)
                {
                    OpenIdConnect(provider);
                }
                else
                {
                    provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(EntraDefaults.BearerScheme);
                }
            });

            Assert.Contains(browser ? EntraDefaults.OpenIdConnectScheme : EntraDefaults.BearerScheme, exception.Message, StringComparison.Ordinal);
            Assert.Contains("Configure", exception.Message, StringComparison.Ordinal);
            Assert.Contains(replacement.Contains("EventsType", StringComparison.Ordinal) ? "EventsType" : "Events", exception.Message, StringComparison.Ordinal);
        }

        // Resolve the startup validator without first requesting either scheme's options: a broken
        // registration must fail before the first authentication request.
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void StartUpRefusesReplacedEntraEvents(bool browser)
        {
            using var provider = Provider(configure: services => ReplaceEntraEvents(services, browser, "Events"));

            var exception = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate());

            Assert.Contains(browser ? EntraDefaults.OpenIdConnectScheme : EntraDefaults.BearerScheme, exception.Message, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void CustomEntraSchemeNamesCannotBypassAdmission(bool browser)
        {
            var services = new ServiceCollection();
            services.AddAllorsEntraUsers(browser ? "Custom.Oidc" : null, browser ? null : "Custom.Bearer");
            services.Configure<OpenIdConnectOptions>("Custom.Oidc", options => options.EventsType = typeof(OpenIdConnectEvents));
            services.Configure<JwtBearerOptions>("Custom.Bearer", options => options.EventsType = typeof(JwtBearerEvents));
            using var provider = services.BuildServiceProvider();

            var exception = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate());

            Assert.Contains(browser ? "Custom.Oidc" : "Custom.Bearer", exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void EntraEventValidationLeavesOtherSchemesAlone()
        {
            var services = new ServiceCollection();
            services.AddAllorsEntraUsers("Custom.Oidc", "Custom.Bearer");
            services.Configure<OpenIdConnectOptions>("Other.Oidc", options => options.EventsType = typeof(OpenIdConnectEvents));
            services.Configure<JwtBearerOptions>("Other.Bearer", options => options.EventsType = typeof(JwtBearerEvents));
            using var provider = services.BuildServiceProvider();

            Assert.Equal(typeof(OpenIdConnectEvents), provider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>().Get("Other.Oidc").EventsType);
            Assert.Equal(typeof(JwtBearerEvents), provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get("Other.Bearer").EventsType);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EntraAllowsOverridesOfUnprotectedEvents(bool browser)
        {
            using var provider = Provider(configure: services =>
            {
                if (browser)
                {
                    services.Configure<OpenIdConnectOptions>(EntraDefaults.OpenIdConnectScheme,
                        options => options.Events = new CustomRemoteFailureEvents());
                }
                else
                {
                    services.Configure<JwtBearerOptions>(EntraDefaults.BearerScheme,
                        options => options.Events = new CustomAuthenticationFailedEvents());
                }
            });

            provider.GetRequiredService<IStartupValidator>().Validate();

            if (browser)
            {
                Assert.IsType<CustomRemoteFailureEvents>(OpenIdConnect(provider).Events);
            }
            else
            {
                Assert.IsType<CustomAuthenticationFailedEvents>(provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(EntraDefaults.BearerScheme).Events);
            }
        }

        [Theory]
        [InlineData("Original")]
        [InlineData("ExistingReplacement")]
        [InlineData("MissingReplacement")]
        [InlineData("NoIdentity")]
        [InlineData("Rejected")]
        public async Task SessionValidationChecksTheApplicationsFinalPrincipal(string outcome)
        {
            var database = NewSessionDatabase();
            var original = NewSessionUser(database);
            var replacement = outcome == "ExistingReplacement"
                ? NewSessionUser(database)
                : new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(EntraClaims.TenantIdClaim, TenantId),
                    new Claim(EntraClaims.ObjectIdClaim, Guid.NewGuid().ToString()),
                }, "Tests"));
            var authentication = new SessionAuthenticationService();
            var calls = 0;
            using var provider = Provider(configure: services =>
            {
                services.AddSingleton<IAuthenticationService>(authentication);
                services.Configure<CookieAuthenticationOptions>(EntraDefaults.SessionScheme, options =>
                    options.Events.OnValidatePrincipal = context =>
                    {
                        calls++;
                        Assert.Same(original, context.Principal);
                        if (outcome == "Rejected")
                        {
                            context.RejectPrincipal();
                        }
                        else if (outcome == "NoIdentity")
                        {
                            context.ReplacePrincipal(new ClaimsPrincipal(new ClaimsIdentity("Tests")));
                        }
                        else if (outcome != "Original")
                        {
                            context.ReplacePrincipal(replacement);
                        }

                        return Task.CompletedTask;
                    });
            });
            provider.GetRequiredService<IDatabaseService>().Database = database;
            var options = provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(EntraDefaults.SessionScheme);
            var context = SessionValidationContext(provider, options, EntraDefaults.SessionScheme, original);

            await options.Events.ValidatePrincipal(context);

            Assert.Equal(1, calls);
            if (outcome == "Original" || outcome == "ExistingReplacement")
            {
                Assert.Same(outcome == "Original" ? original : replacement, context.Principal);
            }
            else
            {
                Assert.Null(context.Principal);
            }

            Assert.Equal(outcome == "MissingReplacement" || outcome == "NoIdentity"
                ? new[] { EntraDefaults.SessionScheme }
                : Array.Empty<string>(), authentication.SignedOut);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CustomSessionsWithoutALifetimeStillCheckEntraUsers(bool coreFirst)
        {
            const string sessionScheme = "Custom.Session";
            var configuration = new ConfigurationBuilder().Build();
            var environment = new StubWebHostEnvironment();
            var services = new ServiceCollection();
            services.AddLogging();
            if (coreFirst)
            {
                services.AddAllorsServer(configuration, environment, new AllorsServerOptions());
            }

            services.AddAllorsEntraUsers("Custom.Oidc", null);
            services.AddAuthentication().AddCookie(sessionScheme);
            if (!coreFirst)
            {
                services.AddAllorsServer(configuration, environment, new AllorsServerOptions());
            }

            services.Configure<AllorsAuthenticationOptions>(options => options.SessionScheme = sessionScheme);
            var authentication = new SessionAuthenticationService();
            services.AddSingleton<IAuthenticationService>(authentication);
            using var provider = services.BuildServiceProvider();
            var database = NewSessionDatabase();
            provider.GetRequiredService<IDatabaseService>().Database = database;
            var principal = NewSessionUser(database);
            var options = provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(sessionScheme);
            Assert.Null(provider.GetRequiredService<IOptions<AllorsAuthenticationOptions>>().Value.SessionLifetime);
            var existing = SessionValidationContext(provider, options, sessionScheme, principal);
            await options.Events.ValidatePrincipal(existing);
            Assert.Same(principal, existing.Principal);
            Assert.Empty(authentication.SignedOut);
            using (var transaction = database.CreateTransaction())
            {
                new Users(transaction).FindByEntraIdentity(principal.TenantId().Value, principal.ObjectId().Value).Delete();
                transaction.Derive();
                transaction.Commit();
            }

            var missing = SessionValidationContext(provider, options, sessionScheme, principal);
            await options.Events.ValidatePrincipal(missing);

            Assert.Null(missing.Principal);
            Assert.Equal(new[] { sessionScheme }, authentication.SignedOut);
        }

        [Theory]
        [InlineData("Callback")]
        [InlineData("Override")]
        public void EntraSessionValidationCannotBeReplacedWithoutALifetime(string replacement)
        {
            using var provider = Provider(configure: services =>
            {
                services.Configure<AllorsAuthenticationOptions>(options => options.SessionLifetime = null);
                if (replacement == "Callback")
                {
                    services.PostConfigure<CookieAuthenticationOptions>(EntraDefaults.SessionScheme,
                        options => options.Events.OnValidatePrincipal = _ => Task.CompletedTask);
                }
                else
                {
                    services.Configure<CookieAuthenticationOptions>(EntraDefaults.SessionScheme,
                        options => options.Events = new SkippedSessionValidationEvents());
                }
            });

            var exception = Assert.Throws<OptionsValidationException>(() =>
                provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(EntraDefaults.SessionScheme));

            Assert.Contains(EntraDefaults.SessionScheme, exception.Message, StringComparison.Ordinal);
            Assert.Contains("ValidatePrincipal", exception.Message, StringComparison.Ordinal);
        }

        private static IDatabase NewSessionDatabase()
        {
            var meta = new MetaBuilder().Build();
            var database = new MemoryDatabase(new DefaultDatabaseServices(new Engine(Rules.Create(meta))),
                new MemoryConfiguration { ObjectFactory = new Allors.Database.ObjectFactory(meta, typeof(User)) });
            database.Init();
            new Setup(database, new Config { SetupSecurity = false }).Apply();
            return database;
        }

        private static ClaimsPrincipal NewSessionUser(IDatabase database)
        {
            var objectId = Guid.NewGuid();
            using var transaction = database.CreateTransaction();
            var user = new PersonBuilder(transaction).Build();
            user.EntraTenantId = Guid.Parse(TenantId);
            user.EntraObjectId = objectId;
            transaction.Derive();
            transaction.Commit();
            return new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(EntraClaims.TenantIdClaim, TenantId),
                new Claim(EntraClaims.ObjectIdClaim, objectId.ToString()),
            }, "Tests"));
        }

        private static CookieValidatePrincipalContext SessionValidationContext(IServiceProvider provider,
            CookieAuthenticationOptions options, string scheme, ClaimsPrincipal principal)
        {
            var properties = new AuthenticationProperties();
            properties.Items[AllorsSessionCookie.SessionStartKey] = DateTimeOffset.UtcNow.ToString("o");
            return new CookieValidatePrincipalContext(new DefaultHttpContext { RequestServices = provider },
                new AuthenticationScheme(scheme, null, typeof(CookieAuthenticationHandler)), options,
                new AuthenticationTicket(principal, properties, scheme));
        }

        private sealed class SkippedSessionValidationEvents : CookieAuthenticationEvents
        {
            public override Task ValidatePrincipal(CookieValidatePrincipalContext context) => Task.CompletedTask;
        }

        private sealed class SessionAuthenticationService : IAuthenticationService
        {
            public List<string> SignedOut { get; } = new();

            public Task<AuthenticateResult> AuthenticateAsync(HttpContext context, string scheme) => throw new NotSupportedException();

            public Task ChallengeAsync(HttpContext context, string scheme, AuthenticationProperties properties) => throw new NotSupportedException();

            public Task ForbidAsync(HttpContext context, string scheme, AuthenticationProperties properties) => throw new NotSupportedException();

            public Task SignInAsync(HttpContext context, string scheme, ClaimsPrincipal principal, AuthenticationProperties properties) => throw new NotSupportedException();

            public Task SignOutAsync(HttpContext context, string scheme, AuthenticationProperties properties)
            {
                this.SignedOut.Add(scheme);
                return Task.CompletedTask;
            }
        }

        private static void ReplaceEntraEvents(IServiceCollection services, bool browser, string replacement)
        {
            if (replacement == "MicrosoftIdentityEventsType")
            {
                services.Configure<MicrosoftIdentityOptions>(EntraDefaults.OpenIdConnectScheme, options => options.EventsType = typeof(OpenIdConnectEvents));
                return;
            }

            if (browser)
            {
                Action<OpenIdConnectOptions> replace = options =>
                {
                    switch (replacement)
                    {
                        case "EventsType":
                        case "LateEventsType":
                            options.EventsType = typeof(OpenIdConnectEvents);
                            break;
                        case "Events":
                            options.Events = new OpenIdConnectEvents();
                            break;
                        case "Admission":
                            options.Events.OnTicketReceived = _ => Task.CompletedTask;
                            break;
                        case "Certificate":
                            options.Events.OnAuthorizationCodeReceived = _ => Task.CompletedTask;
                            break;
                        case "AdmissionOverride":
                            options.Events = new SkippedTicketReceivedEvents();
                            break;
                        case "CertificateOverride":
                            options.Events = new SkippedAuthorizationCodeReceivedEvents();
                            break;
                    }
                };
                if (replacement == "EventsType" || replacement.EndsWith("Override", StringComparison.Ordinal))
                {
                    services.Configure(EntraDefaults.OpenIdConnectScheme, replace);
                }
                else
                {
                    services.PostConfigure(EntraDefaults.OpenIdConnectScheme, replace);
                }
            }
            else
            {
                Action<JwtBearerOptions> replace = options =>
                {
                    switch (replacement)
                    {
                        case "EventsType":
                        case "LateEventsType":
                            options.EventsType = typeof(JwtBearerEvents);
                            break;
                        case "Events":
                            options.Events = new JwtBearerEvents();
                            break;
                        case "Admission":
                            options.Events.OnTokenValidated = _ => Task.CompletedTask;
                            break;
                        case "AdmissionOverride":
                            options.Events = new SkippedTokenValidatedEvents();
                            break;
                    }
                };
                if (replacement == "EventsType" || replacement.EndsWith("Override", StringComparison.Ordinal))
                {
                    services.Configure(EntraDefaults.BearerScheme, replace);
                }
                else
                {
                    services.PostConfigure(EntraDefaults.BearerScheme, replace);
                }
            }
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

        // Configuring a certificate must authenticate the code exchange itself. The local token
        // endpoint verifies the assertion against the registered public key before issuing tokens.
        [Fact]
        public async Task ACertificateCredentialCompletesTheBrowserCodeFlow()
        {
            var certificate = NewClientCertificate();
            await using var host = await CertificateBrowser.CreateAsync(certificate, certificate);

            var first = await host.SignInAsync();
            var second = await host.SignInAsync();

            Assert.Equal(2, host.TokenRequests.Count);
            Assert.All(host.TokenRequests, form =>
            {
                Assert.False(string.IsNullOrWhiteSpace(form["client_assertion"]));
                Assert.Equal("urn:ietf:params:oauth:client-assertion-type:jwt-bearer", form["client_assertion_type"]);
                Assert.True(string.IsNullOrEmpty(form["client_secret"]));
                Assert.Equal(ClientId, form["client_id"]);
                Assert.Equal("authorization_code", form["grant_type"]);
                Assert.False(string.IsNullOrWhiteSpace(form["code_verifier"]));
            });
            Assert.Equal(2, host.ValidAssertions.Count);
            Assert.Equal(2, host.ValidAssertions.Select(v => v.Id).Distinct().Count());
            Assert.True(first.Status == HttpStatusCode.OK, first.Body);
            Assert.True(second.Status == HttpStatusCode.OK, second.Body);
            Assert.False(string.IsNullOrEmpty(first.SessionCookie));
            Assert.False(string.IsNullOrEmpty(second.SessionCookie));
            Assert.Equal(FakeEntraAccounts.Tester.UserName, first.UserName);
            Assert.Equal(FakeEntraAccounts.Tester.UserName, second.UserName);
            Assert.IsType<Person>(Assert.Single(host.Users()));
        }

        [Fact]
        public async Task AnUnregisteredCertificateCannotCreateASessionOrUser()
        {
            await using var host = await CertificateBrowser.CreateAsync(NewClientCertificate(), NewClientCertificate());

            var result = await host.SignInAsync();

            var request = Assert.Single(host.TokenRequests);
            Assert.False(string.IsNullOrWhiteSpace(request["client_assertion"]));
            Assert.Empty(host.ValidAssertions);
            Assert.InRange((int)result.Status, 400, 599);
            Assert.Null(result.SessionCookie);
            Assert.Empty(host.Users());
        }

        [Fact]
        public async Task ACertificateSignInWithTheWrongNonceCreatesNoSessionOrUser()
        {
            var certificate = NewClientCertificate();
            await using var host = await CertificateBrowser.CreateAsync(certificate, certificate);

            var result = await host.SignInAsync(wrongNonce: true);

            Assert.Single(host.ValidAssertions);
            Assert.InRange((int)result.Status, 400, 599);
            Assert.Contains("OpenIdConnectProtocolInvalidNonceException", result.Body, StringComparison.Ordinal);
            Assert.Null(result.SessionCookie);
            Assert.Empty(host.Users());
        }

        [Fact]
        public async Task ACertificateWithoutAPrivateKeyCannotCreateASessionOrUser()
        {
            var registeredCertificate = NewClientCertificate();
            using var certificate = X509CertificateLoader.LoadPkcs12(registeredCertificate, null);
            using var publicCertificate = X509CertificateLoader.LoadCertificate(certificate.Export(X509ContentType.Cert));
            await using var host = await CertificateBrowser.CreateAsync(publicCertificate.Export(X509ContentType.Pkcs12), registeredCertificate);

            var result = await host.SignInAsync();

            Assert.InRange((int)result.Status, 400, 599);
            Assert.Empty(host.TokenRequests);
            Assert.Null(result.SessionCookie);
            Assert.Empty(host.Users());
        }

        // Application handling and an existing client credential take precedence over certificate
        // loading. The malformed certificate makes an unwanted load fail immediately.
        [Theory]
        [InlineData("Handled")]
        [InlineData("Skipped")]
        [InlineData("Failed")]
        [InlineData("Redeemed")]
        [InlineData("Assertion")]
        [InlineData("Secret")]
        public async Task TheApplicationsCodeHandlerControlsCertificateRedemption(string outcome)
        {
            var calls = 0;
            AuthorizationCodeReceivedContext observed = null;
            var rejection = new InvalidOperationException("The application rejected the authorization code.");
            var redeemed = new OpenIdConnectMessage { AccessToken = "application-access-token", IdToken = "application-id-token" };
            using var provider = Provider(new Dictionary<string, string>
            {
                ["Entra:ClientSecret"] = string.Empty,
                ["Entra:ClientCredentials:0:SourceType"] = "Base64Encoded",
                ["Entra:ClientCredentials:0:Base64EncodedValue"] = "not-a-certificate",
            }, configure: services => services.Configure<OpenIdConnectOptions>(EntraDefaults.OpenIdConnectScheme, options =>
                options.Events.OnAuthorizationCodeReceived = context =>
                {
                    calls++;
                    observed = context;
                    Assert.Null(context.TokenEndpointRequest.ClientAssertion);
                    context.TokenEndpointRequest.Resource = "application-resource";
                    switch (outcome)
                    {
                        case "Handled":
                            context.HandleResponse();
                            break;
                        case "Skipped":
                            context.SkipHandler();
                            break;
                        case "Failed":
                            context.Fail(rejection);
                            break;
                        case "Redeemed":
                            context.HandleCodeRedemption(redeemed);
                            break;
                        case "Assertion":
                            context.TokenEndpointRequest.ClientAssertion = "application-assertion";
                            context.TokenEndpointRequest.ClientAssertionType = "application-assertion-type";
                            break;
                        case "Secret":
                            context.TokenEndpointRequest.ClientSecret = "application-secret";
                            break;
                    }

                    return Task.CompletedTask;
                }));
            var options = OpenIdConnect(provider);
            var request = new OpenIdConnectMessage
            {
                IssuerAddress = $"https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/token",
                ClientId = ClientId,
                ClientSecret = options.ClientSecret,
                Code = "browser-code",
                GrantType = "authorization_code",
                RedirectUri = "https://example.com/signin-oidc",
            };
            request.SetParameter("code_verifier", "browser-code-verifier");
            var context = new AuthorizationCodeReceivedContext(
                new DefaultHttpContext { RequestServices = provider },
                new AuthenticationScheme(EntraDefaults.OpenIdConnectScheme, null, typeof(OpenIdConnectHandler)),
                options, new AuthenticationProperties())
            {
                TokenEndpointRequest = request,
            };

            await options.Events.AuthorizationCodeReceived(context);

            Assert.Equal(1, calls);
            Assert.Same(context, observed);
            Assert.Same(request, context.TokenEndpointRequest);
            Assert.Equal("application-resource", request.Resource);
            Assert.Equal("browser-code", request.Code);
            Assert.Equal("browser-code-verifier", request.GetParameter("code_verifier"));
            Assert.Equal(ClientId, request.ClientId);
            Assert.Equal("authorization_code", request.GrantType);
            Assert.Equal($"https://login.microsoftonline.com/{TenantId}/oauth2/v2.0/token", request.IssuerAddress);
            Assert.Equal("https://example.com/signin-oidc", request.RedirectUri);
            Assert.Equal(outcome == "Handled", context.Result?.Handled == true);
            Assert.Equal(outcome == "Skipped", context.Result?.Skipped == true);
            Assert.Same(outcome == "Failed" ? rejection : null, context.Result?.Failure);
            Assert.Equal(outcome == "Redeemed", context.HandledCodeRedemption);
            Assert.Same(outcome == "Redeemed" ? redeemed : null, context.TokenEndpointResponse);
            Assert.Equal(outcome == "Assertion" ? "application-assertion" : null, request.ClientAssertion);
            Assert.Equal(outcome == "Assertion" ? "application-assertion-type" : null, request.ClientAssertionType);
            Assert.Equal(outcome == "Secret" ? "application-secret" : options.ClientSecret, request.ClientSecret);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("/configured-remote-sign-out")]
        public void EntraDisablesIncomingRemoteSignOut(string configuredPath)
        {
            using var provider = Provider(configuredPath == null ? null :
                new Dictionary<string, string> { ["Entra:RemoteSignOutPath"] = configuredPath });

            provider.GetRequiredService<IStartupValidator>().Validate();
            var options = OpenIdConnect(provider);

            Assert.False(options.RemoteSignOutPath.HasValue);
            Assert.Equal("/signin-oidc", options.CallbackPath.Value);
            Assert.Equal("/signout-callback-oidc", options.SignedOutCallbackPath.Value);
            Assert.Equal(EntraDefaults.SessionScheme, options.SignOutScheme);
        }

        [Theory]
        [InlineData(false, "/signout-oidc")]
        [InlineData(false, "/custom-remote-sign-out")]
        [InlineData(true, "/signout-oidc")]
        [InlineData(true, "/custom-remote-sign-out")]
        public void StartUpRefusesReenabledRemoteSignOut(bool customScheme, string path)
        {
            var scheme = customScheme ? "Custom.Oidc" : EntraDefaults.OpenIdConnectScheme;
            var services = new ServiceCollection();
            services.AddAllorsEntraUsers(scheme, null);
            services.PostConfigure<OpenIdConnectOptions>(scheme, options => options.RemoteSignOutPath = path);
            using var provider = customScheme ? services.BuildServiceProvider() : Provider(configure: registered =>
                registered.PostConfigure<OpenIdConnectOptions>(scheme, options => options.RemoteSignOutPath = path));

            var exception = Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IStartupValidator>().Validate());

            Assert.Contains(scheme, exception.Message, StringComparison.Ordinal);
            Assert.Contains("RemoteSignOutPath", exception.Message, StringComparison.Ordinal);
            Assert.Contains("empty", exception.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(EntraPaths.SignOut, exception.Message, StringComparison.Ordinal);
        }

        [Fact]
        public void CustomEntraSchemesDisableOnlyIncomingRemoteSignOut()
        {
            var services = new ServiceCollection();
            services.Configure<OpenIdConnectOptions>("Custom.Oidc", options =>
            {
                options.RemoteSignOutPath = "/custom-remote-sign-out";
                options.SignedOutCallbackPath = "/custom-sign-out-callback";
                options.SignOutScheme = "Custom.Session";
            });
            services.Configure<OpenIdConnectOptions>("Other.Oidc", options => options.RemoteSignOutPath = "/other-remote-sign-out");
            services.AddAllorsEntraUsers("Custom.Oidc", null);
            using var provider = services.BuildServiceProvider();

            provider.GetRequiredService<IStartupValidator>().Validate();
            var options = provider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>();

            Assert.False(options.Get("Custom.Oidc").RemoteSignOutPath.HasValue);
            Assert.Equal("/custom-sign-out-callback", options.Get("Custom.Oidc").SignedOutCallbackPath.Value);
            Assert.Equal("Custom.Session", options.Get("Custom.Oidc").SignOutScheme);
            Assert.Equal("/other-remote-sign-out", options.Get("Other.Oidc").RemoteSignOutPath.Value);
        }

        [Fact]
        public void BearerOnlyEntraRegistrationLeavesRemoteSignOutAlone()
        {
            var services = new ServiceCollection();
            services.AddAllorsEntraUsers(null, "Custom.Bearer");
            using var provider = services.BuildServiceProvider();

            provider.GetRequiredService<IStartupValidator>().Validate();

            Assert.Equal("/signout-oidc", provider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>()
                .Get("Other.Oidc").RemoteSignOutPath.Value);
        }

        private static byte[] NewClientCertificate()
        {
            using var key = RSA.Create(2048);
            var request = new CertificateRequest("CN=Allors Entra certificate test", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddHours(1));
            return certificate.Export(X509ContentType.Pkcs12);
        }

        private static OpenIdConnectOptions OpenIdConnect(IServiceProvider provider) =>
            provider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>().Get(EntraDefaults.OpenIdConnectScheme);

        // The services of a server that selects the Entra plug-in, configured for a tenant.
        private static ServiceProvider Provider(IDictionary<string, string> configurationValues = null, string environmentName = "Development", Action<IServiceCollection> configure = null)
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
            configure?.Invoke(services);

            // Registered last: MVC would otherwise look for an assembly with the application's name.
            services.AddSingleton<IWebHostEnvironment>(environment);

            return services.BuildServiceProvider();
        }

        // A private browser host keeps certificate configuration, the database and the assertion
        // validator local to one test. All other fake Entra endpoints are the test server's own.
        private sealed class CertificateBrowser : IAsyncDisposable
        {
            private readonly WebApplication app;
            private readonly IDatabase database;
            private readonly X509Certificate2 registeredCertificate;
            private readonly HashSet<string> assertionIds = new();

            private CertificateBrowser(WebApplication app, IDatabase database, X509Certificate2 registeredCertificate)
            {
                this.app = app;
                this.database = database;
                this.registeredCertificate = registeredCertificate;
            }

            public List<IFormCollection> TokenRequests { get; } = new();

            public List<JsonWebToken> ValidAssertions { get; } = new();

            public static async Task<CertificateBrowser> CreateAsync(byte[] clientCertificate, byte[] registeredCertificate)
            {
                // The issuer validator caches by authority across service providers. Give this host
                // its own tenant so another options test cannot supply its metadata backchannel.
                var tenantId = Guid.NewGuid().ToString();
                var builder = WebApplication.CreateBuilder(new WebApplicationOptions
                {
                    EnvironmentName = "Development",
                    ContentRootPath = System.IO.Path.GetTempPath(),
                });
                builder.Logging.ClearProviders();
                builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["Entra:TenantId"] = tenantId,
                    ["Entra:ClientId"] = ClientId,
                    ["Entra:ClientSecret"] = string.Empty,
                    ["Entra:ClientCredentials:0:SourceType"] = "Base64Encoded",
                    ["Entra:ClientCredentials:0:Base64EncodedValue"] = Convert.ToBase64String(clientCertificate),
                });
                builder.Services.AddAllorsServer(builder.Configuration, builder.Environment, new AllorsServerOptions { ApplicationName = "Allors.CertificateTests" })
                    .AddApplicationPart(typeof(UserInfoController).Assembly);
                builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
                builder.Services.AddAllorsEntra(builder.Configuration, builder.Environment);
                builder.Services.AddSingleton<IUserFactory, TestUserFactory>();
                builder.Services.AddFakeEntra();

                var metaPopulation = new MetaBuilder().Build();
                var database = new MemoryDatabase(
                    new DefaultDatabaseServices(new Engine(Rules.Create(metaPopulation))),
                    new MemoryConfiguration { ObjectFactory = new Allors.Database.ObjectFactory(metaPopulation, typeof(User)) });
                database.Init();
                new Setup(database, new Config { SetupSecurity = false }).Apply();

                using var registeredPrivateCertificate = X509CertificateLoader.LoadPkcs12(registeredCertificate, null);
                var publicCertificate = X509CertificateLoader.LoadCertificate(registeredPrivateCertificate.Export(X509ContentType.Cert));
                var app = builder.Build();
                var host = new CertificateBrowser(app, database, publicCertificate);
                app.Services.GetRequiredService<IDatabaseService>().Database = database;
                app.Use(async (context, next) =>
                {
                    if (context.Request.Path == $"{FakeEntra.PathPrefix}/{tenantId}/oauth2/v2.0/token")
                    {
                        await host.RedeemAsync(context);
                        return;
                    }

                    await next(context);
                });
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseAllorsServer(endpoints =>
                {
                    endpoints.MapAllorsEntra();
                    endpoints.MapFakeEntra();
                });
                app.Urls.Add("http://127.0.0.1:0");
                await app.StartAsync();
                return host;
            }

            public async ValueTask DisposeAsync()
            {
                await this.app.DisposeAsync();
                this.registeredCertificate.Dispose();
            }

            public User[] Users()
            {
                using var transaction = this.database.CreateTransaction();
                return new Users(transaction).Extent().ToArray();
            }

            public async Task<(HttpStatusCode Status, string SessionCookie, string UserName, string Body)> SignInAsync(bool wrongNonce = false)
            {
                var origin = new Uri(this.app.Urls.Single());
                using var handler = new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = new CookieContainer() };
                using var browser = new HttpClient(handler) { BaseAddress = origin };
                var challenge = await browser.GetAsync("/entra/sign-in?returnUrl=%2Fallors%2FUserInfo");
                Assert.Equal(HttpStatusCode.Redirect, challenge.StatusCode);
                var authorization = challenge.Headers.Location;
                var query = QueryHelpers.ParseQuery(authorization.Query);
                Assert.Equal("code", query["response_type"]);
                Assert.Equal("S256", query["code_challenge_method"]);
                Assert.False(string.IsNullOrEmpty(query["code_challenge"]));
                query["account"] = FakeEntraAccounts.Tester.Id;
                if (wrongNonce)
                {
                    query["nonce"] = Guid.NewGuid().ToString("N");
                }

                var response = await browser.GetAsync(QueryHelpers.AddQueryString(
                    authorization.GetLeftPart(UriPartial.Path), query.ToDictionary(v => v.Key, v => v.Value.ToString())));
                for (var redirect = 0; redirect < 6 && response.StatusCode == HttpStatusCode.Redirect; redirect++)
                {
                    response = await browser.GetAsync(response.Headers.Location);
                }

                var body = await response.Content.ReadAsStringAsync();
                string userName = null;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    using var json = JsonDocument.Parse(body);
                    userName = json.RootElement.GetProperty("userName").GetString();
                }

                return (response.StatusCode, handler.CookieContainer.GetCookies(origin)["Allors.Auth"]?.Value, userName, body);
            }

            private async Task RedeemAsync(HttpContext context)
            {
                var form = await context.Request.ReadFormAsync();
                this.TokenRequests.Add(form);
                if (form["client_assertion_type"] != "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" ||
                    string.IsNullOrWhiteSpace(form["client_assertion"]) ||
                    !string.IsNullOrEmpty(form["client_secret"]) ||
                    form["client_id"] != ClientId || form["grant_type"] != "authorization_code")
                {
                    await InvalidClientAsync(context);
                    return;
                }

                var validation = await new JsonWebTokenHandler().ValidateTokenAsync(form["client_assertion"], new TokenValidationParameters
                {
                    ValidIssuer = ClientId,
                    ValidAudience = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}",
                    IssuerSigningKey = new X509SecurityKey(this.registeredCertificate),
                    ValidAlgorithms = new[] { SecurityAlgorithms.RsaSsaPssSha256 },
                    ValidateIssuerSigningKey = true,
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                });
                if (!validation.IsValid || validation.SecurityToken is not JsonWebToken assertion ||
                    assertion.Subject != ClientId || string.IsNullOrWhiteSpace(assertion.Id) ||
                    !assertion.TryGetHeaderValue<string>("x5t#S256", out var thumbprint) ||
                    thumbprint != Base64UrlEncoder.Encode(this.registeredCertificate.GetCertHash(HashAlgorithmName.SHA256)) ||
                    assertion.IssuedAt < DateTime.UtcNow.AddMinutes(-1) || assertion.IssuedAt > DateTime.UtcNow.AddSeconds(5) ||
                    assertion.ValidFrom < DateTime.UtcNow.AddMinutes(-1) ||
                    assertion.ValidTo <= assertion.IssuedAt || assertion.ValidTo > assertion.IssuedAt.AddMinutes(10) ||
                    !this.assertionIds.Add(assertion.Id))
                {
                    await InvalidClientAsync(context);
                    return;
                }

                this.ValidAssertions.Add(assertion);
                var fake = this.app.Services.GetRequiredService<FakeEntra>();
                var issued = fake.Redeem(form["code"], ClientId, form["redirect_uri"], form["code_verifier"], out var error);
                if (issued == null)
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsJsonAsync(new { error = "invalid_grant", error_description = error });
                    return;
                }

                await context.Response.WriteAsJsonAsync(new
                {
                    token_type = "Bearer",
                    expires_in = 3599,
                    access_token = fake.AccessToken(fake.TenantId, issued.Account, ClientId, Array.Empty<string>()),
                    id_token = fake.IdToken(fake.TenantId, issued.Account, ClientId, issued.Nonce),
                });
            }

            private static async Task InvalidClientAsync(HttpContext context)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "invalid_client", error_description = "The code exchange requires a valid certificate assertion." });
            }
        }

        private sealed class SkippedTicketReceivedEvents : OpenIdConnectEvents
        {
            public override Task TicketReceived(TicketReceivedContext context) => Task.CompletedTask;
        }

        private sealed class SkippedAuthorizationCodeReceivedEvents : OpenIdConnectEvents
        {
            public override Task AuthorizationCodeReceived(AuthorizationCodeReceivedContext context) => Task.CompletedTask;
        }

        private sealed class SkippedTokenValidatedEvents : JwtBearerEvents
        {
            public override Task TokenValidated(Microsoft.AspNetCore.Authentication.JwtBearer.TokenValidatedContext context) => Task.CompletedTask;
        }

        private sealed class CustomRemoteFailureEvents : OpenIdConnectEvents
        {
            public override Task RemoteFailure(RemoteFailureContext context) => Task.CompletedTask;
        }

        private sealed class CustomAuthenticationFailedEvents : JwtBearerEvents
        {
            public override Task AuthenticationFailed(Microsoft.AspNetCore.Authentication.JwtBearer.AuthenticationFailedContext context) => Task.CompletedTask;
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
