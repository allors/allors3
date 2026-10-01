// <copyright file="DisabledUserTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Database.Domain;
    using Xunit;

    [Collection("Api")]
    public class DisabledUserTests : ApiTest
    {
        [Fact]
        public async Task DisablingAUserInvalidatesItsCookie()
        {
            var user = new PersonBuilder(this.Transaction).WithUserName("disable-me").Build();
            user.SetPassword("p@ssw0rd");
            this.Transaction.Derive();
            this.Transaction.Commit();

            var client = await this.SignInWithCookieAsync("disable-me", "p@ssw0rd");

            var before = await client.GetAsync(new Uri("allors/UserInfo", UriKind.Relative));
            Assert.Equal(HttpStatusCode.OK, before.StatusCode);

            // Disable the user out-of-band; the UserIsDisabledRule rotates its security stamp.
            user.IsDisabled = true;
            this.Transaction.Derive();
            this.Transaction.Commit();

            // Development revalidates the stamp every request, so the now-stale cookie is rejected.
            var after = await client.GetAsync(new Uri("allors/UserInfo", UriKind.Relative));
            Assert.Equal(HttpStatusCode.Unauthorized, after.StatusCode);
        }
    }
}
