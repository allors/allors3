// <copyright file="Profile.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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
    using Allors.Workspace.Domain;
    using Allors.Workspace.Meta;
    using Allors.Workspace.Meta.Lazy;
    using Xunit;
    using Configuration = Allors.Workspace.Adapters.Remote.Configuration;
    using DatabaseConnection = Allors.Workspace.Adapters.Remote.SystemText.DatabaseConnection;

    public class Profile : IProfile
    {
        public const string Url = "http://localhost:5000/allors/";

        public const string SetupUrl = "Test/Setup?population=full";
        public const string LoginUrl = "TestAuthentication/Token";

        private readonly Func<IWorkspaceServices> servicesBuilder;
        private readonly IdGenerator idGenerator;
        private readonly DefaultRanges defaultRanges;
        private readonly Configuration configuration;

        private HttpClient httpClient;

        public Profile()
        {
            this.servicesBuilder = () => new WorkspaceContext();
            this.idGenerator = new IdGenerator();
            this.defaultRanges = new DefaultRanges();

            var metaPopulation = new MetaBuilder().Build();
            var objectFactory = new ReflectionObjectFactory(metaPopulation, typeof(Allors.Workspace.Domain.Person));
            this.configuration = new Configuration("Default", metaPopulation, objectFactory);
        }

        IWorkspace IProfile.Workspace => this.Workspace;

        public DatabaseConnection Database { get; private set; }

        public IWorkspace Workspace { get; private set; }

        public M M => this.Workspace.Context().M;

        public async Task InitializeAsync()
        {
            this.httpClient = new HttpClient { BaseAddress = new Uri(Url), Timeout = TimeSpan.FromMinutes(30) };
            var response = await this.httpClient.GetAsync(SetupUrl);
            Assert.True(response.IsSuccessStatusCode);

            this.Database = new DatabaseConnection(this.configuration, this.servicesBuilder, this.httpClient, this.idGenerator, this.defaultRanges);
            this.Workspace = this.Database.CreateWorkspace();

            await this.Login("administrator");
        }

        public Task DisposeAsync() => Task.CompletedTask;

        public IDatabaseConnection CreateDatabase() => new DatabaseConnection(this.configuration, this.servicesBuilder, this.httpClient, this.idGenerator, this.defaultRanges);

        public IWorkspace CreateWorkspace() => this.Database.CreateWorkspace();

        public async Task Login(string user)
        {
            var uri = new Uri(LoginUrl, UriKind.Relative);
            var response = await this.Database.Login(uri, user, null);
            Assert.True(response);
        }
    }
}
