// <copyright file="AllorsUserStoreTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Collections.Generic;
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
    using Microsoft.Extensions.Logging;
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

        // Reusing a person by email must not replace their credentials or retain any changes the
        // factory made in the store's transaction before returning the existing person.
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task CreateAsyncRefusesAnExistingUserWithoutChangingCredentialsOrPermissions(bool factoryChangesUser)
        {
            var database = NewDatabase();
            var lockoutEnd = new System.DateTime(2030, 1, 2, 3, 4, 5, DateTimeKind.Utc);
            long userId;
            long groupId;
            int groupCount;
            using (var transaction = database.CreateTransaction())
            {
                var user = new PersonBuilder(transaction).Build();
                user.UserName = "original-user";
                user.UserPasswordHash = "original-password-hash";
                user.UserEmail = "jane@example.com";
                user.UserEmailConfirmed = true;
                user.UserPhoneNumber = "+32000000001";
                user.UserPhoneNumberConfirmed = true;
                user.UserTwoFactorEnabled = true;
                user.UserLockoutEnd = lockoutEnd;
                user.UserLockoutEnabled = true;
                user.UserAccessFailedCount = 5;
                user.UserSecurityStamp = "original-security-stamp";
                var group = new UserGroupBuilder(transaction).WithName("Original access").WithMember(user).Build();
                transaction.Derive();
                transaction.Commit();
                userId = user.Id;
                groupId = group.Id;
                groupCount = new UserGroups(transaction).Extent().Count;
            }

            var factory = new ReusingUserFactory(factoryChangesUser);
            var logger = new RecordingLogger();
            var store = new AllorsUserStore(new StubDatabaseService { Database = database }, logger, factory);
            var identityUser = NewIdentityUser();
            identityUser.EmailConfirmed = false;
            identityUser.PhoneNumber = "+32000000002";
            var incomingId = identityUser.Id;

            var result = await store.CreateAsync(identityUser, CancellationToken.None);

            Assert.True(factory.ReturnedExistingUser);
            Assert.False(result.Succeeded);
            var error = Assert.Single(result.Errors);
            Assert.Equal("ExistingUserFromFactory", error.Code);
            Assert.Contains(nameof(IUserFactory), error.Description, StringComparison.Ordinal);
            Assert.Contains("new user or null", error.Description, StringComparison.Ordinal);
            Assert.Equal(incomingId, identityUser.Id);
            var log = Assert.Single(logger.Messages);
            Assert.Equal(LogLevel.Error, log.Level);
            Assert.Contains(nameof(IUserFactory), log.Message, StringComparison.Ordinal);
            Assert.Contains("existing user", log.Message, StringComparison.Ordinal);

            using var verification = database.CreateTransaction();
            var unchanged = (User)verification.Instantiate(userId);
            Assert.Equal("original-user", unchanged.UserName);
            Assert.Equal("ORIGINAL-USER", unchanged.NormalizedUserName);
            Assert.Equal("original-password-hash", unchanged.UserPasswordHash);
            Assert.Equal("jane@example.com", unchanged.UserEmail);
            Assert.Equal("JANE@EXAMPLE.COM", unchanged.NormalizedUserEmail);
            Assert.True(unchanged.UserEmailConfirmed);
            Assert.Equal("+32000000001", unchanged.UserPhoneNumber);
            Assert.True(unchanged.UserPhoneNumberConfirmed);
            Assert.True(unchanged.UserTwoFactorEnabled);
            Assert.Equal(lockoutEnd, unchanged.UserLockoutEnd);
            Assert.True(unchanged.UserLockoutEnabled);
            Assert.Equal(5, unchanged.UserAccessFailedCount);
            Assert.Equal("original-security-stamp", unchanged.UserSecurityStamp);
            Assert.Equal(groupId, Assert.Single(unchanged.UserGroupsWhereMember).Id);
            Assert.Equal(groupCount, new UserGroups(verification).Extent().Count);
            Assert.False(ExistsUser(database, identityUser.UserName));
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

        private sealed class ReusingUserFactory : IUserFactory
        {
            private readonly bool changesUser;

            public ReusingUserFactory(bool changesUser) => this.changesUser = changesUser;

            public bool ReturnedExistingUser { get; private set; }

            public User Create(ITransaction transaction, ClaimsPrincipal principal)
            {
                var m = transaction.Database.Services.Get<MetaPopulation>();
                var user = new Users(transaction).FindBy(m.User.UserEmail, principal.FindFirstValue(ClaimTypes.Email));
                this.ReturnedExistingUser = user != null && !user.Strategy.IsNewInTransaction;
                if (this.changesUser)
                {
                    user.UserEmailConfirmed = false;
                    new UserGroupBuilder(transaction).WithName("Factory access").WithMember(user).Build();
                }

                return user;
            }
        }

        private sealed class RecordingLogger : ILogger<AllorsUserStore>
        {
            public List<(LogLevel Level, string Message)> Messages { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
                this.Messages.Add((logLevel, formatter(state, exception)));
        }

        private sealed class StubDatabaseService : IDatabaseService
        {
            public Func<IDatabase> Build { get; set; }

            public IDatabase Database { get; set; }
        }
    }
}
