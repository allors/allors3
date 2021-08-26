// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.DatabaseAssociation.SessionRelation.DatabaseRole
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
        private Func<ISession, Task>[] pushes;

        protected ManyToManyTests(Fixture fixture) : base(fixture)
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
                (_) => Task.CompletedTask,
                (session) =>
                {
                    session.PushToWorkspace();
                    return Task.CompletedTask;
                },
                (session) =>
                {
                    session.PullFromWorkspace();
                    return Task.CompletedTask;
                },
                (session) =>
                {
                    session.PushToWorkspace();
                    session.PullFromWorkspace();
                    return Task.CompletedTask;
                },
                async (session) => await this.AsyncDatabaseClient.PushAsync(session),
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

                            await this.AsyncDatabaseClient.PushAsync(session2);
                            var result = await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1y_2 });

                            var c1y_1 = (C1)result.Objects.Values.First();

                            c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                            c1x_1.AddSessionC1Many2Many(c1y_1);

                            Assert.Single(c1x_1.SessionC1Many2Manies);
                            Assert.Single(c1y_1.C1sWhereSessionC1Many2Many);
                            c1x_1.SessionC1Many2Manies.ShouldContain(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1sWhereSessionC1Many2Many.ShouldContain(c1x_1, ctx, mode1, mode2);

                            await push(session1);

                            Assert.Single(c1x_1.SessionC1Many2Manies);
                            Assert.Single(c1y_1.C1sWhereSessionC1Many2Many);
                            c1x_1.SessionC1Many2Manies.ShouldContain(c1y_1, ctx, mode1);
                            c1y_1.C1sWhereSessionC1Many2Many.ShouldContain(c1x_1, ctx, mode1, mode2);
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

                            await this.AsyncDatabaseClient.PushAsync(session2);
                            var result = await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1y_2 });

                            var c1y_1 = (C1)result.Objects.Values.First();

                            c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                            c1x_1.AddSessionC1Many2Many(null);

                            Assert.Empty(c1x_1.SessionC1Many2Manies);

                            c1x_1.AddSessionC1Many2Many(c1y_1);

                            c1x_1.SessionC1Many2Manies.ShouldContain(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1sWhereSessionC1Many2Many.ShouldContain(c1x_1, ctx, mode1, mode2);

                            Assert.Single(c1y_1.C1sWhereSessionC1Many2Many.Where(v => v.Equals(c1x_1)));

                            await push(session1);

                            c1x_1.SessionC1Many2Manies.ShouldContain(c1y_1, ctx, mode1);
                            c1y_1.C1sWhereSessionC1Many2Many.ShouldContain(c1x_1, ctx, mode1, mode2);
                            Assert.Single(c1y_1.C1sWhereSessionC1Many2Many.Where(v => v.Equals(c1x_1)));
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

                            await this.AsyncDatabaseClient.PushAsync(session2);
                            var result = await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1y_2 });

                            var c1y_1 = (C1)result.Objects.Values.First();

                            c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                            c1x_1.AddSessionC1Many2Many(c1y_1);

                            await push(session1);

                            c1x_1.RemoveSessionC1Many2Many(c1y_1);

                            c1x_1.SessionC1Many2Manies.ShouldNotContain(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1sWhereSessionC1Many2Many.ShouldNotContain(c1x_1, ctx, mode1, mode2);

                            await push(session1);

                            c1x_1.SessionC1Many2Manies.ShouldNotContain(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1sWhereSessionC1Many2Many.ShouldNotContain(c1x_1, ctx, mode1, mode2);
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

                            await this.AsyncDatabaseClient.PushAsync(session2);
                            var result = await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1y_2 });

                            var c1y_1 = (C1)result.Objects.Values.First();

                            c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                            c1x_1.AddSessionC1Many2Many(null);
                            Assert.Empty(c1x_1.SessionC1Many2Manies);

                            c1x_1.AddSessionC1Many2Many(c1y_1);

                            c1x_1.SessionC1Many2Manies.ShouldContain(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1sWhereSessionC1Many2Many.ShouldContain(c1x_1, ctx, mode1, mode2);
                            Assert.Single(c1y_1.C1sWhereSessionC1Many2Many.Where(v => v.Equals(c1x_1)));

                            await push(session1);

                            c1x_1.RemoveSessionC1Many2Many(null);

                            c1x_1.SessionC1Many2Manies.ShouldContain(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1sWhereSessionC1Many2Many.ShouldContain(c1x_1, ctx, mode1, mode2);

                            await push(session1);

                            c1x_1.RemoveSessionC1Many2Many(c1y_1);

                            c1x_1.SessionC1Many2Manies.ShouldNotContain(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1sWhereSessionC1Many2Many.ShouldNotContain(c1x_1, ctx, mode1, mode2);

                            await push(session1);

                            c1x_1.SessionC1Many2Manies.ShouldNotContain(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1sWhereSessionC1Many2Many.ShouldNotContain(c1x_1, ctx, mode1, mode2);
                        }
                    }
                }
            }
        }
    }
}
