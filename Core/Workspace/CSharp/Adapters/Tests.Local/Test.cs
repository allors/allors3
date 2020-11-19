// <copyright file="DomainTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Tests.Workspace.Remote
{
    using System;
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
                using var session = this.Database.CreateSession();

                new Setup(session, new Config()).Apply();
                session.Commit();

                var administrator = new PersonBuilder(session).WithUserName("administrator").Build();
                new UserGroups(session).Administrators.AddMember(administrator);
                session.Context().User = administrator;

                new TestPopulation(session, "full").Apply();
                session.Commit();
            }

            this.Workspace = new Workspace(
                new Allors.Workspace.Meta.MetaBuilder().Build(),
                typeof(Allors.Workspace.Domain.User),
                new WorkspaceContext(),
                this.Database);
        }

        public Database Database { get; }

        public Workspace Workspace { get; }

        public void Dispose()
        {
        }
   }
}
