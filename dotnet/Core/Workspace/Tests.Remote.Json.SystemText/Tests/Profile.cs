// <copyright file="Profile.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Tests.Workspace.Remote
{
    using System;
    using System.Net.Http;
    using System.Collections.Generic;
    using System.Linq;
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
    using Users = Allors.Database.Domain.Users;

    public class Profile : IProfile
    {
        public const string Url = "http://localhost:5000/allors/";

        public const string SetupUrl = "Test/Setup?population=full";

        private readonly Func<IWorkspaceServices> servicesBuilder;
        private readonly IdGenerator idGenerator;
        private readonly DefaultRanges<long> defaultRanges;
        private readonly Configuration configuration;

        private readonly Func<HttpMessageHandler> handlerFactory;
        private readonly List<(DatabaseConnection Connection, HttpClient Client)> connections = new List<(DatabaseConnection, HttpClient)>();
        private string credential;

        public Profile(Func<HttpMessageHandler> handlerFactory = null)
        {
            this.handlerFactory = handlerFactory;
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
            using var setup = new HttpClient { BaseAddress = new Uri(Environment.GetEnvironmentVariable("ALLORS_TEST_URL") ?? Url), Timeout = TimeSpan.FromMinutes(30) };
            using var response = await setup.GetAsync(SetupUrl);
            Assert.True(response.IsSuccessStatusCode);
            await this.Login("administrator");
        }

        public async Task DisposeAsync()
        {
            var owned = this.connections.ToArray();
            this.connections.Clear();
            this.DatabaseConnection = null;
            this.Workspace = null;
            var errors = new List<Exception>();
            var draining = owned.Select(pair => pair.Connection.DisposeAsync()).ToArray();
            for (var i = 0; i < owned.Length; ++i)
            {
                try { await draining[i]; }
                catch (Exception exception) { errors.Add(exception); }
                try { owned[i].Client.Dispose(); }
                catch (Exception exception) { errors.Add(exception); }
            }
            if (errors.Count != 0) throw new AggregateException(errors);
        }

        public async Task<IWorkspace> CreateExclusiveWorkspaceAsync()
        {
            if (this.DatabaseConnection == null) throw new InvalidOperationException("Log in before creating a workspace.");
            var connection = await this.CreateConnectionAsync();
            return connection.CreateWorkspace();
        }

        public IWorkspace CreateWorkspace() => (this.DatabaseConnection ?? throw new InvalidOperationException("Log in before creating a workspace.")).CreateWorkspace();

        internal Task<HttpTestResources> CreateHttpTestResourcesAsync(string revision, string context, HttpMessageHandler handler = null,
            Action applicationServicesCreated = null) =>
            HttpTestResources.CreateAsync(this.configuration, this.servicesBuilder,
                new Uri(Environment.GetEnvironmentVariable("ALLORS_TEST_URL") ?? Url), this.credential, revision, context, handler, applicationServicesCreated);

        public async Task Login(string user)
        {
            await this.DisposeAsync();
            // The harness resolves this fixed test credential to its committed Allors user.
            this.credential = Users.TestUserId(user).ToString();
            var connection = await this.CreateConnectionAsync();
            this.DatabaseConnection = connection;
            this.Workspace = connection.CreateWorkspace();
        }

        private async Task<DatabaseConnection> CreateConnectionAsync()
        {
            var client = this.handlerFactory == null ? new HttpClient() : new HttpClient(this.handlerFactory());
            try
            {
                client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("ALLORS_TEST_URL") ?? Url);
                client.Timeout = TimeSpan.FromMinutes(30);
                client.DefaultRequestHeaders.Add("X-Allors-TestUser", this.credential);
                var connection = await DatabaseConnection.CreateAsync(this.configuration, this.servicesBuilder, client,
                    this.idGenerator, this.defaultRanges, "core-read-v1");
                this.connections.Add((connection, client));
                return connection;
            }
            catch
            {
                client.Dispose();
                throw;
            }
        }
    }
}
