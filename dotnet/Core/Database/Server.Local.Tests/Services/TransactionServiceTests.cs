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
