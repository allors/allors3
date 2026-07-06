// <copyright file="UserTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Configuration.Derivations.Default;
    using Xunit;

    public class UserTests : DomainTest, IClassFixture<Fixture>
    {
        public UserTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void NewUserIsNotDisabled()
        {
            var user = new PersonBuilder(this.Transaction).WithUserName("isdisabled-default").Build();

            this.Transaction.Derive();

            Assert.True(user.ExistIsDisabled);
            Assert.False(user.IsDisabled);
        }

        [Fact]
        public void DisablingAUserLocksItOutAndRotatesTheStamp()
        {
            var user = new PersonBuilder(this.Transaction).WithUserName("to-disable").Build();
            this.Transaction.Derive();
            var stampBefore = user.UserSecurityStamp;

            user.IsDisabled = true;
            this.Transaction.Derive();

            Assert.True(user.UserLockoutEnabled);
            Assert.Equal(System.DateTime.MaxValue, user.UserLockoutEnd);
            Assert.NotEqual(stampBefore, user.UserSecurityStamp);
        }

        [Fact]
        public void ReEnablingAUserClearsTheLockout()
        {
            var user = new PersonBuilder(this.Transaction).WithUserName("to-reenable").Build();
            user.IsDisabled = true;
            this.Transaction.Derive();

            user.IsDisabled = false;
            this.Transaction.Derive();

            Assert.False(user.ExistUserLockoutEnd);
            Assert.Equal(0, user.UserAccessFailedCount);
        }

        [Fact]
        public void SetPasswordRotatesTheStamp()
        {
            var user = new PersonBuilder(this.Transaction).WithUserName("password-rotate").Build();
            this.Transaction.Derive();
            var stampBefore = user.UserSecurityStamp;

            user.SetPassword("p@ssw0rd");

            Assert.NotEqual(stampBefore, user.UserSecurityStamp);
        }
    }
}
