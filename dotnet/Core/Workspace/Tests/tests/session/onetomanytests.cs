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

                        c1x_1.AddSessionC1One2Many(c1y_1);

                        c1x_1.SessionC1One2Manies.ShouldContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1WhereSessionC1One2Many.ShouldEqual(c1x_1, ctx, mode1, mode2);
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

                        c1x_1.AddSessionC1One2Many(null);

                        Assert.Empty(c1x_1.SessionC1One2Manies);
                        c1y_1.C1WhereSessionC1One2Many.ShouldEqual(null, ctx, mode1, mode2);

                        c1x_1.AddSessionC1One2Many(c1y_1);

                        c1x_1.SessionC1One2Manies.ShouldContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1WhereSessionC1One2Many.ShouldEqual(c1x_1, ctx, mode1, mode2);
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

                        c1x_1.AddSessionC1One2Many(c1y_1);

                        c1x_1.SessionC1One2Manies.ShouldContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1WhereSessionC1One2Many.ShouldEqual(c1x_1, ctx, mode1, mode2);

                        c1x_1.RemoveSessionC1One2Many(c1y_1);

                        c1x_1.SessionC1One2Manies.ShouldNotContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1WhereSessionC1One2Many.ShouldNotEqual(c1x_1, ctx, mode1, mode2);
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

                        c1x_1.AddSessionC1One2Many(c1y_1);

                        c1x_1.RemoveSessionC1One2Many(null);

                        c1x_1.SessionC1One2Manies.ShouldContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1WhereSessionC1One2Many.ShouldEqual(c1x_1, ctx, mode1, mode2);

                        c1x_1.RemoveSessionC1One2Many(c1y_1);

                        c1x_1.SessionC1One2Manies.ShouldNotContain(c1y_1, ctx, mode1, mode2);
                        c1y_1.C1WhereSessionC1One2Many.ShouldNotEqual(c1x_1, ctx, mode1, mode2);
                    }
                }
            }
        }
    }
}
