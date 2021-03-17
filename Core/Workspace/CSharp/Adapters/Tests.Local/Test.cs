// <copyright file="DomainTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Tests.Workspace.Local
{
    using System;
    using Allors.Database;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using Allors.Database.Adapters.Memory;
    using Allors.Workspace;
    using Allors.Workspace.Adapters.Local;

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
                this.Transaction = this.Database.CreateTransaction();

                new Setup(this.Transaction, new Config()).Apply();

                this.Administrator = new PersonBuilder(this.Transaction).WithUserName("administrator").Build();
                var administrators = new UserGroups(this.Transaction).Administrators;
                administrators.AddMember(this.Administrator);
                this.Transaction.Context().User = this.Administrator;

                var defaultSecurityToken = new SecurityTokens(this.Transaction).DefaultSecurityToken;
                var administratorRole = new Roles(this.Transaction).Administrator;
                var acl = new AccessControlBuilder(this.Transaction).WithRole(administratorRole).WithSubjectGroup(administrators).WithSecurityToken(defaultSecurityToken).Build();

                this.Transaction.Derive();

                new TestPopulation(this.Transaction, "full").Apply();

                this.Transaction.Commit();
            }

            this.Workspace = new LocalWorkspace(
                "Default",
                this.Administrator.Id,
                new Allors.Workspace.Meta.MetaBuilder().Build(),
                typeof(Allors.Workspace.Domain.User),
                new WorkspaceContext(),
                this.Database);
        }

        public Person Administrator { get; set; }

        public IDatabase Database { get; }

        public ITransaction Transaction { get; private set; }

        public LocalWorkspace Workspace { get; }

        public void Dispose()
        {
            this.Transaction.Dispose();
            this.Transaction = null;
        }
    }
}
