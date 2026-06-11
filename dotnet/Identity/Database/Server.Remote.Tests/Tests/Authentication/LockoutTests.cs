// <copyright file="LockoutTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using Database.Domain;
    using Protocol.Json.Auth;
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
        public async void LockedOutAfterFailedAttempts()
        {
            var tokenUri = new Uri("Authentication/Token", UriKind.Relative);

            // Default Identity lockout is 5 failed attempts.
            for (var i = 0; i < 5; i++)
            {
                await this.PostAsJsonAsync(tokenUri, new AuthenticationTokenRequest { l = "Jane", p = "wrong" });
            }

            // The account is now locked: even the correct password is rejected.
            var response = await this.PostAsJsonAsync(tokenUri, new AuthenticationTokenRequest { l = "Jane", p = "p@ssw0rd" });
            var signInResponse = await this.ReadAsAsync<AuthenticationTokenResponse>(response);

            Assert.False(signInResponse.a);
        }
    }
}
