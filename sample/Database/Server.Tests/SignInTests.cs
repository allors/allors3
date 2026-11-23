// <copyright file="SignInTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Server.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Threading.Tasks;
    using Allors.Database.Domain;
    using Xunit;

    public class SignInTests : ApiTest
    {
        private const string Password = "correct horse battery";

        private readonly long janeId;

        public SignInTests() : base(new Dictionary<string, string>
        {
            ["JwtToken:Issuer"] = "sample",
            ["JwtToken:Audience"] = "workspace",
            ["JwtToken:Expiration"] = "01:00:00",
        })
        {
            using var transaction = this.Database.CreateTransaction();
            var jane = new PersonBuilder(transaction).WithUserName("jane@example.com").WithInUserPassword(Password).Build();
            new UserGroups(transaction).Workers.AddMember(jane);
            transaction.Derive();
            transaction.Commit();

            this.janeId = jane.Id;
        }

        [Fact]
        public async Task AUserWithTheRightPasswordGetsABearerToken()
        {
            var response = await this.SignIn("jane@example.com", Password);

            Assert.True(response.a);
            Assert.Equal(this.janeId.ToString(), response.u);
            Assert.False(string.IsNullOrEmpty(response.t));
        }

        [Fact]
        public async Task AUserWithAnotherPasswordGetsNoToken()
        {
            var response = await this.SignIn("jane@example.com", "another horse battery");

            Assert.False(response.a);
            Assert.Null(response.t);
        }

        [Theory]
        [InlineData("access")]
        [InlineData("invoke")]
        [InlineData("permission")]
        [InlineData("pull")]
        [InlineData("push")]
        [InlineData("sync")]
        public async Task TheJsonApiRequiresAValidBearerToken(string route)
        {
            using var anonymousResponse = await this.PostAsJsonAsync(route, new object());
            Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

            this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid");
            using var invalidTokenResponse = await this.PostAsJsonAsync(route, new object());
            Assert.Equal(HttpStatusCode.Unauthorized, invalidTokenResponse.StatusCode);
        }

        [Fact]
        public async Task ATokenExpiresAfterTheLifetimeOfTheSettingJwtTokenExpiration()
        {
            var response = await this.SignIn("jane@example.com", Password);

            var token = new JwtSecurityTokenHandler().ReadJwtToken(response.t);
            Assert.InRange(token.ValidTo - DateTime.UtcNow, TimeSpan.FromMinutes(59), TimeSpan.FromMinutes(60));
        }

        [Theory]
        [InlineData("another-issuer", "workspace")]
        [InlineData("sample", "another-audience")]
        public async Task TheApiRefusesATokenForAnotherIssuerOrAudience(string issuer, string audience)
        {
            var token = await this.SignIn("jane@example.com", Password);
            using var accepted = await this.PostAsJsonAsync("permission", new object());
            Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);

            using var factory = new ServerFactory(new Dictionary<string, string>
            {
                ["adapter"] = "snapshot",
                ["JwtToken:Key"] = Key,
                ["JwtToken:Issuer"] = issuer,
                ["JwtToken:Audience"] = audience,
            });
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.t);

            using var response = await client.PostAsJsonAsync("http://localhost:5000/allors/permission", new object());

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
