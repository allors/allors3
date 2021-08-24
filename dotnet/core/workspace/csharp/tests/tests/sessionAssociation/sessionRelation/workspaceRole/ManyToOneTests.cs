// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.SessionAssociation.SessionRelation.WorkspaceRole
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Allors.Workspace;
    using Xunit;
    using System;

    public abstract class ManyToOneTests : Test
    {
        private Action<ISession>[] pushes;

        private Func<Context>[] contextFactories;

        protected ManyToOneTests(Fixture fixture) : base(fixture)
        {

        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");

            this.pushes = new Action<ISession>[]
            {
                (session) => { },
                (session) =>  session.PushToWorkspace(),
                (session) => { session.PushToWorkspace();  session.PullFromWorkspace(); }
            };

            var singleSessionContext = new SingleSessionContext(this, "Single shared");
            var multipleSessionContext = new MultipleSessionContext(this, "Multiple shared");

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
                foreach (WorkspaceMode mode in Enum.GetValues(typeof(WorkspaceMode)))
                {
                    foreach (var contextFactory in this.contextFactories)
                    {
                        var ctx = contextFactory();
                        var (session1, session2) = ctx;

                        var c1x_1 = ctx.Session1.Create<SC1>();
                        var c1y_2 = await ctx.Create<WC1>(session2, mode);

                        c1x_1.ShouldNotBeNull(ctx, mode);
                        c1y_2.ShouldNotBeNull(ctx, mode);

                        session2.PushToWorkspace();
                        session1.PullFromWorkspace();

                        var c1y_1 = session1.Instantiate(c1y_2);

                        c1y_1.ShouldNotBeNull(ctx, mode);

                        c1x_1.SessionWC1Many2One = c1y_1;

                        c1x_1.SessionWC1Many2One.ShouldEqual(c1y_1, ctx, mode);
                        c1y_1.SC1sWhereSessionWC1Many2One.ShouldContain(c1x_1, ctx, mode);

                        push(session1);

                        c1x_1.SessionWC1Many2One.ShouldEqual(c1y_1, ctx, mode);
                        c1y_1.SC1sWhereSessionWC1Many2One.ShouldContain(c1x_1, ctx, mode);
                    }
                }
            }
        }

        [Fact]
        public async void RemoveRole()
        {
            foreach (var push in this.pushes)
            {
                foreach (WorkspaceMode mode in Enum.GetValues(typeof(WorkspaceMode)))
                {
                    foreach (var contextFactory in this.contextFactories)
                    {
                        var ctx = contextFactory();
                        var (session1, session2) = ctx;

                        var c1x_1 = ctx.Session1.Create<SC1>();
                        var c1y_2 = await ctx.Create<WC1>(session2, mode);

                        c1x_1.ShouldNotBeNull(ctx, mode);
                        c1y_2.ShouldNotBeNull(ctx, mode);

                        session2.PushToWorkspace();
                        session1.PullFromWorkspace();

                        var c1y_1 = session1.Instantiate(c1y_2);

                        c1y_1.ShouldNotBeNull(ctx, mode);

                        c1x_1.SessionWC1Many2One = c1y_1;
                        c1x_1.SessionWC1Many2One.ShouldEqual(c1y_1, ctx, mode);
                        c1y_1.SC1sWhereSessionWC1Many2One.ShouldContain(c1x_1, ctx, mode);

                        c1x_1.RemoveSessionWC1Many2One();
                        c1x_1.SessionWC1Many2One.ShouldNotEqual(c1y_1, ctx, mode);
                        c1y_1.SC1sWhereSessionWC1Many2One.ShouldNotEqual(c1x_1, ctx, mode);

                        push(session1);

                        c1x_1.SessionWC1Many2One.ShouldNotEqual(c1y_1, ctx, mode);
                        c1y_1.SC1sWhereSessionWC1Many2One.ShouldNotEqual(c1x_1, ctx, mode);
                    }
                }
            }
        }
    }
}