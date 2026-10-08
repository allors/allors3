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
    using Allors.Workspace.Connection;
    using Allors.Workspace.Connection.Remote.Newtonsoft;
    using Allors.Workspace.Derivations;
    using Allors.Workspace.Domain;
    using Allors.Workspace.Meta;
    using Allors.Workspace.Meta.Lazy;
    using Allors.Workspace.Session;
    using Xunit;
    using IWorkspaceServices = Allors.Workspace.IWorkspaceServices;
    using Users = Allors.Database.Domain.Users;

    public class Profile : IProfile
    {
        public const string Url = "http://localhost:5000/allors/";

        public const string SetupUrl = "Test/Setup?population=full";

        private const string WorkspaceName = "Default";

        private readonly IdGenerator idGenerator;
        private readonly DefaultRanges<long> ranges;
        private readonly M metaPopulation;
        private readonly IObjectFactory objectFactory;
        private readonly IRule[] rules;

        // One connection pool for every client of this profile; a client per user carries the
        // user's header.
        private readonly HttpMessageHandler httpMessageHandler;

        private string userName;

        public Profile()
        {
            this.idGenerator = new IdGenerator();
            this.ranges = new DefaultStructRanges<long>();

            this.metaPopulation = new MetaBuilder().Build();
            this.objectFactory = new ReflectionObjectFactory(this.metaPopulation, typeof(Allors.Workspace.Domain.Person));
            this.rules = new IRule[] { new PersonSessionFullNameRule(this.metaPopulation) };

            this.httpMessageHandler = new SocketsHttpHandler();
        }

        IWorkspace IProfile.Workspace => this.Workspace;

        public IDatabaseConnection DatabaseConnection { get; private set; }

        public IWorkspace Workspace { get; private set; }

        public M M => ((IWorkspaceServices)this.Workspace.Services).Get<M>();

        public async Task InitializeAsync()
        {
            var response = await this.CreateHttpClient().GetAsync(SetupUrl);
            Assert.True(response.IsSuccessStatusCode);

            await this.Login("administrator");
        }

        public Task DisposeAsync() => Task.CompletedTask;

        public IWorkspace CreateExclusiveWorkspace() => this.CreateWorkspace(this.CreateConnection());

        public IWorkspace CreateWorkspace() => this.CreateWorkspace(this.DatabaseConnection);

        /// <summary>
        /// Signs in as the user: a connection of its own, as a connection serves one user, and a
        /// workspace on it.
        /// </summary>
        public Task Login(string userName)
        {
            this.userName = userName;

            this.DatabaseConnection = this.CreateConnection();
            this.Workspace = this.CreateWorkspace(this.DatabaseConnection);

            return Task.CompletedTask;
        }

        public ITransport CreateTransport(string userName)
        {
            // Authenticate with the test-only X-Allors-TestUser header, which carries the UniqueId of a
            // user of the test population; the harness server resolves it to the same Allors user.
            var httpClient = this.CreateHttpClient();
            httpClient.DefaultRequestHeaders.Add("X-Allors-TestUser", Users.TestUserId(userName).ToString());
            return new HttpTransport(httpClient);
        }

        private HttpClient CreateHttpClient() => new HttpClient(this.httpMessageHandler, false) { BaseAddress = new Uri(Url), Timeout = TimeSpan.FromMinutes(30) };

        private DatabaseConnection CreateConnection() =>
            new DatabaseConnection(WorkspaceName, this.metaPopulation, this.CreateTransport(this.userName), this.ranges);

        private Workspace CreateWorkspace(IDatabaseConnection connection) =>
            new Workspace(connection, this.objectFactory, this.rules, new WorkspaceServices(), this.idGenerator);
    }
}
