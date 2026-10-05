// <copyright file="EntraAdmissionTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Derivations;
    using Allors.Database.Domain;
    using Allors.Database.Domain.Derivations.Rules;
    using Allors.Database.Meta;
    using Allors.Security;
    using Allors.Server;
    using Allors.Services;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authentication.OpenIdConnect;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Xunit;
    using MemoryConfiguration = Allors.Database.Adapters.Memory.Configuration;
    using Agent = Allors.Database.Domain.Agent;
    using MemoryDatabase = Allors.Database.Adapters.Memory.Database;
    using ObjectFactory = Allors.Database.ObjectFactory;
    using Person = Allors.Database.Domain.Person;
    using User = Allors.Database.Domain.User;

    // The plug-in admits a validated principal: it finds the user of the principal's Entra identity, or
    // has the application's factory create one, and writes the Entra fields. The factory decides whom
    // it admits and as what; the plug-in decides neither. The memory adapter has one transaction, so
    // what happens between transactions, parallel first requests for instance, is for the tests
    // against the server.
    public class EntraAdmissionTests
    {
        private static readonly Guid Tenant = new Guid("cd3598ab-ef18-4774-bfce-c1b3cebe5a42");

        private static readonly Guid CustomerTenant = new Guid("2baf9a07-5a2e-4065-adf2-2cf6a9bccff5");

        private static readonly Guid ObjectId = new Guid("5f1c2d3e-4a5b-4c6d-8e9f-0a1b2c3d4e5f");

        [Fact]
        public void AFirstSignInCreatesTheUserThroughTheFactoryWithItsEntraFields()
        {
            var database = NewDatabase();
            var factory = new TestFactory();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: factory);

            var reason = admission.Admit(Person(), signIn: true);

            Assert.Null(reason);
            Assert.Equal(1, factory.Calls);
            var user = FindUser(database, Tenant, ObjectId);
            Assert.IsType<Person>(user);
            Assert.Equal("jane@example.com", user.EntraUserName);
            Assert.Equal("Jane Doe", user.EntraDisplayName);
            Assert.Equal("jane@example.com", user.EntraEmail);
            Assert.Equal($"https://login.microsoftonline.com/{Tenant}/v2.0", user.EntraIdentityProvider);
            Assert.False(user.EntraIsGuest);
        }

        // A guest of the tenant comes from another organization: the idp claim names its home, and the
        // acct claim marks it.
        [Fact]
        public void AGuestKeepsItsHomeAndItsStatus()
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new TestFactory());

            var reason = admission.Admit(
                PersonWith(
                    new Claim(EntraClaims.IdentityProviderClaim, $"https://login.microsoftonline.com/{CustomerTenant}/v2.0"),
                    new Claim(EntraClaims.AccountTypeClaim, "1")),
                signIn: true);

            Assert.Null(reason);
            var user = FindUser(database, Tenant, ObjectId);
            Assert.Equal($"https://login.microsoftonline.com/{CustomerTenant}/v2.0", user.EntraIdentityProvider);
            Assert.True(user.EntraIsGuest);
        }

        // A program's token is admitted the same way; the factory makes it an agent. It has no user
        // name, display name or e-mail.
        [Fact]
        public void AProgramIsAdmittedAsTheFactoryDecides()
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new TestFactory());

            var reason = admission.Admit(Program(), signIn: false);

            Assert.Null(reason);
            var user = FindUser(database, Tenant, ObjectId);
            Assert.IsType<Agent>(user);
            Assert.False(user.ExistEntraUserName);
            Assert.False(user.ExistEntraDisplayName);
            Assert.False(user.ExistEntraEmail);
            Assert.Equal($"https://login.microsoftonline.com/{Tenant}/v2.0", user.EntraIdentityProvider);
            Assert.False(user.EntraIsGuest);
        }

        [Theory]
        [InlineData(EntraClaims.RolesClaim)]
        [InlineData(ClaimTypes.Role)]
        public void APersonWithAnApplicationRoleAndNoScopesIsAdmittedAsAPerson(string roleClaim)
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new Allors.Server.TestUserFactory());

            var reason = admission.Admit(BrowserPersonWithRole(roleClaim), signIn: true);

            Assert.Null(reason);
            Assert.IsType<Person>(FindUser(database, Tenant, ObjectId));
        }

        [Theory]
        [InlineData(EntraClaims.RolesClaim)]
        [InlineData(ClaimTypes.Role)]
        public void ARefusedPersonWithAnApplicationRoleLeavesNoUser(string roleClaim)
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new Allors.Server.TestUserFactory());

            var reason = admission.Admit(BrowserPersonWithRole(roleClaim, "refused@example.com"), signIn: true);

            Assert.Contains("did not admit", reason, StringComparison.Ordinal);
            Assert.Empty(AllUsers(database));
        }

        [Fact]
        public void AKnownIdentityIsFoundAndTheFactoryIsNotAsked()
        {
            var database = NewDatabase();
            var factory = new TestFactory();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: factory);

            Assert.Null(admission.Admit(Person(), signIn: true));
            Assert.Null(admission.Admit(Person(), signIn: true));
            Assert.Null(admission.Admit(Person(), signIn: false));

            Assert.Equal(1, factory.Calls);
            Assert.Single(AllUsers(database));
        }

        // The directory can change what it says about a person. A browser sign-in brings the current
        // values; a bearer token does not, so that the fields are what the person last signed in with.
        [Fact]
        public void ASignInRefreshesTheProfileFieldsAndABearerTokenDoesNot()
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new TestFactory());
            Assert.Null(admission.Admit(Person(), signIn: true));

            Assert.Null(admission.Admit(Person(name: "Jane Doe-Smith", email: "jane.smith@example.com"), signIn: false));
            var user = FindUser(database, Tenant, ObjectId);
            Assert.Equal("Jane Doe", user.EntraDisplayName);
            Assert.Equal("jane@example.com", user.EntraEmail);

            Assert.Null(admission.Admit(Person(name: "Jane Doe-Smith", email: "jane.smith@example.com"), signIn: true));
            user = FindUser(database, Tenant, ObjectId);
            Assert.Equal("Jane Doe-Smith", user.EntraDisplayName);
            Assert.Equal("jane.smith@example.com", user.EntraEmail);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AFailedProfileRefreshIsRefusedAndRolledBack(bool throwException)
        {
            var database = NewFailingDatabase(out var rule);
            var logger = new RecordingLogger();
            var factory = new TestFactory();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, logger, factory);
            Assert.Null(admission.Admit(Person(), signIn: true));
            logger.Messages.Clear();
            var failure = new InvalidOperationException("Private derivation failure details.");
            rule.Enabled = true;
            rule.Failure = throwException ? failure : null;

            var reason = admission.Admit(Person(
                name: "Jane Updated", email: "updated@example.com",
                more: new[]
                {
                    new Claim(EntraClaims.AccountTypeClaim, "1"),
                    new Claim(EntraClaims.IdentityProviderClaim, $"https://login.microsoftonline.com/{CustomerTenant}/v2.0"),
                }), signIn: true);

            Assert.False(string.IsNullOrWhiteSpace(reason));
            Assert.DoesNotContain(failure.Message, reason, StringComparison.Ordinal);
            Assert.DoesNotContain(RejectingProfileRule.ValidationError, reason, StringComparison.Ordinal);
            var log = Assert.Single(logger.Messages);
            Assert.Equal(LogLevel.Error, log.Level);
            Assert.Contains(ObjectId.ToString(), log.Message, StringComparison.Ordinal);
            Assert.Same(throwException ? failure : null, log.Exception);
            if (!throwException)
            {
                Assert.Contains(RejectingProfileRule.ValidationError, log.Message, StringComparison.Ordinal);
                Assert.Contains("not valid", reason, StringComparison.Ordinal);
            }

            Assert.Equal(1, rule.Calls);
            Assert.Equal(1, factory.Calls);
            using var verification = database.CreateTransaction();
            var user = Assert.Single(new Users(verification).Extent());
            Assert.Equal(Tenant, user.EntraTenantId);
            Assert.Equal(ObjectId, user.EntraObjectId);
            Assert.Equal("jane@example.com", user.EntraUserName);
            Assert.Equal("Jane Doe", user.EntraDisplayName);
            Assert.Equal("jane@example.com", user.EntraEmail);
            Assert.Equal($"https://login.microsoftonline.com/{Tenant}/v2.0", user.EntraIdentityProvider);
            Assert.False(user.EntraIsGuest);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ATransactionFailureRefusesAdmissionAndIsLogged(bool signIn)
        {
            var database = NewFailingDatabase(out _);
            NewUser(database, Tenant, ObjectId);
            var failure = new InvalidOperationException("Private database connection details.");
            database.TransactionFailure = failure;
            var logger = new RecordingLogger();
            var factory = new TestFactory();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, logger, factory);

            var reason = admission.Admit(Person(), signIn);

            Assert.False(string.IsNullOrWhiteSpace(reason));
            Assert.DoesNotContain(failure.Message, reason, StringComparison.Ordinal);
            var log = Assert.Single(logger.Messages);
            Assert.Equal(LogLevel.Error, log.Level);
            Assert.Same(failure, log.Exception);
            Assert.Contains(ObjectId.ToString(), log.Message, StringComparison.Ordinal);
            Assert.Equal(0, factory.Calls);
            database.TransactionFailure = null;
            Assert.Single(AllUsers(database));
        }

        // The admission reason must reach the handlers' existing refusal path instead of escaping
        // TicketReceived or TokenValidated as an unhandled infrastructure exception.
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ATransactionFailureStopsTheAuthenticationHandler(bool signIn)
        {
            var database = NewFailingDatabase(out _);
            NewUser(database, Tenant, ObjectId);
            database.TransactionFailure = new InvalidOperationException("Private database connection details.");
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IDatabaseService>(new StubDatabaseService { Database = database });
            services.AddAllorsEntraUsers(EntraDefaults.OpenIdConnectScheme, EntraDefaults.BearerScheme);
            using var provider = services.BuildServiceProvider();
            var http = new DefaultHttpContext { RequestServices = provider };
            var principal = Person();

            if (signIn)
            {
                var options = provider.GetRequiredService<IOptionsMonitor<OpenIdConnectOptions>>().Get(EntraDefaults.OpenIdConnectScheme);
                Exception failure = null;
                options.Events.OnRemoteFailure = context =>
                {
                    failure = context.Failure;
                    return Task.CompletedTask;
                };
                var context = new TicketReceivedContext(http,
                    new AuthenticationScheme(EntraDefaults.OpenIdConnectScheme, null, typeof(OpenIdConnectHandler)), options,
                    new AuthenticationTicket(principal, new AuthenticationProperties(), EntraDefaults.OpenIdConnectScheme));

                await options.Events.TicketReceived(context);

                Assert.IsType<EntraAdmission.NotAdmittedException>(failure);
                Assert.Equal(StatusCodes.Status403Forbidden, http.Response.StatusCode);
                Assert.True(context.Result.Handled);
                Assert.Same(principal, context.Principal);
            }
            else
            {
                var options = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(EntraDefaults.BearerScheme);
                var context = new Microsoft.AspNetCore.Authentication.JwtBearer.TokenValidatedContext(http,
                    new AuthenticationScheme(EntraDefaults.BearerScheme, null, typeof(JwtBearerHandler)), options)
                {
                    Principal = principal,
                };

                await options.Events.TokenValidated(context);

                Assert.NotNull(context.Result.Failure);
                Assert.False(context.Result.Succeeded);
                Assert.DoesNotContain(database.TransactionFailure.Message, context.Result.Failure.Message, StringComparison.Ordinal);
            }
        }

        // The identity is the pair of the tenant id and the object id; a token without either stands
        // for nobody the plug-in can know again.
        [Theory]
        [InlineData(EntraClaims.TenantIdClaim)]
        [InlineData(EntraClaims.ObjectIdClaim)]
        public void WithoutTheTenantOrTheObjectTheTokenIsRefused(string missing)
        {
            var database = NewDatabase();
            var factory = new TestFactory();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: factory);
            var claims = Person().Claims.Where(v => v.Type != missing).ToArray();

            var reason = admission.Admit(new ClaimsPrincipal(new ClaimsIdentity(claims, "Tests")), signIn: true);

            Assert.Contains(EntraClaims.TenantIdClaim, reason, StringComparison.Ordinal);
            Assert.Contains(EntraClaims.ObjectIdClaim, reason, StringComparison.Ordinal);
            Assert.Equal(0, factory.Calls);
            Assert.Empty(AllUsers(database));
        }

        // The handlers map some claims to long names unless an application switches that off; both
        // names are read.
        [Fact]
        public void MappedClaimNamesAreReadToo()
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new TestFactory());

            var reason = admission.Admit(
                new ClaimsPrincipal(new ClaimsIdentity(
                    new[]
                    {
                        new Claim(EntraClaims.TenantIdMappedClaim, Tenant.ToString()),
                        new Claim(EntraClaims.ObjectIdMappedClaim, ObjectId.ToString()),
                        new Claim(EntraClaims.PreferredUserNameClaim, "jane@example.com"),
                        new Claim(EntraClaims.IdentityProviderMappedClaim, $"https://login.microsoftonline.com/{CustomerTenant}/v2.0"),
                    },
                    "Tests")),
                signIn: true);

            Assert.Null(reason);
            var user = FindUser(database, Tenant, ObjectId);
            Assert.Equal("jane@example.com", user.EntraUserName);
            Assert.Equal($"https://login.microsoftonline.com/{CustomerTenant}/v2.0", user.EntraIdentityProvider);
        }

        // Without a factory no plug-in creates users: a known identity is still admitted, a new one is
        // refused with the reason.
        [Fact]
        public void WithoutAFactoryNobodyIsCreated()
        {
            var database = NewDatabase();
            NewUser(database, Tenant, ObjectId);
            var admission = new EntraAdmission(new StubDatabaseService { Database = database });

            Assert.Null(admission.Admit(Person(), signIn: true));

            var reason = admission.Admit(Person(objectId: Guid.NewGuid()), signIn: true);

            Assert.Contains(nameof(IUserFactory), reason, StringComparison.Ordinal);
            Assert.Single(AllUsers(database));
        }

        [Fact]
        public void AFactoryThatRefusesLeavesNoUser()
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new RefusingFactory());

            var reason = admission.Admit(Person(), signIn: true);

            Assert.Contains("did not admit", reason, StringComparison.Ordinal);
            Assert.Empty(AllUsers(database));
        }

        // A factory creates a user for an identity; it does not choose an existing one, which would
        // bind a second identity to that user or move the identity of one user to another.
        [Fact]
        public void AFactoryThatReturnsAnExistingUserIsRefused()
        {
            var database = NewDatabase();
            var existing = NewUser(database, Tenant, Guid.NewGuid());
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new RebindingFactory(existing));

            var reason = admission.Admit(Person(), signIn: true);

            Assert.Contains("existing user", reason, StringComparison.Ordinal);
            Assert.Null(FindUser(database, Tenant, ObjectId));
            Assert.Single(AllUsers(database));
        }

        // The role types of the strings allow 256 characters, and only the database would refuse more.
        [Fact]
        public void ALongClaimIsCutToTheRoleSize()
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new TestFactory());

            Assert.Null(admission.Admit(Person(name: new string('x', 300)), signIn: true));

            Assert.Equal(256, FindUser(database, Tenant, ObjectId).EntraDisplayName.Length);
        }

        // The browser session keeps the identity and the user name, nothing else of the token; Core's
        // API reads the user name from the identity's name.
        [Fact]
        public void TheSessionPrincipalCarriesTheIdentityAndTheName()
        {
            var principal = PersonWith(new Claim("aud", "the-client"), new Claim("nonce", "n"));

            var session = EntraAdmission.SessionPrincipal(principal);

            Assert.Equal(3, session.Claims.Count());
            Assert.Equal(Tenant, session.TenantId());
            Assert.Equal(ObjectId, session.ObjectId());
            Assert.Equal("jane@example.com", session.Identity?.Name);
            Assert.Equal("Tests", session.Identity?.AuthenticationType);
        }

        [Theory]
        [InlineData(EntraClaims.PreferredUserNameClaim, false)]
        [InlineData(EntraClaims.PreferredUserNameClaim, true)]
        [InlineData(EntraClaims.NameClaim, false)]
        [InlineData(EntraClaims.NameClaim, true)]
        [InlineData(EntraClaims.EmailClaim, false)]
        [InlineData(EntraClaims.EmailClaim, true)]
        public void ASignInKeepsProfileFieldsWhoseClaimsAreMissing(string claimType, bool empty)
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new TestFactory());
            Assert.Null(admission.Admit(Person(), signIn: false));
            var claims = Person().Claims.Where(v => v.Type != claimType).ToList();
            if (empty)
            {
                claims.Add(new Claim(claimType, string.Empty));
            }

            Assert.Null(admission.Admit(new ClaimsPrincipal(new ClaimsIdentity(claims, "Tests")), signIn: true));

            var user = FindUser(database, Tenant, ObjectId);
            Assert.Equal("jane@example.com", user.EntraUserName);
            Assert.Equal("Jane Doe", user.EntraDisplayName);
            Assert.Equal("jane@example.com", user.EntraEmail);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("2")]
        [InlineData("guest")]
        public void ASignInKeepsGuestStatusWithoutAnExplicitAccountType(string accountType)
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new TestFactory());
            Assert.Null(admission.Admit(PersonWith(new Claim(EntraClaims.AccountTypeClaim, "1")), signIn: false));
            var principal = accountType == null ? Person() : PersonWith(new Claim(EntraClaims.AccountTypeClaim, accountType));

            Assert.Null(admission.Admit(principal, signIn: true));

            Assert.True(FindUser(database, Tenant, ObjectId).EntraIsGuest);
        }

        [Theory]
        [InlineData("0", false)]
        [InlineData("1", true)]
        public void ASignInAppliesAnExplicitGuestStatus(string accountType, bool isGuest)
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new TestFactory());
            Assert.Null(admission.Admit(PersonWith(new Claim(EntraClaims.AccountTypeClaim, isGuest ? "0" : "1")), signIn: false));

            Assert.Null(admission.Admit(PersonWith(new Claim(EntraClaims.AccountTypeClaim, accountType)), signIn: true));

            Assert.Equal(isGuest, FindUser(database, Tenant, ObjectId).EntraIsGuest);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("0")]
        [InlineData("1")]
        public void ASignInKeepsTheKnownProviderWhenIdpIsMissing(string accountType)
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new TestFactory());
            var home = $"https://sts.windows.net/{CustomerTenant}/";
            Assert.Null(admission.Admit(PersonWith(
                new Claim(EntraClaims.IdentityProviderClaim, home),
                new Claim(EntraClaims.AccountTypeClaim, "1")), signIn: false));
            var principal = accountType == null ? Person() : PersonWith(new Claim(EntraClaims.AccountTypeClaim, accountType));

            Assert.Null(admission.Admit(principal, signIn: true));

            var user = FindUser(database, Tenant, ObjectId);
            Assert.Equal(home, user.EntraIdentityProvider);
            Assert.Equal(accountType != "0", user.EntraIsGuest);
        }

        [Theory]
        [InlineData(false, EntraClaims.IdentityProviderClaim, false)]
        [InlineData(true, EntraClaims.IdentityProviderClaim, false)]
        [InlineData(false, EntraClaims.IdentityProviderMappedClaim, false)]
        [InlineData(true, EntraClaims.IdentityProviderMappedClaim, false)]
        [InlineData(false, EntraClaims.IdentityProviderClaim, true)]
        [InlineData(true, EntraClaims.IdentityProviderClaim, true)]
        public void ASignInKeepsTheProviderRepresentationWhenOnlyTheTokenVersionChanges(bool firstV2, string claimType, bool trailingSlash)
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new TestFactory());
            var v1 = $"https://sts.windows.net/{CustomerTenant}/";
            var v2 = $"https://login.microsoftonline.com/{CustomerTenant}/v2.0" + (trailingSlash ? "/" : string.Empty);
            var first = firstV2 ? v2 : v1;
            var next = firstV2 ? v1 : v2;
            Assert.Null(admission.Admit(PersonWith(
                new Claim(claimType, first), new Claim(EntraClaims.AccountTypeClaim, "1")), signIn: false));

            Assert.Null(admission.Admit(PersonWith(
                new Claim(claimType, next),
                new Claim(EntraClaims.IdentityProviderClaim, string.Empty)), signIn: true));

            var user = FindUser(database, Tenant, ObjectId);
            Assert.Equal(first, user.EntraIdentityProvider);
            Assert.True(user.EntraIsGuest);
        }

        [Theory]
        [InlineData("https://login.microsoftonline.com/{0}/v2.0", EntraClaims.IdentityProviderClaim)]
        [InlineData("https://login.microsoftonline.com/{0}/v2.0", EntraClaims.IdentityProviderMappedClaim)]
        [InlineData("live.com", EntraClaims.IdentityProviderClaim)]
        [InlineData("https://example.com/2baf9a07-5a2e-4065-adf2-2cf6a9bccff5/v2.0", EntraClaims.IdentityProviderClaim)]
        [InlineData("http://login.microsoftonline.com/2baf9a07-5a2e-4065-adf2-2cf6a9bccff5/v2.0", EntraClaims.IdentityProviderClaim)]
        [InlineData("https://login.microsoftonline.com:444/2baf9a07-5a2e-4065-adf2-2cf6a9bccff5/v2.0", EntraClaims.IdentityProviderClaim)]
        [InlineData("https://login.microsoftonline.com/2baf9a07-5a2e-4065-adf2-2cf6a9bccff5/v2.0?other", EntraClaims.IdentityProviderClaim)]
        [InlineData("https://login.microsoftonline.com/2baf9a07-5a2e-4065-adf2-2cf6a9bccff5/v2.0#other", EntraClaims.IdentityProviderClaim)]
        [InlineData("https://other@login.microsoftonline.com/2baf9a07-5a2e-4065-adf2-2cf6a9bccff5/v2.0", EntraClaims.IdentityProviderClaim)]
        [InlineData("https://login.microsoftonline.com/2baf9a07-5a2e-4065-adf2-2cf6a9bccff5/v2.0/extra", EntraClaims.IdentityProviderClaim)]
        public void ASignInRecordsAnExplicitlyDifferentProvider(string providerFormat, string claimType)
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new TestFactory());
            Assert.Null(admission.Admit(PersonWith(
                new Claim(EntraClaims.IdentityProviderClaim, $"https://sts.windows.net/{CustomerTenant}/")), signIn: false));
            var provider = string.Format(System.Globalization.CultureInfo.InvariantCulture, providerFormat, Tenant);

            Assert.Null(admission.Admit(PersonWith(new Claim(claimType, provider)), signIn: true));

            Assert.Equal(provider, FindUser(database, Tenant, ObjectId).EntraIdentityProvider);
        }

        [Fact]
        public void ASignInInitializesAMissingProviderFromTheIssuer()
        {
            var database = NewDatabase();
            NewUser(database, Tenant, ObjectId);
            var admission = new EntraAdmission(new StubDatabaseService { Database = database });

            Assert.Null(admission.Admit(Person(), signIn: true));

            Assert.Equal($"https://login.microsoftonline.com/{Tenant}/v2.0", FindUser(database, Tenant, ObjectId).EntraIdentityProvider);
        }

        [Fact]
        public void ASparseBrowserSignInKeepsAV1GuestsProfile()
        {
            var database = NewDatabase();
            var factory = new TestFactory();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: factory);
            var home = $"https://sts.windows.net/{CustomerTenant}/";
            var bearer = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(EntraClaims.TenantIdClaim, Tenant.ToString()),
                new Claim(EntraClaims.ObjectIdClaim, ObjectId.ToString()),
                new Claim(EntraClaims.IssuerClaim, $"https://sts.windows.net/{Tenant}/"),
                new Claim(EntraClaims.UpnClaim, "jane@example.com"),
                new Claim(EntraClaims.NameClaim, "Jane Doe"),
                new Claim(EntraClaims.EmailClaim, "jane@example.com"),
                new Claim(EntraClaims.IdentityProviderClaim, home),
                new Claim(EntraClaims.AccountTypeClaim, "1"),
            }, "Tests"));
            Assert.Null(admission.Admit(bearer, signIn: false));
            var browser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(EntraClaims.TenantIdMappedClaim, Tenant.ToString()),
                new Claim(EntraClaims.ObjectIdMappedClaim, ObjectId.ToString()),
                new Claim(EntraClaims.IssuerClaim, $"https://login.microsoftonline.com/{Tenant}/v2.0"),
                new Claim(EntraClaims.PreferredUserNameClaim, "jane.smith@example.com"),
                new Claim(EntraClaims.NameClaim, "Jane Doe-Smith"),
                new Claim(EntraClaims.IdentityProviderClaim, string.Empty),
            }, "Tests"));

            Assert.Null(admission.Admit(browser, signIn: true));

            var user = FindUser(database, Tenant, ObjectId);
            Assert.Equal("jane.smith@example.com", user.EntraUserName);
            Assert.Equal("Jane Doe-Smith", user.EntraDisplayName);
            Assert.Equal("jane@example.com", user.EntraEmail);
            Assert.True(user.EntraIsGuest);
            Assert.Equal(home, user.EntraIdentityProvider);
            Assert.Equal(1, factory.Calls);
            Assert.Single(AllUsers(database));
        }

        [Theory]
        [InlineData("unique_name", false)]
        [InlineData("unique_name", true)]
        [InlineData(ClaimTypes.Name, false)]
        [InlineData(ClaimTypes.Name, true)]
        [InlineData(EntraClaims.EmailClaim, false)]
        [InlineData(EntraClaims.EmailClaim, true)]
        [InlineData(ClaimTypes.Email, false)]
        [InlineData(ClaimTypes.Email, true)]
        public void AGuestWithoutUpnIsAdmittedWithAnotherUserNameClaim(string claimType, bool signIn)
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new TestUserFactory());
            var claims = Person().Claims
                .Where(v => v.Type != EntraClaims.PreferredUserNameClaim && v.Type != EntraClaims.EmailClaim)
                .Concat(new[]
                {
                    new Claim(claimType, "guest@customer.example"),
                    new Claim(EntraClaims.AccountTypeClaim, "1"),
                });
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Tests"));

            Assert.Null(admission.Admit(principal, signIn));

            var user = FindUser(database, Tenant, ObjectId);
            Assert.IsType<Person>(user);
            Assert.True(user.EntraIsGuest);
            Assert.Equal("guest@customer.example", user.EntraUserName);
            Assert.Single(AllUsers(database));
            var session = EntraAdmission.SessionPrincipal(principal);
            Assert.Equal(Tenant, session.TenantId());
            Assert.Equal(ObjectId, session.ObjectId());
            Assert.Equal("guest@customer.example", session.Identity.Name);
        }

        [Theory]
        [InlineData(EntraClaims.PreferredUserNameClaim, EntraClaims.UpnClaim)]
        [InlineData(EntraClaims.UpnClaim, ClaimTypes.Upn)]
        [InlineData(ClaimTypes.Upn, "unique_name")]
        [InlineData("unique_name", ClaimTypes.Name)]
        [InlineData(ClaimTypes.Name, EntraClaims.EmailClaim)]
        [InlineData(EntraClaims.EmailClaim, ClaimTypes.Email)]
        public void AUserNameFallbackCannotBypassTheFactorysRefusal(string preferredClaim, string fallbackClaim)
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new TestUserFactory());
            var claims = Person().Claims
                .Where(v => v.Type != EntraClaims.PreferredUserNameClaim && v.Type != EntraClaims.EmailClaim)
                .Concat(new[]
                {
                    new Claim(fallbackClaim, "guest@customer.example"),
                    new Claim(preferredClaim, "refused@customer.example"),
                });
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Tests"));

            Assert.Equal("refused@customer.example", principal.UserName());
            Assert.Contains("did not admit", admission.Admit(principal, signIn: false), StringComparison.Ordinal);
            Assert.Empty(AllUsers(database));
        }

        [Fact]
        public void EmptyUserNameClaimsDoNotHideAnAvailableEmail()
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new TestUserFactory());
            var claims = Person().Claims
                .Where(v => v.Type != EntraClaims.PreferredUserNameClaim && v.Type != EntraClaims.EmailClaim)
                .Concat(new[]
                {
                    new Claim(EntraClaims.PreferredUserNameClaim, string.Empty),
                    new Claim(EntraClaims.UpnClaim, string.Empty),
                    new Claim(ClaimTypes.Upn, string.Empty),
                    new Claim("unique_name", string.Empty),
                    new Claim(ClaimTypes.Name, string.Empty),
                    new Claim(EntraClaims.EmailClaim, string.Empty),
                    new Claim(ClaimTypes.Email, "guest@customer.example"),
                });

            Assert.Null(admission.Admit(new ClaimsPrincipal(new ClaimsIdentity(claims, "Tests")), signIn: false));
            Assert.Equal("guest@customer.example", FindUser(database, Tenant, ObjectId).EntraUserName);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ADisplayNameAloneDoesNotSatisfyTheFactorysUserNameRequirement(bool empty)
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new TestUserFactory());
            var claims = Person().Claims
                .Where(v => v.Type != EntraClaims.PreferredUserNameClaim && v.Type != EntraClaims.EmailClaim).ToList();
            if (empty)
            {
                claims.AddRange(new[] { EntraClaims.PreferredUserNameClaim, EntraClaims.UpnClaim, ClaimTypes.Upn,
                    "unique_name", ClaimTypes.Name, EntraClaims.EmailClaim, ClaimTypes.Email }
                    .Select(type => new Claim(type, string.Empty)));
            }

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Tests"));

            Assert.Null(principal.UserName());
            Assert.Contains("did not admit", admission.Admit(principal, signIn: false), StringComparison.Ordinal);
            Assert.Empty(AllUsers(database));
        }

        [Fact]
        public void ASignInCanRefreshTheUserNameFromEmailWithoutChangingTheIdentity()
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new TestUserFactory());
            Assert.Null(admission.Admit(Person(), signIn: true));
            var existingId = FindUser(database, Tenant, ObjectId).Id;
            var claims = Person(email: "jane.smith@example.com").Claims
                .Where(v => v.Type != EntraClaims.PreferredUserNameClaim);

            Assert.Null(admission.Admit(new ClaimsPrincipal(new ClaimsIdentity(claims, "Tests")), signIn: true));

            var user = FindUser(database, Tenant, ObjectId);
            Assert.Equal(existingId, user.Id);
            Assert.Equal("jane.smith@example.com", user.EntraUserName);
            Assert.Equal("jane.smith@example.com", user.EntraEmail);
            Assert.Single(AllUsers(database));
        }

        [Fact]
        public void ASignInKeepsTheUserNameWhenAllUserNameClaimsAreMissing()
        {
            var database = NewDatabase();
            var admission = new EntraAdmission(new StubDatabaseService { Database = database }, userFactory: new TestUserFactory());
            Assert.Null(admission.Admit(Person(), signIn: true));
            var claims = Person().Claims
                .Where(v => v.Type != EntraClaims.PreferredUserNameClaim && v.Type != EntraClaims.EmailClaim);

            Assert.Null(admission.Admit(new ClaimsPrincipal(new ClaimsIdentity(claims, "Tests")), signIn: true));

            Assert.Equal("jane@example.com", FindUser(database, Tenant, ObjectId).EntraUserName);
            Assert.Single(AllUsers(database));
        }

        private static ClaimsPrincipal PersonWith(params Claim[] more) => Person(more: more);

        private static ClaimsPrincipal Person(string name = "Jane Doe", string email = "jane@example.com", Guid? objectId = null, params Claim[] more)
        {
            var claims = new List<Claim>
            {
                new(EntraClaims.TenantIdClaim, Tenant.ToString()),
                new(EntraClaims.ObjectIdClaim, (objectId ?? ObjectId).ToString()),
                new(EntraClaims.IssuerClaim, $"https://login.microsoftonline.com/{Tenant}/v2.0"),
                new(EntraClaims.PreferredUserNameClaim, "jane@example.com"),
                new(EntraClaims.NameClaim, name),
                new(EntraClaims.EmailClaim, email),
                new(EntraClaims.ScopeClaim, "access_as_user"),
            };
            claims.AddRange(more);
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Tests", EntraClaims.PreferredUserNameClaim, ClaimTypes.Role));
        }

        private static ClaimsPrincipal BrowserPersonWithRole(string roleClaim, string userName = "jane@example.com")
        {
            var claims = Person().Claims
                .Where(v => v.Type != EntraClaims.ScopeClaim && v.Type != EntraClaims.PreferredUserNameClaim)
                .Concat(new[]
                {
                    new Claim(EntraClaims.PreferredUserNameClaim, userName),
                    new Claim(roleClaim, "People.Access"),
                });
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Tests", EntraClaims.PreferredUserNameClaim, ClaimTypes.Role));
        }

        private static ClaimsPrincipal Program() =>
            new(new ClaimsIdentity(
                new[]
                {
                    new Claim(EntraClaims.TenantIdClaim, Tenant.ToString()),
                    new Claim(EntraClaims.ObjectIdClaim, ObjectId.ToString()),
                    new Claim(EntraClaims.IssuerClaim, $"https://login.microsoftonline.com/{Tenant}/v2.0"),
                    new Claim(EntraClaims.IdentityTypeClaim, "app"),
                    new Claim(EntraClaims.RolesClaim, "Programs.Access"),
                    new Claim(EntraClaims.AuthorizedPartyClaim, "7f1d2c3b-4a59-4e6f-8b7c-9d0e1f2a3b4c"),
                },
                "Tests"));

        private static User FindUser(IDatabase database, Guid tenantId, Guid objectId)
        {
            var transaction = database.CreateTransaction();
            return new Users(transaction).FindByEntraIdentity(tenantId, objectId);
        }

        private static User[] AllUsers(IDatabase database)
        {
            using var transaction = database.CreateTransaction();
            return new Users(transaction).Extent().ToArray();
        }

        private static User NewUser(IDatabase database, Guid tenantId, Guid objectId)
        {
            using var transaction = database.CreateTransaction();
            var person = new PersonBuilder(transaction).Build();
            person.EntraTenantId = tenantId;
            person.EntraObjectId = objectId;
            transaction.Derive();
            transaction.Commit();
            return person;
        }

        private static IDatabase NewDatabase()
        {
            var metaPopulation = new MetaBuilder().Build();
            var database = new MemoryDatabase(
                new DefaultDatabaseServices(new Engine(Rules.Create(metaPopulation))),
                new MemoryConfiguration
                {
                    ObjectFactory = new ObjectFactory(metaPopulation, typeof(User)),
                });

            database.Init();
            new Setup(database, new Config { SetupSecurity = false }).Apply();

            return database;
        }

        private static FailingDatabase NewFailingDatabase(out RejectingProfileRule rule)
        {
            var metaPopulation = new MetaBuilder().Build();
            rule = new RejectingProfileRule(metaPopulation);
            var database = new FailingDatabase(
                new DefaultDatabaseServices(new Engine(Rules.Create(metaPopulation).Append(rule).ToArray())),
                new MemoryConfiguration { ObjectFactory = new ObjectFactory(metaPopulation, typeof(User)) });
            database.Init();
            new Setup(database, new Config { SetupSecurity = false }).Apply();
            return database;
        }

        private sealed class RejectingProfileRule : Rule
        {
            public const string ValidationError = "The application rejected the refreshed profile.";

            public RejectingProfileRule(MetaPopulation m) : base(m, new Guid("e8a7b438-062e-4817-aac5-4d7e5caa8830")) =>
                this.Patterns = new[] { m.User.RolePattern(v => v.EntraDisplayName) };

            public bool Enabled { get; set; }

            public Exception Failure { get; set; }

            public int Calls { get; private set; }

            public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
            {
                if (!this.Enabled)
                {
                    return;
                }

                this.Calls++;
                if (this.Failure != null)
                {
                    throw this.Failure;
                }

                cycle.Validation.AddError(ValidationError);
            }
        }

        private sealed class FailingDatabase : MemoryDatabase
        {
            public FailingDatabase(IDatabaseServices services, MemoryConfiguration configuration) : base(services, configuration)
            {
            }

            public InvalidOperationException TransactionFailure { get; set; }

            protected override Allors.Database.Adapters.Memory.Transaction Transaction =>
                this.TransactionFailure != null ? throw this.TransactionFailure : base.Transaction;
        }

        private sealed class RecordingLogger : ILogger<EntraAdmission>
        {
            public List<(LogLevel Level, string Message, Exception Exception)> Messages { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
                this.Messages.Add((logLevel, formatter(state, exception), exception));
        }

        // The factory of this tree's concrete domain, as the test server has it: an agent for a
        // program, a person for a person.
        private sealed class TestFactory : IUserFactory
        {
            private int calls;

            public int Calls => this.calls;

            public User Create(ITransaction transaction, ClaimsPrincipal principal)
            {
                System.Threading.Interlocked.Increment(ref this.calls);
                return principal.IsApplication() ? new AgentBuilder(transaction).Build() : new PersonBuilder(transaction).Build();
            }
        }

        private sealed class RefusingFactory : IUserFactory
        {
            public User Create(ITransaction transaction, ClaimsPrincipal principal) => null;
        }

        private sealed class RebindingFactory : IUserFactory
        {
            private readonly User existing;

            public RebindingFactory(User existing) => this.existing = existing;

            public User Create(ITransaction transaction, ClaimsPrincipal principal) => (User)transaction.Instantiate(this.existing.Id);
        }

        private sealed class StubDatabaseService : IDatabaseService
        {
            public Func<IDatabase> Build { get; set; }

            public IDatabase Database { get; set; }
        }
    }
}
