// <copyright file="Profile.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Local
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Allors.Database.Protocol.Json;
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Database.Adapters;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using Allors.Database.Services;
    using Allors.Ranges;
    using Allors.Workspace;
    using Allors.Workspace.Adapters;
    using Allors.Workspace.Derivations;
    using Allors.Workspace.Domain;
    using Allors.Workspace.Meta;
    using Allors.Workspace.Meta.Lazy;
    using Microsoft.Extensions.Configuration;
    using Configuration = Allors.Workspace.Adapters.Remote.Configuration;
    using DatabaseConnection = Allors.Workspace.Adapters.Local.DatabaseConnection;
    using IWorkspaceServices = Allors.Workspace.IWorkspaceServices;
    using Person = Allors.Workspace.Domain.Person;

    public class Profile : IProfile
    {
        private readonly Func<IWorkspaceServices> servicesBuilder;
        private readonly IdGenerator idGenerator;
        private readonly DefaultStructRanges<long> ranges;
        private readonly Configuration configuration;
        private readonly Fixture fixture;

        private IDatabase database;
        private LocalReadIdentity identity;
        private readonly List<DatabaseConnection> connections = new List<DatabaseConnection>();

        public IDatabase Database => this.database ??= this.CreateDatabase();

        public DatabaseConnection DatabaseConnection { get; private set; }

        IWorkspace IProfile.Workspace => this.Workspace;

        public IWorkspace Workspace { get; private set; }

        public M M => this.Workspace.Services.Get<M>();

        public Profile(Fixture fixture)
        {
            this.fixture = fixture;
            this.servicesBuilder = () => new WorkspaceServices();
            this.idGenerator = new IdGenerator();
            this.ranges = new DefaultStructRanges<long>();

            var metaPopulation = new MetaBuilder().Build();
            var objectFactory = new ReflectionObjectFactory(metaPopulation, typeof(Person));
            var rules = new IRule[] { new PersonSessionFullNameRule(metaPopulation) };
            this.configuration = new Configuration("Default", metaPopulation, objectFactory, rules);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            var owned = this.connections.ToArray();
            this.connections.Clear();
            this.DatabaseConnection = null;
            this.Workspace = null;
            var errors = new List<Exception>();
            var draining = owned.Select(connection => connection.DisposeAsync()).ToArray();
            foreach (var closing in draining)
            {
                try { await closing; }
                catch (Exception exception) { errors.Add(exception); }
            }
            if (errors.Count != 0) throw new AggregateException(errors);
        }

        public async Task<IWorkspace> CreateExclusiveWorkspaceAsync()
        {
            if (this.DatabaseConnection == null) throw new InvalidOperationException("Log in before creating a workspace.");
            var connection = await DatabaseConnection.CreateAsync(this.configuration, this.servicesBuilder, this.Database,
                this.identity, this.idGenerator, this.ranges, "test-read-v1");
            this.connections.Add(connection);
            return connection.CreateWorkspace();
        }

        public IWorkspace CreateWorkspace() => (this.DatabaseConnection ?? throw new InvalidOperationException("Log in before creating a workspace.")).CreateWorkspace();

        public async Task Login(string userName)
        {
            await this.DisposeAsync();
            long userId;
            using (var transaction = this.Database.CreateTransaction())
            {
                var uniqueId = Users.TestUserId(userName);
                userId = new Users(transaction).Extent().ToArray().First(v => v.UniqueId == uniqueId).Id;
            }
            this.identity = new LocalReadIdentity(((Allors.Database.Adapters.Sql.Database)this.Database).PopulationLifecycle.RequireReady().Incarnation, userId);
            var connection = await DatabaseConnection.CreateAsync(this.configuration, this.servicesBuilder, this.Database,
                this.identity, this.idGenerator, this.ranges, "test-read-v1");
            this.connections.Add(connection);
            this.DatabaseConnection = connection;
            this.Workspace = connection.CreateWorkspace();
        }

        private Allors.Database.Adapters.Sql.Database CreateDatabase()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddAllorsConfiguration("core", "commands");
            var database = (Allors.Database.Adapters.Sql.Database)new DatabaseBuilder(
                new DefaultDatabaseServices(this.fixture.Engine),
                configurationBuilder.Build(),
                new ObjectFactory(this.fixture.M, typeof(Allors.Database.Domain.Person))).Build();

            using var maintenance = database.PopulationLifecycle.BeginMaintenance();
            database.Init();
            new Setup(database, new Config()).Apply();

            using (var transaction = database.CreateTransaction())
            {
                var administrator = new PersonBuilder(transaction).WithUserName("administrator").WithUniqueId(Users.AdministratorId).Build();
                new UserGroups(transaction).Administrators.AddMember(administrator);
                transaction.Services.Get<IUserService>().User = administrator;

                new TestPopulation(transaction).Apply();
                transaction.Derive();
                transaction.Commit();
            }

            maintenance.Complete();
            return database;
        }
    }
}
