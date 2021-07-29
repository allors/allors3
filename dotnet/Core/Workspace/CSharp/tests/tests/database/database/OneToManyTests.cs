// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.OriginDatabase.DatabaseDatabase
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
        private Func<ISession, Task>[] pushes;

        protected OneToManyTests(Fixture fixture) : base(fixture)
        {

        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");

            var singleSessionContext = new SingleSessionContext(this, "Single shared");
            var multipleSessionContext = new MultipleSessionContext(this, "Multiple shared");

            this.pushes = new Func<ISession, Task>[]
            {
                (session) => Task.CompletedTask,
                async (session) => await session.Push()
            };

            this.contextFactories = new Func<Context>[]
            {
                () => singleSessionContext,
                () => new SingleSessionContext(this, "Single"),
                () => multipleSessionContext,
                () => new MultipleSessionContext(this, "Multiple"),
            };
        }

        [Fact]
        public async void SetRole()
        {
            foreach (var push in this.pushes)
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

                            await session2.Push();
                            var result = await session1.Pull(new Pull { Object = c1y_2 });

                            var c1y_1 = (C1)result.Objects.Values.First();

                            c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                            if (!c1x_1.CanWriteC1C1One2Manies)
                            {
                                await session1.Pull(new Pull { Object = c1x_1 });
                            }

                            c1x_1.AddC1C1One2Many(c1y_1);

                            c1x_1.C1C1One2Manies.ShouldContains(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1WhereC1C1One2Many.ShouldEqual(c1x_1, ctx, mode1, mode2);

                            await push(session1);

                            c1x_1.C1C1One2Manies.ShouldContains(c1y_1, ctx, mode1);
                            c1y_1.C1WhereC1C1One2Many.ShouldEqual(c1x_1, ctx, mode1, mode2);
                        }
                    }
                }
            }
        }

        [Fact]
        public async void SetRoleToNull()
        {
            foreach (var push in this.pushes)
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

                            await session2.Push();
                            var result = await session1.Pull(new Pull { Object = c1y_2 });

                            var c1y_1 = (C1)result.Objects.Values.First();

                            c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                            if (!c1x_1.CanWriteC1C1One2Manies)
                            {
                                await session1.Pull(new Pull { Object = c1x_1 });
                            }

                            c1x_1.AddC1C1One2Many(null);
                            Assert.Empty(c1x_1.C1C1One2Manies);

                            c1x_1.AddC1C1One2Many(c1y_1);

                            c1x_1.C1C1One2Manies.ShouldContains(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1WhereC1C1One2Many.ShouldEqual(c1x_1, ctx, mode1, mode2);

                            await push(session1);

                            c1x_1.C1C1One2Manies.ShouldContains(c1y_1, ctx, mode1);
                            c1y_1.C1WhereC1C1One2Many.ShouldEqual(c1x_1, ctx, mode1, mode2);
                        }
                    }
                }
            }
        }

        [Fact]
        public async void RemoveRole()
        {
            foreach (var push in this.pushes)
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

                            await session2.Push();
                            var result = await session1.Pull(new Pull { Object = c1y_2 });

                            var c1y_1 = (C1)result.Objects.Values.First();

                            c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                            if (!c1x_1.CanWriteC1C1One2Manies)
                            {
                                await session1.Pull(new Pull { Object = c1x_1 });
                            }

                            c1x_1.AddC1C1One2Many(c1y_1);

                            c1x_1.C1C1One2Manies.ShouldContains(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1WhereC1C1One2Many.ShouldEqual(c1x_1, ctx, mode1, mode2);

                            c1x_1.RemoveC1C1One2Many(c1y_1);

                            c1x_1.C1C1One2Manies.ShouldNotContains(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1WhereC1C1One2Many.ShouldNotEqual(c1x_1, ctx, mode1, mode2);

                            await push(session1);

                            c1x_1.C1C1One2Manies.ShouldNotContains(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1WhereC1C1One2Many.ShouldNotEqual(c1x_1, ctx, mode1, mode2);
                        }
                    }
                }
            }
        }

        [Fact]
        public async void RemoveNullRole()
        {
            foreach (var push in this.pushes)
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

                            await session2.Push();
                            var result = await session1.Pull(new Pull { Object = c1y_2 });

                            var c1y_1 = (C1)result.Objects.Values.First();

                            c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                            if (!c1x_1.CanWriteC1C1One2Manies)
                            {
                                await session1.Pull(new Pull { Object = c1x_1 });
                            }

                            c1x_1.AddC1C1One2Many(null);
                            Assert.Empty(c1x_1.C1C1One2Manies);

                            c1x_1.AddC1C1One2Many(c1y_1);

                            c1x_1.C1C1One2Manies.ShouldContains(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1WhereC1C1One2Many.ShouldEqual(c1x_1, ctx, mode1, mode2);

                            c1x_1.RemoveC1C1One2Many(null);
                            c1x_1.C1C1One2Manies.ShouldContains(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1WhereC1C1One2Many.ShouldEqual(c1x_1, ctx, mode1, mode2);

                            c1x_1.RemoveC1C1One2Many(c1y_1);

                            c1x_1.C1C1One2Manies.ShouldNotContains(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1WhereC1C1One2Many.ShouldNotEqual(c1x_1, ctx, mode1, mode2);

                            await push(session1);

                            c1x_1.C1C1One2Manies.ShouldNotContains(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1WhereC1C1One2Many.ShouldNotEqual(c1x_1, ctx, mode1, mode2);
                        }
                    }
                }
            }
        }
    }
}
