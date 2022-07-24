// <copyright file="AccessControlListTests.cs" company="Allors bvba">
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
    using Object = Domain.Object;
    using Permission = Domain.Permission;

    public class AccessControlListTests : DomainTest, IClassFixture<Fixture>
    {
        public AccessControlListTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void GivenAnAuthenticationPopulationWhenCreatingAnAccessListForGuestThenPermissionIsDenied()
        {
            this.Session.Derive();
            this.Session.Commit();

            var sessions = new[] { this.Session };
            foreach (var session in sessions)
            {
                session.Commit();

                var guest = new AutomatedAgents(this.Session).Guest;
                var acls = new DatabaseAccessControl(this.Security, guest);
                foreach (Object aco in (IObject[])session.Extent(this.M.Organisation))
                {
                    // When
                    var accessList = acls[aco];

                    // Then
                    Assert.False(accessList.CanExecute(this.M.Organisation.JustDoIt));
                }

                session.Rollback();
            }
        }

        private Permission FindPermission(RoleType roleType, Operations operation)
        {
            var objectType = (Class)roleType.AssociationType.ObjectType;
            return new Permissions(this.Session).Get(objectType, roleType, operation);
        }
    }
}
