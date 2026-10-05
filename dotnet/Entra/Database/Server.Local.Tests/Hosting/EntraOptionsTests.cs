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
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
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
