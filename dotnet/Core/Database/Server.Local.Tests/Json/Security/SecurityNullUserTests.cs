// <copyright file="SecurityNullUserTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests
{
    using Allors.Database.Domain;
    using Allors.Database.Services;
    using Xunit;

    public class SecurityNullUserTests : ApiTest, IClassFixture<Fixture>
    {
        public SecurityNullUserTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void GetVersionedGrantsWithNullUserDoesNotFault()
        {
            var grant = new Grants(this.Transaction).Administrator;
            var securityToken = new SecurityTokenBuilder(this.Transaction).WithGrant(grant).Build();
            this.Transaction.Derive();

            // An anonymous request can reach the ACL with no user; it must not dereference user.Id.
            var versionedGrants = this.Security.GetVersionedGrants(this.Transaction, null, new[] { securityToken });

            Assert.Empty(versionedGrants);
        }
    }
}
