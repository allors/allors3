// <copyright file="ObjectTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Origin.Workspace.ToWorkspace
{
    using Allors.Workspace.Domain;
    using Nito.AsyncEx;
    using Xunit;

    public class ManyToOneTests : Test
    {
        [Fact]
        public void SetRole() =>
            AsyncContext.Run(
                async () =>
                {
                    var session1 = this.Workspace.CreateSession();

                    var workspaceOrganisation1 = session1.Create<WorkspaceOrganisation>();
                    var workspacePerson1 = session1.Create<WorkspacePerson>();

                    workspaceOrganisation1.WorkspaceWorkspaceOwner = workspacePerson1;

                    var session2 = this.Workspace.CreateSession();

                    var workspaceOrganisation2 = session2.Instantiate(workspaceOrganisation1);
                    var workspacePerson2 = session2.Instantiate(workspacePerson1);

                    Assert.Equal(workspacePerson2, workspaceOrganisation2.WorkspaceWorkspaceOwner);
                    Assert.Equal(workspacePerson1, workspaceOrganisation1.WorkspaceWorkspaceOwner);
                });

        [Fact]
        public void RemoveRole() =>
            AsyncContext.Run(
                async () =>
                {
                    var session = this.Workspace.CreateSession();

                    var workspaceOrganisation1 = session.Create<WorkspaceOrganisation>();
                    var workspacePerson1 = session.Create<WorkspacePerson>();

                    workspaceOrganisation1.WorkspaceWorkspaceOwner = workspacePerson1;

                    var session2 = this.Workspace.CreateSession();

                    var workspaceOrganisation2 = session2.Instantiate(workspaceOrganisation1);

                    workspaceOrganisation1.RemoveWorkspaceWorkspaceOwner();

                    Assert.Null(workspaceOrganisation2.WorkspaceWorkspaceOwner);
                    Assert.Null(workspaceOrganisation1.WorkspaceWorkspaceOwner);
                });


    }
}
