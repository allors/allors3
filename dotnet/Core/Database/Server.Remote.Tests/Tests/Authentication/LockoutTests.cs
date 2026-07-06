// <copyright file="LockoutTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System.Threading.Tasks;
    using Database.Domain;
    using Xunit;

    [Collection("Api")]
    public class LockoutTests : ApiTest
    {
        public LockoutTests()
        {
            var jane = new PersonBuilder(this.Transaction).WithUserName("Jane").Build();
            jane.UserLockoutEnabled = true;
            jane.SetPassword("p@ssw0rd");

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        [Fact]
        public async Task LockedOutAfterFailedAttempts()
        {
            // The configured lockout threshold (AllorsServerServiceCollectionExtensions) is 10 failed attempts.
            for (var i = 0; i < 10; i++)
            {
                await this.CookieLoginSucceedsAsync("Jane", "wrong");
            }

            // The account is now locked: even the correct password no longer authenticates.
            Assert.False(await this.CookieLoginSucceedsAsync("Jane", "p@ssw0rd"));
        }
    }
}
