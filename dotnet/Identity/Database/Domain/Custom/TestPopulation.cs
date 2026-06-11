// <copyright file="TestPopulation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public class TestPopulation
    {
        private readonly ITransaction transaction;

        public TestPopulation(ITransaction transaction) => this.transaction = transaction;

        public void Apply()
        {
            new PersonBuilder(this.transaction).WithUserName("noacl").WithFirstName("no").WithLastName("acl").Build();

            var noperm = new PersonBuilder(this.transaction).WithUserName("noperm").WithFirstName("no").WithLastName("perm").Build();
            var emptyRole = new RoleBuilder(this.transaction).WithName("Empty").Build();
            var defaultSecurityToken = new SecurityTokens(this.transaction).DefaultSecurityToken;

            new GrantBuilder(this.transaction).WithRole(emptyRole).WithSubject(noperm).WithSecurityToken(defaultSecurityToken).Build();
        }
    }
}
