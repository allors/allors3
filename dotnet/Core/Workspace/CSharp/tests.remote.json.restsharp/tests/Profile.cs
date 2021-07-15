// <copyright file="Profile.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Remote
{
    using System;
    using System.Threading.Tasks;
    using Allors.Ranges;
    using Allors.Workspace;
    using Allors.Workspace.Adapters;
    using Allors.Workspace.Domain;
    using Allors.Workspace.Meta;
    using Allors.Workspace.Meta.Lazy;
    using RestSharp;
    using RestSharp.Serializers.NewtonsoftJson;
    using Xunit;
    using Configuration = Allors.Workspace.Adapters.Remote.Configuration;
    using DatabaseConnection = Allors.Workspace.Adapters.Remote.ResthSharp.DatabaseConnection;

    public class Profile : IProfile
    {
        public const string Url = "http://localhost:5000/allors/";

        public const string SetupUrl = "Test/Setup?population=full";
        public const string LoginUrl = "TestAuthentication/Token";

        private readonly Configuration configuration;
        private readonly IdGenerator idGenerator;
        private readonly DefaultRanges defaultRanges;

        IWorkspace IProfile.Workspace => this.Workspace;

        public DatabaseConnection Database { get; private set; }

        public IWorkspace Workspace { get; private set; }

        public M M => this.Workspace.Context().M;

        public Profile()
        {
            var metaPopulation = new MetaBuilder().Build();
            var objectFactory = new ReflectionObjectFactory(metaPopulation, typeof(Allors.Workspace.Domain.Person));
            this.configuration = new Configuration("Default", metaPopulation, objectFactory);
            this.idGenerator = new IdGenerator();
            this.defaultRanges = new DefaultRanges();
        }

        public async Task InitializeAsync()
        {
            var request = new RestRequest($"{Url}{SetupUrl}", RestSharp.Method.GET, DataFormat.Json);
            var restClient = this.CreateRestClient();
            var response = await restClient.ExecuteAsync(request);
            Assert.True(response.IsSuccessful);

            this.Database = new DatabaseConnection(this.configuration, () => new WorkspaceContext(), this.CreateRestClient, this.idGenerator, this.defaultRanges);
            this.Workspace = this.Database.CreateWorkspace();

            await this.Login("administrator");
        }

        public Task DisposeAsync() => Task.CompletedTask;

        public IDatabaseConnection CreateDatabase() => new DatabaseConnection(this.configuration, () => new WorkspaceContext(), this.CreateRestClient, this.idGenerator, this.defaultRanges);

        public IWorkspace CreateWorkspace() => this.Database.CreateWorkspace();

        public async Task Login(string user)
        {
            var uri = new Uri(LoginUrl, UriKind.Relative);
            var response = await this.Database.Login(uri, user, null);
            Assert.True(response);
        }

        private IRestClient CreateRestClient() => new RestClient(Url).UseNewtonsoftJson();
    }
}
