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
    using Allors.Ranges;
    using Allors.Workspace.Adapters;
    using Allors.Workspace.Domain;
    using Allors.Workspace.Meta.Lazy;
    using Configuration = Allors.Database.Adapters.Memory.Configuration;
    using DatabaseConnection = Allors.Workspace.Adapters.Local.DatabaseConnection;
    using Person = Allors.Database.Domain.Person;
    using User = Allors.Database.Domain.User;

    public class Profile : IProfile
    {
        private readonly Func<IRanges> rangesFactory;
        private readonly Func<IWorkspaceServices> servicesBuilder;
        private readonly Allors.Workspace.Adapters.Local.Configuration configuration;

        private User user;

        public Database Database { get; private set; }

        public DatabaseConnection DatabaseConnection { get; private set; }

        IWorkspace IProfile.Workspace => this.Workspace;

        public IWorkspace Workspace { get; private set; }

        public M M => this.Workspace.Context().M;

        public Profile(Fixture fixture)
        {
            this.rangesFactory = () => new DefaultRanges();
            this.servicesBuilder = () => new WorkspaceContext();

            var metaPopulation = new MetaBuilder().Build();
            var objectFactory = new ReflectionObjectFactory(metaPopulation, typeof(Allors.Workspace.Domain.Person));
            this.configuration = new Allors.Workspace.Adapters.Local.Configuration("Default", metaPopulation, objectFactory);

            this.Database = new Database(
                new DefaultDomainDatabaseServices(fixture.Engine),
                new Configuration
                {
                    ObjectFactory = new Allors.Database.ObjectFactory(fixture.M, typeof(Person)),
                });

            this.Database.Init();

            using var transaction = this.Database.CreateTransaction();
            var config = new Config();
            new Setup(transaction, config).Apply();
            transaction.Derive();
            transaction.Commit();

            var administrator = new PersonBuilder(transaction).WithUserName("administrator").Build();
            new UserGroups(transaction).Administrators.AddMember(administrator);
            transaction.Services().User = administrator;

            new TestPopulation(transaction, "full").Apply();
            transaction.Derive();
            transaction.Commit();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => Task.CompletedTask;

        public IDatabaseConnection CreateDatabase() => new DatabaseConnection(this.configuration, this.Database, this.servicesBuilder, this.rangesFactory) { UserId = this.user.Id };

        public IWorkspace CreateWorkspace() => this.DatabaseConnection.CreateWorkspace();

        public Task Login(string userName)
        {
            using var transaction = this.Database.CreateTransaction();
            this.user = new Users(transaction).Extent().ToArray().First(v => v.UserName.Equals(userName, StringComparison.InvariantCultureIgnoreCase));
            transaction.Services().User = this.user;

            this.DatabaseConnection = new DatabaseConnection(this.configuration, this.Database, this.servicesBuilder, this.rangesFactory) { UserId = this.user.Id };

            this.Workspace = this.DatabaseConnection.CreateWorkspace();

            return Task.CompletedTask;
        }
    }
}
