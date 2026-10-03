// <copyright file="AntiforgeryTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;

    // Core's antiforgery protects the Allors API against the session cookie the Entra plug-in signs a
    // browser in with, and leaves a bearer token alone: a token is not sent by the browser on its own.
    [Collection("Api")]
    public class AntiforgeryTests : ApiTest
    {
        [Fact]
        public async Task ASessionPostWithoutTheXsrfHeaderIs400()
        {
            var (browser, _) = NewBrowser();
            await SignInAsync(browser, FakeEntraAccounts.Tester);
            await browser.GetAsync("allors/Test/Ready");

            var response = await browser.PostAsync("allors/pull", EmptyPull());

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task ASessionPostWithTheXsrfHeaderSucceeds()
        {
            var (browser, jar) = NewBrowser();
            await SignInAsync(browser, FakeEntraAccounts.Tester);
            await browser.GetAsync("allors/Test/Ready");

            var request = new HttpRequestMessage(HttpMethod.Post, "allors/pull") { Content = EmptyPull() };
            request.Headers.Add("X-XSRF-TOKEN", Cookie(jar, "XSRF-TOKEN"));
            var response = await browser.SendAsync(request);

            Assert.True(response.IsSuccessStatusCode, $"A session POST with the XSRF header should succeed; was {(int)response.StatusCode}.");
        }

        [Fact]
        public async Task ABearerPostNeedsNoXsrfHeader()
        {
            var token = await this.PersonTokenAsync(FakeEntraAccounts.Tester);

            var response = await this.HttpClient.SendAsync(Bearer(HttpMethod.Post, "/allors/pull", token, EmptyPull()));

            Assert.True(response.IsSuccessStatusCode, $"A bearer POST without the XSRF header should succeed; was {(int)response.StatusCode}.");
        }
    }
}
