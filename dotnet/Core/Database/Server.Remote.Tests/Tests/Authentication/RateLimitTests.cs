// <copyright file="RateLimitTests.cs" company="Allors bv">
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
    public class RateLimitTests : ApiTest
    {
        [Fact]
        public async Task LoopbackClientsHaveHeadroomForTestLoops()
        {
            for (var i = 0; i < 15; i++)
            {
                var response = await this.RequestRateLimitedAuthPathAsync();
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task RemoteClientsAreLimitedPerForwardedIp()
        {
            // Loopback is a trusted proxy (UseForwardedHeaders), so a forwarded address becomes the
            // client IP the limiter partitions on. A unique address keeps this test's partition to itself.
            var forwardedIp = $"203.0.113.{Random.Shared.Next(1, 254)}";

            for (var i = 0; i < 10; i++)
            {
                var response = await this.RequestRateLimitedAuthPathAsync(forwardedIp);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            var limited = await this.RequestRateLimitedAuthPathAsync(forwardedIp);
            Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        }

        private async Task<HttpResponseMessage> RequestRateLimitedAuthPathAsync(string forwardedIp = null)
        {
            // The Identity login page is on the rate-limiter's auth-path list; a GET returns 200 unless
            // the partition is exhausted (429). It is site-root (not under /allors), hence the absolute URL.
            using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5000/Identity/Account/Login");

            if (forwardedIp != null)
            {
                request.Headers.Add("X-Forwarded-For", forwardedIp);
            }

            return await this.HttpClient.SendAsync(request);
        }
    }
}
