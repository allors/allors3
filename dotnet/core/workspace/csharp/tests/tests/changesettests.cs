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

            Assert.Empty(changeSet.Instantiated);
        }

        [Fact]
        public async void Instantiated()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Instantiated);

            var c1a = result.GetCollection<C1>()[0];

            Assert.Equal(c1a.Strategy, changeSet.Instantiated.First());
        }


        [Fact]
        public async void ChangeSetAfterPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";

            await session.PushAsync();

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
        }

        [Fact]
        public async void ChangeSetPushChangeNoPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a_1 = result.GetCollection<C1>()[0];

            c1a_1.C1AllorsString = "X";

            await session.PushAsync();

            var changeSet = session.Checkpoint();
            Assert.Single(changeSet.AssociationsByRoleType);

            result = await session.PullAsync(pull);
            var c1a_2 = result.GetCollection<C1>()[0];

            c1a_2.C1AllorsString = "Y";

            changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
        }

        [Fact]
        public async void ChangeSetPushChangePush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a_1 = result.GetCollection<C1>()[0];

            c1a_1.C1AllorsString = "X";

            await session.PushAsync();

            var changeSet = session.Checkpoint();
            Assert.Single(changeSet.AssociationsByRoleType);

            result = await session.PullAsync(pull);
            var c1a_2 = result.GetCollection<C1>()[0];

            c1a_2.C1AllorsString = "Y";

            await session.PushAsync();

            changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
        }

        [Fact]
        public async void ChangeSetAfterPushWithNoChanges()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var c1a = session.Create<C1>();

            await session.PushAsync();
            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);

            await session.PushAsync();
            changeSet = session.Checkpoint();
            Assert.Empty(changeSet.Created);
        }

        [Fact]
        public async void ChangeSetAfterPushWithPull()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";

            await session.PushAsync();

            await session.PullAsync(pull);

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
        }

        [Fact]
        public async void ChangeSetAfterPushWithPullWithNoChanges()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];

            await session.PushAsync();
            await session.PullAsync(pull);

            var changeSet = session.Checkpoint();

            Assert.Empty(changeSet.Created);
            Assert.Empty(changeSet.AssociationsByRoleType);

            await session.PushAsync();
            changeSet = session.Checkpoint();

            Assert.Empty(changeSet.Created);
            Assert.Empty(changeSet.AssociationsByRoleType);
        }

        [Fact]
        public async void ChangeSetOne2One()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();
            var c1c = session.Create<C1>();

            session.Checkpoint();

            c1a.C1C1One2One = c1b;

            var changeSet = session.Checkpoint();

            Assert.Empty(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);

            c1a.C1C1One2One = c1c;

            changeSet = session.Checkpoint();

            Assert.Empty(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);
        }

        [Fact]
        public async void ChangeSetAfterPushOne2One()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.C1C1One2One = c1b;

            await session.PushAsync();

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);

            await session.PushAsync();
            changeSet = session.Checkpoint();

            Assert.Empty(changeSet.Created);
            Assert.Empty(changeSet.AssociationsByRoleType);
            Assert.Empty(changeSet.RolesByAssociationType);
        }

        [Fact]
        public async void ChangeSetAfterPushOne2OneRemove()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>().First();
            var c1b = session.Create<C1>();

            c1a.C1C1One2One = c1b;

            await session.PushAsync();
            await session.PullAsync(pull);
            session.Checkpoint();

            c1a.RemoveC1C1One2One();

            await session.PushAsync();

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);
        }

        [Fact]
        public async void ChangeSetAfterPushMany2One()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.C1C1Many2One = c1b;

            await session.PushAsync();

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);

            await session.PushAsync();
            changeSet = session.Checkpoint();

            Assert.Empty(changeSet.Created);
            Assert.Empty(changeSet.AssociationsByRoleType);
            Assert.Empty(changeSet.RolesByAssociationType);
        }

        [Fact]
        public async void ChangeSetAfterPushMany2OneRemove()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>().First();
            var c1b = session.Create<C1>();

            await session.PushAsync();
            result = await session.PullAsync(new Pull { Object = c1b });

            c1b = result.GetObject<C1>();

            c1a.C1C1Many2One = c1b;

            await session.PushAsync();

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);

            result = await session.PullAsync(pull);

            c1a.RemoveC1C1Many2One();

            await session.PushAsync();

            changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);
        }

        [Fact]
        public async void ChangeSetAfterPushOne2Many()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.AddC1C1One2Many(c1b);

            await session.PushAsync();

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);

            await session.PushAsync();
            changeSet = session.Checkpoint();

            Assert.Empty(changeSet.Created);
            Assert.Empty(changeSet.AssociationsByRoleType);
            Assert.Empty(changeSet.RolesByAssociationType);
        }

        [Fact]
        public async void ChangeSetAfterPushOne2ManyRemove()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>().First();
            var c1b = session.Create<C1>();

            c1a.AddC1C1One2Many(c1b);

            await session.PushAsync();

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);

            result = await session.PullAsync(pull);

            c1a.RemoveC1C1One2Manies();

            await session.PushAsync();

            changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);
        }

        [Fact]
        public async void ChangeSetMany2Many()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            session.Checkpoint();

            c1a.AddC1C1Many2Many(c1b);

            var changeSet = session.Checkpoint();

            Assert.Empty(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);

            c1a.RemoveC1C1Many2Many(c1b);

            changeSet = session.Checkpoint();

            Assert.Empty(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);
        }

        [Fact]
        public async void ChangeSetAfterPushMany2Many()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.AddC1C1Many2Many(c1b);

            await session.PushAsync();

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);

            await session.PushAsync();
            changeSet = session.Checkpoint();

            Assert.Empty(changeSet.Created);
            Assert.Empty(changeSet.AssociationsByRoleType);
            Assert.Empty(changeSet.RolesByAssociationType);
        }

        [Fact]
        public async void ChangeSetAfterPushMany2ManyRemove()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>().First();
            var c1b = session.Create<C1>();

            c1a.AddC1C1Many2Many(c1b);

            await session.PushAsync();

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);

            await session.PushAsync();
            changeSet = session.Checkpoint();

            Assert.Empty(changeSet.Created);
            Assert.Empty(changeSet.AssociationsByRoleType);
            Assert.Empty(changeSet.RolesByAssociationType);

            result = await session.PullAsync(pull);
            Assert.False(result.HasErrors);

            c1a.RemoveC1C1Many2Manies();

            await session.PushAsync();

            changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);
        }

        [Fact]
        public async void ChangeSetAfterPullInNewSessionButNoPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            await session.PullAsync();

            var changeSet = session.Checkpoint();
            Assert.Empty(changeSet.AssociationsByRoleType);
            Assert.Empty(changeSet.RolesByAssociationType);
            Assert.Empty(changeSet.Instantiated);
            Assert.Empty(changeSet.Created);
        }

        [Fact]
        public async void ChangeSetAfterDoubleDatabaseReset()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a_1 = result.GetCollection<C1>()[0];

            session.Checkpoint();

            c1a_1.C1AllorsString = "X";

            await session.PushAsync();

            result = await session.PullAsync(pull);
            Assert.False(result.HasErrors);

            var c1a_2 = result.GetCollection<C1>()[0];

            c1a_2.C1AllorsString = "Y";

            await session.PushAsync();

            c1a_2.Strategy.Reset();
            c1a_2.Strategy.Reset();

            var changeSet = session.Checkpoint();

            Assert.Empty(changeSet.Created);
            Assert.Empty(changeSet.Instantiated);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Empty(changeSet.RolesByAssociationType);
        }

        [Fact]
        public async void ChangeSetAfterDoubleWorkspaceReset()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a_1 = result.GetCollection<C1>()[0];

            session.Checkpoint();

            c1a_1.C1AllorsString = "X";

            await session.PushAsync();

            result = await session.PullAsync(pull);
            Assert.False(result.HasErrors);

            var c1a_2 = result.GetCollection<C1>()[0];

            c1a_2.C1AllorsString = "Y";

            await session.PushAsync();

            c1a_2.Strategy.Reset();
            c1a_2.Strategy.Reset();

            var changeSet = session.Checkpoint();

            Assert.Empty(changeSet.Created);
            Assert.Empty(changeSet.Instantiated);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Empty(changeSet.RolesByAssociationType);
        }
    }
}
