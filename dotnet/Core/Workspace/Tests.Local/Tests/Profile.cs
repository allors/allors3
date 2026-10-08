// <copyright file="Profile.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Local
{
    using System.Linq;
    using System.Threading.Tasks;
    using Allors;
    using Allors.Database;
    using Allors.Database.Adapters.Memory;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using Allors.Database.Services;
    using Allors.Ranges;
    using Allors.Workspace;
    using Allors.Workspace.Connection;
    using Allors.Workspace.Connection.Local;
    using Allors.Workspace.Derivations;
    using Allors.Workspace.Domain;
    using Allors.Workspace.Meta;
    using Allors.Workspace.Meta.Lazy;
    using Allors.Workspace.Session;
    using IObjectFactory = Allors.Workspace.IObjectFactory;
    using IWorkspaceServices = Allors.Workspace.IWorkspaceServices;
    using Person = Allors.Workspace.Domain.Person;
    using User = Allors.Database.Domain.User;

    public class Profile : IProfile
    {
        private const string WorkspaceName = "Default";

        private readonly IdGenerator idGenerator;
        private readonly DefaultStructRanges<long> ranges;
        private readonly M metaPopulation;
        private readonly IObjectFactory objectFactory;
        private readonly IRule[] rules;

        private User user;

        public Database Database { get; }

        public IDatabaseConnection DatabaseConnection { get; private set; }

        IWorkspace IProfile.Workspace => this.Workspace;

        public IWorkspace Workspace { get; private set; }

        public M M => this.Workspace.Services.Get<M>();

        public Profile(Fixture fixture)
        {
            this.idGenerator = new IdGenerator();
            this.ranges = new DefaultStructRanges<long>();

            this.metaPopulation = new MetaBuilder().Build();
            this.objectFactory = new ReflectionObjectFactory(this.metaPopulation, typeof(Person));
            this.rules = new IRule[] { new PersonSessionFullNameRule(this.metaPopulation) };

            this.Database = new Database(
                new DefaultDatabaseServices(fixture.Engine),
                new Allors.Database.Adapters.Memory.Configuration
                {
                    ObjectFactory = new ObjectFactory(fixture.M, typeof(Allors.Database.Domain.Person)),
                });

            this.Database.Init();

            var config = new Config();
            new Setup(this.Database, config).Apply();

            using var transaction = this.Database.CreateTransaction();

            var administrator = new PersonBuilder(transaction).WithUserName("administrator").WithUniqueId(Users.AdministratorId).Build();
            new UserGroups(transaction).Administrators.AddMember(administrator);
            transaction.Services.Get<IUserService>().User = administrator;

            new TestPopulation(transaction).Apply();
            transaction.Derive();
            transaction.Commit();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => Task.CompletedTask;

        public IWorkspace CreateExclusiveWorkspace() => this.CreateWorkspace(this.CreateConnection());

        public IWorkspace CreateWorkspace() => this.CreateWorkspace(this.DatabaseConnection);

        public Task Login(string userName)
        {
            this.user = this.FindUser(userName);

            this.DatabaseConnection = this.CreateConnection();
            this.Workspace = this.CreateWorkspace(this.DatabaseConnection);

            return Task.CompletedTask;
        }

        public ITransport CreateTransport(string userName) => new LocalTransport(this.Database, this.FindUser(userName).Id, WorkspaceName);

        public Task RemoveAdministratorPermission(IRoleType roleType, Operations operation)
        {
            using var transaction = this.Database.CreateTransaction();
            TestSecurity.RemoveAdministratorPermission(transaction, roleType.RelationType.Tag, operation);
            return Task.CompletedTask;
        }

        public Task DenyPermission(IRoleType roleType, Operations operation)
        {
            using var transaction = this.Database.CreateTransaction();
            TestSecurity.DenyPermission(transaction, roleType.RelationType.Tag, operation);
            return Task.CompletedTask;
        }

        private User FindUser(string userName)
        {
            using var transaction = this.Database.CreateTransaction();
            var uniqueId = Users.TestUserId(userName);
            return new Users(transaction).Extent().ToArray().First(v => v.UniqueId == uniqueId);
        }

        private DatabaseConnection CreateConnection() =>
            new DatabaseConnection(WorkspaceName, this.metaPopulation, new LocalTransport(this.Database, this.user.Id, WorkspaceName), this.ranges);

        private Workspace CreateWorkspace(IDatabaseConnection connection) =>
            new Workspace(connection, this.objectFactory, this.rules, new WorkspaceServices(), this.idGenerator);
    }
}
