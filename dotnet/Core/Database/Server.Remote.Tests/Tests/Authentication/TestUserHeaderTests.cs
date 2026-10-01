// <copyright file="TestUserHeaderTests.cs" company="Allors bv">
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
    using Database.Domain;
    using Xunit;

    [Collection("Api")]
    public class TestUserHeaderTests : ApiTest
    {
        private const string HeaderName = "X-Allors-TestUser";

        // A user of the test population is sent as its UniqueId; any other value is sent as is.
        private HttpClient HeaderClient(string alias)
        {
            var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                BaseAddress = new Uri(Url),
            };
            client.DefaultRequestHeaders.Add(HeaderName, Users.TryGetTestUserId(alias, out var uniqueId) ? uniqueId.ToString() : alias);
            return client;
        }

        [Fact]
        public async Task HeaderAuthenticatesUserInfo()
        {
            using var client = this.HeaderClient("jane@example.com");

            var response = await client.GetAsync(new Uri("UserInfo", UriKind.Relative));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task HeaderAuthenticatesAnAgent()
        {
            var agent = new Users(this.Transaction).FindBy(this.M.User.UserName, "agent");

            using var client = this.HeaderClient("agent");

            var response = await client.GetAsync(new Uri("UserInfo", UriKind.Relative));
            var body = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            using var document = JsonDocument.Parse(body);
            Assert.Equal(agent.Id.ToString(), document.RootElement.GetProperty("u").GetString());
        }

        [Fact]
        public async Task HeaderAuthorizesPullWithoutAntiforgery()
        {
            using var client = this.HeaderClient("jane@example.com");

            var content = new StringContent("{}", Encoding.UTF8, "application/json");
            var response = await client.PostAsync(new Uri("pull", UriKind.Relative), content);

            // Authorized by the fallback policy (not 401) and exempt from antiforgery (not 403), because
            // the test-header identity is not the Identity application cookie.
            Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.NotEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task UnknownHeaderUserIsUnauthorized()
        {
            using var client = this.HeaderClient("nobody@example.com");

            var response = await client.GetAsync(new Uri("UserInfo", UriKind.Relative));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // The header carries the UniqueId of a user of the test population, so the test sign-in does
        // not depend on how an authentication plug-in names a user: a user name no longer signs in.
        [Fact]
        public async Task HeaderWithAUserNameIsUnauthorized()
        {
            using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
            {
                BaseAddress = new Uri(Url),
            };
            client.DefaultRequestHeaders.Add(HeaderName, "jane@example.com");

            var response = await client.GetAsync(new Uri("UserInfo", UriKind.Relative));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
