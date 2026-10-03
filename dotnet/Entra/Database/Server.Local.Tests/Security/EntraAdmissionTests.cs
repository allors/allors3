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
    using Allors.Database;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Allors.Security;
    using Allors.Services;
    using Xunit;
    using MemoryConfiguration = Allors.Database.Adapters.Memory.Configuration;
    using Agent = Allors.Database.Domain.Agent;
    using MemoryDatabase = Allors.Database.Adapters.Memory.Database;
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
