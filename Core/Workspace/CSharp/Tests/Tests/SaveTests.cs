// <copyright file="SaveTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using Allors.Workspace;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class SaveTests : Test
    {
        protected SaveTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async void ShouldSyncNewlyCreatedObject()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var newObject = session.Create<C1>();

            var saved = await session.Push();

            foreach (var roleType in this.M.C1.RoleTypes)
            {
                Assert.False(newObject.Strategy.Exist(roleType));
            }

            foreach (var associationType in this.M.C1.AssociationTypes)
            {
                if (associationType.IsOne)
                {
                    var association = newObject.Strategy.GetComposite<IObject>(associationType);
                    Assert.Null(association);
                }
                else
                {
                    var association = newObject.Strategy.GetComposites<IObject>(associationType);
                    Assert.Empty(association);
                }
            }
        }
    }
}
