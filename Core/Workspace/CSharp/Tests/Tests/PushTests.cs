// <copyright file="SaveTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System.Linq;
    using Allors.Workspace;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class PushTests : Test
    {
        protected PushTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async void NewPushedObjectShouldBeSynced()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var newObject = session.Create<C1>();

            var pushResult = await session.Push();

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


        [Fact]
        public async void ChangesShouldRemainAfterPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Extent(this.M.C1)
                {
                    Predicate = new Equals(this.M.C1.C1AllorsString) {Value = "c1A"}
                }
            };

            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>().First();

            c1a.C1AllorsString = "X";

            Assert.Equal("X", c1a.C1AllorsString);

            _ = await session.Push();

            Assert.Equal("X", c1a.C1AllorsString);
        }


        [Fact]
        public async void ChangesBeforeCheckpointShouldBePushed()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Extent(this.M.C1),
            };

            var result = await session.Pull(pull);

            var c1a = result.GetCollection<C1>().First(v => v.Name.Equals("c1A"));

            c1a.C1AllorsString = "X";

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);

            _ = await session.Push();

            var session2 = this.Workspace.CreateSession();

            result = await session2.Pull(pull);

            var c1aSession2 = result.GetCollection<C1>().First(v => v.Name.Equals("c1A"));

            Assert.Equal("X", c1aSession2.C1AllorsString);

            result = await session.Pull(pull);

            var c1aSession1 = result.GetCollection<C1>().First(v => v.Name.Equals("c1A"));

            Assert.Equal("X", c1aSession1.C1AllorsString);
        }

    }
}
