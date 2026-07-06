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
    }
}
