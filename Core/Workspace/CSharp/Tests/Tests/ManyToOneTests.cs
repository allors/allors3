// <copyright file="Many2OneTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace
{
    using System.Linq;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public abstract class Many2OneTests : Test
    {
        protected Many2OneTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async void WorkspaceDatabase_SetRole()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var result = await session1.Pull(new[]
            {
                new Pull
                {
                    Extent = new Extent(this.M.Person),
                },
            });

            var workspaceOrganisation1 = session1.Create<WorkspaceOrganisation>();
            var databasePerson1 = result.GetCollection<Person>().First();

            workspaceOrganisation1.WorkspaceDatabaseOwner = databasePerson1;

            await session1.Push();

            var session2 = this.Workspace.CreateSession();

            var workspaceOrganisation2 = session2.Get(workspaceOrganisation1);
            var databasePerson2 = session2.Get(databasePerson1);

            Assert.Equal(databasePerson2, workspaceOrganisation2.WorkspaceDatabaseOwner);
            Assert.Equal(databasePerson1, workspaceOrganisation1.WorkspaceDatabaseOwner);
        }

        [Fact]
        public async void WorkspaceDatabase_RemoveRole()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var result = await session1.Pull(new[]
            {
                new Pull
                {
                    Extent = new Extent(this.M.Person),
                },
            });

            var workspaceOrganisation1 = session1.Create<WorkspaceOrganisation>();
            var databasePerson1 = result.GetCollection<Person>().First();

            workspaceOrganisation1.WorkspaceDatabaseOwner = databasePerson1;

            await session1.Push();

            var session2 = this.Workspace.CreateSession();

            var workspaceOrganisation2 = session2.Get(workspaceOrganisation1);

            workspaceOrganisation1.RemoveWorkspaceWorkspaceOwner();

            Assert.Null(workspaceOrganisation2.WorkspaceWorkspaceOwner);
            Assert.Null(workspaceOrganisation1.WorkspaceWorkspaceOwner);
        }

        [Fact]
        public async void WorkspaceWorkspace_SetRole()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var workspaceOrganisation1 = session1.Create<WorkspaceOrganisation>();
            var workspacePerson1 = session1.Create<WorkspacePerson>();

            workspaceOrganisation1.WorkspaceWorkspaceOwner = workspacePerson1;

            await session1.Push();

            var session2 = this.Workspace.CreateSession();

            var workspaceOrganisation2 = session2.Get(workspaceOrganisation1);
            var workspacePerson2 = session2.Get(workspacePerson1);

            Assert.Equal(workspacePerson2, workspaceOrganisation2.WorkspaceWorkspaceOwner);
            Assert.Equal(workspacePerson1, workspaceOrganisation1.WorkspaceWorkspaceOwner);
        }

        [Fact]
        public async void WorkspaceWorkspace_RemoveRole()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var workspaceOrganisation1 = session.Create<WorkspaceOrganisation>();
            var workspacePerson1 = session.Create<WorkspacePerson>();

            workspaceOrganisation1.WorkspaceWorkspaceOwner = workspacePerson1;

            await session.Push();

            var session2 = this.Workspace.CreateSession();

            var workspaceOrganisation2 = session2.Get(workspaceOrganisation1);

            workspaceOrganisation1.RemoveWorkspaceWorkspaceOwner();

            Assert.Null(workspaceOrganisation2.WorkspaceWorkspaceOwner);
            Assert.Null(workspaceOrganisation1.WorkspaceWorkspaceOwner);
        }
    }
}
