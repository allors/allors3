// <copyright file="LoginTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the PersonTests type.
// </summary>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class LoginTests : DomainTest, IClassFixture<Fixture>
    {
        public LoginTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void WhenDeletingUserThenLoginShouldAlsoBeDeleted()
        {
            var person = new PersonBuilder(this.Transaction).WithUserName("user").Build();

            this.Transaction.Derive();

            var login = new LoginBuilder(this.Transaction).WithProvider("MyProvider").WithKey("XXXYYYZZZ").Build();
            person.AddLogin(login);

            this.Transaction.Derive();

            person.Delete();

            this.Transaction.Derive();

            Assert.True(login.Strategy.IsDeleted);
        }
    }
}
