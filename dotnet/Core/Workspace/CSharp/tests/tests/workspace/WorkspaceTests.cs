// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.OriginWorkspace
{
    using System.Threading.Tasks;
    using Allors.Workspace.Domain;
    using Xunit;
    using System;
    using Allors.Workspace.Data;

    public abstract class WorkspaceTests : Test
    {
        protected WorkspaceTests(Fixture fixture) : base(fixture)
        {

        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");
        }

        [Fact]
        public async void PullingAWorkspaceObjectShouldThrowError()
        {
            var session1 = this.Workspace.CreateSession();

            var c1 = session1.Create<WorkspaceC1>();
            Assert.NotNull(c1);

            await session1.Push();

            var session2 = this.Workspace.CreateSession();
            bool hasErrors;

            try
            {
                var result = await session2.Pull(new Pull { Object = c1 });
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
