// <copyright file="AllorsUserStoreTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using System;
    using System.Threading;
    using Allors.Database;
    using Allors.Security;
    using Allors.Services;
    using Microsoft.AspNetCore.Identity;
    using Xunit;

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

        private sealed class StubDatabaseService : IDatabaseService
        {
            public Func<IDatabase> Build { get; set; }

            public IDatabase Database { get; set; }
        }
    }
}
