// <copyright file="InvokeReadTransport.cs" company="Allors bv">
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
    using Allors.Protocol.Json.Api.InvokeRead;
    using Allors.Protocol.Json.SystemTextJson;

    /// <summary>
    /// Executes one InvokeRead through its strict raw boundary using a host-bound Local identity.
    /// It owns no Session, cache, retry policy or ambient editing transaction.
    /// </summary>
    public sealed class InvokeReadTransport : Remote.IInvokeReadTransport
    {
        private readonly InvokeReadExecutor executor;
        private readonly string workspaceName;
        private readonly LocalReadIdentity identity;

        public InvokeReadTransport(InvokeReadExecutor executor, string workspaceName, LocalReadIdentity identity)
        {
            this.executor = executor ?? throw new ArgumentNullException(nameof(executor));
            this.workspaceName = workspaceName;
            this.identity = identity ?? throw new ArgumentNullException(nameof(identity));
        }

        public InvokeReadRequest CaptureRequest(InvokeReadRequest request)
        {
            var raw = JsonSerializer.Serialize(request);
            if (System.Text.Encoding.UTF8.GetByteCount(raw) > InvokeReadExecutor.MaxRequestBytes)
                throw new FormatException("Reduce the InvokeRead request to at most 1 MiB before sending it.");
            return InvokeReadJson.DeserializeRequest(raw);
        }

        public InvokeReadReceipt CaptureReceipt(string response, InvokeReadRequest request) => InvokeReadJson.DeserializeReceipt(response, request);
        public object CapturePart(string response, InvokeReadRequest request) => InvokeReadJson.DeserializeChanged(response, request);
        public Task<string> Send(InvokeReadRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var captured = this.CaptureRequest(request);
            return Task.FromResult(this.executor.Execute(JsonSerializer.Serialize(captured), this.workspaceName, this.identity, cancellationToken));
        }

        public Task<InvokeReadResponse> InvokeRead(InvokeReadRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return this.InvokeRead(JsonSerializer.Serialize(request), cancellationToken);
        }

        public Task<InvokeReadResponse> InvokeRead(string rawRequest, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (rawRequest != null && System.Text.Encoding.UTF8.GetByteCount(rawRequest) > InvokeReadExecutor.MaxRequestBytes)
            {
                throw new FormatException("Reduce the InvokeRead request to at most 1 MiB before sending it.");
            }

            var request = InvokeReadJson.DeserializeRequest(rawRequest);
            var rawResponse = this.executor.Execute(rawRequest, this.workspaceName, this.identity, cancellationToken);
            // The executor preserves a known commit even if cancellation arrived after it.
            return Task.FromResult(InvokeReadJson.DeserializeResponse(rawResponse, request));
        }
    }
}
