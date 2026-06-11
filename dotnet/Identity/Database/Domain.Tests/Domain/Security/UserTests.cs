// <copyright file="UserTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Xunit;

    public class UserTests : DomainTest, IClassFixture<Fixture>
    {
        public UserTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void WhenBuildingUserThenUserSecurityStampExists()
        {
            var person = new PersonBuilder(this.Transaction).WithUserName("user@example.com").Build();

            Assert.True(person.ExistUserSecurityStamp);
        }

        [Fact]
        public void WhenSettingUserNameThenNormalizedUserNameIsDerived()
        {
            var person = new PersonBuilder(this.Transaction).WithUserName("Jane@Example.com").Build();

            this.Transaction.Derive();

            Assert.Equal("JANE@EXAMPLE.COM", person.NormalizedUserName);
        }

        [Fact]
        public void WhenSettingUserEmailThenNormalizedUserEmailIsDerived()
        {
            var person = new PersonBuilder(this.Transaction).WithUserName("jane").Build();
            person.UserEmail = "Jane@Example.com";

            this.Transaction.Derive();

            Assert.Equal("JANE@EXAMPLE.COM", person.NormalizedUserEmail);
        }

        [Fact]
        public void WhenSettingPasswordThenPasswordCanBeVerified()
        {
            var person = new PersonBuilder(this.Transaction).WithUserName("jane").Build();

            person.SetPassword("it's a secret");

            Assert.True(person.VerifyPassword("it's a secret"));
            Assert.False(person.VerifyPassword("not the secret"));
            Assert.False(person.VerifyPassword(string.Empty));
        }

        [Fact]
        public void WhenSettingInUserPasswordThenUserPasswordHashIsDerived()
        {
            var person = new PersonBuilder(this.Transaction).WithUserName("jane").Build();

            this.Transaction.Derive();

            person.InUserPassword = "it's a secret";

            this.Transaction.Derive();

            Assert.True(person.ExistUserPasswordHash);
            Assert.False(person.ExistInUserPassword);
            Assert.True(person.VerifyPassword("it's a secret"));
        }
    }
}
