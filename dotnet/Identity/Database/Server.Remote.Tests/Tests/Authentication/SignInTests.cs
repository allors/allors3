// <copyright file="SignInTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System.Threading.Tasks;
    using Database.Domain;
    using Xunit;

    [Collection("Api")]
    public class SignInTests : ApiTest
    {
        public SignInTests()
        {
            new PersonBuilder(this.Transaction).WithUserName("John").Build();
            new PersonBuilder(this.Transaction).WithUserName("Jane").Build().SetPassword("p@ssw0rd");
            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        [Fact]
        public async Task CorrectUserAndPassword() =>
            Assert.True(await this.CookieLoginSucceedsAsync("Jane", "p@ssw0rd"));

        [Fact]
        public async Task NonExistingUser() =>
            Assert.False(await this.CookieLoginSucceedsAsync("Jeff", "p@ssw0rd"));

        [Fact]
        public async Task EmptyStringPassword() =>
            Assert.False(await this.CookieLoginSucceedsAsync("John", string.Empty));

        // John has no password; the Identity form login has no distinct "no password" case, so an
        // empty submission stands in for it — it must still be rejected.
        [Fact]
        public async Task NoPassword() =>
            Assert.False(await this.CookieLoginSucceedsAsync("John", string.Empty));
    }
}
