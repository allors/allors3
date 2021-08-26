// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.DatabaseAssociation.SessionRelation.WorkspaceRole
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Allors.Workspace;
    using Xunit;
    using System;

    public abstract class ManyToOneTests : Test
    {
        private Func<Context>[] contextFactories;
        private Func<ISession, Task>[] databasePushes;
        private Action<ISession>[] workspacePushes;

        protected ManyToOneTests(Fixture fixture) : base(fixture)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");

            var singleSessionContext = new SingleSessionContext(this, "Single shared");
            var multipleSessionContext = new MultipleSessionContext(this, "Multiple shared");

            this.databasePushes = new Func<ISession, Task>[]
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

            this.workspacePushes = new Action<ISession>[]
            {
                (_) => { },
                (session) => session.PushToWorkspace(),
                (session) => session.PullFromWorkspace(),
                (session) =>
                {
                    session.PushToWorkspace();
                    session.PullFromWorkspace();
                },
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
            foreach (var databasePush in this.databasePushes)
            {
                foreach (var workspacePush in this.workspacePushes)
                {
                    foreach (DatabaseMode mode1 in Enum.GetValues(typeof(DatabaseMode)))
                    {
                        foreach (WorkspaceMode mode2 in Enum.GetValues(typeof(WorkspaceMode)))
                        {
                            foreach (var contextFactory in this.contextFactories)
                            {
                                var ctx = contextFactory();
                                var (session1, session2) = ctx;

                                var c1x_1 = await ctx.Create<C1>(session1, mode1);
                                var c1y_2 = ctx.Create<WC1>(session2, mode2);

                                session2.PushToWorkspace();

                                var c1y_1 = session1.Instantiate(c1y_2);

                                workspacePush(session2);

                                c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                                c1x_1.SessionWC1Many2One = c1y_1;

                                c1x_1.SessionWC1Many2One.ShouldEqual(c1y_1, ctx, mode1, mode2);
                                c1y_1.C1sWhereSessionWC1Many2One.ShouldContain(c1x_1, ctx, mode1, mode2);

                                await databasePush(session1);
                                workspacePush(session2);

                                c1x_1.SessionWC1Many2One.ShouldEqual(c1y_1, ctx, mode1, mode2);
                                c1y_1.C1sWhereSessionWC1Many2One.ShouldContain(c1x_1, ctx, mode1, mode2);
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        public async void RemoveRole()
        {
            foreach (var databasePush in this.databasePushes)
            {
                foreach (var workspacePush in this.workspacePushes)
                {
                    foreach (DatabaseMode mode1 in Enum.GetValues(typeof(DatabaseMode)))
                    {
                        foreach (WorkspaceMode mode2 in Enum.GetValues(typeof(WorkspaceMode)))
                        {
                            foreach (var contextFactory in this.contextFactories)
                            {
                                var ctx = contextFactory();
                                var (session1, session2) = ctx;

                                var c1x_1 = await ctx.Create<C1>(session1, mode1);
                                var c1y_2 = ctx.Create<WC1>(session2, mode2);

                                session2.PushToWorkspace();

                                var c1y_1 = session1.Instantiate(c1y_2);

                                workspacePush(session2);

                                c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                                c1x_1.SessionWC1Many2One = c1y_1;

                                c1x_1.SessionWC1Many2One.ShouldEqual(c1y_1, ctx, mode1, mode2);
                                c1y_1.C1sWhereSessionWC1Many2One.ShouldContain(c1x_1, ctx, mode1, mode2);

                                c1x_1.RemoveSessionWC1Many2One();

                                c1x_1.SessionWC1Many2One.ShouldNotEqual(c1y_1, ctx, mode1, mode2);
                                c1y_1.C1sWhereSessionWC1Many2One.ShouldNotContain(c1x_1, ctx, mode1, mode2);

                                await databasePush(session1);
                                workspacePush(session2);

                                c1x_1.SessionWC1Many2One.ShouldNotEqual(c1y_1, ctx, mode1, mode2);
                                c1y_1.C1sWhereSessionWC1Many2One.ShouldNotContain(c1x_1, ctx, mode1, mode2);
                            }
                        }
                    }
                }
            }
        }
    }
}
