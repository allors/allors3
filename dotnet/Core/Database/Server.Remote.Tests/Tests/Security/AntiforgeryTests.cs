// <copyright file="AntiforgeryTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;

    // Core's antiforgery for the Allors API: a safe request hands out the token cookie, and a client
    // that does not sign in with a cookie is exempt. The cookie cases run in the Identity tree,
    // against the plug-in that signs in with one.
    [Collection("Api")]
    public class AntiforgeryTests : ApiTest
    {
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
    }
}
