// <copyright file="SignOutTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using Database.Domain;
    using Protocol.Json.Auth;
    using Xunit;

    [Collection("Api")]
    public class SignOutTests : ApiTest
    {
        public SignOutTests()
        {
            new PersonBuilder(this.Transaction).WithUserName("user").Build().SetPassword("p@ssw0rd");
            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        [Fact]
        public async void Successful()
        {
            var args = new AuthenticationTokenRequest
            {
                l = "user",
                p = "p@ssw0rd",
            };

            var signInUri = new Uri("Authentication/Token", UriKind.Relative);
            await this.PostAsJsonAsync(signInUri, args);

            var signOutUri = new Uri("Authentication/SignOut", UriKind.Relative);
            await this.PostAsJsonAsync(signOutUri, null);
        }
    }
}
