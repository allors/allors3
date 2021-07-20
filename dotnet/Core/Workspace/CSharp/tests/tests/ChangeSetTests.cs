// <copyright file="ChangeSetTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Tests.Workspace
{
    using System.Linq;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class ChangeSetTests : Test
    {
        protected ChangeSetTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public async void CreatingChangeSetAfterCreatingSession()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var changeSet = session.Checkpoint();

            Assert.Null(changeSet.Instantiated);
        }

        [Fact]
        public async void Instantiated()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            await session.Pull(pull);

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Instantiated);
        }

        [Fact]
        public async void ChangeSetAfterPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>().First();

            c1a.C1AllorsString = "X";

            await session.Push();

            //await session.Pull(pull);

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
        }

        [Fact]
        public async void ChangeSetAfterPushWithNoChanges()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var c1 = session.Create<C1>();

            await session.Push();
            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);

            await session.Push();
            changeSet = session.Checkpoint();
            Assert.Null(changeSet.Created);
        }

        [Fact]
        public async void ChangeSetAfterPushWithPull()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>().First();

            c1a.C1AllorsString = "X";

            await session.Push();

            await session.Pull(pull);

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
        }
    }
}
