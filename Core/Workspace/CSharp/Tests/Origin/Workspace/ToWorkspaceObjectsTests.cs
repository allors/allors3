// <copyright file="ObjectTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Origin.Workspace
{
    using Allors.Workspace.Domain;
    using Nito.AsyncEx;
    using Xunit;

    public class ToWorkspaceObjectTests : Test
    {
        [Fact]
        public void Many2One() =>
            AsyncContext.Run(
                async () =>
                {
                    var session = this.Workspace.CreateSession();

                    var workspaceOrganisation = session.Create<WorkspaceOrganisation>();
                    var workspacePerson = session.Create<WorkspacePerson>();

                    workspaceOrganisation.WorkspaceWorkspaceOwner = workspacePerson;

                    var session2 = this.Workspace.CreateSession();

                    var workspaceOrganisation2 = session2.Instantiate(workspaceOrganisation);
                    var workspacePerson2 = session2.Instantiate(workspacePerson);

                    Assert.Equal(workspacePerson2, workspaceOrganisation2.WorkspaceWorkspaceOwner);
                });
    }
}
