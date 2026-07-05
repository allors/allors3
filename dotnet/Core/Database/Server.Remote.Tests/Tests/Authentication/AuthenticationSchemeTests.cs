// <copyright file="AuthenticationSchemeTests.cs" company="Allors bv">
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
    public class AuthenticationSchemeTests : ApiTest
    {
        [Fact]
        public async Task AnonymousAuthorizedEndpointReturns401NotRedirect()
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                BaseAddress = new Uri(Url),
            };

            var response = await client.PostAsync(new Uri("Organisations/Pull", UriKind.Relative), null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.False(response.Headers.Contains("Location"), "An /allors request must get a raw 401, not a login-page redirect.");
        }

        [Fact]
        public async Task BearerRequestStillAuthenticates()
        {
            await this.SignIn(this.Administrator);

            var response = await this.HttpClient.PostAsync(new Uri("Organisations/Pull", UriKind.Relative), null);

            Assert.True(response.IsSuccessStatusCode, $"Bearer-authenticated request should succeed, was {(int)response.StatusCode}.");
        }
    }
}
