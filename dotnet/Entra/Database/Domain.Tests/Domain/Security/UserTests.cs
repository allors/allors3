// <copyright file="UserTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System;
    using Database.Security;
    using Xunit;

    // The Entra identity of a user is the pair of a tenant id and an object id. The Entra domain
    // declares it, together with what the directory says about the person.
    public class UserTests : DomainTest, IClassFixture<Fixture>
    {
        private static readonly Guid Tenant = new Guid("cd3598ab-ef18-4774-bfce-c1b3cebe5a42");

        public UserTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        // The application's domain creates a user; the Entra domain adds nothing to it. The plug-in
        // gives the user its Entra identity afterwards. Core still gives it its owner grant and
        // security token.
        [Fact]
        public void ANewUserHasNoEntraIdentity()
        {
            var user = new PersonBuilder(this.Transaction).Build();

            this.Transaction.Derive();

            Assert.False(user.ExistEntraTenantId);
            Assert.False(user.ExistEntraObjectId);
            Assert.False(user.ExistEntraUserName);
            Assert.False(user.ExistEntraDisplayName);
            Assert.False(user.ExistEntraEmail);
            Assert.True(user.ExistOwnerGrant);
            Assert.True(user.ExistOwnerSecurityToken);
        }

        // The Entra fields are derived: the plug-in writes them from a validated token, and an
        // access list refuses the write whatever the grants say. Otherwise whoever may write users
        // could give another user's object their own identity and sign in as that user.
        [Fact]
        public void NobodyWritesTheEntraFieldsThroughAnAccessList()
        {
            var administrator = new PersonBuilder(this.Transaction).Build();
            new UserGroups(this.Transaction).Administrators.AddMember(administrator);
            var user = this.NewUser(Tenant, Guid.NewGuid());

            this.Transaction.Derive();
            this.Transaction.Commit();

            var security = this.Transaction.Database.Services.Get<ISecurity>();
            var acl = new DatabaseAccessControl(security, administrator)[user];

            var entraFields = new Meta.IRoleType[]
            {
                this.M.User.EntraTenantId,
                this.M.User.EntraObjectId,
                this.M.User.EntraUserName,
                this.M.User.EntraDisplayName,
                this.M.User.EntraEmail,
            };

            foreach (var entraField in entraFields)
            {
                Assert.True(entraField.RelationType.IsDerived);
                Assert.True(acl.CanRead(entraField));
                Assert.False(acl.CanWrite(entraField));
            }

            // The administrator does write a field that is not derived.
            Assert.True(acl.CanWrite(this.M.User.UniqueId));
        }

        // What the directory says about a person is for a user interface to show; the two ids are
        // of no use to it. This tree has no workspace, so the assignment takes effect in an
        // application that puts its user class in the Default workspace.
        [Fact]
        public void TheProfileFieldsAreAssignedToTheDefaultWorkspaceAndTheIdsAreNot()
        {
            Assert.Equal("Default", Assert.Single(this.M.User.EntraUserName.RelationType.AssignedWorkspaceNames));
            Assert.Equal("Default", Assert.Single(this.M.User.EntraDisplayName.RelationType.AssignedWorkspaceNames));
            Assert.Equal("Default", Assert.Single(this.M.User.EntraEmail.RelationType.AssignedWorkspaceNames));

            Assert.Empty(this.M.User.EntraTenantId.RelationType.AssignedWorkspaceNames);
            Assert.Empty(this.M.User.EntraObjectId.RelationType.AssignedWorkspaceNames);
        }

        // A clone copies the fields that are not derived, so it does not get the identity of the
        // user it was cloned from.
        [Fact]
        public void ACloneHasNoEntraIdentity()
        {
            var user = this.NewUser(Tenant, Guid.NewGuid());
            user.EntraUserName = "jane@example.com";
            user.EntraDisplayName = "Jane Doe";
            user.EntraEmail = "jane@example.com";

            this.Transaction.Derive();

            var clone = user.Clone();

            Assert.False(clone.ExistEntraTenantId);
            Assert.False(clone.ExistEntraObjectId);
            Assert.False(clone.ExistEntraUserName);
            Assert.False(clone.ExistEntraDisplayName);
            Assert.False(clone.ExistEntraEmail);
        }

        private Person NewUser(Guid tenantId, Guid objectId)
        {
            var user = new PersonBuilder(this.Transaction).Build();
            user.EntraTenantId = tenantId;
            user.EntraObjectId = objectId;
            return user;
        }
    }
}
