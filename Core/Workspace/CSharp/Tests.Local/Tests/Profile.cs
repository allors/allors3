// <copyright file="Profile.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Local
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Allors.Workspace;
    using Allors.Workspace.Meta;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using Allors.Database.Adapters.Memory;
    using Allors.Workspace.Adapters.Local;
    using Allors.Workspace.Domain;
    using Allors.Workspace.Meta.Lazy;
    using Person = Allors.Database.Domain.Person;

    public class Profile : IProfile
    {
        public Database Database { get; private set; }

        IWorkspace IProfile.Workspace => this.Workspace;

        public Workspace Workspace { get; private set; }

        public M M => this.Workspace.Context().M;

        public Profile(Fixture fixture)
        {
            this.Database = new Database(
                new DefaultDatabaseContext(fixture.Engine),
                new Configuration
                {
                    ObjectFactory = new Allors.Database.ObjectFactory(fixture.M, typeof(Person)),
                });


            this.Database.Init();

            using var session = this.Database.CreateTransaction();
            new Setup(session, new Config()).Apply();

            var administrator = new PersonBuilder(session).WithUserName("administrator").Build();
            var administrators = new UserGroups(session).Administrators;
            administrators.AddMember(administrator);
            session.Context().User = administrator;

            var defaultSecurityToken = new SecurityTokens(session).DefaultSecurityToken;
            var administratorRole = new Roles(session).Administrator;
            var acl = new AccessControlBuilder(session).WithRole(administratorRole).WithSubjectGroup(administrators).WithSecurityToken(defaultSecurityToken).Build();

            session.Derive();

            new TestPopulation(session, "full").Apply();

            session.Commit();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => Task.CompletedTask;

        public Task Login(string userName)
        {
            using var transaction = this.Database.CreateTransaction();
            var user = new Users(transaction).Extent().ToArray().First(v => v.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase));
            transaction.Context().User = user;

            this.Workspace = new Workspace(
                "Default",
                user.Id,
                new MetaBuilder().Build(),
                typeof(Allors.Workspace.Domain.User),
                new WorkspaceContext(),
                this.Database);

            return Task.CompletedTask;
        }
    }
}
