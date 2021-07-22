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

    public abstract class ResetTests : Test
    {
        protected ResetTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public async void ResetTest()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";

            Assert.Equal("X", c1a.C1AllorsString);

            await session.Push();

            Assert.Equal("X", c1a.C1AllorsString);

            c1a.Strategy.Reset();

            Assert.Null(c1a.C1AllorsString);
        }

        [Fact]
        public async void ResetwithMultipleChanges()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";

            Assert.Equal("X", c1a.C1AllorsString);

            await session.Push();
            result = await session.Pull(pull);
            var c2a = result.GetCollection<C1>()[0];

            Assert.Equal("X", c1a.C1AllorsString);

            c2a.C1AllorsString = "Y";

            await session.Push();

            Assert.Equal("Y", c2a.C1AllorsString);

            c1a.Strategy.Reset();

            Assert.Equal("X", c2a.C1AllorsString);
        }

        [Fact]
        public async void ResetWithOne2One()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.C1C1One2One = c1b;

            Assert.Equal(c1b, c1a.C1C1One2One);
            Assert.Equal(c1a, c1b.C1WhereC1C1One2One);

            await session.Push();

            c1a.Strategy.Reset();

            Assert.Null(c1a.C1C1One2One);
            Assert.Null(c1b.C1WhereC1C1One2One);
        }

        [Fact]
        public async void ResetWithOne2OneRemove()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.C1C1One2One = c1b;

            Assert.Equal(c1b, c1a.C1C1One2One);
            Assert.Equal(c1a, c1b.C1WhereC1C1One2One);

            await session.Push();
            result = await session.Pull(pull);

            c1a.RemoveC1C1One2One();

            Assert.Null(c1a.C1C1One2One);
            Assert.Null(c1b.C1WhereC1C1One2One);

            c1a.Strategy.Reset();

            Assert.Equal(c1b, c1a.C1C1One2One);
            Assert.Equal(c1a, c1b.C1WhereC1C1One2One);
        }

        [Fact]
        public async void ResetWithMany2One()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.C1C1Many2One = c1b;

            Assert.Equal(c1b, c1a.C1C1Many2One);
            Assert.Contains(c1a, c1b.C1sWhereC1C1Many2One);

            await session.Push();

            c1a.Strategy.Reset();

            Assert.Null(c1a.C1C1Many2One);
            Assert.Empty(c1b.C1sWhereC1C1Many2One);
        }

        [Fact]
        public async void ResetWithMany2OneRemove()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.C1C1Many2One = c1b;

            Assert.Equal(c1b, c1a.C1C1Many2One);
            Assert.Contains(c1a, c1b.C1sWhereC1C1Many2One);

            await session.Push();
            result = await session.Pull(pull);

            c1a.RemoveC1C1Many2One();

            Assert.Null(c1a.C1C1Many2One);
            Assert.Empty(c1b.C1sWhereC1C1Many2One);

            c1a.Strategy.Reset();

            Assert.Equal(c1b, c1a.C1C1Many2One);
            Assert.Contains(c1a, c1b.C1sWhereC1C1Many2One);
        }

        [Fact]
        public async void ResetWithOne2Many()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.AddC1C1One2Many(c1b);

            Assert.Contains(c1b, c1a.C1C1One2Manies);
            Assert.Equal(c1a, c1b.C1WhereC1C1One2Many);

            await session.Push();

            c1a.Strategy.Reset();

            Assert.Empty(c1a.C1C1One2Manies);
            Assert.Null(c1b.C1WhereC1C1One2Many);
        }

        [Fact]
        public async void ResetWithOne2ManyRemove()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            await session.Push();
            result = await session.Pull(new Pull { Object = c1b });
            var c1b_2 = (C1)result.Objects.Values.First();

            c1a.AddC1C1One2Many(c1b_2);

            Assert.Contains(c1b, c1a.C1C1One2Manies);
            Assert.Equal(c1a, c1b.C1WhereC1C1One2Many);

            await session.Push();
            result = await session.Pull(pull);
            c1a = result.GetCollection<C1>()[0];

            c1a.RemoveC1C1One2Many(c1b_2);

            Assert.Empty(c1a.C1C1One2Manies);
            Assert.Null(c1b_2.C1WhereC1C1One2Many);

            c1a.Strategy.Reset();

            Assert.Contains(c1b_2, c1a.C1C1One2Manies);
            Assert.Equal(c1a, c1b_2.C1WhereC1C1One2Many);
        }

        [Fact]
        public async void ResetWithMany2Many()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.AddC1C1Many2Many(c1b);

            Assert.Contains(c1b, c1a.C1C1Many2Manies);
            Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);

            await session.Push();

            c1a.Strategy.Reset();

            Assert.Empty(c1a.C1C1Many2Manies);
            Assert.Empty(c1b.C1sWhereC1C1Many2Many);
        }

        [Fact]
        public async void ResetWithMany2ManyRemove()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.AddC1C1Many2Many(c1b);

            Assert.Contains(c1b, c1a.C1C1Many2Manies);
            Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);

            await session.Push();
            result = await session.Pull(new Pull { Object = c1b }, pull);
            var c1b_2 = (C1)result.Objects.Values.First();

            c1a.RemoveC1C1Many2Many(c1b_2);

            Assert.Empty(c1a.C1C1Many2Manies);
            Assert.Empty(c1b.C1sWhereC1C1Many2Many);

            c1a.Strategy.Reset();

            Assert.Contains(c1b, c1a.C1C1Many2Manies);
            Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);
        }

        [Fact]
        public async void ResetWithoutPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.Pull(pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";

            await session.Push();
            result = await session.Pull(pull);
            var c2a = result.GetCollection<C1>()[0];

            var c2aString = c2a.C1AllorsString;

            c1a.Strategy.Reset();

            Assert.Equal(c2aString, c1a.C1AllorsString);

        }
    }
}
