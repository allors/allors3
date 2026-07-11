// <copyright file="WorkspaceFixture.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Blazor.Bootstrap.Tests
{
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Ranges;
    using Allors.Workspace.Adapters;
    using Allors.Workspace.Derivations;
    using Allors.Workspace.Meta;
    using DatabaseMetaBuilder = Allors.Database.Meta.MetaBuilder;
    using DatabaseObjectFactory = Allors.Database.ObjectFactory;
    using DatabaseUser = Allors.Database.Domain.User;
    using LocalConfiguration = Allors.Workspace.Adapters.Local.Configuration;
    using LocalDatabaseConnection = Allors.Workspace.Adapters.Local.DatabaseConnection;
    using MemoryConfiguration = Allors.Database.Adapters.Memory.Configuration;
    using MemoryDatabase = Allors.Database.Adapters.Memory.Database;
    using WorkspaceMetaBuilder = Allors.Workspace.Meta.Lazy.MetaBuilder;
    using WorkspacePerson = Allors.Workspace.Domain.Person;

    /// <summary>
    /// Builds a real Allors Local (in-memory) workspace so the Blazor.Bootstrap Role components can be
    /// rendered against live <see cref="IObject"/> instances. Shared across a test class via
    /// <see cref="Xunit.IClassFixture{TFixture}"/>; each test creates its own session and objects.
    /// </summary>
    public sealed class WorkspaceFixture
    {
        public WorkspaceFixture()
        {
            // In-memory database.
            var databaseMeta = new DatabaseMetaBuilder().Build();
            var engine = new Engine(Rules.Create(databaseMeta));
            var database = new MemoryDatabase(
                new DefaultDatabaseServices(engine),
                new MemoryConfiguration
                {
                    ObjectFactory = new DatabaseObjectFactory(databaseMeta, typeof(DatabaseUser)),
                });

            database.Init();
            new Setup(database, new Config { SetupSecurity = false }).Apply();

            // Local workspace over the in-memory database.
            var workspaceMeta = new WorkspaceMetaBuilder().Build();
            var objectFactory = new ReflectionObjectFactory(workspaceMeta, typeof(WorkspacePerson));
            var configuration = new LocalConfiguration("Default", workspaceMeta, objectFactory, new IRule[] { });
            var connection = new LocalDatabaseConnection(
                configuration,
                database,
                () => new WorkspaceServices(),
                () => new DefaultStructRanges<long>());

            this.Workspace = connection.CreateWorkspace();
        }

        public IWorkspace Workspace { get; }

        public M M => this.Workspace.Services.Get<M>();
    }
}
