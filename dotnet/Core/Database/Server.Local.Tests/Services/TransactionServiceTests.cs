// <copyright file="TransactionServiceTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Security.Claims;
    using Allors.Database;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Allors.Database.Services;
    using Allors.Services;
    using Xunit;
    using MemoryConfiguration = Allors.Database.Adapters.Memory.Configuration;
    using MemoryDatabase = Allors.Database.Adapters.Memory.Database;
    using User = Allors.Database.Domain.User;

    // The current user of a request: the authentication plug-in's IUserResolver answers for a signed-in
    // principal; an anonymous request has no user.
    public class TransactionServiceTests
    {
        [Fact]
        public void SignedInRequestGetsTheUserTheResolverReturns()
        {
            var database = NewDatabase();
            var userId = NewUser(database, "signed-in");
            var resolver = new RecordingUserResolver(userId);
            var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "signed-in") }, "Tests"));

            using var transactionService = new TransactionService(new StubDatabaseService { Database = database }, new ClaimsPrincipalService { User = principal }, resolver);

            Assert.Same(principal, resolver.Principal);
            Assert.Equal(userId, transactionService.Transaction.Services.Get<IUserService>().User?.Id);
        }

        [Fact]
        public void AnonymousRequestHasNoUser()
        {
            var resolver = new RecordingUserResolver(0);
            var anonymous = new ClaimsPrincipal(new ClaimsIdentity());

            using var transactionService = new TransactionService(new StubDatabaseService { Database = NewDatabase() }, new ClaimsPrincipalService { User = anonymous }, resolver);

            Assert.Null(resolver.Principal);
            Assert.Null(transactionService.Transaction.Services.Get<IUserService>().User);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ReadIdentityRejectsAnAnonymousOrMissingPrincipalBeforeResolving(bool missing)
        {
            var resolver = new RecordingUserResolver(0);
            var principal = missing ? null : new ClaimsPrincipal(new ClaimsIdentity());

            var exception = Assert.Throws<InvalidOperationException>(() => ReadUserIdentity.Capture(principal, resolver));

            Assert.Contains("authenticated", exception.Message);
            Assert.Null(resolver.Principal);
        }

        [Fact]
        public void ReadIdentityOwnsClaimsAndIdentityMetadataCapturedBeforeResolution()
        {
            var database = NewDatabase();
            var userId = NewUser(database, "captured");
            var resolver = new RecordingUserResolver(userId);
            var identity = new ClaimsIdentity(new[]
            {
                new Claim("name", "captured", ClaimValueTypes.String, "issuer", "original-issuer"),
                new Claim("role", "reader"),
            }, "Tests", "name", "role")
            {
                Label = "captured identity",
                Actor = new ClaimsIdentity(new[] { new Claim("actor", "original") }, "Actor"),
            };
            identity.FindFirst("name").Properties["source"] = "original";
            var principal = new ClaimsPrincipal(identity);
            var captured = ReadUserIdentity.Capture(principal, resolver);

            identity.FindFirst("name").Properties["source"] = "changed";
            identity.RemoveClaim(identity.FindFirst("name"));
            identity.AddClaim(new Claim("name", "changed"));
            identity.Actor.RemoveClaim(identity.Actor.FindFirst("actor"));
            identity.Label = "changed identity";
            principal.AddIdentity(new ClaimsIdentity("later"));
            using var transaction = database.CreateTransaction();

            Assert.Equal(userId, captured.Resolve(transaction).Id);

            var resolvedIdentity = Assert.Single(resolver.Principal.Identities);
            Assert.NotSame(identity, resolvedIdentity);
            Assert.Equal("captured", resolvedIdentity.Name);
            Assert.Equal("Tests", resolvedIdentity.AuthenticationType);
            Assert.Equal("captured identity", resolvedIdentity.Label);
            Assert.True(resolver.Principal.IsInRole("reader"));
            Assert.Equal("original", resolvedIdentity.Actor.FindFirst("actor").Value);
            var claim = resolvedIdentity.FindFirst("name");
            Assert.Equal("issuer", claim.Issuer);
            Assert.Equal("original-issuer", claim.OriginalIssuer);
            Assert.Equal("original", claim.Properties["source"]);
        }

        [Fact]
        public void ReadIdentityGivesTheResolverAFreshPrincipalOnEveryResolution()
        {
            using var transaction = NewDatabase().CreateTransaction();
            var resolver = new RecordingUserResolver(0);
            var captured = ReadUserIdentity.Capture(new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "captured") }, "Tests")), resolver);

            Assert.Null(captured.Resolve(transaction));
            var first = resolver.Principal;
            ((ClaimsIdentity)first.Identity).RemoveClaim(first.FindFirst(ClaimTypes.Name));

            Assert.Null(captured.Resolve(transaction));

            Assert.NotSame(first, resolver.Principal);
            Assert.Equal("captured", resolver.Principal.Identity.Name);
        }

        [Fact]
        public void ReadIdentityUsesOnlyTheSuppliedTransactionAndDoesNotSetItsUserService()
        {
            var database = NewDatabase();
            var userId = NewUser(database, "captured");
            using var transaction = database.CreateTransaction();
            var captured = ReadUserIdentity.Capture(new ClaimsPrincipal(new ClaimsIdentity("Tests")), new RecordingUserResolver(userId));

            var user = captured.Resolve(transaction);

            Assert.Same(transaction, user.Strategy.Transaction);
            Assert.Null(transaction.Services.Get<IUserService>().User);
            Assert.Empty(transaction.Checkpoint().Roles);
        }

        [Fact]
        public void ReadIdentityRequiresAResolver()
        {
            var principal = new ClaimsPrincipal(new ClaimsIdentity("Tests"));

            Assert.Throws<ArgumentNullException>(() => ReadUserIdentity.Capture(principal, null));
        }

        private static long NewUser(IDatabase database, string userName)
        {
            using var transaction = database.CreateTransaction();
            var person = new PersonBuilder(transaction).WithUserName(userName).Build();
            transaction.Derive();
            transaction.Commit();
            return person.Id;
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

        private sealed class RecordingUserResolver : IUserResolver
        {
            private readonly long userId;

            public RecordingUserResolver(long userId) => this.userId = userId;

            public ClaimsPrincipal Principal { get; private set; }

            public User Resolve(ClaimsPrincipal principal, ITransaction transaction)
            {
                this.Principal = principal;
                return (User)transaction.Instantiate(this.userId);
            }
        }

        private sealed class StubDatabaseService : IDatabaseService
        {
            public Func<IDatabase> Build { get; set; }

            public IDatabase Database { get; set; }
        }
    }
}
