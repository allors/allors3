// <copyright file="ObjectTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Origin.Workspace.ToDatabase
{
    using System.Linq;
    using Allors.Workspace.Data;
    using Allors.Workspace.Domain;
    using Xunit;

    public class Many2OneTests : Test
    {
        [Fact]
        public async void SetRole()
        {
            var session1 = this.Workspace.CreateSession();

            var result = await session1.Load(new[]
            {
                        new Pull
                        {
                            Extent = new Extent(this.M.Person.ObjectType),
                        },
                    });

            var workspaceOrganisation1 = session1.Create<WorkspaceOrganisation>();
            var databasePerson1 = result.GetCollection<Person>().First();

            workspaceOrganisation1.WorkspaceDatabaseOwner = databasePerson1;

            var session2 = this.Workspace.CreateSession();

            var workspaceOrganisation2 = session2.Instantiate(workspaceOrganisation1);
            var databasePerson2 = session2.Instantiate(databasePerson1);

            Assert.Equal(databasePerson2, workspaceOrganisation2.WorkspaceDatabaseOwner);
            Assert.Equal(databasePerson1, workspaceOrganisation1.WorkspaceDatabaseOwner);
        }

        [Fact]
        public async void RemoveRole()
        {
            var session1 = this.Workspace.CreateSession();

            var result = await session1.Load(new[]
            {
                new Pull
                {
                    Extent = new Extent(this.M.Person.ObjectType),
                },
            });

            var workspaceOrganisation1 = session1.Create<WorkspaceOrganisation>();
            var databasePerson1 = result.GetCollection<Person>().First();

            workspaceOrganisation1.WorkspaceDatabaseOwner = databasePerson1;

            var session2 = this.Workspace.CreateSession();

            var workspaceOrganisation2 = session2.Instantiate(workspaceOrganisation1);

            workspaceOrganisation1.RemoveWorkspaceWorkspaceOwner();

            Assert.Null(workspaceOrganisation2.WorkspaceWorkspaceOwner);
            Assert.Null(workspaceOrganisation1.WorkspaceWorkspaceOwner);
        }
    }
}
