// <copyright file="PushReadTransport.cs" company="Allors bv">
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
    using Allors.Protocol.Json.Api.PushRead;
    using Allors.Protocol.Json.SystemTextJson;

    /// <summary>
    /// Executes one PushRead through its strict raw boundary using a host-bound Local identity.
    /// It owns no Session, cache, retry policy or ambient editing transaction.
    /// </summary>
    public sealed class PushReadTransport : Remote.IPushReadTransport
    {
        private readonly PushReadExecutor executor;
        private readonly string workspaceName;
        private readonly LocalReadIdentity identity;

        public PushReadTransport(PushReadExecutor executor, string workspaceName, LocalReadIdentity identity)
        {
            this.executor = executor ?? throw new ArgumentNullException(nameof(executor));
            this.workspaceName = workspaceName;
            this.identity = identity ?? throw new ArgumentNullException(nameof(identity));
        }

        public PushReadRequest CaptureRequest(PushReadRequest request)
        {
            var raw = JsonSerializer.Serialize(request);
            if (System.Text.Encoding.UTF8.GetByteCount(raw) > PushReadExecutor.MaxRequestBytes)
                throw new FormatException("Reduce the PushRead request to at most 1 MiB before sending it.");
            return PushReadJson.DeserializeRequest(raw);
        }

        public PushReadReceipt CaptureReceipt(string response, PushReadRequest request) => PushReadJson.DeserializeReceipt(response, request);
        public object CapturePart(string response, PushReadRequest request, bool changed) => PushReadJson.DeserializePart(response, request, changed);
        public Task<string> Send(PushReadRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var captured = this.CaptureRequest(request);
            return Task.FromResult(this.executor.Execute(JsonSerializer.Serialize(captured), this.workspaceName, this.identity, cancellationToken));
        }

        public Task<PushReadResponse> PushRead(PushReadRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return this.PushRead(JsonSerializer.Serialize(request), cancellationToken);
        }

        public Task<PushReadResponse> PushRead(string rawRequest, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (rawRequest != null && System.Text.Encoding.UTF8.GetByteCount(rawRequest) > PushReadExecutor.MaxRequestBytes)
            {
                throw new FormatException("Reduce the PushRead request to at most 1 MiB before sending it.");
            }

            var request = PushReadJson.DeserializeRequest(rawRequest);
            var rawResponse = this.executor.Execute(rawRequest, this.workspaceName, this.identity, cancellationToken);
            // The executor preserves a known commit even if cancellation arrived after it.
            return Task.FromResult(PushReadJson.DeserializeResponse(rawResponse, request));
        }
    }
}
