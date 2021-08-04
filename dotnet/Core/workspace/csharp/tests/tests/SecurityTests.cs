// <copyright file="Many2OneTests.cs" company="Allors bvba">
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

    public abstract class SecurityTests : Test
    {
        protected SecurityTests(Fixture fixture) : base(fixture)
        {

        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await this.Login("administrator");
        }

        [Fact]
        public async void CanWriteDeniedPermissions()
        {
            var session = this.Workspace.CreateSession();

            var result = await this.AsyncDatabaseClient.PullAsync(session, new Pull { Extent = new Filter(this.M.Denied) });
            var denied = result.GetCollection<Denied>()[0];

            Assert.True(denied.CanReadDefaultWorkspaceProperty);
            Assert.False(denied.CanWriteDefaultWorkspaceProperty);
        }
    }
}
