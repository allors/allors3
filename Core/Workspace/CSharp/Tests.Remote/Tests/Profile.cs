// <copyright file="Profile.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Remote
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Allors.Workspace;
    using Allors.Workspace.Adapters;
    using Allors.Workspace.Domain;
    using Allors.Workspace.Meta;
    using Allors.Workspace.Meta.Lazy;
    using Xunit;
    using DatabaseConnection = Allors.Workspace.Adapters.Remote.DatabaseConnection;
    using User = Allors.Workspace.Domain.User;
    using Workspace = Allors.Workspace.Adapters.Remote.Workspace;

    public class Profile : IProfile
    {
        public const string Url = "http://localhost:5000/allors/";

        public const string SetupUrl = "Test/Setup?population=full";
        public const string LoginUrl = "TestAuthentication/Token";

        IWorkspace IProfile.Workspace => this.Workspace;

        public DatabaseConnection Database { get; private set; }

        public IWorkspace Workspace { get; private set; }

        public M M => this.Workspace.Context().M;

        public async Task InitializeAsync()
        {
            var metaPopulation = new MetaBuilder().Build();
            var objectFactory = new ReflectionObjectFactory(metaPopulation, typeof(Allors.Workspace.Domain.Person));
            var configuration = new Allors.Workspace.Adapters.Remote.Configuration("Default", metaPopulation, objectFactory, new WorkspaceContext());
            var httpClient = new HttpClient() { BaseAddress = new Uri(Url) };
            this.Database = new DatabaseConnection(configuration, httpClient);

            this.Workspace = this.Database.CreateWorkspace();

            var response = await this.Database.HttpClient.GetAsync(SetupUrl);
            Assert.True(response.IsSuccessStatusCode);
            await this.Login("administrator");
        }

        public Task DisposeAsync() => Task.CompletedTask;

        public async Task Login(string user)
        {

            var uri = new Uri(LoginUrl, UriKind.Relative);
            var response = await this.Database.Login(uri, user, null);
            Assert.True(response);
        }
    }
}
