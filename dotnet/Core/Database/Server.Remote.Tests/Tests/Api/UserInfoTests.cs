// <copyright file="UserInfoTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Xunit;

    [Collection("Api")]
    public class UserInfoTests : ApiTest
    {
        [Fact]
        public async Task AuthenticatedRequestReturnsTheUserId()
        {
            await this.SignIn(this.Administrator);

            var response = await this.HttpClient.GetAsync(new Uri("UserInfo", UriKind.Relative));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var document = JsonDocument.Parse(body);
            Assert.Equal(this.Administrator.Id.ToString(), document.RootElement.GetProperty("u").GetString());
        }

        [Fact]
        public async Task AnonymousRequestIsDenied()
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                BaseAddress = new Uri(Url),
            };

            var response = await client.GetAsync(new Uri("UserInfo", UriKind.Relative));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
