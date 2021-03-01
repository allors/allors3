// <copyright file="SaveTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System;
    using Allors.Workspace.Domain;
    using Remote;
    using Xunit;

    public abstract class SaveTests : Test
    {
        protected SaveTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async void ShouldSyncNewlyCreatedObject()
        {
            var session = this.Workspace.CreateSession();

            var newObject = session.Create<C1>();

            var saved = await session.Save();

            foreach (var roleType in this.M.C1.ObjectType.RoleTypes)
            {
                var role = newObject.Strategy.Get(roleType);
                Assert.True(role == null || (role is Array array && (array.Length == 0)));
            }

            foreach (var associationType in this.M.C1.ObjectType.AssociationTypes)
            {
                if (associationType.IsOne)
                {
                    var association = newObject.Strategy.GetAssociation(associationType);
                    Assert.Null(association);
                }
                else
                {
                    var association = newObject.Strategy.GetAssociations(associationType);
                    Assert.Empty(association);
                }
            }
        }
    }
}
