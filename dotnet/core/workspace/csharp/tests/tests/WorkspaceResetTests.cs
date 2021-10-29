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

    public abstract class WorkspaceResetTests : Test
    {
        protected WorkspaceResetTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public async void ResetUnitWithoutPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";

            await session.PushAsync();
            result = await session.PullAsync(pull);
            var c2a = result.GetCollection<C1>()[0];

            var c2aString = c2a.C1AllorsString;

            c1a.Strategy.Reset();

            Assert.Equal(c2aString, c1a.C1AllorsString);

        }

        [Fact]
        public async void ResetUnitAfterPushTest()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";

            await session.PushAsync();

            Assert.Equal("X", c1a.C1AllorsString);

            c1a.Strategy.Reset();

            Assert.Null(c1a.C1AllorsString);
        }

        [Fact]
        public async void ResetUnitAfterDoublePush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];

            c1a.C1AllorsString = "X";

            await session.PushAsync();
            result = await session.PullAsync(pull);
            var c2a = result.GetCollection<C1>()[0];

            c2a.C1AllorsString = "Y";

            await session.PushAsync();

            c1a.Strategy.Reset();

            Assert.Equal("X", c2a.C1AllorsString);
        }

        [Fact]
        public async void ResetOne2OneWithoutPush()
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

            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1x = session.Create<C1>();

            c1a.C1C1One2One = c1x;

            c1a.Strategy.Reset();

            Assert.NotNull(Record.Exception(() =>
            {
                var x = c1a.C1C1One2One;
            }));

            Assert.Null(c1x.C1WhereC1C1One2One);
        }

        [Fact]
        public async void ResetOne2OneIncludeWithoutPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new Equals(this.M.C1.Name) { Value = "c1A" }
                },
                Results = new[]
                {
                    new Result
                    {
                        Include = new[]{ new Node(this.M.C1.C1C1One2One)}
                    }
                }
            };

            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = c1a.C1C1One2One;
            var c1x = session.Create<C1>();

            c1a.C1C1One2One = c1x;

            c1a.Strategy.Reset();

            Assert.Equal(c1b, c1a.C1C1One2One);
            Assert.Null(c1x.C1WhereC1C1One2One);
        }

        [Fact]
        public async void ResetOne2OneAfterPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.C1C1One2One = c1b;

            await session.PushAsync();

            c1a.Strategy.Reset();

            Assert.NotNull(Record.Exception(() =>
            {
                var x = c1a.C1C1One2One;
            }));

            Assert.Null(c1b.C1WhereC1C1One2One);
        }

        [Fact]
        public async void ResetOne2OneIncludeAfterPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull
            {
                Extent = new Filter(this.M.C1)
                {
                    Predicate = new Equals(this.M.C1.Name) { Value = "c1A" }
                },
                Results = new[]
                {
                    new Result
                    {
                        Include = new[]{ new Node(this.M.C1.C1C1One2One)}
                    }
                }
            };

            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = c1a.C1C1One2One;
            var c1x = session.Create<C1>();

            c1a.C1C1One2One = c1x;

            await session.PushAsync();

            c1a.Strategy.Reset();

            Assert.Equal(c1b, c1a.C1C1One2One);
            Assert.Null(c1x.C1WhereC1C1One2One);
        }

        [Fact]
        public async void ResetOne2OneRemoveAfterPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.C1C1One2One = c1b;

            Assert.Equal(c1b, c1a.C1C1One2One);
            Assert.Equal(c1a, c1b.C1WhereC1C1One2One);

            await session.PushAsync();
            result = await session.PullAsync(pull);

            c1a.RemoveC1C1One2One();

            Assert.Null(c1a.C1C1One2One);
            Assert.Null(c1b.C1WhereC1C1One2One);

            c1a.Strategy.Reset();

            Assert.Equal(c1b, c1a.C1C1One2One);
            Assert.Equal(c1a, c1b.C1WhereC1C1One2One);
        }

        [Fact]
        public async void ResetMany2OneWithoutPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.C1C1Many2One = c1b;

            c1a.Strategy.Reset();

            Assert.Null(c1a.C1C1Many2One);
            Assert.Empty(c1b.C1sWhereC1C1Many2One);
        }

        [Fact]
        public async void ResetMany2OneAfterPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.C1C1Many2One = c1b;

            await session.PushAsync();

            c1a.Strategy.Reset();

            Assert.Null(c1a.C1C1Many2One);
            Assert.Empty(c1b.C1sWhereC1C1Many2One);
        }

        [Fact]
        public async void ResetMany2OneRemoveAfterPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.C1C1Many2One = c1b;

            Assert.Equal(c1b, c1a.C1C1Many2One);
            Assert.Contains(c1a, c1b.C1sWhereC1C1Many2One);

            await session.PushAsync();
            result = await session.PullAsync(pull);

            c1a.RemoveC1C1Many2One();

            Assert.Null(c1a.C1C1Many2One);
            Assert.Empty(c1b.C1sWhereC1C1Many2One);

            c1a.Strategy.Reset();

            Assert.Equal(c1b, c1a.C1C1Many2One);
            Assert.Contains(c1a, c1b.C1sWhereC1C1Many2One);
        }

        [Fact]
        public async void ResetOne2ManyWithoutPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.AddC1C1One2Many(c1b);

            c1a.Strategy.Reset();

            Assert.Empty(c1a.C1C1One2Manies);
            Assert.Null(c1b.C1WhereC1C1One2Many);
        }

        [Fact]
        public async void ResetOne2ManyAfterPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.AddC1C1One2Many(c1b);

            await session.PushAsync();

            c1a.Strategy.Reset();

            Assert.Empty(c1a.C1C1One2Manies);
            Assert.Null(c1b.C1WhereC1C1One2Many);
        }

        [Fact]
        public async void ResetOne2ManyRemoveAfterPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            await session.PushAsync();
            result = await session.PullAsync(new Pull { Extent = new Filter(M.C1) });

            c1a.AddC1C1One2Many(c1b);

            await session.PushAsync();
            await session.PullAsync(pull);

            c1a.RemoveC1C1One2Many(c1b);

            c1a.Strategy.Reset();

            Assert.Contains(c1b, c1a.C1C1One2Manies);
            Assert.Equal(c1a, c1b.C1WhereC1C1One2Many);
        }

        [Fact]
        public async void ResetMany2ManyWithoutPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.AddC1C1Many2Many(c1b);

            c1a.Strategy.Reset();

            Assert.Empty(c1a.C1C1Many2Manies);
            Assert.Empty(c1b.C1sWhereC1C1Many2Many);
        }

        [Fact]
        public async void ResetMany2ManyAfterPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            c1a.AddC1C1Many2Many(c1b);

            await session.PushAsync();

            c1a.Strategy.Reset();

            Assert.Empty(c1a.C1C1Many2Manies);
            Assert.Empty(c1b.C1sWhereC1C1Many2Many);
        }

        [Fact]
        public async void ResetMany2ManyRemoveAfterPush()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var pull = new Pull { Extent = new Filter(this.M.C1) { Predicate = new Equals(this.M.C1.Name) { Value = "c1A" } } };
            var result = await session.PullAsync(pull);
            var c1a = result.GetCollection<C1>()[0];
            var c1b = session.Create<C1>();

            await session.PushAsync();
            result = await session.PullAsync(new Pull { Object = c1b });
            var c1b_2 = (C1)result.Objects.Values.First();

            c1a.AddC1C1Many2Many(c1b_2);

            Assert.Contains(c1b_2, c1a.C1C1Many2Manies);
            Assert.Contains(c1a, c1b_2.C1sWhereC1C1Many2Many);

            await session.PushAsync();
            result = await session.PullAsync(pull);
            c1a = result.GetCollection<C1>()[0];

            c1a.RemoveC1C1Many2Many(c1b_2);

            Assert.Empty(c1a.C1C1Many2Manies);
            Assert.Empty(c1b_2.C1sWhereC1C1Many2Many);

            c1a.Strategy.Reset();

            Assert.Contains(c1b, c1a.C1C1Many2Manies);
            Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);
        }
    }
}
