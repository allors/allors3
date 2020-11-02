// <copyright file="ObjectTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Origin.Workspace
{
    using Allors.Workspace.Domain;
    using Xunit;

    public class LifecycleTests : Test
    {
        [Fact]
        public async void Instantiate()
        {
            var session1 = this.Workspace.CreateSession();

            var workspaceOrganisation1 = session1.Create<WorkspaceOrganisation>();

            var session2 = this.Workspace.CreateSession();

            var workspaceOrganisation2 = session2.Instantiate(workspaceOrganisation1);

            Assert.NotNull(workspaceOrganisation1);
        }
    }
}
