// <copyright file="Profile.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Remote
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Allors.Ranges;
    using Allors.Workspace;
    using Allors.Workspace.Adapters;
    using Allors.Workspace.Derivations;
    using Allors.Workspace.Domain;
    using Allors.Workspace.Meta;
    using Allors.Workspace.Meta.Lazy;
    using Xunit;
    using Configuration = Allors.Workspace.Adapters.Remote.Configuration;
    using DatabaseConnection = Allors.Workspace.Adapters.Remote.SystemText.DatabaseConnection;
    using IWorkspaceServices = Allors.Workspace.IWorkspaceServices;

    public class Profile : IProfile
    {
        public const string Url = "http://localhost:5000/allors/";

        public const string SetupUrl = "Test/Setup?population=full";

        private readonly Func<IWorkspaceServices> servicesBuilder;
        private readonly IdGenerator idGenerator;
        private readonly DefaultRanges<long> defaultRanges;
        private readonly Configuration configuration;

        private HttpClient httpClient;

        public Profile()
        {
            this.servicesBuilder = () => new WorkspaceServices();
            this.idGenerator = new IdGenerator();
            this.defaultRanges = new DefaultStructRanges<long>();

            var metaPopulation = new MetaBuilder().Build();
            var objectFactory = new ReflectionObjectFactory(metaPopulation, typeof(Allors.Workspace.Domain.Person));
            var rules = new IRule[] { new PersonSessionFullNameRule(metaPopulation) };
            this.configuration = new Configuration("Default", metaPopulation, objectFactory, rules);
        }

        IWorkspace IProfile.Workspace => this.Workspace;

        public DatabaseConnection DatabaseConnection { get; private set; }

        public IWorkspace Workspace { get; private set; }

        public M M => ((IWorkspaceServices)this.Workspace.Services).Get<M>();

        public async Task InitializeAsync()
        {
            this.httpClient = new HttpClient { BaseAddress = new Uri(Url), Timeout = TimeSpan.FromMinutes(30) };
            var response = await this.httpClient.GetAsync(SetupUrl);
            Assert.True(response.IsSuccessStatusCode);

            this.DatabaseConnection = new DatabaseConnection(this.configuration, this.servicesBuilder, this.httpClient, this.idGenerator, this.defaultRanges);
            this.Workspace = this.DatabaseConnection.CreateWorkspace();

            await this.Login("administrator");
        }

        public Task DisposeAsync() => Task.CompletedTask;

        public IWorkspace CreateExclusiveWorkspace()
        {
            var database = new DatabaseConnection(this.configuration, this.servicesBuilder, this.httpClient, this.idGenerator, this.defaultRanges);
            return database.CreateWorkspace();
        }

        public IWorkspace CreateWorkspace() => this.DatabaseConnection.CreateWorkspace();

        public Task Login(string user)
        {
            // Bearer/JWT is retired; authenticate with the test-only X-Allors-TestUser header, which the
            // harness server resolves to the same Allors user.
            this.httpClient.DefaultRequestHeaders.Remove("X-Allors-TestUser");
            this.httpClient.DefaultRequestHeaders.Add("X-Allors-TestUser", user);
            return Task.CompletedTask;
        }
    }
}
