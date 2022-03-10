// <copyright file="DelegateAccessTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Meta;
    using Xunit;
    using Permission = Domain.Permission;

    public class DelegateAccessTests : DomainTest, IClassFixture<Fixture>
    {
        public DelegateAccessTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void WithoutDelegate()
        {
            var user = new PersonBuilder(this.Transaction).WithUserName("user").Build();
            var accessClass = new AccessClassBuilder(this.Transaction).Build();

            var securityToken = new SecurityTokenBuilder(this.Transaction).Build();
            var permission = this.FindPermission(this.M.AccessClass.Property, Operations.Read);
            var role = new RoleBuilder(this.Transaction).WithName("Role").WithPermission(permission).Build();

            securityToken.AddGrant(
                new GrantBuilder(this.Transaction)
                    .WithRole(role)
                    .WithSubject(user)
                    .Build());

            this.Transaction.Derive();
            this.Transaction.Commit();

            var acl = new DatabaseAccessControl(user)[accessClass];
            Assert.False(acl.CanRead(this.M.AccessClass.Property));
            Assert.False(acl.CanRead(this.M.AccessClass.Property));
        }

        [Fact]
        public void WithDelegate()
        {
            var user = new PersonBuilder(this.Transaction).WithUserName("user").Build();

            var delegatedAccessClass = new AccessClassBuilder(this.Transaction).Build();
            var accessClass = new AccessClassBuilder(this.Transaction).WithDelegatedAccess(delegatedAccessClass).Build();

            var securityToken = new SecurityTokenBuilder(this.Transaction).Build();
            var permission = this.FindPermission(this.M.AccessClass.Property, Operations.Read);
            var role = new RoleBuilder(this.Transaction).WithName("Role").WithPermission(permission).Build();

            securityToken.AddGrant(
                new GrantBuilder(this.Transaction)
                    .WithRole(role)
                    .WithSubject(user)
                    .Build());

            delegatedAccessClass.AddSecurityToken(securityToken);

            this.Transaction.Derive();
            this.Transaction.Commit();

            // Use default security from Singleton
            var acl = new DatabaseAccessControl(user)[accessClass];
            Assert.True(acl.CanRead(this.M.AccessClass.Property));
            Assert.True(acl.CanRead(this.M.AccessClass.Property));
        }

        private Permission FindPermission(IRoleType roleType, Operations operation)
        {
            var objectType = (Class)roleType.AssociationType.ObjectType;
            return new Permissions(this.Transaction).Get(objectType, roleType, operation);
        }
    }
}
