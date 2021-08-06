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
            var result = await this.AsyncDatabaseClient.PullAsync(session, pull);

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
            var result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";

            await this.AsyncDatabaseClient.PushAsync(session);

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
        }

        [Fact]
        public async void ChangeSetPushChangeNoPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            var c1a_1 = result.GetCollection<C1>()[0];

            c1a_1.C1AllorsString = "X";

            await this.AsyncDatabaseClient.PushAsync(session);

            var changeSet = session.Checkpoint();
            Assert.Single(changeSet.AssociationsByRoleType);

            result = await this.AsyncDatabaseClient.PullAsync(session, pull);
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
            var result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";

            await this.AsyncDatabaseClient.PushAsync(session);

            var changeSet = session.Checkpoint();
            Assert.Single(changeSet.AssociationsByRoleType);

            result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            var c1b = result.GetCollection<C1>()[0];

            c1b.C1AllorsString = "Y";

            await this.AsyncDatabaseClient.PushAsync(session);

            changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
        }

        [Fact]
        public async void ChangeSetAfterPushWithNoChanges()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var c1 = session.Create<C1>();

            await this.AsyncDatabaseClient.PushAsync(session);
            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);

            await this.AsyncDatabaseClient.PushAsync(session);
            changeSet = session.Checkpoint();
            Assert.Empty(changeSet.Created);
        }

        [Fact]
        public async void ChangeSetAfterPushWithPull()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";

            await this.AsyncDatabaseClient.PushAsync(session);

            await this.AsyncDatabaseClient.PullAsync(session, pull);

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
        }

        [Fact]
        public async void ChangeSetAfterPushWithPullWithNoChanges()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            var c1a = result.GetCollection<C1>()[0];

            await this.AsyncDatabaseClient.PushAsync(session);
            await this.AsyncDatabaseClient.PullAsync(session, pull);

            var changeSet = session.Checkpoint();

            Assert.Empty(changeSet.Created);
            Assert.Empty(changeSet.AssociationsByRoleType);

            await this.AsyncDatabaseClient.PushAsync(session);
            changeSet = session.Checkpoint();

            Assert.Empty(changeSet.Created);
            Assert.Empty(changeSet.AssociationsByRoleType);
        }

        [Fact]
        public async void ChangeSetAfterPushOne2One()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.C1C1One2One = c1b;

            await this.AsyncDatabaseClient.PushAsync(session);

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);

            await this.AsyncDatabaseClient.PushAsync(session);
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
            var result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            var c1a = result.GetCollection<C1>().First();
            var c1b = session.Create<C1>();

            c1a.C1C1One2One = c1b;

            await this.AsyncDatabaseClient.PushAsync(session);

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);

            await this.AsyncDatabaseClient.PushAsync(session);
            changeSet = session.Checkpoint();

            Assert.Empty(changeSet.Created);
            Assert.Empty(changeSet.AssociationsByRoleType);
            Assert.Empty(changeSet.RolesByAssociationType);


            result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            //var c1b_2 = (C1)result.Objects.Values.First();

            c1a.RemoveC1C1One2One();

            await this.AsyncDatabaseClient.PushAsync(session);

            changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);
        }

        [Fact]
        public async void ChangeSetAfterPushMany2One()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.C1C1Many2One = c1b;

            await this.AsyncDatabaseClient.PushAsync(session);

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);

            await this.AsyncDatabaseClient.PushAsync(session);
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
            var result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            var c1a = result.GetCollection<C1>().First();
            var c1b = session.Create<C1>();

            await this.AsyncDatabaseClient.PushAsync(session);
            result = await this.AsyncDatabaseClient.PullAsync(session, new Pull { Object = c1b });

            var c2b = result.GetObject<C1>();

            c1a.C1C1Many2One = c1b;

            await this.AsyncDatabaseClient.PushAsync(session);

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);

            result = await this.AsyncDatabaseClient.PullAsync(session, pull);

            c1a.RemoveC1C1Many2One();

            await this.AsyncDatabaseClient.PushAsync(session);

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
            var result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.AddC1C1One2Many(c1b);

            await this.AsyncDatabaseClient.PushAsync(session);

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);

            await this.AsyncDatabaseClient.PushAsync(session);
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
            var result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            var c1a = result.GetCollection<C1>().First();
            var c1b = session.Create<C1>();

            c1a.AddC1C1One2Many(c1b);

            await this.AsyncDatabaseClient.PushAsync(session);

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);

            result = await this.AsyncDatabaseClient.PullAsync(session, pull);

            c1a.RemoveC1C1One2Manies();

            await this.AsyncDatabaseClient.PushAsync(session);

            changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);
        }

        [Fact]
        public async void ChangeSetAfterPushMany2Many()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.AddC1C1Many2Many(c1b);

            await this.AsyncDatabaseClient.PushAsync(session);

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);

            await this.AsyncDatabaseClient.PushAsync(session);
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
            var result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            var c1a = result.GetCollection<C1>().First();
            var c1b = session.Create<C1>();

            c1a.AddC1C1Many2Many(c1b);

            await this.AsyncDatabaseClient.PushAsync(session);

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.Created);
            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);

            await this.AsyncDatabaseClient.PushAsync(session);
            changeSet = session.Checkpoint();

            Assert.Empty(changeSet.Created);
            Assert.Empty(changeSet.AssociationsByRoleType);
            Assert.Empty(changeSet.RolesByAssociationType);

            result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            Assert.False(result.HasErrors);

            c1a.RemoveC1C1Many2Manies();

            await this.AsyncDatabaseClient.PushAsync(session);

            changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Single(changeSet.RolesByAssociationType);
        }

        [Fact]
        public async void ChangeSetAfterPullInNewSessionButNoPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            await this.AsyncDatabaseClient.PullAsync(session);

            var changeSet = session.Checkpoint();
            Assert.Empty(changeSet.AssociationsByRoleType);
            Assert.Empty(changeSet.RolesByAssociationType);
            Assert.Empty(changeSet.Instantiated);
            Assert.Empty(changeSet.Created);
        }

        [Fact]
        public async void ChangeSetBeforeAndAfterResetWithSessionObject()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var sessionC1a = session.Create<SessionC1>();

            sessionC1a.SessionC1AllorsString = "X";

            await this.AsyncDatabaseClient.PushAsync(session);

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);

            sessionC1a.Strategy.Reset();
            changeSet = session.Checkpoint();

            Assert.Empty(changeSet.AssociationsByRoleType);
        }

        [Fact]
        public async void ChangeSetBeforeAndAfterResetWithChangeSessionObject()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var sessionC1a = session.Create<SessionC1>();

            sessionC1a.SessionC1AllorsString = "X";

            await this.AsyncDatabaseClient.PushAsync(session);

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);

            sessionC1a.SessionC1AllorsString = "Y";

            changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
            Assert.Equal("Y", sessionC1a.SessionC1AllorsString);

            sessionC1a.Strategy.Reset();
            changeSet = session.Checkpoint();

            Assert.Empty(changeSet.AssociationsByRoleType);
            Assert.Equal("Y", sessionC1a.SessionC1AllorsString);
        }

        [Fact]
        public async void ChangeSetAfterDoubleReset()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";

            await this.AsyncDatabaseClient.PushAsync(session);

            result = await this.AsyncDatabaseClient.PullAsync(session, pull);
            Assert.False(result.HasErrors);

            var c1b = result.GetCollection<C1>()[0];

            c1b.C1AllorsString = "Y";

            await this.AsyncDatabaseClient.PushAsync(session);

            c1b.Strategy.Reset();
            c1b.Strategy.Reset();

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);
        }
    }
}
