// <copyright file="ServicesTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class SandboxTests : Test
    {
        private Func<Context>[] contextFactories;

        protected SandboxTests(Fixture fixture) : base(fixture)
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
        public async void Test()
        {
            var contextFactory = this.contextFactories[0];

            {
                var mode1 = DatabaseMode.NoPush;
                var mode2 = DatabaseMode.NoPush;

                var ctx = contextFactory();
                var (session1, session2) = ctx;

                var c1x_1 = await ctx.Create<C1>(session1, mode1);
                var c1y_2 = await ctx.Create<C1>(session2, mode2);

                c1x_1.ShouldNotBeNull(ctx, mode1, mode2);
                c1y_2.ShouldNotBeNull(ctx, mode1, mode2);

                var pushResult = await session2.PushAsync();
                Assert.False(pushResult.HasErrors);

                var result = await session1.PullAsync(new Pull { Object = c1y_2 });

                var c1y_1 = (C1)result.Objects.Values.First();

                c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                if (!c1x_1.CanWriteC1C1Many2One)
                {
                    await session1.PullAsync(new Pull { Object = c1x_1 });
                }

                c1x_1.C1C1Many2One = c1y_1;

                c1x_1.C1C1Many2One.ShouldEqual(c1y_1, ctx, mode1, mode2);
                c1y_1.C1sWhereC1C1Many2One.ShouldContain(c1x_1, ctx, mode1, mode2);

                pushResult = await session1.PushAsync();
                Assert.False(pushResult.HasErrors);

                pushResult = await session2.PushAsync();
                Assert.False(pushResult.HasErrors);
            }

            {
                var mode1 = DatabaseMode.NoPush;
                var mode2 = DatabaseMode.Push;

                var ctx = contextFactory();
                var (session1, session2) = ctx;

                var c1x_1 = await ctx.Create<C1>(session1, mode1);
                var c1y_2 = await ctx.Create<C1>(session2, mode2);

                c1x_1.ShouldNotBeNull(ctx, mode1, mode2);
                c1y_2.ShouldNotBeNull(ctx, mode1, mode2);

                var pushResult = await session2.PushAsync();
                Assert.False(pushResult.HasErrors);

                var result = await session1.PullAsync(new Pull { Object = c1y_2 });

                var c1y_1 = (C1)result.Objects.Values.First();

                c1y_1.ShouldNotBeNull(ctx, mode1, mode2);

                if (!c1x_1.CanWriteC1C1Many2One)
                {
                    await session1.PullAsync(new Pull { Object = c1x_1 });
                }

                c1x_1.C1C1Many2One = c1y_1;

                c1x_1.C1C1Many2One.ShouldEqual(c1y_1, ctx, mode1, mode2);
                c1y_1.C1sWhereC1C1Many2One.ShouldContain(c1x_1, ctx, mode1, mode2);

                pushResult = await session1.PushAsync();
                Assert.False(pushResult.HasErrors);

                pushResult = await session2.PushAsync();
                Assert.False(pushResult.HasErrors);
            }
        }
    }
}
