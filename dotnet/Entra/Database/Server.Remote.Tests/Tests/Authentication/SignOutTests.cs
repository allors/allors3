// <copyright file="SignOutTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;

    // The sign-out endpoint ends the session and the sign-in with Entra, for a request that carries the
    // antiforgery token, as the sign-out is a POST a page of another site could otherwise make.
    [Collection("Api")]
    public class SignOutTests : ApiTest
    {
        [Fact]
        public async Task SignOutEndsTheSessionAndTheSignInWithEntra()
        {
            var (browser, jar) = NewBrowser();
            await SignInAsync(browser, FakeEntraAccounts.Tester);
            await browser.GetAsync("allors/Test/Ready");

            var signOut = new HttpRequestMessage(HttpMethod.Post, EntraPaths.SignOut.TrimStart('/'));
            signOut.Headers.Add("X-XSRF-TOKEN", Cookie(jar, "XSRF-TOKEN"));
            var response = await browser.SendAsync(signOut);

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Equal($"{FakeEntra.PathPrefix}/{this.TenantId}/oauth2/v2.0/logout", response.Headers.Location?.AbsolutePath);

            var api = await browser.GetAsync("allors/UserInfo");
            Assert.Equal(HttpStatusCode.Unauthorized, api.StatusCode);
        }

        [Fact]
        public async Task SignOutWithoutTheAntiforgeryTokenIs400()
        {
            var (browser, _) = NewBrowser();
            await SignInAsync(browser, FakeEntraAccounts.Tester);

            var response = await browser.PostAsync(EntraPaths.SignOut.TrimStart('/'), null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await browser.GetAsync("allors/UserInfo")).StatusCode);
        }
    }
}
