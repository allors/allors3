// <copyright file="AuthenticationSchemeTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using Allors.Protocol.Json.Api.Read;
    using Allors.Protocol.Json.SystemTextJson;
    using Xunit;

    [Collection("Api")]
    public class AuthenticationSchemeTests : ApiTest
    {
        private const string ReadRequestJson = "{\"v\":1,\"i\":\"named-route-security\",\"k\":\"read\",\"p\":{\"l\":[],\"d\":[]}}";

        [Fact]
        public async Task AnonymousAuthorizedEndpointReturns401NotRedirect()
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                BaseAddress = new Uri(Url),
            };

            var response = await client.PostAsync(new Uri("read", UriKind.Relative), CreateReadContent());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.False(response.Headers.Contains("Location"), "An /allors request must get a raw 401, not a login-page redirect.");
        }

        [Fact]
        public async Task JwtTokenEndpointIsGone()
        {
            // Authenticate first, so the default-deny fallback is satisfied and the request reaches
            // routing: the deleted route then 404s (an anonymous probe would be challenged with 401
            // by the fallback before routing, which would not prove the endpoint is gone).
            await this.SignIn(this.Administrator);

            var response = await this.HttpClient.PostAsync(new Uri("Authentication/Token", UriKind.Relative), null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Theory]
        [InlineData("Organisations/Pull")]
        [InlineData("People/Pull")]
        [InlineData("PeopleSheet/Pull")]
        [InlineData("TestEmployees/Pull")]
        [InlineData("TestHome/Pull")]
        [InlineData("TestNoTree/Pull")]
        [InlineData("TestPull/Pull")]
        [InlineData("TestShareHolders/Pull")]
        public async Task NamedWorkspaceQueryRoutesAreGone(string route)
        {
            await this.SignIn(this.Administrator);

            var readResponse = await this.HttpClient.PostAsync(new Uri("read", UriKind.Relative), CreateReadContent());
            await this.AssertCompleteReadAsync(readResponse);

            var response = await this.HttpClient.PostAsync(new Uri(route, UriKind.Relative), null);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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
