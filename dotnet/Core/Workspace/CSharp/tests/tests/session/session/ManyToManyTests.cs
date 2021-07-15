// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.SessionSession
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

            var multipleSessionContext = new MultipleSessionContext(this, "Multiple shared");

            this.contextFactories = new Func<Context>[]
            {
                () => multipleSessionContext,
                () => new MultipleSessionContext(this, "Multiple"),
            };
        }

        [Fact]
        public async void SetRole()
        {
            foreach (var push in this.pushes)
            {
                foreach (Mode mode in Enum.GetValues(typeof(Mode)))
                {
                    foreach (var contextFactory in this.contextFactories)
                    {
                        var ctx = contextFactory();
                        var (session1, session2) = ctx;

                        var c1x_1 = ctx.Session1.Create<SessionC1>();
                        var c1y_2 = ctx.Session1.Create<SessionC1>();

                        c1x_1.ShouldNotBeNull(ctx, mode);
                        c1y_2.ShouldNotBeNull(ctx, mode);

                        await session1.Push();

                        c1x_1.AddSessionC1SessionC1Many2Many(c1y_2);

                        c1x_1.SessionC1SessionC1Many2Manies.ShouldContains(c1y_2, ctx, mode);

                        await push(session1);

                        c1x_1.SessionC1SessionC1Many2Manies.ShouldContains(c1y_2, ctx, mode);
                    }
                }
            }
        }

        [Fact]
        public async void RemoveRole()
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
                        var c1y_2 = ctx.Session1.Create<SessionC1>();

                        c1x_1.ShouldNotBeNull(ctx, mode);
                        c1y_2.ShouldNotBeNull(ctx, mode);

                        await session1.Push();

                        c1x_1.AddSessionC1SessionC1Many2Many(c1y_2);
                        c1x_1.SessionC1SessionC1Many2Manies.ShouldContains(c1y_2, ctx, mode);

                        c1x_1.RemoveSessionC1SessionC1Many2Many(c1y_2);
                        c1x_1.SessionC1SessionC1Many2Manies.ShouldNotContains(c1y_2, ctx, mode);

                        await push1(session1);

                        c1x_1.SessionC1SessionC1Many2Manies.ShouldNotContains(c1y_2, ctx, mode);
                    }
                }
            }
        }

        [Fact]
        public void CrossSessionShouldThrowError()
        {
            var session1 = this.Workspace.CreateSession();
            var session2 = this.Workspace.CreateSession();

            var c1x = session1.Create<SessionC1>();
            var c1y = session2.Create<SessionC1>();
            Assert.NotNull(c1x);
            Assert.NotNull(c1y);

            bool hasErrors;

            try
            {
                c1x.AddSessionC1SessionC1Many2Many(c1y);
                hasErrors = false;
            }
            catch (Exception)
            {
                hasErrors = true;
            }

            Assert.True(hasErrors);
        }

        //[Fact]
        //public void SetRole_WithoutPush()
        //{
        //    this.c1.AddSessionC1SessionC2Many2Many(this.c2);

        //    Assert.Contains(this.c2, this.c1.SessionC1SessionC2Many2Manies);
        //}

        //[Fact]
        //public async void SetRole_WithPush()
        //{
        //    this.c1.AddSessionC1SessionC2Many2Many(this.c2);

        //    Assert.Contains(this.c2, this.c1.SessionC1SessionC2Many2Manies);

        //    await this.session1.Push();

        //    Assert.Contains(this.c2, this.c1.SessionC1SessionC2Many2Manies);
        //}

        //[Fact]
        //public void RemoveRole_WithoutPush()
        //{
        //    this.c1.AddSessionC1SessionC2Many2Many(this.c2);

        //    Assert.Contains(this.c2, this.c1.SessionC1SessionC2Many2Manies);

        //    this.c1.RemoveSessionC1SessionC2Many2Many(this.c2);

        //    Assert.DoesNotContain(this.c2, this.c1.SessionC1SessionC2Many2Manies);
        //}

        //[Fact]
        //public async void RemoveRole_WithPush()
        //{
        //    this.c1.AddSessionC1SessionC2Many2Many(this.c2);

        //    Assert.Contains(this.c2, this.c1.SessionC1SessionC2Many2Manies);

        //    await this.session1.Push();

        //    Assert.Contains(this.c2, this.c1.SessionC1SessionC2Many2Manies);

        //    this.c1.RemoveSessionC1SessionC2Many2Many(this.c2);

        //    Assert.DoesNotContain(this.c2, this.c1.SessionC1SessionC2Many2Manies);

        //    await this.session1.Push();

        //    Assert.DoesNotContain(this.c2, this.c1.SessionC1SessionC2Many2Manies);
        //}
    }
}
