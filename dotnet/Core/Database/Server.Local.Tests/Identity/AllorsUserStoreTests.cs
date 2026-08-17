// <copyright file="AllorsUserStoreTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Linq;
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

        private static IdentityUser NewIdentityUser() =>
            new IdentityUser
            {
                UserName = "jane@example.com",
                PasswordHash = "a-hash",
                Email = "jane@example.com",
                EmailConfirmed = true,
                SecurityStamp = "a-stamp",
            };

        private static AllorsUserStore NewStore() => new AllorsUserStore(new StubDatabaseService { Database = NewDatabase() });

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

        private sealed class StubDatabaseService : IDatabaseService
        {
            public Func<IDatabase> Build { get; set; }

            public IDatabase Database { get; set; }
        }
    }
}
