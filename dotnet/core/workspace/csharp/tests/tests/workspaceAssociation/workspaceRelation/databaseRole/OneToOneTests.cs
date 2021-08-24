// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.WorkspaceAssociation.WorkspaceRelation.DatabaseRole
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Allors.Workspace;
    using Xunit;
    using System;
    using Allors.Workspace.Data;
    using System.Linq;

    public abstract class OneToOneTests : Test
    {
        private Func<Context>[] contextFactories;
        private Action<ISession>[] pushes;

        protected OneToOneTests(Fixture fixture) : base(fixture)
        {

        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");

            var singleSessionContext = new SingleSessionContext(this, "Single shared");
            var multipleSessionContext = new MultipleSessionContext(this, "Multiple shared");

            this.pushes = new Action<ISession>[]
            {
                (session) => {},
                 (session) =>  session.PushToWorkspace(),
                 (session) => { session.PushToWorkspace();  session.PullFromWorkspace(); }
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
                foreach (WorkspaceMode mode1 in Enum.GetValues(typeof(WorkspaceMode)))
                {
                    foreach (DatabaseMode mode2 in Enum.GetValues(typeof(DatabaseMode)))
                    {
                        foreach (var contextFactory in this.contextFactories)
                        {
                            var ctx = contextFactory();
                            var (session1, session2) = ctx;

                            var c1x_1 = await ctx.Create<WC1>(session1, mode1);
                            var c1y_2 = await ctx.Create<C1>(session2, mode2);

                            c1x_1.ShouldNotBeNull(ctx, mode1, mode2);
                            c1y_2.ShouldNotBeNull(ctx, mode1, mode2);

                            await this.AsyncDatabaseClient.PushAsync(session2);
                            var result = await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1y_2 });

                            var c1y_1 = (C1)result.Objects.Values.First();

                            session2.PushToWorkspace();
                            c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                            c1x_1.WorkspaceC1One2One = c1y_1;

                            c1x_1.WorkspaceC1One2One.ShouldEqual(c1y_1, ctx, mode1, mode2);
                            c1y_1.WC1WhereWorkspaceC1One2One.ShouldEqual(c1x_1, ctx, mode1, mode2);

                            push(session1);

                            c1x_1.WorkspaceC1One2One.ShouldEqual(c1y_1, ctx, mode1, mode2);
                            c1y_1.WC1WhereWorkspaceC1One2One.ShouldEqual(c1x_1, ctx, mode1, mode2);
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
                foreach (WorkspaceMode mode1 in Enum.GetValues(typeof(WorkspaceMode)))
                {
                    foreach (DatabaseMode mode2 in Enum.GetValues(typeof(DatabaseMode)))
                    {
                        foreach (var contextFactory in this.contextFactories)
                        {
                            var ctx = contextFactory();
                            var (session1, session2) = ctx;

                            var c1x_1 = await ctx.Create<WC1>(session1, mode1);
                            var c1y_2 = await ctx.Create<C1>(session2, mode2);

                            c1x_1.ShouldNotBeNull(ctx, mode1, mode2);
                            c1y_2.ShouldNotBeNull(ctx, mode1, mode2);

                            await this.AsyncDatabaseClient.PushAsync(session2);
                            var result = await this.AsyncDatabaseClient.PullAsync(session1, new Pull { Object = c1y_2 });

                            var c1y_1 = (C1)result.Objects.Values.First();

                            session2.PushToWorkspace();

                            c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                            c1x_1.WorkspaceC1One2One = c1y_1;
                            c1x_1.WorkspaceC1One2One.ShouldEqual(c1y_1, ctx, mode1, mode2);
                            c1y_1.WC1WhereWorkspaceC1One2One.ShouldEqual(c1x_1, ctx, mode1, mode2);

                            c1x_1.RemoveWorkspaceC1One2One();
                            c1x_1.WorkspaceC1One2One.ShouldNotEqual(c1y_1, ctx, mode1, mode2);
                            c1y_1.WC1WhereWorkspaceC1One2One.ShouldNotEqual(c1x_1, ctx, mode1, mode2);

                            push(session1);

                            c1x_1.WorkspaceC1One2One.ShouldNotEqual(c1y_1, ctx, mode1, mode2);
                            c1y_1.WC1WhereWorkspaceC1One2One.ShouldNotEqual(c1x_1, ctx, mode1, mode2);
                        }
                    }
                }
            }
        }
    }
}