// <copyright file="SaveTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System;
    using System.Linq;
    using Allors.Workspace;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;
    using Version = Allors.Version;

    public abstract class PushTests : Test
    {
        protected PushTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async void PushNewObject()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var newObject = session.Create<C1>();

            var result = await session.Push();
            Assert.False(result.HasErrors);

            await session.Pull(new Pull { Object = newObject });

            foreach (var roleType in this.M.C1.RoleTypes)
            {
                Assert.False(newObject.Strategy.ExistRole(roleType));
            }

            foreach (var associationType in this.M.C1.AssociationTypes)
            {
                if (associationType.IsOne)
                {
                    var association = newObject.Strategy.GetCompositeAssociation<IObject>(associationType);
                    Assert.Null(association);
                }
                else
                {
                    var association = newObject.Strategy.GetCompositesAssociation<IObject>(associationType);
                    Assert.Empty(association);
                }
            }
        }

        [Fact]
        public async void PushNewObjectWithChangedRoles()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var newObject = session.Create<C1>();
            newObject.C1AllorsString = "A new object";

            var result = await session.Push();
            Assert.False(result.HasErrors);

            await session.Pull(new Pull { Object = newObject });

            Assert.Equal("A new object", newObject.C1AllorsString);
        }

        [Fact]
        public async void PushExistingObjectWithChangedRoles()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new Equals(this.M.C1.Name) { Value = "c1A" }
                }
            };

            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";

            Assert.Equal("X", c1a.C1AllorsString);

            await session.Push();
            await session.Pull(pull);

            Assert.Equal("X", c1a.C1AllorsString);
        }


        [Fact]
        public async void ChangesBeforeCheckpointShouldBePushed()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
            };

            var result = await session.Pull(pull);

            var c1a = result.GetCollection<C1>().First(v => v.Name.Equals("c1A"));

            c1a.C1AllorsString = "X";

            var changeSet = session.Checkpoint();

            Assert.Single(changeSet.AssociationsByRoleType);

            await session.Push();

            var session2 = this.Workspace.CreateSession();

            result = await session2.Pull(new Pull { Object = c1a });

            var c1aSession2 = result.GetObject<C1>();

            Assert.Equal("X", c1aSession2.C1AllorsString);

            result = await session.Pull(new Pull { Object = c1a });

            var c1aSession1 = result.GetObject<C1>();

            Assert.Equal("X", c1aSession1.C1AllorsString);
        }

        [Fact]
        public async void PushShouldUpdateId()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var person = session.Create<Person>();
            person.FirstName = "Johny";
            person.LastName = "Doey";

            Assert.True(person.Id < 0);

            Assert.False((await session.Push()).HasErrors);

            Assert.True(person.Id > 0);
        }

        [Fact]
        public async void PushShouldNotUpdateVersion()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var person = session.Create<Person>();
            person.FirstName = "Johny";
            person.LastName = "Doey";

            Assert.Equal(Version.WorkspaceInitial.Value, person.Strategy.Version);

            Assert.False((await session.Push()).HasErrors);

            Assert.Equal(Version.WorkspaceInitial.Value, person.Strategy.Version);
        }

        [Fact]
        public async void PushShouldDerive()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var person = session.Create<Person>();
            person.FirstName = "Johny";
            person.LastName = "Doey";

            Assert.False((await session.Push()).HasErrors);

            var pull = new Pull
            {
                Object = person
            };

            Assert.False((await session.Pull(pull)).HasErrors);

            Assert.Equal("Johny Doey", person.DomainFullName);
        }

        [Fact]
        public async void PushTwice()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var c1x_1 = session.Create<C1>();
            var c1y_2 = session.Create<C1>();

            await session.Push();

            var result = await session.Pull(new Pull { Object = c1y_2 });

            var c1y_1 = (C1)result.Objects.Values.First();

            if (!c1x_1.CanWriteC1C1Many2One)
            {
                await session.Pull(new Pull { Object = c1x_1 });
            }

            c1x_1.C1C1Many2One = c1y_1;

            var pushResult = await session.Push();
            Assert.False(pushResult.HasErrors);

            pushResult = await session.Push();
            Assert.False(pushResult.HasErrors);
        }
    }
}
