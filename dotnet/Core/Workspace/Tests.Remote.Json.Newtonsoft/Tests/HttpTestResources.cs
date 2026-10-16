// <copyright file="HttpTestResources.cs" company="Allors bv">
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
    using Configuration = Allors.Workspace.Adapters.Remote.Configuration;
    using DatabaseConnection = Allors.Workspace.Adapters.Remote.Newtonsoft.DatabaseConnection;
    using Session = Allors.Workspace.Adapters.Remote.Session;

    internal sealed class HttpTestResources : IAsyncDisposable
    {
        private readonly HttpClient client;

        private HttpTestResources(DatabaseConnection connection, HttpClient client)
        {
            this.Connection = connection;
            this.client = client;
            this.Workspace = connection.CreateWorkspace();
        }

        internal DatabaseConnection Connection { get; }
        internal IWorkspace Workspace { get; }

        internal static async Task<HttpTestResources> CreateAsync(Configuration configuration, Func<IWorkspaceServices> servicesBuilder,
            Uri endpoint, string credential, string revision, string context, HttpMessageHandler handler = null, Action applicationServicesCreated = null)
        {
            var client = handler == null ? new HttpClient() : new HttpClient(handler);
            DatabaseConnection connection = null;
            try
            {
                client.BaseAddress = endpoint;
                client.Timeout = TimeSpan.FromMinutes(30);
                client.DefaultRequestHeaders.Add("X-Allors-TestUser", credential);
                connection = await DatabaseConnection.CreateAsync(configuration, () =>
                {
                    applicationServicesCreated?.Invoke();
                    return servicesBuilder();
                }, client, new IdGenerator(), new DefaultStructRanges<long>(), revision, context);
                return new HttpTestResources(connection, client);
            }
            catch
            {
                try { if (connection != null) await connection.DisposeAsync(); }
                finally { client.Dispose(); }
                throw;
            }
        }

        internal Session CreateSession(IWorkspace workspace = null) => (Session)(workspace ?? this.Workspace).CreateSession();

        public async ValueTask DisposeAsync()
        {
            try { await this.Connection.DisposeAsync(); }
            finally { this.client.Dispose(); }
        }
    }
}
