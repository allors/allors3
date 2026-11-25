// <copyright file="Profile.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Workspace.Tests.Local
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Ranges;
    using Allors.Workspace;
    using Allors.Workspace.Adapters;
    using Allors.Workspace.Derivations;
    using Microsoft.Extensions.Configuration;
    using Configuration = Allors.Workspace.Adapters.Local.Configuration;
    using DatabaseBuilder = Allors.Database.Adapters.DatabaseBuilder;
    using DatabaseConnection = Allors.Workspace.Adapters.Local.DatabaseConnection;
    using DatabaseMetaBuilder = Allors.Database.Meta.MetaBuilder;
    using MetaBuilder = Allors.Workspace.Meta.Lazy.MetaBuilder;
    using ObjectFactory = Allors.Database.ObjectFactory;
    using Person = Allors.Workspace.Domain.Person;
    using User = Allors.Database.Domain.User;

    // The Local adapter, in the process of the database. It has no sign-in: a workspace acts for the user whose
    // id the application gives it. The database runs on the Snapshot adapter.
    public class Profile : IProfile
    {
        private readonly Configuration configuration;

        public Profile()
        {
            var databaseMetaPopulation = new DatabaseMetaBuilder().Build();
            var engine = new Engine(Rules.Create(databaseMetaPopulation));
            var settings = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { ["adapter"] = "snapshot" })
                .Build();
            var databaseObjectFactory = new ObjectFactory(databaseMetaPopulation, typeof(User));
            this.Database = new DatabaseBuilder(new DefaultDatabaseServices(engine), settings, databaseObjectFactory).Build();

            this.Database.Init();
            new Setup(this.Database, new Config()).Apply();
            Population.Apply(this.Database);

            var metaPopulation = new MetaBuilder().Build();
            var objectFactory = new ReflectionObjectFactory(metaPopulation, typeof(Person));
            this.configuration = new Configuration("Default", metaPopulation, objectFactory, Array.Empty<IRule>());
        }

        public IDatabase Database { get; }

        public Task<IWorkspace> CreateWorkspace(string userName)
        {
            using var transaction = this.Database.CreateTransaction();
            var users = new Users(transaction);
            var user = users.FindBy(users.Meta.UserName, userName);

            var connection = new DatabaseConnection(this.configuration, this.Database, () => new WorkspaceServices(), () => new DefaultStructRanges<long>())
            {
                UserId = user.Id,
            };

            return Task.FromResult(connection.CreateWorkspace());
        }

        public void Dispose()
        {
        }
    }
}
