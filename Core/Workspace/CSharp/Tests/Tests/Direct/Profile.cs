// <copyright file="ObjectTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Direct
{
    using System;
    using System.Threading.Tasks;
    using Allors.Workspace;
    using Allors.Workspace.Meta;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using Allors.Database.Adapters.Memory;
    using Allors.Workspace.Adapters.Direct;
    using Remote;
    using Person = Allors.Database.Domain.Person;

    public class Profile : IProfile
    {
        IWorkspace IProfile.Workspace => this.Workspace;

        public Workspace Workspace { get; }

        public Database Database { get; }

        public M M => this.Workspace.Context().M;

        private long administratorId;

        public Profile(Fixture fixture)
        {
            this.Database = new Database(
                new DefaultDatabaseContext(),
                new Configuration
                {
                    ObjectFactory = new Allors.Database.ObjectFactory(fixture.MetaPopulation, typeof(Person)),
                });

            this.Database.Init();

            this.Database.RegisterDerivations();

            this.Workspace = new Workspace(
                "Default",
                new Allors.Workspace.Meta.MetaBuilder().Build(),
                 typeof(Allors.Workspace.Domain.User),
                new WorkspaceContext(),
                this.Database);
        }

        public async Task InitializeAsync()
        {
            using var session = this.Database.CreateTransaction();
            new Setup(session, new Config()).Apply();

            var administrator = new PersonBuilder(session).WithUserName("administrator").Build();
            var administrators = new UserGroups(session).Administrators;
            administrators.AddMember(administrator);
            session.Context().User = administrator;

            var defaultSecurityToken = new SecurityTokens(session).DefaultSecurityToken;
            var administratorRole = new Roles(session).Administrator;
            var acl = new AccessControlBuilder(session).WithRole(administratorRole).WithSubjectGroup(administrators).WithSecurityToken(defaultSecurityToken).Build();

            this.administratorId = administrator.Strategy.ObjectId;

            session.Derive();

            new TestPopulation(session, "full").Apply();

            session.Commit();
        }

        public Task DisposeAsync() => Task.CompletedTask;

        public async Task Login(string user)
        {
            using var session = this.Database.CreateTransaction();
            var administrator = session.Instantiate(this.administratorId) as Person;
            session.Context().User = administrator;
        }
    }
}
