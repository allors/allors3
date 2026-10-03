// <copyright file="SignInTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using Database.Domain;
    using Microsoft.AspNetCore.WebUtilities;
    using Xunit;

    // A browser signs in with Entra through the plug-in's sign-in endpoint, Core's session rules and
    // Microsoft.Identity.Web, and the application's factory creates the user at the first sign-in.
    [Collection("Api")]
    public class SignInTests : ApiTest
    {
        // A caller of the Allors API gets a status code, never a redirect to Entra, and no cookie.
        [Fact]
        public async Task AnAnonymousApiRequestGets401WithoutARedirectOrACookie()
        {
            var (browser, jar) = NewBrowser();

            var response = await browser.GetAsync("allors/UserInfo");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Null(response.Headers.Location);
            Assert.Equal(0, jar.Count);
        }

        // The sign-in endpoint challenges Entra: the authorization code flow with PKCE, the code coming
        // back in the query, and the cookies of the sign-in Lax, so that a browser over http in
        // Development sends them.
        [Fact]
        public async Task TheSignInEndpointSendsTheBrowserToEntraWithTheCodeFlow()
        {
            var (browser, jar) = NewBrowser();

            var response = await browser.GetAsync("entra/sign-in?returnUrl=%2Fallors%2FUserInfo");

            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            var location = response.Headers.Location;
            Assert.Equal($"{FakeEntra.PathPrefix}/{this.TenantId}/oauth2/v2.0/authorize", location.AbsolutePath);
            var query = QueryHelpers.ParseQuery(location.Query);
            Assert.Equal("code", query["response_type"]);
            Assert.Equal("S256", query["code_challenge_method"]);
            Assert.Equal(this.ClientId, query["client_id"]);
            Assert.Equal(Origin + "/signin-oidc", query["redirect_uri"]);
            Assert.Contains("openid", query["scope"].ToString(), StringComparison.Ordinal);
            Assert.False(query.ContainsKey("response_mode"));

            var cookies = response.Headers.GetValues("Set-Cookie").ToArray();
            Assert.Equal(2, cookies.Length);
            Assert.All(cookies, v => Assert.Contains("samesite=lax", v, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task TheFirstSignInCreatesThePersonWithItsEntraFields()
        {
            var (browser, jar) = NewBrowser();

            var response = await SignInAsync(browser, FakeEntraAccounts.Tester);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var userInfo = await JsonAsync(response);
            var user = this.FindUser(FakeEntraAccounts.Tester);
            Assert.IsType<Person>(user);
            Assert.Equal(user.Id.ToString(), userInfo.GetProperty("u").GetString());
            Assert.Equal(FakeEntraAccounts.Tester.UserName, userInfo.GetProperty("userName").GetString());
            Assert.Equal(FakeEntraAccounts.Tester.UserName, user.EntraUserName);
            Assert.Equal(FakeEntraAccounts.Tester.DisplayName, user.EntraDisplayName);
            Assert.Equal(FakeEntraAccounts.Tester.Email, user.EntraEmail);
            Assert.Equal(FakeEntra.IssuerV2(this.TenantId), user.EntraIdentityProvider);
            Assert.False(user.EntraIsGuest);

            // Core's session cookie, and no cookie of the sign-in left behind.
            Assert.NotNull(Cookie(jar, "Allors.Auth"));
            Assert.DoesNotContain(jar.GetAllCookies(), v => v.Name.Contains("Correlation", StringComparison.Ordinal) && !v.Expired);
        }

        [Fact]
        public async Task ASecondSignInFindsTheSameUser()
        {
            var (first, _) = NewBrowser();
            var (second, _) = NewBrowser();

            var firstInfo = await JsonAsync(await SignInAsync(first, FakeEntraAccounts.Tester));
            var secondInfo = await JsonAsync(await SignInAsync(second, FakeEntraAccounts.Tester));

            Assert.Equal(firstInfo.GetProperty("u").GetString(), secondInfo.GetProperty("u").GetString());
            Assert.Single(this.AllUsers());
        }

        // A guest of the tenant, invited from the customer's tenant, is a person like the others; the
        // directory says where it comes from.
        [Fact]
        public async Task AGuestIsAPersonWithItsHomeAndItsStatus()
        {
            var (browser, _) = NewBrowser();

            var response = await SignInAsync(browser, FakeEntraAccounts.Guest);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var user = this.FindUser(FakeEntraAccounts.Guest);
            Assert.IsType<Person>(user);
            Assert.Equal(FakeEntraAccounts.Guest.UserName, user.EntraUserName);
            Assert.True(user.EntraIsGuest);
            Assert.Contains(FakeEntraAccounts.CustomerTenantId, user.EntraIdentityProvider, StringComparison.OrdinalIgnoreCase);
        }

        // The application decides whom it admits: an account its factory refuses is told so, gets no
        // session, and leaves no user.
        [Fact]
        public async Task ARefusedAccountGets403WithoutASessionOrAUser()
        {
            var (browser, jar) = NewBrowser();

            var response = await SignInAsync(browser, FakeEntraAccounts.Refused);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
            Assert.Null(Cookie(jar, "Allors.Auth"));
            Assert.Null(this.FindUser(FakeEntraAccounts.Refused));

            var api = await browser.GetAsync("allors/UserInfo");
            Assert.Equal(HttpStatusCode.Unauthorized, api.StatusCode);
        }

        // Signed in, the browser goes to the returnUrl of this site only.
        [Fact]
        public async Task TheReturnUrlMustBeLocal()
        {
            var (browser, _) = NewBrowser();
            await SignInAsync(browser, FakeEntraAccounts.Tester);

            var local = await browser.GetAsync("entra/sign-in?returnUrl=%2Fallors%2FTest%2FReady");
            Assert.Equal(HttpStatusCode.Redirect, local.StatusCode);
            Assert.Equal("/allors/Test/Ready", local.Headers.Location?.ToString());

            var elsewhere = await browser.GetAsync("entra/sign-in?returnUrl=" + Uri.EscapeDataString("https://example.com/"));
            Assert.Equal(HttpStatusCode.BadRequest, elsewhere.StatusCode);

            var protocolRelative = await browser.GetAsync("entra/sign-in?returnUrl=" + Uri.EscapeDataString("//example.com/"));
            Assert.Equal(HttpStatusCode.BadRequest, protocolRelative.StatusCode);
        }
    }
}
