// <copyright file="RemoteDatabase.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote.SystemText
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.SystemTextJson;
    using Ranges;

    public class DatabaseConnection : Remote.DatabaseConnection
    {
        private DatabaseConnection(Configuration configuration, Func<IWorkspaceServices> servicesBuilder, HttpClient httpClient,
            IdGenerator idGenerator, IRanges<long> ranges) : base(configuration, idGenerator, servicesBuilder, ranges)
        {
            this.HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.UnitConvert = new UnitConvert();
        }

        public static async Task<DatabaseConnection> CreateAsync(Configuration configuration, Func<IWorkspaceServices> servicesBuilder,
            HttpClient httpClient, IdGenerator idGenerator, IRanges<long> ranges, string revision,
            string context = null, CancellationToken cancellationToken = default)
        {
            var connection = new DatabaseConnection(configuration, servicesBuilder, httpClient, idGenerator, ranges);
            await connection.InitializeAsync(new ReadTransport(httpClient), revision, context,
                new PushReadTransport(httpClient), new InvokeReadTransport(httpClient), cancellationToken).ConfigureAwait(false);
            return connection;
        }

        public override IUnitConvert UnitConvert { get; }
        public HttpClient HttpClient { get; }
    }
}
