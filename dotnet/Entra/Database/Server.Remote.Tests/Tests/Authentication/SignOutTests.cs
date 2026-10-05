// <copyright file="SignOutTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
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

        [Theory]
        [InlineData(null, false)]
        [InlineData(null, true)]
        [InlineData("invalid-ticket", false)]
        [InlineData("invalid-ticket", true)]
        public async Task SignOutWithoutASessionIsANoOp(string sessionCookie, bool includeToken)
        {
            var (browser, jar) = NewBrowser();
            if (sessionCookie != null)
            {
                jar.Add(new Uri(Origin), new Cookie("Allors.Auth", sessionCookie, "/"));
            }

            var request = new HttpRequestMessage(HttpMethod.Post, EntraPaths.SignOut);
            if (includeToken)
            {
                request.Headers.Add("X-XSRF-TOKEN", "an-old-token");
            }

            var response = await browser.SendAsync(request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Null(response.Headers.Location);
            Assert.False(response.Headers.Contains("Set-Cookie"));
            Assert.Empty(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.Unauthorized, (await browser.GetAsync("allors/UserInfo")).StatusCode);
            Assert.Empty(this.AllUsers());
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task SignOutWithAnExpiredSessionIsANoOp(bool absoluteLifetime, bool includeToken)
        {
            var (browser, jar) = NewBrowser();
            Assert.Equal(HttpStatusCode.OK, (await SignInAsync(browser, FakeEntraAccounts.Tester)).StatusCode);
            var token = Cookie(jar, "XSRF-TOKEN");
            Assert.False(string.IsNullOrEmpty(token));
            ExpireSession(jar, absoluteLifetime);

            var request = new HttpRequestMessage(HttpMethod.Post, EntraPaths.SignOut);
            if (includeToken)
            {
                request.Headers.Add("X-XSRF-TOKEN", token);
            }

            var response = await browser.SendAsync(request);

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Null(response.Headers.Location);
            Assert.Empty(await response.Content.ReadAsStringAsync());
            Assert.Equal(HttpStatusCode.Unauthorized, (await browser.GetAsync("allors/UserInfo")).StatusCode);
            Assert.Single(this.AllUsers());
        }

        [Fact]
        public async Task SignOutWithOnlyABearerTokenIsANoOp()
        {
            var (browser, jar) = NewBrowser();
            var token = await this.PersonTokenAsync(FakeEntraAccounts.Tester);
            var response = await browser.SendAsync(Bearer(HttpMethod.Post, EntraPaths.SignOut, token));

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            Assert.Null(response.Headers.Location);
            Assert.False(response.Headers.Contains("Set-Cookie"));
            Assert.Null(Cookie(jar, "Allors.Auth"));
            Assert.Equal(HttpStatusCode.Unauthorized, (await browser.GetAsync("allors/UserInfo")).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await browser.SendAsync(Bearer(HttpMethod.Get, "/allors/UserInfo", token))).StatusCode);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SignOutWithABearerHeaderStillProtectsTheSession(bool includeToken)
        {
            var (browser, jar) = NewBrowser();
            Assert.Equal(HttpStatusCode.OK, (await SignInAsync(browser, FakeEntraAccounts.Tester)).StatusCode);
            var request = new HttpRequestMessage(HttpMethod.Post, EntraPaths.SignOut);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await this.PersonTokenAsync(FakeEntraAccounts.Guest));
            if (includeToken)
            {
                request.Headers.Add("X-XSRF-TOKEN", Cookie(jar, "XSRF-TOKEN"));
            }

            var response = await browser.SendAsync(request);

            Assert.Equal(includeToken ? HttpStatusCode.Redirect : HttpStatusCode.BadRequest, response.StatusCode);
            if (includeToken)
            {
                Assert.Equal($"{FakeEntra.PathPrefix}/{this.TenantId}/oauth2/v2.0/logout", response.Headers.Location?.AbsolutePath);
            }
            else
            {
                Assert.Null(response.Headers.Location);
            }

            Assert.Equal(includeToken ? HttpStatusCode.Unauthorized : HttpStatusCode.OK, (await browser.GetAsync("allors/UserInfo")).StatusCode);
        }

        // The published test server and its tests run under the same account with the same default
        // key ring. Use the cookie handler's own format and application name to expire a real ticket,
        // without adding an endpoint that changes the server's clock or authentication state.
        private static void ExpireSession(CookieContainer jar, bool absoluteLifetime)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDataProtection().SetApplicationName("Allors.Entra");
            services.AddAuthentication().AddCookie(EntraDefaults.SessionScheme);
            using var provider = services.BuildServiceProvider();
            var format = provider.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>().Get(EntraDefaults.SessionScheme).TicketDataFormat;
            var ticket = format.Unprotect(Cookie(jar, "Allors.Auth"));
            Assert.NotNull(ticket);
            if (absoluteLifetime)
            {
                ticket.Properties.Items[AllorsSessionCookie.SessionStartKey] = DateTimeOffset.UtcNow.AddDays(-2).ToString("o", CultureInfo.InvariantCulture);
                Assert.True(ticket.Properties.ExpiresUtc > DateTimeOffset.UtcNow);
            }
            else
            {
                ticket.Properties.IssuedUtc = DateTimeOffset.UtcNow.AddDays(-2);
                ticket.Properties.ExpiresUtc = DateTimeOffset.UtcNow.AddDays(-1);
            }

            jar.Add(new Uri(Origin), new Cookie("Allors.Auth", format.Protect(ticket), "/"));
        }
    }
}
