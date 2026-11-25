// <copyright file="Profile.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Workspace.Tests.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Database.Domain;
    using Allors.Ranges;
    using Allors.Services;
    using Allors.Workspace;
    using Allors.Workspace.Adapters;
    using Allors.Workspace.Derivations;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using Configuration = Allors.Workspace.Adapters.Remote.Configuration;
    using DatabaseConnection = Allors.Workspace.Adapters.Remote.SystemText.DatabaseConnection;
    using MetaBuilder = Allors.Workspace.Meta.Lazy.MetaBuilder;
    using Person = Allors.Workspace.Domain.Person;

    // The Remote adapter, over the JSON API of the sample's server, which runs in the process of the test on the
    // Snapshot adapter. The connection of a workspace signs in at the sign-in endpoint, and sends the bearer
    // token with the requests that follow.
    public class Profile : IProfile
    {
        // The last character takes two UTF-8 bytes, bringing the key to the 32 bytes HS256 requires.
        private const string Key = "012345678901234567890123456789é";

        private readonly ServerFactory factory;
        private readonly List<HttpClient> httpClients = new List<HttpClient>();
        private readonly Configuration configuration;

        public Profile()
        {
            this.factory = new ServerFactory(new Dictionary<string, string>
            {
                ["adapter"] = "snapshot",
                ["JwtToken:Key"] = Key,
            });

            this.Database = this.factory.Services.GetRequiredService<IDatabaseService>().Database;

            this.Database.Init();
            new Setup(this.Database, new Config()).Apply();
            Population.Apply(this.Database);

            var metaPopulation = new MetaBuilder().Build();
            var objectFactory = new ReflectionObjectFactory(metaPopulation, typeof(Person));
            this.configuration = new Configuration("Default", metaPopulation, objectFactory, Array.Empty<IRule>());
        }

        public IDatabase Database { get; }

        public async Task<IWorkspace> CreateWorkspace(string userName)
        {
            var httpClient = this.factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("http://localhost:5000/allors/"),
            });
            this.httpClients.Add(httpClient);

            var connection = new DatabaseConnection(this.configuration, () => new WorkspaceServices(), httpClient, new IdGenerator(), new DefaultStructRanges<long>());
            if (!await connection.Login(new Uri("Authentication/Token", UriKind.Relative), userName, Population.Password))
            {
                throw new InvalidOperationException($"The user {userName} could not sign in.");
            }

            return connection.CreateWorkspace();
        }

        public void Dispose()
        {
            foreach (var httpClient in this.httpClients)
            {
                httpClient.Dispose();
            }

            this.factory.Dispose();
        }
    }
}
