// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.OriginSession
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Allors.Workspace;
    using Xunit;
    using System;
    using Allors.Workspace.Data;
    using System.Linq;

    public abstract class SessionTests : Test
    {
        protected SessionTests(Fixture fixture) : base(fixture)
        {

        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");
        }

        [Fact]
        public async void PullingASessionObjectShouldThrowError()
        {
            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<SessionC1>();
            Assert.NotNull(c1);

            await session1.Push();

            var session2 = this.Workspace.CreateSession();
            bool hasErrors;

            try
            {
                var result = await session2.Pull(new Pull { Object = c1 });
                hasErrors = false;
            }
            catch (ArgumentException)
            {
                hasErrors = true;
            }

            Assert.True(hasErrors);
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
    }
}
