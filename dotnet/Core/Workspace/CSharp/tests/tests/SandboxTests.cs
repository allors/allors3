// <copyright file="ServicesTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Antlr.Runtime.Misc;
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

            var singleSessionContext = new SingleSessionContext(this);
            var multipleSessionContext = new MultipleSessionContext(this);

            this.contextFactories = new Func<Context>[]
            {
                () => singleSessionContext,
                () => new SingleSessionContext(this),
                () => multipleSessionContext,
                () => new MultipleSessionContext(this),
            };
        }

        [Fact]
        public async void Test()
        {
            foreach (Mode mode in System.Enum.GetValues(typeof(Mode)))
            {
                foreach (var contextFactory in this.contextFactories)
                {
                    var ctx = contextFactory();
                    var (session1, session2) = ctx;

                    var c1x_1 = await ctx.Create<C1>(session1, mode);
                    var c1y_2 = await ctx.Create<C1>(session2, mode);

                    Assert.NotNull(c1x_1);
                    Assert.NotNull(c1y_2);

                    await session2.Push();
                    var result = await session1.Pull(new Pull { Object = c1y_2 });

                    var c1y_1 = (C1)result.Objects.Values.First();

                    Assert.NotNull(c1y_1);

                    if (!c1x_1.CanWriteC1C1One2One)
                    {
                        await session1.Pull(new Pull { Object = c1x_1 });
                    }

                    c1x_1.C1C1One2One = c1y_1;

                    Assert.Equal(c1y_1, c1x_1.C1C1One2One);
                }
            }

        }
    }
}
