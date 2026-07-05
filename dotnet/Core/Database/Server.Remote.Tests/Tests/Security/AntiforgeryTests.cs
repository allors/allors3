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
        public async Task BearerPostWithoutXsrfHeaderSucceeds()
        {
            await this.SignIn(this.Administrator);

            var response = await this.HttpClient.PostAsync(new Uri("Organisations/Pull", UriKind.Relative), null);

            Assert.True(response.IsSuccessStatusCode, $"Bearer clients are antiforgery-exempt; was {(int)response.StatusCode}.");
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

        private static string ExtractSetCookieValue(HttpResponseMessage response, string name)
        {
            var setCookies = response.Headers.TryGetValues("Set-Cookie", out var values) ? values : Enumerable.Empty<string>();
            var cookie = setCookies.FirstOrDefault(v => v.StartsWith(name + "=", StringComparison.Ordinal));
            return cookie == null ? null : Regex.Match(cookie, name + "=([^;]+)").Groups[1].Value;
        }
    }
}
