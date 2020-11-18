// <copyright file="ObjectTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Local
{
    using Allors.Database;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using Allors.Workspace;

    public class Profile : IProfile
    {
        IWorkspace IProfile.Workspace => this.Workspace;

        public Allors.Workspace.Adapters.Local.Workspace Workspace { get; }

        public Allors.Workspace.Adapters.Local.WorkspaceDatabase WorkspaceDatabase => this.Workspace.WorkspaceDatabase;

        public Allors.Workspace.Meta.M M => this.Workspace.Context().M;

        public Allors.Database.Adapters.Memory.Database Database { get; }

        public Profile()
        {
            var metaPopulation = new Allors.Database.Meta.MetaBuilder().Build();
            this.Database = new Allors.Database.Adapters.Memory.Database(
                new ValidatingDatabaseContext(),
                new Allors.Database.Adapters.Memory.Configuration
                {
                    ObjectFactory = new ObjectFactory(metaPopulation, typeof(Allors.Database.Domain.C1)),
                });

            this.Workspace = new Allors.Workspace.Adapters.Local.Workspace(
                new Allors.Workspace.Meta.MetaBuilder().Build(),
                typeof(Allors.Workspace.Domain.User),
                new WorkspaceContext(),
                this.Database);
        }

        public async System.Threading.Tasks.Task InitializeAsync()
        {
            var database = this.Database;
            database.Init();

            using var session = database.CreateSession();
            var config = new Config();
            new Setup(session, config).Apply();
            session.Derive();
            session.Commit();

            var administrator = new Allors.Database.Domain.PersonBuilder(session).WithUserName("administrator").Build();
            new Allors.Database.Domain.UserGroups(session).Administrators.AddMember(administrator);
            session.Context().User = administrator;

            new TestPopulation(session, "full").Apply();
            session.Derive();
            session.Commit();
        }

        public System.Threading.Tasks.Task DisposeAsync() => System.Threading.Tasks.Task.CompletedTask;

        public async System.Threading.Tasks.Task Login(string user)
        {
            var m = this.Database.Context().M;

            using var session = this.Database.CreateSession();
            session.Context().User = new Allors.Database.Domain.Users(session).FindBy(m.User.UserName, user);
        }
    }
}
