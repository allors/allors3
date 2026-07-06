// <copyright file="AntiforgeryTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Database.Domain;
    using Xunit;

    [Collection("Api")]
    public class AntiforgeryTests : ApiTest
    {
        public AntiforgeryTests()
        {
            var user = new PersonBuilder(this.Transaction).WithUserName("xsrfuser").Build();
            user.SetPassword("xsrf-password");

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        [Fact]
        public async Task SafeAllorsRequestIssuesXsrfCookie()
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false })
            {
                BaseAddress = new Uri(Url),
            };

            var response = await client.GetAsync(new Uri("Test/Ready", UriKind.Relative));

            Assert.True(response.IsSuccessStatusCode);
            var setCookies = response.Headers.TryGetValues("Set-Cookie", out var values) ? values : Enumerable.Empty<string>();
            Assert.Contains(setCookies, v => v.StartsWith("XSRF-TOKEN=", StringComparison.Ordinal));
        }

        [Fact]
        public async Task TestHeaderPostWithoutXsrfHeaderSucceeds()
        {
            await this.SignIn(this.Administrator);

            var response = await this.HttpClient.PostAsync(new Uri("Organisations/Pull", UriKind.Relative), null);

            Assert.True(response.IsSuccessStatusCode, $"Non-cookie (test-header) clients are antiforgery-exempt; was {(int)response.StatusCode}.");
        }

        [Fact]
        public async Task CookiePostWithoutXsrfHeaderReturns400()
        {
            // The cookie client's base address is the site root (for /Identity), so /allors is explicit here.
            var client = await this.SignInWithCookieAsync("xsrfuser", "xsrf-password");
            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));

            var response = await client.PostAsync(new Uri("allors/Organisations/Pull", UriKind.Relative), null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task CookiePostWithXsrfHeaderSucceeds()
        {
            var client = await this.SignInWithCookieAsync("xsrfuser", "xsrf-password");

            var safeResponse = await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));
            var xsrf = ExtractSetCookieValue(safeResponse, "XSRF-TOKEN");

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("allors/Organisations/Pull", UriKind.Relative));
            request.Headers.Add("X-XSRF-TOKEN", xsrf);
            var response = await client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode, $"Cookie POST with the XSRF header should succeed; was {(int)response.StatusCode}.");
        }

        [Fact]
        public async Task CookiePostAfterAnonymousFirstVisitSucceeds()
        {
            var (client, jar) = CreateCookieClient();

            // The SPA's bootstrap GET mints a token while still anonymous; the token the browser
            // holds after the login redirect must validate for the now-authenticated user.
            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));
            await LoginWithCookieAsync(client, "xsrfuser", "xsrf-password");
            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));

            var xsrf = JarXsrfToken(jar);
            Assert.False(string.IsNullOrEmpty(xsrf), "A readable XSRF-TOKEN cookie should be present after login.");

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("allors/Organisations/Pull", UriKind.Relative));
            request.Headers.Add("X-XSRF-TOKEN", xsrf);
            var response = await client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode, $"Cookie POST after an anonymous first visit should succeed; was {(int)response.StatusCode}.");
        }

        [Fact]
        public async Task LogoutAndReloginRotateTheXsrfToken()
        {
            var (client, jar) = CreateCookieClient();

            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));
            await LoginWithCookieAsync(client, "xsrfuser", "xsrf-password");
            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));

            // The Angular LogoutService shape: a POST with the XSRF header and no form body.
            var logout = new HttpRequestMessage(HttpMethod.Post, new Uri("Identity/Account/Logout", UriKind.Relative));
            logout.Headers.Add("X-XSRF-TOKEN", JarXsrfToken(jar));
            var logoutResponse = await client.SendAsync(logout);
            Assert.Equal(HttpStatusCode.Redirect, logoutResponse.StatusCode);

            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));
            await LoginWithCookieAsync(client, "xsrfuser", "xsrf-password");
            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("allors/Organisations/Pull", UriKind.Relative));
            request.Headers.Add("X-XSRF-TOKEN", JarXsrfToken(jar));
            var response = await client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode, $"Cookie POST after logout and re-login should succeed; was {(int)response.StatusCode}.");
        }

        [Fact]
        public async Task CorruptedXsrfTokenSelfHeals()
        {
            var (client, jar) = CreateCookieClient();

            await LoginWithCookieAsync(client, "xsrfuser", "xsrf-password");
            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));

            // A token that no longer validates (corruption, data-protection key loss) must not
            // wedge the session: the rejecting 400 drops it and the next safe GET re-mints.
            jar.Add(new Cookie("XSRF-TOKEN", "garbage", "/", "localhost"));
            var poisoned = new HttpRequestMessage(HttpMethod.Post, new Uri("allors/Organisations/Pull", UriKind.Relative));
            poisoned.Headers.Add("X-XSRF-TOKEN", "garbage");
            var poisonedResponse = await client.SendAsync(poisoned);
            Assert.Equal(HttpStatusCode.BadRequest, poisonedResponse.StatusCode);

            await client.GetAsync(new Uri("allors/Test/Ready", UriKind.Relative));

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri("allors/Organisations/Pull", UriKind.Relative));
            request.Headers.Add("X-XSRF-TOKEN", JarXsrfToken(jar));
            var response = await client.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode, $"Cookie POST after antiforgery self-heal should succeed; was {(int)response.StatusCode}.");
        }

        private static (HttpClient Client, CookieContainer Jar) CreateCookieClient()
        {
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer(),
                AllowAutoRedirect = false,
            };
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5000/") };
            return (client, handler.CookieContainer);
        }

        // The jar is what Angular's XSRF interceptor sees (document.cookie), unlike a single
        // response's Set-Cookie header.
        private static string JarXsrfToken(CookieContainer jar) =>
            jar.GetCookies(new Uri("http://localhost:5000/"))["XSRF-TOKEN"]?.Value;

        private static string ExtractSetCookieValue(HttpResponseMessage response, string name)
        {
            var setCookies = response.Headers.TryGetValues("Set-Cookie", out var values) ? values : Enumerable.Empty<string>();
            var cookie = setCookies.FirstOrDefault(v => v.StartsWith(name + "=", StringComparison.Ordinal));
            return cookie == null ? null : Regex.Match(cookie, name + "=([^;]+)").Groups[1].Value;
        }
    }
}
