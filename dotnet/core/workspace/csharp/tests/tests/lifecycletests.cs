// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Xunit;
    using Allors.Workspace.Data;
    using System;

    public abstract class LifecycleTests : Test
    {
        protected LifecycleTests(Fixture fixture) : base(fixture)
        {

        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");
        }

        [Fact]
        public async void PullSameSessionNotPushedException()
        {
            var session = this.Workspace.CreateSession();

            var c1 = session.Create<C1>();
            Assert.NotNull(c1);

            bool hasErrors;

            try
            {
                var result = await session.PullAsync(new Pull { Object = c1 });
                hasErrors = false;
            }
            catch (Exception)
            {
                hasErrors = true;
            }

            Assert.True(hasErrors);
        }

        [Fact]
        public async void PullOtherSessionNotPushedException()
        {
            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<C1>();
            Assert.NotNull(c1);

            var session2 = this.Workspace.CreateSession();

            bool hasErrors;

            try
            {
                var result = await session2.PullAsync(new Pull { Object = c1 });
                hasErrors = false;
            }
            catch (Exception)
            {
                hasErrors = true;
            }

            Assert.True(hasErrors);
        }
    }
}
