// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.SessionDatabase
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Allors.Workspace;
    using Xunit;
    using System;
    using Allors.Workspace.Data;
    using System.Linq;

    public abstract class ManyToManyTests : Test
    {
        private Func<ISession, Task>[] pushes;

        private Func<Context>[] contextFactories;

        protected ManyToManyTests(Fixture fixture) : base(fixture)
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
            foreach (var push1 in this.pushes)
            {
                foreach (Mode mode in Enum.GetValues(typeof(Mode)))
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

                        c1x_1.AddSessionC1DatabaseC1Many2Many(c1y_1);

                        c1x_1.SessionC1DatabaseC1Many2Manies.ShouldContains(c1y_1, ctx, mode);

                        await push1(session1);

                        c1x_1.SessionC1DatabaseC1Many2Manies.ShouldContains(c1y_1, ctx, mode);
                    }
                }
            }
        }

        //[Fact]
        //public void SetRole_WithoutPush()
        //{
        //    this.c1.AddSessionC1DatabaseC2Many2Many(this.c2);

        //    Assert.Contains(this.c2, this.c1.SessionC1DatabaseC2Many2Manies);
        //}

        //[Fact]
        //public async void SetRole_WithPush()
        //{
        //    await this.session1.Push();

        //    this.c1.AddSessionC1DatabaseC2Many2Many(this.c2);

        //    Assert.Contains(this.c2, this.c1.SessionC1DatabaseC2Many2Manies);

        //    await this.session1.Push();

        //    Assert.Contains(this.c2, this.c1.SessionC1DatabaseC2Many2Manies);
        //}

        //[Fact]
        //public void RemoveRole_WithoutPush()
        //{
        //    this.c1.AddSessionC1DatabaseC2Many2Many(this.c2);

        //    Assert.Contains(this.c2, this.c1.SessionC1DatabaseC2Many2Manies);

        //    this.c1.RemoveSessionC1DatabaseC2Many2Many(this.c2);

        //    Assert.DoesNotContain(this.c2, this.c1.SessionC1DatabaseC2Many2Manies);
        //}

        //[Fact]
        //public async void RemoveRole_WithPush()
        //{
        //    await this.session1.Push();

        //    this.c1.AddSessionC1DatabaseC2Many2Many(this.c2);

        //    Assert.Contains(this.c2, this.c1.SessionC1DatabaseC2Many2Manies);

        //    await this.session1.Push();

        //    Assert.Contains(this.c2, this.c1.SessionC1DatabaseC2Many2Manies);

        //    this.c1.RemoveSessionC1DatabaseC2Many2Many(this.c2);

        //    Assert.DoesNotContain(this.c2, this.c1.SessionC1DatabaseC2Many2Manies);

        //    await this.session1.Push();

        //    Assert.DoesNotContain(this.c2, this.c1.SessionC1DatabaseC2Many2Manies);
        //}
    }
}
