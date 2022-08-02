// <copyright file="DelegateAccessTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Meta;
    using Xunit;
    using Permission = Domain.Permission;

    public class DelegateAccessGrantTests : DomainTest, IClassFixture<Fixture>
    {
        public DelegateAccessGrantTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void WithSecurityTokenAndDelegateWithoutSecurityToken()
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

            accessClass.AddSecurityToken(securityToken);

            this.Transaction.Derive();
            this.Transaction.Commit();

            // Use default security from Singleton
            var acl = new DatabaseAccessControl(this.Security, user)[accessClass];
            Assert.True(acl.CanRead(this.M.AccessClass.Property));
            Assert.True(acl.CanRead(this.M.AccessClass.Property));

            Assert.False(acl.CanRead(this.M.AccessClass.AnotherProperty));
            Assert.False(acl.CanRead(this.M.AccessClass.AnotherProperty));
        }

        [Fact]
        public void WithoutSecurityTokenAndDelegateWithoutSecurityToken()
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

            this.Transaction.Derive();
            this.Transaction.Commit();

            // Use default security from Singleton
            var acl = new DatabaseAccessControl(this.Security, user)[accessClass];
            Assert.False(acl.CanRead(this.M.AccessClass.Property));
            Assert.False(acl.CanRead(this.M.AccessClass.Property));

            Assert.False(acl.CanRead(this.M.AccessClass.AnotherProperty));
            Assert.False(acl.CanRead(this.M.AccessClass.AnotherProperty));
        }

        [Fact]
        public void WithSecurityTokenAndDelegateWithSecurityToken()
        {
            var user = new PersonBuilder(this.Transaction).WithUserName("user").Build();

            var delegatedAccessClass = new AccessClassBuilder(this.Transaction).Build();
            var accessClass = new AccessClassBuilder(this.Transaction).WithDelegatedAccess(delegatedAccessClass).Build();

            var securityToken1 = new SecurityTokenBuilder(this.Transaction).Build();
            var permission1 = this.FindPermission(this.M.AccessClass.Property, Operations.Read);
            var role1 = new RoleBuilder(this.Transaction).WithName("Role").WithPermission(permission1).Build();

            securityToken1.AddGrant(
                new GrantBuilder(this.Transaction)
                    .WithRole(role1)
                    .WithSubject(user)
                    .Build());

            var securityToken2 = new SecurityTokenBuilder(this.Transaction).Build();
            var permission2 = this.FindPermission(this.M.AccessClass.AnotherProperty, Operations.Read);
            var role2 = new RoleBuilder(this.Transaction).WithName("Role").WithPermission(permission2).Build();

            securityToken2.AddGrant(
                new GrantBuilder(this.Transaction)
                    .WithRole(role2)
                    .WithSubject(user)
                    .Build());

            accessClass.AddSecurityToken(securityToken1);
            delegatedAccessClass.AddSecurityToken(securityToken2);

            this.Transaction.Derive();
            this.Transaction.Commit();

            // Use default security from Singleton
            var acl = new DatabaseAccessControl(this.Security, user)[accessClass];
            Assert.True(acl.CanRead(this.M.AccessClass.Property));
            Assert.True(acl.CanRead(this.M.AccessClass.Property));

            Assert.True(acl.CanRead(this.M.AccessClass.AnotherProperty));
            Assert.True(acl.CanRead(this.M.AccessClass.AnotherProperty));
        }

        [Fact]
        public void WithoutSecurityTokenAndDelegateWithSecurityToken()
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
            var acl = new DatabaseAccessControl(this.Security, user)[accessClass];
            Assert.True(acl.CanRead(this.M.AccessClass.Property));
            Assert.True(acl.CanRead(this.M.AccessClass.Property));

            Assert.False(acl.CanRead(this.M.AccessClass.AnotherProperty));
            Assert.False(acl.CanRead(this.M.AccessClass.AnotherProperty));
        }

        private Permission FindPermission(IRoleType roleType, Operations operation)
        {
            var objectType = (Class)roleType.AssociationType.ObjectType;
            return new Permissions(this.Transaction).Get(objectType, roleType, operation);
        }
    }
}
