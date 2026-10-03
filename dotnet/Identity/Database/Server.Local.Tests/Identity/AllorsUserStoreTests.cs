// <copyright file="AllorsUserStoreTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Allors.Security;
    using Allors.Services;
    using Microsoft.AspNetCore.Identity;
    using Xunit;
    using MemoryConfiguration = Allors.Database.Adapters.Memory.Configuration;
    using MemoryDatabase = Allors.Database.Adapters.Memory.Database;
    using User = Allors.Database.Domain.User;

    public class AllorsUserStoreTests
    {
        [Fact]
        public async void HasPasswordAsyncIsTrueWhenAPasswordHashIsSet()
        {
            var store = new AllorsUserStore(new StubDatabaseService());
            var user = new IdentityUser { PasswordHash = "a-hash" };

            var hasPassword = await store.HasPasswordAsync(user, CancellationToken.None);

            Assert.True(hasPassword);
        }

        [Fact]
        public async void HasPasswordAsyncIsFalseWhenNoPasswordHashIsSet()
        {
            var store = new AllorsUserStore(new StubDatabaseService());
            var user = new IdentityUser { PasswordHash = null };

            var hasPassword = await store.HasPasswordAsync(user, CancellationToken.None);

            Assert.False(hasPassword);
        }

        [Fact]
        public async Task CreateAsyncSucceedsForAUserWithAPasswordHash()
        {
            var store = NewStore();
            var identityUser = NewIdentityUser();

            var result = await store.CreateAsync(identityUser, CancellationToken.None);

            Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(v => v.Description)));
        }

        [Fact]
        public async Task CreateAsyncStoresThePasswordHash()
        {
            var store = NewStore();
            var identityUser = NewIdentityUser();

            await store.CreateAsync(identityUser, CancellationToken.None);
            var created = await store.FindByIdAsync(identityUser.Id, CancellationToken.None);

            Assert.Equal(identityUser.PasswordHash, created?.PasswordHash);
        }

        [Fact]
        public async Task CreateAsyncSucceedsForAUserWithoutAPasswordHash()
        {
            var store = NewStore();
            var identityUser = NewIdentityUser();
            identityUser.PasswordHash = null;

            var result = await store.CreateAsync(identityUser, CancellationToken.None);

            Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(v => v.Description)));
        }

        // The plug-in does not know the classes of the application's domain, so it creates no user
        // itself: the application's user factory does, and without one nobody is created.
        [Fact]
        public async Task CreateAsyncWithoutAUserFactoryFailsAndNamesTheSeam()
        {
            var database = NewDatabase();
            var store = new AllorsUserStore(new StubDatabaseService { Database = database });

            var result = await store.CreateAsync(NewIdentityUser(), CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, v => v.Description.Contains(nameof(IUserFactory), StringComparison.Ordinal));
            Assert.False(ExistsUser(database, "jane@example.com"));
        }

        [Fact]
        public async Task CreateAsyncFailsWhenTheApplicationDoesNotAdmitTheUser()
        {
            var database = NewDatabase();
            var store = new AllorsUserStore(new StubDatabaseService { Database = database }, userFactory: new RefusingUserFactory());

            var result = await store.CreateAsync(NewIdentityUser(), CancellationToken.None);

            Assert.False(result.Succeeded);
            Assert.False(ExistsUser(database, "jane@example.com"));
        }

        // What ASP.NET Core Identity knows about the person it is asked to store: a name and an
        // e-mail address. Nobody signed in, so the principal is not authenticated.
        [Fact]
        public async Task CreateAsyncTellsTheFactoryWhatIdentityKnowsAboutTheUser()
        {
            var factory = new PersonFactory();
            var store = new AllorsUserStore(new StubDatabaseService { Database = NewDatabase() }, userFactory: factory);

            await store.CreateAsync(NewIdentityUser(), CancellationToken.None);

            Assert.Equal("jane@example.com", factory.Principal?.FindFirstValue(ClaimTypes.Name));
            Assert.Equal("jane@example.com", factory.Principal?.FindFirstValue(ClaimTypes.Email));
            Assert.False(factory.Principal?.Identity?.IsAuthenticated);
        }

        // The build hooks of the Identity domain give a new user a security stamp and switch lockout
        // on. The store writes what ASP.NET Core Identity handed it, and must not undo either with
        // an IdentityUser that carries neither.
        [Fact]
        public async Task CreateAsyncLeavesANewUserWithASecurityStampAndLockoutEnabled()
        {
            var store = NewStore();
            var identityUser = NewIdentityUser();
            identityUser.SecurityStamp = null;
            identityUser.LockoutEnabled = false;

            await store.CreateAsync(identityUser, CancellationToken.None);
            var created = await store.FindByIdAsync(identityUser.Id, CancellationToken.None);

            Assert.False(string.IsNullOrEmpty(created?.SecurityStamp));
            Assert.True(created?.LockoutEnabled);
        }

        [Fact]
        public async Task CreateAsyncStoresTheSecurityStampOfTheIdentityUser()
        {
            var store = NewStore();
            var identityUser = NewIdentityUser();

            await store.CreateAsync(identityUser, CancellationToken.None);
            var created = await store.FindByIdAsync(identityUser.Id, CancellationToken.None);

            Assert.Equal("a-stamp", created?.SecurityStamp);
        }

        private static IdentityUser NewIdentityUser() =>
            new IdentityUser
            {
                UserName = "jane@example.com",
                PasswordHash = "a-hash",
                Email = "jane@example.com",
                EmailConfirmed = true,
                SecurityStamp = "a-stamp",
            };

        private static AllorsUserStore NewStore() => new AllorsUserStore(new StubDatabaseService { Database = NewDatabase() }, userFactory: new PersonFactory());

        private static bool ExistsUser(IDatabase database, string userName)
        {
            using var transaction = database.CreateTransaction();
            var m = database.Services.Get<MetaPopulation>();
            return new Users(transaction).FindBy(m.User.UserName, userName) != null;
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

        // The user factory of this tree's concrete domain, Test, whose users are people.
        private sealed class PersonFactory : IUserFactory
        {
            public ClaimsPrincipal Principal { get; private set; }

            public User Create(ITransaction transaction, ClaimsPrincipal principal)
            {
                this.Principal = principal;
                return new PersonBuilder(transaction).Build();
            }
        }

        private sealed class RefusingUserFactory : IUserFactory
        {
            public User Create(ITransaction transaction, ClaimsPrincipal principal) => null;
        }

        private sealed class StubDatabaseService : IDatabaseService
        {
            public Func<IDatabase> Build { get; set; }

            public IDatabase Database { get; set; }
        }
    }
}
