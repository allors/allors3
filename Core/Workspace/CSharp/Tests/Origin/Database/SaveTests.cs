// <copyright file="PullTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Origin.Database
{
    using System;
    using Allors.Workspace.Adapters.Remote;
    using Xunit;

    public class SaveTests : Test
    {
        [Fact]
        public async void ShouldSyncNewlyCreatedObject()
        {
            var session = this.Workspace.CreateSession();

            var newObject = session.Create(this.M.C1.Class);

            var saved = await session.Save();

            foreach (var roleType in this.M.C1.ObjectType.RoleTypes)
            {
                var role = newObject.Strategy.Get(roleType);
                Assert.True(role == null || role is Array array && array.Length == 0);
            }

            foreach (var associationType in this.M.C1.ObjectType.AssociationTypes)
            {
                if (associationType.IsOne)
                {
                    var association = ((DatabaseStrategy)newObject.Strategy).GetAssociation(associationType);
                    Assert.Null(association);
                }
                else
                {
                    var association = ((DatabaseStrategy)newObject.Strategy).GetAssociations(associationType);
                    Assert.Empty(association);
                }
            }
        }
    }
}
