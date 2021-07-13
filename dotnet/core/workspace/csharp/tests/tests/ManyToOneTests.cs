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
                    Extent = new Filter(this.M.Person)
                }
            });

            var workspaceOrganisation1 = session1.Create<WorkspaceOrganisation>();
            var databasePerson1 = result.GetCollection<Person>().First();

            workspaceOrganisation1.WorkspaceDatabaseOwner = databasePerson1;

            await session1.Push();

            var session2 = this.Workspace.CreateSession();

            var workspaceOrganisation2 = session2.GetOne(workspaceOrganisation1);
            var databasePerson2 = session2.GetOne(databasePerson1);

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
                    Extent = new Filter(this.M.Person)
                }
            });

            var workspaceOrganisation1 = session1.Create<WorkspaceOrganisation>();
            workspaceOrganisation1.WorkspaceDatabaseOwner = result.GetCollection<Person>().First();

            await session1.Push();

            var session2 = this.Workspace.CreateSession();

            var workspaceOrganisation2 = session2.GetOne(workspaceOrganisation1);

            workspaceOrganisation1.RemoveWorkspaceDatabaseOwner();

            Assert.Null(workspaceOrganisation2.WorkspaceDatabaseOwner);
            Assert.Null(workspaceOrganisation1.WorkspaceDatabaseOwner);
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

            var workspaceOrganisation2 = session2.GetOne(workspaceOrganisation1);
            var workspacePerson2 = session2.GetOne(workspacePerson1);

            Assert.Equal(workspacePerson2, workspaceOrganisation2.WorkspaceWorkspaceOwner);
            Assert.Equal(workspacePerson1, workspaceOrganisation1.WorkspaceWorkspaceOwner);
        }

        [Fact]
        public async void WorkspaceWorkspace_RemoveRole()
        {
            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var workspaceOrganisation1 = session1.Create<WorkspaceOrganisation>();
            workspaceOrganisation1.WorkspaceWorkspaceOwner = session1.Create<WorkspacePerson>();

            await session1.Push();

            var session2 = this.Workspace.CreateSession();

            var workspaceOrganisation2 = session2.GetOne(workspaceOrganisation1);

            Assert.NotNull(workspaceOrganisation2.WorkspaceWorkspaceOwner);

            workspaceOrganisation1.RemoveWorkspaceWorkspaceOwner();

            Assert.NotNull(workspaceOrganisation2.WorkspaceWorkspaceOwner);
            Assert.Null(workspaceOrganisation1.WorkspaceWorkspaceOwner);

            await session1.Push();
            await session2.Pull(new Pull
            {
                Object = workspaceOrganisation1
            });

            Assert.Null(workspaceOrganisation2.WorkspaceWorkspaceOwner);
            Assert.Null(workspaceOrganisation1.WorkspaceWorkspaceOwner);
        }

        [Fact]
        public async void DatabaseDatabase_RemoveRoleWithPush()
        {

            await this.Login("administrator");

            var session1 = this.Workspace.CreateSession();

            var organisation1 = session1.Create<Organisation>();
            organisation1.Name = "Allors";
            var person1 = session1.Create<Person>();
            organisation1.Owner = person1;

            Assert.Equal(person1, organisation1.Owner);

            await session1.Push();

            var session2 = this.Workspace.CreateSession();

            #region pulls
            var pulls = new Pull
            {
                Object = organisation1,
                Results = new[]
                {
                   new Result
                   {
                       Select = new Select
                       {
                           Include = new[] {new Node(this.M.Organisation.Owner)}
                       }
                   }
               }
            };
            #endregion

            var pullResult = await session2.Pull(pulls);

            var organisation2 = pullResult.GetObject<Organisation>();
            var person2 = organisation2.Owner;

            Assert.Equal(organisation1.Id, organisation2.Id);
            Assert.Equal(person1.Id, person2.Id);

            var canRemoveBeforePull = organisation1.CanWriteOwner;

            await session1.Pull(pulls);

            var canRemoveAfterPull = organisation1.CanWriteOwner;

            organisation1.RemoveOwner();

            Assert.Null(organisation1.Owner);
            Assert.NotNull(organisation2.Owner);

            var pushResult = await session1.Push();

            Assert.Null(organisation1.Owner);
            Assert.NotNull(organisation2.Owner);

            await session2.Pull(pulls);

            person2 = organisation2.Owner;

            Assert.Null(organisation1.Owner);
            Assert.Null(organisation2.Owner);
        }

        [Fact]
        public async void DatabaseDatabase_RemoveRole()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var organisation = session.Create<Organisation>();
            var person1 = session.Create<Person>();
            organisation.Owner = person1;

            Assert.Equal(person1, organisation.Owner);
        }

        [Fact]
        public async void SessionSession_SetRole()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var organisation = session.Create<SessionOrganisation>();
            var person1 = session.Create<SessionPerson>();
            organisation.SessionSessionOwner = person1;

            Assert.Equal(person1, organisation.SessionSessionOwner);
        }

        [Fact]
        public async void SessionSession_RemoveRole()
        {
            await this.Login("administrator");

            var session = this.Workspace.CreateSession();

            var organisation1 = session.Create<SessionOrganisation>();
            var person1 = session.Create<SessionPerson>();
            organisation1.SessionSessionOwner = person1;

            Assert.Equal(person1, organisation1.SessionSessionOwner);

            organisation1.RemoveSessionSessionOwner();

            Assert.Null(organisation1.SessionSessionOwner);
        }
    }
}
