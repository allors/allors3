// <copyright file="ChangeSetTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//
// </summary>

namespace Tests.Workspace
{
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Workspace;
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

            Assert.Equal("X", c1a.C1AllorsString);
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

            Assert.Equal("Y", c2a.C1AllorsString);
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

            Assert.Equal(c1b, c1a.C1C1One2One);

            Assert.Equal(c1a, c1b.C1WhereC1C1One2One);
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

            Assert.Equal(c1x, c1a.C1C1One2One);
            Assert.Equal(c1a, c1x.C1WhereC1C1One2One);
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

            Assert.Equal(c1b, c1a.C1C1Many2One);
            Assert.Contains(c1a, c1b.C1sWhereC1C1Many2One);
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

            Assert.Contains(c1b, c1a.C1C1One2Manies);
            Assert.Equal(c1a, c1b.C1WhereC1C1One2Many);
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

            Assert.Contains(c1b, c1a.C1C1Many2Manies);
            Assert.Contains(c1a, c1b.C1sWhereC1C1Many2Many);
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

        [Theory]
        [InlineData("unit")]
        [InlineData("double-unit")]
        [InlineData("one-one")]
        [InlineData("one-one-included")]
        [InlineData("many-one")]
        [InlineData("one-many")]
        [InlineData("many-many")]
        public async Task ResetAfterPushWithoutRefreshUsesPriorBaseline(string shape)
        {
            await this.Login("administrator");
            using var session = this.Workspace.CreateSession();
            var target = session.Create<C1>();
            var related = session.Create<C1>();
            var original = session.Create<C1>();
            if (shape == "double-unit") target.C1AllorsString = "X";
            if (shape == "one-one-included") target.C1C1One2One = original;
            Assert.Equal(MutationOutcome.Committed, (await session.PushAsync()).Outcome);
            session.Checkpoint();

            switch (shape)
            {
                case "unit": target.C1AllorsString = "X"; break;
                case "double-unit": target.C1AllorsString = "Y"; break;
                case "one-one":
                case "one-one-included": target.C1C1One2One = related; break;
                case "many-one": target.C1C1Many2One = related; break;
                case "one-many": target.AddC1C1One2Many(related); break;
                case "many-many": target.AddC1C1Many2Many(related); break;
            }
            var saved = await session.PushAsync(new PushOptions { Refresh = false });
            Assert.Equal(MutationOutcome.Committed, saved.Outcome);
            Assert.Equal(MutationReadStatus.NotRequested, saved.Refresh.Status);
            Assert.Equal(MutationAcknowledgementStatus.Applied, saved.Application.Acknowledgement);
            var role = shape switch
            {
                "unit" or "double-unit" => this.M.C1.C1AllorsString,
                "one-one" or "one-one-included" => (Allors.Workspace.Meta.IRoleType)this.M.C1.C1C1One2One,
                "many-one" => this.M.C1.C1C1Many2One,
                "one-many" => this.M.C1.C1C1One2Manies,
                _ => this.M.C1.C1C1Many2Manies,
            };
            Assert.False(target.Strategy.CanWrite(role));
            target.Strategy.Reset();
            if (shape == "unit" || shape == "double-unit") Assert.Equal(shape == "unit" ? null : "X", target.C1AllorsString);
            if (shape == "one-one" || shape == "one-one-included")
            {
                Assert.Equal(shape == "one-one" ? null : original, target.C1C1One2One);
                Assert.Null(related.C1WhereC1C1One2One);
                if (shape == "one-one-included") Assert.Same(target, original.C1WhereC1C1One2One);
            }
            if (shape == "many-one")
            {
                Assert.Null(target.C1C1Many2One);
                Assert.Empty(related.C1sWhereC1C1Many2One);
            }
            if (shape == "one-many")
            {
                Assert.Empty(target.C1C1One2Manies);
                Assert.Null(related.C1WhereC1C1One2Many);
            }
            if (shape == "many-many")
            {
                Assert.Empty(target.C1C1Many2Manies);
                Assert.Empty(related.C1sWhereC1C1Many2Many);
            }
            Assert.False(target.Strategy.CanWrite(role));
            Assert.False((await session.PullAsync(new Pull { Object = target }, new Pull { Object = related }, new Pull { Object = original })).HasErrors);
            Assert.True(target.Strategy.CanWrite(role));
            if (shape == "unit" || shape == "double-unit") Assert.Equal(shape == "unit" ? "X" : "Y", target.C1AllorsString);
            if (shape == "one-one" || shape == "one-one-included")
            {
                Assert.Same(related, target.C1C1One2One);
                Assert.Same(target, related.C1WhereC1C1One2One);
            }
            if (shape == "many-one")
            {
                Assert.Same(related, target.C1C1Many2One);
                Assert.Contains(target, related.C1sWhereC1C1Many2One);
            }
            if (shape == "one-many")
            {
                Assert.Contains(related, target.C1C1One2Manies);
                Assert.Same(target, related.C1WhereC1C1One2Many);
            }
            if (shape == "many-many")
            {
                Assert.Contains(related, target.C1C1Many2Manies);
                Assert.Contains(target, related.C1sWhereC1C1Many2Many);
            }
        }
    }
}
