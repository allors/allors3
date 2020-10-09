// <copyright file="PullTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Adapters
{
    using System;
    using Allors.Workspace.Adapters.Remote;
    using Xunit;

    public class SaveTests : Test
    {
        [Fact]
        public async void ShouldSyncNewlyCreatedObject()
        {
            var context = this.Workspace.CreateSession();

            var newObject = context.Create(M.C1.Class);

            var saved = await context.Save();

            foreach (var roleType in M.C1.ObjectType.RoleTypes)
            {
                var role = newObject.Strategy.Get(roleType);
                Assert.True(role == null || (role is Array array && array.Length == 0));
            }

            foreach (var associationType in M.C1.ObjectType.AssociationTypes)
            {
                if (associationType.IsOne)
                {
                    var association = ((Strategy)newObject.Strategy).GetAssociation(associationType);
                    Assert.Null(association);
                }
                else
                {
                    var association = ((Strategy)newObject.Strategy).GetAssociations(associationType);
                    Assert.Empty(association);
                }
            }
        }
    }
}
