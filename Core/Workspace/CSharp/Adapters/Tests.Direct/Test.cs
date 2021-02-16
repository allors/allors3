// <copyright file="DomainTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Tests.Workspace.Remote
{
    using System;
    using Allors.Database;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using Allors.Database.Adapters.Memory;
    using Allors.Workspace;
    using Allors.Workspace.Adapters.Direct;
    using ISession = Allors.Database.ISession;

    public class Test : IDisposable
    {
        public Test(Fixture fixture, bool populate = true)
        {
            this.Database = new Database(
                new DefaultDatabaseContext(),
                new Configuration
                {
                    ObjectFactory = new Allors.Database.ObjectFactory(fixture.MetaPopulation, typeof(C1)),
                });

            this.Database.Init();

            this.Database.RegisterDerivations();

            if (populate)
            {
                this.Session = this.Database.CreateSession();

                new Setup(this.Session, new Config()).Apply();

                this.Administrator = new PersonBuilder(this.Session).WithUserName("administrator").Build();
                var administrators = new UserGroups(this.Session).Administrators;
                administrators.AddMember(this.Administrator);
                this.Session.Context().User = this.Administrator;

                var defaultSecurityToken = new SecurityTokens(this.Session).DefaultSecurityToken;
                var administratorRole = new Roles(this.Session).Administrator;
                var acl = new AccessControlBuilder(this.Session).WithRole(administratorRole).WithSubjectGroup(administrators).WithSecurityToken(defaultSecurityToken).Build();

                this.Session.Derive();

                new TestPopulation(this.Session, "full").Apply();

                this.Session.Commit();
            }

            this.Workspace = new Workspace(
                "Default",
                new Allors.Workspace.Meta.MetaBuilder().Build(),
                typeof(Allors.Workspace.Domain.User),
                new WorkspaceContext(),
                this.Database);
        }

        public Person Administrator { get; set; }

        public IDatabase Database { get; }

        public ISession Session { get; private set; }

        public Workspace Workspace { get; }

        public void Dispose()
        {
            this.Session.Dispose();
            this.Session = null;
        }
    }
}
