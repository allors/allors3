// <copyright file="ReadTransport.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using Allors.Database.Protocol.Json;
    using Allors.Protocol.Json.Api.Read;
    using Allors.Protocol.Json.SystemTextJson;

    /// <summary>
    /// Opt-in Local transport over the same strict raw boundary as HTTP. The host supplies an
    /// authenticated population-bound identity after admission; no ambient transaction is reused.
    /// </summary>
    public sealed class ReadTransport : Allors.Workspace.Adapters.Remote.IReadTransport
    {
        private readonly ReadExecutor executor;
        private readonly string workspaceName;
        private readonly LocalReadIdentity identity;

        public ReadTransport(ReadExecutor executor, string workspaceName, LocalReadIdentity identity)
        {
            this.executor = executor ?? throw new ArgumentNullException(nameof(executor));
            this.workspaceName = workspaceName;
            this.identity = identity ?? throw new ArgumentNullException(nameof(identity));
        }

        public ReadRequest CaptureRequest(ReadRequest request) => ReadJson.DeserializeRequest(JsonSerializer.Serialize(request));

        public int RequestByteCount(ReadRequest request) => System.Text.Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(request));

        public object CaptureResponse(object response, ReadRequest request)
        {
            var raw = JsonSerializer.Serialize(response);
            if (System.Text.Encoding.UTF8.GetByteCount(raw) > 16 * 1024 * 1024)
            {
                throw new FormatException("The workspace read response exceeds 16 MiB. Request a smaller selection.");
            }

            var captured = ReadJson.DeserializeResponse(raw, request);

            if (captured is ReadSuccess complete)
            {
                foreach (var record in complete.p)
                {
                    foreach (var role in record.p.ro ?? Array.Empty<Allors.Protocol.Json.Api.Sync.SyncResponseRole>())
                    {
                        role.v = NormalizeValue(role.v);
                    }
                }
            }

            return captured;
        }

        private static object NormalizeValue(object encoded)
        {
            if (!(encoded is JsonElement value)) { return encoded; }
            return value.ValueKind switch
            {
                JsonValueKind.Null => null,
                JsonValueKind.String => value.GetString(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number => value.TryGetInt64(out var integer) ? (object)integer : value.GetDouble(),
                _ => throw new FormatException("Encode a read unit as a scalar value."),
            };
        }

        public Task<object> Read(ReadRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return this.Read(JsonSerializer.Serialize(request), cancellationToken);
        }

        public Task<object> Read(string rawRequest, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (rawRequest != null && System.Text.Encoding.UTF8.GetByteCount(rawRequest) > ReadExecutor.MaxRequestBytes)
            {
                throw new FormatException("Reduce the workspace read request to at most 1 MiB before sending it.");
            }

            var request = ReadJson.DeserializeRequest(rawRequest);
            var rawResponse = this.executor.Execute(rawRequest, this.workspaceName, this.identity, cancellationToken);
            var response = ReadJson.DeserializeResponse(rawResponse, request);
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(response);
        }
    }
}
