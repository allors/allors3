// <copyright file="Many2OneTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.DatabaseAssociation.DatabaseRelation.DatabaseRole
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Xunit;
    using Allors.Workspace.Data;
    using System;
    using System.Linq;

    public abstract class OneToManyTests : Test
    {
        private Func<Context>[] contextFactories;

        protected OneToManyTests(Fixture fixture) : base(fixture)
        {

        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");

            var singleSessionContext = new SingleSessionContext(this, "Single shared");
            var multipleSessionContext = new MultipleSessionContext(this, "Multiple shared");

            this.contextFactories = new Func<Context>[]
            {
                () => singleSessionContext,
                //() => new SingleSessionContext(this, "Single"),
                //() => multipleSessionContext,
                () => new MultipleSessionContext(this, "Multiple"),
            };
        }

        [Fact]
        public async Task AddMovesRoleFromPreviousAssociation()
        {
            var session = this.Workspace.CreateSession();

            var c1a = session.Create<C1>();
            var c1b = session.Create<C1>();
            var c1c = session.Create<C1>();
            var c1d = session.Create<C1>();

            c1a.AddC1C1One2Many(c1b);
            c1a.AddC1C1One2Many(c1c);

            Assert.False((await session.PushAsync()).HasErrors);
            Assert.False((await session.PullAsync(new Pull { Object = c1a }, new Pull { Object = c1b }, new Pull { Object = c1c }, new Pull { Object = c1d })).HasErrors);

            c1d.AddC1C1One2Many(c1b);

            Assert.Same(c1c, Assert.Single(c1a.C1C1One2Manies));
            Assert.Same(c1b, Assert.Single(c1d.C1C1One2Manies));
            Assert.Same(c1d, c1b.C1WhereC1C1One2Many);

            Assert.False((await session.PushAsync()).HasErrors);
            Assert.False((await session.PullAsync(new Pull { Object = c1a }, new Pull { Object = c1d })).HasErrors);

            Assert.Same(c1c, Assert.Single(c1a.C1C1One2Manies));
            Assert.Same(c1b, Assert.Single(c1d.C1C1One2Manies));
            Assert.Same(c1d, c1b.C1WhereC1C1One2Many);
        }

        [Fact]
        public async void SetRole()
        {
            foreach (DatabaseMode mode1 in Enum.GetValues(typeof(DatabaseMode)))
            {
                foreach (DatabaseMode mode2 in Enum.GetValues(typeof(DatabaseMode)))
                {
                    foreach (var contextFactory in this.contextFactories)
                    {
                        var ctx = contextFactory();
                        var (session1, session2) = ctx;

                        var c1x_1 = await ctx.Create<C1>(session1, mode1);
                        var c1y_2 = await ctx.Create<C1>(session2, mode2);


                        await session2.PushAsync();
                        var result = await session1.PullAsync(new Pull { Object = c1y_2 });

                        var c1y_1 = (C1)result.Objects.Values.First();

                        c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                        if (!c1x_1.CanWriteC1C1One2Manies)
                        {
                            await session1.PullAsync(new Pull { Object = c1x_1 });
                        }

                        c1x_1.AddC1C1One2Many(c1y_1);

                        c1x_1.C1C1One2Manies.ShouldContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1WhereC1C1One2Many.ShouldEqual(c1x_1, ctx, mode1, mode2);
                    }
                }
            }
        }

        [Fact]
        public async void SetRoleToNull()
        {
            foreach (DatabaseMode mode1 in Enum.GetValues(typeof(DatabaseMode)))
            {
                foreach (DatabaseMode mode2 in Enum.GetValues(typeof(DatabaseMode)))
                {
                    foreach (var contextFactory in this.contextFactories)
                    {
                        var ctx = contextFactory();
                        var (session1, session2) = ctx;

                        var c1x_1 = await ctx.Create<C1>(session1, mode1);
                        var c1y_2 = await ctx.Create<C1>(session2, mode2);

                        await session2.PushAsync();
                        var result = await session1.PullAsync(new Pull { Object = c1y_2 });

                        var c1y_1 = (C1)result.Objects.Values.First();

                        c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                        if (!c1x_1.CanWriteC1C1One2Manies)
                        {
                            await session1.PullAsync(new Pull { Object = c1x_1 });
                        }

                        c1x_1.AddC1C1One2Many(null);

                        Assert.Empty(c1x_1.C1C1One2Manies);
                        c1y_1.C1WhereC1C1One2Many.ShouldEqual(null, ctx, mode1, mode2);

                        if (!c1x_1.CanWriteC1C1One2Manies)
                        {
                            await session1.PullAsync(new Pull { Object = c1x_1 });
                        }

                        c1x_1.AddC1C1One2Many(c1y_1);

                        c1x_1.C1C1One2Manies.ShouldContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1WhereC1C1One2Many.ShouldEqual(c1x_1, ctx, mode1, mode2);
                    }
                }
            }
        }

        [Fact]
        public async void RemoveRole()
        {
            foreach (DatabaseMode mode1 in Enum.GetValues(typeof(DatabaseMode)))
            {
                foreach (DatabaseMode mode2 in Enum.GetValues(typeof(DatabaseMode)))
                {
                    foreach (var contextFactory in this.contextFactories)
                    {
                        var ctx = contextFactory();
                        var (session1, session2) = ctx;

                        var c1x_1 = await ctx.Create<C1>(session1, mode1);
                        var c1y_2 = await ctx.Create<C1>(session2, mode2);

                        await session2.PushAsync();
                        var result = await session1.PullAsync(new Pull { Object = c1y_2 });

                        var c1y_1 = (C1)result.Objects.Values.First();

                        c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                        if (!c1x_1.CanWriteC1C1One2Manies)
                        {
                            await session1.PullAsync(new Pull { Object = c1x_1 });
                        }

                        c1x_1.AddC1C1One2Many(c1y_1);

                        c1x_1.C1C1One2Manies.ShouldContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1WhereC1C1One2Many.ShouldEqual(c1x_1, ctx, mode1, mode2);

                        c1x_1.RemoveC1C1One2Many(c1y_1);

                        c1x_1.C1C1One2Manies.ShouldNotContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1WhereC1C1One2Many.ShouldNotEqual(c1x_1, ctx, mode1, mode2);
                    }
                }
            }
        }

        [Fact]
        public async void RemoveNullRole()
        {
            foreach (DatabaseMode mode1 in Enum.GetValues(typeof(DatabaseMode)))
            {
                foreach (DatabaseMode mode2 in Enum.GetValues(typeof(DatabaseMode)))
                {
                    foreach (var contextFactory in this.contextFactories)
                    {
                        var ctx = contextFactory();
                        var (session1, session2) = ctx;

                        var c1x_1 = await ctx.Create<C1>(session1, mode1);
                        var c1y_2 = await ctx.Create<C1>(session2, mode2);

                        await session2.PushAsync();
                        var result = await session1.PullAsync(new Pull { Object = c1y_2 });

                        var c1y_1 = (C1)result.Objects.Values.First();

                        c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                        if (!c1x_1.CanWriteC1C1One2Manies)
                        {
                            await session1.PullAsync(new Pull { Object = c1x_1 });
                        }

                        c1x_1.AddC1C1One2Many(c1y_1);

                        if (!c1x_1.CanWriteC1C1One2Manies)
                        {
                            await session1.PullAsync(new Pull { Object = c1x_1 });
                        }

                        c1x_1.RemoveC1C1One2Many(null);

                        c1x_1.C1C1One2Manies.ShouldContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1WhereC1C1One2Many.ShouldEqual(c1x_1, ctx, mode1, mode2);

                        c1x_1.C1C1One2Manies.ShouldContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1WhereC1C1One2Many.ShouldEqual(c1x_1, ctx, mode1, mode2);

                        if (!c1x_1.CanWriteC1C1One2Manies)
                        {
                            await session1.PullAsync(new Pull { Object = c1x_1 });
                        }

                        c1x_1.RemoveC1C1One2Many(c1y_1);

                        c1x_1.C1C1One2Manies.ShouldNotContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1WhereC1C1One2Many.ShouldNotEqual(c1x_1, ctx, mode1, mode2);
                    }
                }
            }
        }
    }
}
