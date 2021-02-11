// <copyright file="ObjectTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using Allors.Workspace.Domain;
    using Remote;
    using Xunit;

    public abstract class LifecycleTests : Test
    {
        protected LifecycleTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async void Workspace_Instantiate()
        {
            var session1 = this.Workspace.CreateSession();

            var workspaceOrganisation1 = session1.Create<WorkspaceOrganisation>();

            var session2 = this.Workspace.CreateSession();

            var workspaceOrganisation2 = session2.Instantiate(workspaceOrganisation1);

            Assert.NotNull(workspaceOrganisation1);
        }

        [Fact]
        public async void Session_Instantiate()
        {
            var session1 = this.Workspace.CreateSession();

            var sessionOrganisation1 = session1.Create<SessionOrganisation>();
            
            var session2 = this.Workspace.CreateSession();

            var sessionOrganisation2 = session2.Instantiate(sessionOrganisation1);

            Assert.Null(sessionOrganisation2);
        }
    }
}
