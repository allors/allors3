// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.DatabaseAssociation.DatabaseRelation.DatabaseRole
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Allors.Workspace;
    using Xunit;
    using Allors.Workspace.Data;
    using System;
    using System.Linq;

    public abstract class ManyToOneTests : Test
    {

        private Func<Context>[] contextFactories;
        private Func<ISession, Task>[] pushes;

        protected ManyToOneTests(Fixture fixture) : base(fixture)
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

                            if (!c1x_1.CanWriteC1C1Many2One)
                            {
                                await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1x_1 });
                            }

                            c1x_1.C1C1Many2One = c1y_1;

                            c1x_1.C1C1Many2One.ShouldEqual(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1sWhereC1C1Many2One.ShouldContain(c1x_1, ctx, mode1, mode2);

                            await push(session1);

                            c1x_1.C1C1Many2One.ShouldEqual(c1y_1, ctx, mode1);
                            c1y_1.C1sWhereC1C1Many2One.ShouldContain(c1x_1, ctx, mode1, mode2);

                            await this.AsyncDatabaseClient.PushAsync(session1);
                            await this.AsyncDatabaseClient.PushAsync(session2);
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

                            await this.AsyncDatabaseClient.PushAsync(session2);
                            var result = await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1y_2 });

                            var c1y_1 = (C1)result.Objects.Values.First();

                            c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                            if (!c1x_1.CanWriteC1C1Many2One)
                            {
                                await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1x_1 });
                            }

                            c1x_1.C1C1Many2One = c1y_1;
                            c1x_1.C1C1Many2One.ShouldEqual(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1sWhereC1C1Many2One.ShouldContain(c1x_1, ctx, mode1, mode2);

                            c1x_1.RemoveC1C1Many2One();
                            c1x_1.C1C1Many2One.ShouldNotEqual(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1sWhereC1C1Many2One.ShouldNotContain(c1x_1, ctx, mode1, mode2);

                            await push(session1);

                            c1x_1.C1C1Many2One.ShouldNotEqual(c1y_1, ctx, mode1, mode2);
                            c1y_1.C1sWhereC1C1Many2One.ShouldNotContain(c1x_1, ctx, mode1, mode2);
                        }
                    }
                }
            }
        }
    }
}
