// <copyright file="RateLimitTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Allors.Protocol.Json.Auth;
    using Xunit;

    [Collection("Api")]
    public class RateLimitTests : ApiTest
    {
        [Fact]
        public async Task LoopbackClientsHaveHeadroomForTestLoops()
        {
            for (var i = 0; i < 15; i++)
            {
                var response = await this.PostTokenRequestAsync();
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
                var response = await this.PostTokenRequestAsync(forwardedIp);
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }

            var limited = await this.PostTokenRequestAsync(forwardedIp);
            Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        }

        private async Task<HttpResponseMessage> PostTokenRequestAsync(string forwardedIp = null)
        {
            var args = new AuthenticationTokenRequest { l = "jane@example.com" };
            using var request = new HttpRequestMessage(HttpMethod.Post, new Uri("TestAuthentication/Token", UriKind.Relative))
            {
                Content = new StringContent(JsonSerializer.Serialize(args), Encoding.UTF8, "application/json"),
            };

            if (forwardedIp != null)
            {
                request.Headers.Add("X-Forwarded-For", forwardedIp);
            }

            return await this.HttpClient.SendAsync(request);
        }
    }
}
