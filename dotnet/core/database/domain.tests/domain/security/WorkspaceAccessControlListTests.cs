// <copyright file="DatabaseAccessControlListTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using Allors;
    using Database;
    using Domain;
    using Meta;
    using Xunit;
    using AccessControl = Domain.AccessControl;
    using Object = Domain.Object;
    using Permission = Domain.Permission;
    using Role = Domain.Role;

    public class WorkspaceAccessControlListTests : DomainTest, IClassFixture<Fixture>
    {
        private string workspaceName = "Default";

        public WorkspaceAccessControlListTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void InitialWithoutAccessControl()
        {
            var person = new PersonBuilder(this.Transaction).WithFirstName("John").WithLastName("Doe").Build();

            _ = this.Transaction.Derive();
            this.Transaction.Commit();

            var organisation = new OrganisationBuilder(this.Transaction).WithName("Organisation").Build();

            var acl = new WorkspaceAccessControlLists(this.workspaceName, person)[organisation];

            Assert.False(acl.CanRead(this.M.Organisation.Name));
        }

        [Fact]
        public void Initial()
        {
            var permission = this.FindPermission(this.M.Organisation.Name, Operations.Read);
            var role = new RoleBuilder(this.Transaction).WithName("Role").WithPermission(permission).Build();
            var person = new PersonBuilder(this.Transaction).WithFirstName("John").WithLastName("Doe").Build();
            var accessControl = new AccessControlBuilder(this.Transaction).WithSubject(person).WithRole(role).Build();

            var intialSecurityToken = new SecurityTokens(this.Transaction).InitialSecurityToken;
            intialSecurityToken.AddAccessControl(accessControl);

            _ = this.Transaction.Derive();
            this.Transaction.Commit();

            var organisation = new OrganisationBuilder(this.Transaction).WithName("Organisation").Build();

            var acl = new WorkspaceAccessControlLists(this.workspaceName, person)[organisation];

            Assert.True(acl.CanRead(this.M.Organisation.Name));
        }

        private Permission FindPermission(IRoleType roleType, Operations operation)
        {
            var objectType = (Class)roleType.AssociationType.ObjectType;
            return new Permissions(this.Transaction).Get(objectType, roleType, operation);
        }
    }
}
