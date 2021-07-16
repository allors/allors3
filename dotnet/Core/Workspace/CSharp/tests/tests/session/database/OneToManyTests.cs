// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.OriginSession.SessionDatabase
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Allors.Workspace;
    using Xunit;
    using System;
    using Allors.Workspace.Data;
    using System.Linq;

    public abstract class OneToManyTests : Test
    {
        private Func<ISession, Task>[] pushes;

        private Func<Context>[] contextFactories;

        protected OneToManyTests(Fixture fixture) : base(fixture)
        {

        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");

            this.pushes = new Func<ISession, Task>[]
            {
                (session) => Task.CompletedTask,
                async (session) => await session.Push()
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
                foreach (DatabaseMode mode in Enum.GetValues(typeof(DatabaseMode)))
                {
                    foreach (var contextFactory in this.contextFactories)
                    {
                        var ctx = contextFactory();
                        var (session1, session2) = ctx;

                        var c1x_1 = ctx.Session1.Create<SessionC1>();
                        var c1y_2 = await ctx.Create<C1>(session2, mode);

                        c1x_1.ShouldNotBeNull(ctx, mode);
                        c1y_2.ShouldNotBeNull(ctx, mode);

                        await session2.Push();

                        var result = await session1.Pull(new Pull { Object = c1y_2 });

                        var c1y_1 = (C1)result.Objects.Values.First();

                        c1y_1.ShouldNotBeNull(ctx, mode);

                        c1x_1.AddSessionC1DatabaseC1One2Many(c1y_1);

                        c1x_1.SessionC1DatabaseC1One2Manies.ShouldContains(c1y_1, ctx, mode);

                        await push(session1);

                        c1x_1.SessionC1DatabaseC1One2Manies.ShouldContains(c1y_1, ctx, mode);
                    }
                }
            }
        }

        [Fact]
        public async void RemoveRole()
        {
            foreach (var push in this.pushes)
            {
                foreach (DatabaseMode mode in Enum.GetValues(typeof(DatabaseMode)))
                {
                    foreach (var contextFactory in this.contextFactories)
                    {
                        var ctx = contextFactory();
                        var (session1, session2) = ctx;

                        var c1x_1 = ctx.Session1.Create<SessionC1>();
                        var c1y_2 = await ctx.Create<C1>(session2, mode);

                        c1x_1.ShouldNotBeNull(ctx, mode);
                        c1y_2.ShouldNotBeNull(ctx, mode);

                        await session2.Push();

                        var result = await session1.Pull(new Pull { Object = c1y_2 });

                        var c1y_1 = (C1)result.Objects.Values.First();

                        c1y_1.ShouldNotBeNull(ctx, mode);

                        c1x_1.AddSessionC1DatabaseC1One2Many(c1y_1);
                        c1x_1.SessionC1DatabaseC1One2Manies.ShouldContains(c1y_1, ctx, mode);

                        c1x_1.RemoveSessionC1DatabaseC1One2Many(c1y_1);
                        c1x_1.SessionC1DatabaseC1One2Manies.ShouldNotContains(c1y_1, ctx, mode);

                        await push(session1);

                        c1x_1.SessionC1DatabaseC1One2Manies.ShouldNotContains(c1y_1, ctx, mode);
                    }
                }
            }
        }
    }
}
