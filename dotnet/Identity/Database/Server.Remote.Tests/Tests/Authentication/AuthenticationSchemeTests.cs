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

    public class AuthenticationSchemeTests
    {
        private const string Url = "http://localhost:5000/allors/";

        // The Identity application cookie is the default scheme of this server. Its challenge
        // redirects a browser to the login page, but not a request for the Allors API.
        [Fact]
        public async Task AnonymousAuthorizedEndpointReturns401NotRedirect()
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                BaseAddress = new Uri(Url),
            };

            var response = await client.PostAsync(new Uri("pull", UriKind.Relative), null);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.False(response.Headers.Contains("Location"), "An /allors request must get a raw 401, not a login-page redirect.");
        }
    }
}
