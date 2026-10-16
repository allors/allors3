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
    using System.Text;
    using System.Threading.Tasks;
    using Allors.Protocol.Json.Api.Read;
    using Allors.Protocol.Json.SystemTextJson;
    using Xunit;

    // Core's antiforgery for the Allors API: a safe request hands out the token cookie, and a client
    // that does not sign in with a cookie is exempt. The cookie cases run in the Identity tree,
    // against the plug-in that signs in with one.
    [Collection("Api")]
    public class AntiforgeryTests : ApiTest
    {
        private const string ReadRequestJson = "{\"v\":1,\"i\":\"named-route-security\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}";

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

            var response = await this.HttpClient.PostAsync(new Uri("read", UriKind.Relative), CreateReadContent());

            Assert.True(response.IsSuccessStatusCode, $"Non-cookie (test-header) clients are antiforgery-exempt; was {(int)response.StatusCode}.");
            await this.AssertCompleteReadAsync(response);
        }

        private static StringContent CreateReadContent() => new StringContent(ReadRequestJson, Encoding.UTF8, "application/json");

        private async Task AssertCompleteReadAsync(HttpResponseMessage response)
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = Assert.IsType<ReadSuccess>(ReadJson.DeserializeResponse(await response.Content.ReadAsStringAsync(), ReadJson.DeserializeRequest(ReadRequestJson)));
            Assert.Equal("named-route-security", result.i);
            Assert.Equal("complete", result.k);
            Assert.Equal("Default", result.sc.w);
            Assert.Equal("core-read-v1", result.sc.r);
            Assert.Equal(this.Administrator.Id, result.sc.u);
            Assert.Empty(result.l);
            Assert.Empty(result.o);
            Assert.Empty(result.c);
            Assert.Empty(result.p);
        }
    }
}
