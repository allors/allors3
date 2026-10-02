// <copyright file="UpgradeTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.IO;
    using Configuration.Derivations.Default;
    using Xunit;

    public class UpgradeTests : DomainTest, IClassFixture<Fixture>
    {
        public UpgradeTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void UpgradeBackfillsSecurityRolesForExistingUsers()
        {
            var user = new PersonBuilder(this.Transaction).WithUserName("legacy").Build();
            this.Transaction.Derive();

            // Simulate a user persisted before these roles existed / lockout was enabled.
            user.RemoveUserSecurityStamp();
            user.RemoveIsDisabled();
            user.UserLockoutEnabled = false;

            new Upgrade(this.Transaction, new DirectoryInfo(".")).Execute();

            Assert.True(user.ExistUserSecurityStamp);
            Assert.True(user.ExistIsDisabled);
            Assert.False(user.IsDisabled);
            Assert.True(user.UserLockoutEnabled);
        }
    }
}
