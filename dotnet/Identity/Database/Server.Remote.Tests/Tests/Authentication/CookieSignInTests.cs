// <copyright file="CookieSignInTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("Api")]
    public class CookieSignInTests : ApiTest
    {
        [Fact]
        public async Task SignInIssuesAnAuthCookieThatAuthenticatesUserInfo()
        {
            var handler = new HttpClientHandler { UseCookies = true, CookieContainer = new CookieContainer(), AllowAutoRedirect = false };
            using var client = new HttpClient(handler) { BaseAddress = new Uri(Url) };

            var signInResponse = await client.PostAsJsonAsync(new Uri("TestAuthentication/SignIn", UriKind.Relative), new { l = "jane@example.com" });
            Assert.Equal(HttpStatusCode.OK, signInResponse.StatusCode);

            var authCookie = handler.CookieContainer.GetCookies(new Uri(Url));
            Assert.Contains(authCookie, c => c.Name == "Allors.Auth");

            // The cookie alone (no bearer) authenticates a subsequent request.
            var userInfoResponse = await client.GetAsync(new Uri("UserInfo", UriKind.Relative));
            Assert.Equal(HttpStatusCode.OK, userInfoResponse.StatusCode);

            var body = await userInfoResponse.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(body);
            Assert.Equal(this.Administrator.Id.ToString(), document.RootElement.GetProperty("u").GetString());
        }

        [Fact]
        public async Task SignInWithUnknownUserIsDenied()
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = new Uri(Url) };

            var response = await client.PostAsJsonAsync(new Uri("TestAuthentication/SignIn", UriKind.Relative), new { l = "does-not-exist" });

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
