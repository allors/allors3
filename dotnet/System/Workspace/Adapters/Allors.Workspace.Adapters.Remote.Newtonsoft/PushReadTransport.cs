// <copyright file="PushReadTransport.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote.Newtonsoft
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Allors.Protocol.Json.Api.PushRead;
    using Allors.Protocol.Json.Newtonsoft;

    /// <summary>A strict single HTTP attempt. Cancellation after dispatch may leave the mutation uncertain.</summary>
    public sealed class PushReadTransport : IPushReadTransport
    {
        private readonly HttpClient client;
        public PushReadTransport(HttpClient client) => this.client = client ?? throw new ArgumentNullException(nameof(client));
        public PushReadRequest CaptureRequest(PushReadRequest request)
        {
            var raw = global::Newtonsoft.Json.JsonConvert.SerializeObject(request);
            if (Encoding.UTF8.GetByteCount(raw) > ReadTransport.MaxRequestBytes)
                throw new FormatException("Reduce the PushRead request to at most 1 MiB before sending it.");
            return PushReadJson.DeserializeRequest(raw);
        }
        public PushReadReceipt CaptureReceipt(string response, PushReadRequest request) => PushReadJson.DeserializeReceipt(response, request);
        public object CapturePart(string response, PushReadRequest request, bool changed) => PushReadJson.DeserializePart(response, request, changed);
        public async Task<string> Send(PushReadRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var captured = this.CaptureRequest(request);
            using var message = new HttpRequestMessage(HttpMethod.Post, new Uri("push-read", UriKind.Relative))
            { Content = new StringContent(global::Newtonsoft.Json.JsonConvert.SerializeObject(captured), Encoding.UTF8, "application/json") };
            using var response = await this.client.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength > ReadTransport.MaxResponseBytes)
                throw new FormatException("The PushRead response exceeds 16 MiB. Reconcile the mutation before further editing.");
            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var body = new MemoryStream();
            var buffer = new byte[8192];
            int count;
            while ((count = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                if (body.Length + count > ReadTransport.MaxResponseBytes)
                    throw new FormatException("The PushRead response exceeds 16 MiB. Reconcile the mutation before further editing.");
                body.Write(buffer, 0, count);
            }
            // Once the complete envelope is received, preserve its receipt even if cancellation races decoding.
            return new UTF8Encoding(false, true).GetString(body.ToArray());
        }
    }
}
