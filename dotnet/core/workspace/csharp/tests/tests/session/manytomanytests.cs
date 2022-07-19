// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.DatabaseAssociation.SessionRole
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Allors.Workspace;
    using Xunit;
    using Allors.Workspace.Data;
    using System;
    using System.Linq;

    public abstract class ManyToManyTests : Test
    {
        private Func<Context>[] contextFactories;

        protected ManyToManyTests(Fixture fixture) : base(fixture)
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
        public async void SetRoleOld()
        {
            var session1 = this.Workspace.CreateSession();
            var session2 = this.Workspace.CreateSession();

            var c1a_1 = session1.Create<C1>();
            var c1b_1 = session1.Create<C1>();
            var c1c_1 = session1.Create<C1>();
            var c1x_2 = session2.Create<C1>();
            var c1y_2 = session2.Create<C1>();
            var c1z_2 = session2.Create<C1>();

            await session2.PushAsync();
            await session1.PullAsync(new Pull { Object = c1x_2 }, new Pull { Object = c1y_2 }, new Pull { Object = c1z_2 });

            var c1x_1 = session1.Instantiate(c1x_2);
            var c1y_1 = session1.Instantiate(c1y_2);
            var c1z_1 = session1.Instantiate(c1z_2);

            c1a_1.AddSessionC1Many2Many(c1b_1);
            c1c_1.AddSessionC1Many2Many(c1x_1);
            c1y_1.AddSessionC1Many2Many(c1z_1);

            Assert.Single(c1a_1.SessionC1Many2Manies);
            Assert.Single(c1c_1.SessionC1Many2Manies);
            Assert.Single(c1y_1.SessionC1Many2Manies);
            Assert.Single(c1b_1.C1sWhereSessionC1Many2Many);
            Assert.Single(c1x_1.C1sWhereSessionC1Many2Many);
            Assert.Single(c1z_1.C1sWhereSessionC1Many2Many);
            Assert.Contains(c1a_1, c1b_1.C1sWhereSessionC1Many2Many);
            Assert.Contains(c1c_1, c1x_1.C1sWhereSessionC1Many2Many);
            Assert.Contains(c1y_1, c1z_1.C1sWhereSessionC1Many2Many);

            await session1.PushAsync();

            Assert.Single(c1a_1.SessionC1Many2Manies);
            Assert.Single(c1c_1.SessionC1Many2Manies);
            Assert.Single(c1y_1.SessionC1Many2Manies);
            Assert.Single(c1b_1.C1sWhereSessionC1Many2Many);
            Assert.Single(c1x_1.C1sWhereSessionC1Many2Many);
            Assert.Single(c1z_1.C1sWhereSessionC1Many2Many);
            Assert.Contains(c1a_1, c1b_1.C1sWhereSessionC1Many2Many);
            Assert.Contains(c1c_1, c1x_1.C1sWhereSessionC1Many2Many);
            Assert.Contains(c1y_1, c1z_1.C1sWhereSessionC1Many2Many);
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

                        c1x_1.AddSessionC1Many2Many(c1y_1);

                        Assert.Single(c1x_1.SessionC1Many2Manies);
                        Assert.Single(c1y_1.C1sWhereSessionC1Many2Many);
                        c1x_1.SessionC1Many2Manies.ShouldContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1sWhereSessionC1Many2Many.ShouldContain(c1x_1, ctx, mode1, mode2);
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

                        c1x_1.AddSessionC1Many2Many(null);

                        Assert.Empty(c1x_1.SessionC1Many2Manies);

                        c1x_1.AddSessionC1Many2Many(c1y_1);

                        c1x_1.SessionC1Many2Manies.ShouldContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1sWhereSessionC1Many2Many.ShouldContain(c1x_1, ctx, mode1, mode2);

                        Assert.Single(c1y_1.C1sWhereSessionC1Many2Many.Where(v => v.Equals(c1x_1)));
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

                        c1x_1.ShouldNotBeNull(ctx, mode1, mode2);
                        c1y_2.ShouldNotBeNull(ctx, mode1, mode2);

                        await session2.PushAsync();
                        var result = await session1.PullAsync(new Pull { Object = c1y_2 });

                        var c1y_1 = (C1)result.Objects.Values.First();

                        c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                        c1x_1.AddSessionC1Many2Many(c1y_1);

                        c1x_1.RemoveSessionC1Many2Many(c1y_1);

                        c1x_1.SessionC1Many2Manies.ShouldNotContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1sWhereSessionC1Many2Many.ShouldNotContain(c1x_1, ctx, mode1, mode2);
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

                        c1x_1.ShouldNotBeNull(ctx, mode1, mode2);
                        c1y_2.ShouldNotBeNull(ctx, mode1, mode2);

                        await session2.PushAsync();
                        var result = await session1.PullAsync(new Pull { Object = c1y_2 });

                        var c1y_1 = (C1)result.Objects.Values.First();

                        c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                        c1x_1.AddSessionC1Many2Many(null);
                        Assert.Empty(c1x_1.SessionC1Many2Manies);

                        c1x_1.AddSessionC1Many2Many(c1y_1);

                        c1x_1.SessionC1Many2Manies.ShouldContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1sWhereSessionC1Many2Many.ShouldContain(c1x_1, ctx, mode1, mode2);
                        Assert.Single(c1y_1.C1sWhereSessionC1Many2Many.Where(v => v.Equals(c1x_1)));

                        c1x_1.RemoveSessionC1Many2Many(null);

                        c1x_1.SessionC1Many2Manies.ShouldContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1sWhereSessionC1Many2Many.ShouldContain(c1x_1, ctx, mode1, mode2);

                        c1x_1.RemoveSessionC1Many2Many(c1y_1);

                        c1x_1.SessionC1Many2Manies.ShouldNotContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1sWhereSessionC1Many2Many.ShouldNotContain(c1x_1, ctx, mode1, mode2);
                    }
                }
            }
        }
    }
}
