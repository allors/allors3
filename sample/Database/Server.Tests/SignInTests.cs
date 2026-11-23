// <copyright file="SignInTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Server.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Threading.Tasks;
    using Allors.Database.Domain;
    using Xunit;

    public class SignInTests : ApiTest
    {
        private const string Password = "correct horse battery";

        private readonly long janeId;

        public SignInTests() : base(new Dictionary<string, string> { ["JwtToken:Expiration"] = "01:00:00" })
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

        [Fact]
        public async Task ATokenExpiresAfterTheLifetimeOfTheSettingJwtTokenExpiration()
        {
            var response = await this.SignIn("jane@example.com", Password);

            var token = new JwtSecurityTokenHandler().ReadJwtToken(response.t);
            Assert.InRange(token.ValidTo - DateTime.UtcNow, TimeSpan.FromMinutes(59), TimeSpan.FromMinutes(60));
        }
    }
}
