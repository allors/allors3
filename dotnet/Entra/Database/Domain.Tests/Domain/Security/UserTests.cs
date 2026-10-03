// <copyright file="UserTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System;
    using Database.Derivations;
    using Database.Security;
    using Xunit;

    // The Entra identity of a user is the pair of a tenant id and an object id. The Entra domain
    // declares it, finds a user by it and keeps it to one user as far as a transaction can see.
    public class UserTests : DomainTest, IClassFixture<Fixture>
    {
        private static readonly Guid Tenant = new Guid("cd3598ab-ef18-4774-bfce-c1b3cebe5a42");

        private static readonly Guid OtherTenant = new Guid("2baf9a07-5a2e-4065-adf2-2cf6a9bccff5");

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

        [Fact]
        public void FindByEntraIdentityFindsTheUser()
        {
            var objectId = Guid.NewGuid();
            var user = this.NewUser(Tenant, objectId);
            this.NewUser(Tenant, Guid.NewGuid());
            new PersonBuilder(this.Transaction).Build();

            this.Transaction.Derive();

            Assert.Equal(user, new Users(this.Transaction).FindByEntraIdentity(Tenant, objectId));
        }

        // An object id is unique within its tenant only, so the lookup takes both.
        [Fact]
        public void FindByEntraIdentityNeedsTheTenantAndTheObject()
        {
            var objectId = Guid.NewGuid();
            this.NewUser(Tenant, objectId);

            this.Transaction.Derive();

            var users = new Users(this.Transaction);
            Assert.Null(users.FindByEntraIdentity(OtherTenant, objectId));
            Assert.Null(users.FindByEntraIdentity(Tenant, Guid.NewGuid()));
        }

        // No token carries an empty id, so an empty id finds nobody, not even a user that has one.
        [Fact]
        public void FindByEntraIdentityFindsNobodyForAnEmptyId()
        {
            var objectId = Guid.NewGuid();
            this.NewUser(Guid.Empty, Guid.Empty);
            this.NewUser(Tenant, Guid.Empty);
            this.NewUser(Guid.Empty, objectId);

            var users = new Users(this.Transaction);
            Assert.Null(users.FindByEntraIdentity(Guid.Empty, Guid.Empty));
            Assert.Null(users.FindByEntraIdentity(Tenant, Guid.Empty));
            Assert.Null(users.FindByEntraIdentity(Guid.Empty, objectId));
        }

        // The rule counts in the view of its own transaction: it refuses a second user with an
        // identity that the transaction can see. It does not see a parallel transaction, see
        // OfTwoUsersWithTheSameEntraIdentityTheOldestIsFound.
        [Fact]
        public void ASecondUserWithTheSameEntraIdentityIsRefused()
        {
            var objectId = Guid.NewGuid();
            this.NewUser(Tenant, objectId);

            Assert.False(this.Transaction.Derive(false).HasErrors);

            var second = this.NewUser(Tenant, objectId);

            var validation = this.Transaction.Derive(false);

            var error = Assert.Single(validation.Errors);
            Assert.IsType<IDerivationErrorUnique>(error, exactMatch: false);
            Assert.All(error.Relations, v => Assert.Equal(second, v.Association));
            Assert.Contains(this.M.User.EntraTenantId, error.RoleTypes);
            Assert.Contains(this.M.User.EntraObjectId, error.RoleTypes);
        }

        [Fact]
        public void TheSameObjectIdInAnotherTenantIsAllowed()
        {
            var objectId = Guid.NewGuid();
            this.NewUser(Tenant, objectId);
            this.NewUser(OtherTenant, objectId);

            Assert.False(this.Transaction.Derive(false).HasErrors);
        }

        // Half an identity finds nobody, so the plug-in would have a second user created for the
        // same person at the next sign-in.
        [Fact]
        public void HalfAnEntraIdentityIsRefused()
        {
            var withoutTenant = new PersonBuilder(this.Transaction).Build();
            withoutTenant.EntraObjectId = Guid.NewGuid();

            var validation = this.Transaction.Derive(false);

            var error = Assert.Single(validation.Errors);
            Assert.IsType<IDerivationErrorRequired>(error, exactMatch: false);
            Assert.All(error.Relations, v => Assert.Equal(withoutTenant, v.Association));
            Assert.Contains(this.M.User.EntraTenantId, error.RoleTypes);

            withoutTenant.EntraTenantId = Tenant;

            Assert.False(this.Transaction.Derive(false).HasErrors);

            var withoutObject = new PersonBuilder(this.Transaction).Build();
            withoutObject.EntraTenantId = Tenant;

            validation = this.Transaction.Derive(false);

            error = Assert.Single(validation.Errors);
            Assert.IsType<IDerivationErrorRequired>(error, exactMatch: false);
            Assert.All(error.Relations, v => Assert.Equal(withoutObject, v.Association));
            Assert.Contains(this.M.User.EntraObjectId, error.RoleTypes);
        }

        // The store cannot keep an identity unique: two servers that each handle the first sign-in
        // of one person create a user each, and the rule in neither transaction sees the other. The
        // two users below are not derived, as such transactions leave them. Every server then finds
        // the same user, the oldest.
        [Fact]
        public void OfTwoUsersWithTheSameEntraIdentityTheOldestIsFound()
        {
            var objectId = Guid.NewGuid();
            var oldest = new PersonBuilder(this.Transaction).Build();
            var newest = new PersonBuilder(this.Transaction).Build();

            newest.EntraTenantId = Tenant;
            newest.EntraObjectId = objectId;
            oldest.EntraTenantId = Tenant;
            oldest.EntraObjectId = objectId;

            Assert.True(oldest.Id < newest.Id);
            Assert.Equal(oldest, new Users(this.Transaction).FindByEntraIdentity(Tenant, objectId));
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

        // A guest of the tenant comes from another organization: the directory names its home in the
        // identity provider and marks the account as a guest. Both are derived like the other Entra
        // fields, and both are for showing, so they are assigned to the Default workspace.
        [Fact]
        public void TheGuestFieldsAreDerivedAndAssignedToTheDefaultWorkspace()
        {
            var administrator = new PersonBuilder(this.Transaction).Build();
            new UserGroups(this.Transaction).Administrators.AddMember(administrator);
            var user = this.NewUser(Tenant, Guid.NewGuid());

            this.Transaction.Derive();
            this.Transaction.Commit();

            Assert.False(user.ExistEntraIdentityProvider);
            Assert.False(user.ExistEntraIsGuest);

            var security = this.Transaction.Database.Services.Get<ISecurity>();
            var acl = new DatabaseAccessControl(security, administrator)[user];

            foreach (var guestField in new Meta.IRoleType[] { this.M.User.EntraIdentityProvider, this.M.User.EntraIsGuest })
            {
                Assert.True(guestField.RelationType.IsDerived);
                Assert.True(acl.CanRead(guestField));
                Assert.False(acl.CanWrite(guestField));
                Assert.Equal("Default", Assert.Single(guestField.RelationType.AssignedWorkspaceNames));
            }
        }

        [Fact]
        public void ACloneHasNoGuestFields()
        {
            var user = this.NewUser(Tenant, Guid.NewGuid());
            user.EntraIdentityProvider = "https://login.microsoftonline.com/" + OtherTenant + "/v2.0";
            user.EntraIsGuest = true;

            this.Transaction.Derive();

            var clone = user.Clone();

            Assert.False(clone.ExistEntraIdentityProvider);
            Assert.False(clone.ExistEntraIsGuest);
        }

        // A program that calls the application is a user too, of another class than a person. The
        // Entra domain knows only User, so an agent has the identity, is found by it and shares the
        // rule with every other user.
        [Fact]
        public void AnAgentHasAnEntraIdentityLikeAPerson()
        {
            var objectId = Guid.NewGuid();
            var agent = new AgentBuilder(this.Transaction).Build();
            agent.EntraTenantId = Tenant;
            agent.EntraObjectId = objectId;

            Assert.False(this.Transaction.Derive(false).HasErrors);
            Assert.Equal(agent, new Users(this.Transaction).FindByEntraIdentity(Tenant, objectId));

            var person = this.NewUser(Tenant, objectId);

            var error = Assert.Single(this.Transaction.Derive(false).Errors);
            Assert.IsType<IDerivationErrorUnique>(error, exactMatch: false);
            Assert.All(error.Relations, v => Assert.Equal(person, v.Association));
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
