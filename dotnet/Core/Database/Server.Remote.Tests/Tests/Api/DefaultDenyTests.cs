// <copyright file="DefaultDenyTests.cs" company="Allors bv">
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

    [Collection("Api")]
    public class DefaultDenyTests : ApiTest
    {
        // The JSON API endpoints carry no authorization attributes; they are protected solely by the
        // server's default-deny fallback policy. An anonymous request therefore proves the fallback.
        [Theory]
        [InlineData("pull")]
        [InlineData("push")]
        [InlineData("sync")]
        [InlineData("invoke")]
        [InlineData("access")]
        [InlineData("permission")]
        public async Task AnonymousJsonApiIsDenied(string endpoint)
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                BaseAddress = new Uri(Url),
            };

            var response = await client.PostAsync(new Uri(endpoint, UriKind.Relative), null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // The test-harness controllers must opt out of the fallback ([AllowAnonymous]) so Nuke, the
        // e2e runner, jest and the remote suites can reset/populate/sign in without a credential.
        [Fact]
        public async Task AnonymousTestEndpointStaysAccessible()
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                BaseAddress = new Uri(Url),
            };

            var response = await client.GetAsync(new Uri("Test/Ready", UriKind.Relative));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // An authenticated (bearer, during the dual-scheme window) request passes the fallback: the
        // policy authorizes it, so it is never rejected with 401.
        [Fact]
        public async Task AuthenticatedJsonApiIsAuthorized()
        {
            await this.SignIn(this.Administrator);

            var content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
            var response = await this.HttpClient.PostAsync(new Uri("pull", UriKind.Relative), content);

            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
