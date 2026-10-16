// <copyright file="ReadTransport.cs" company="Allors bv">
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
    using Allors.Protocol.Json.Api.Read;
    using Allors.Protocol.Json.Newtonsoft;
    using global::Newtonsoft.Json;

    /// <summary>
    /// Opt-in complete-read transport. Captures strict detached messages and makes one HTTP attempt;
    /// it owns no Session state, authentication admission, caches, or retry policy.
    /// </summary>
    public sealed class ReadTransport : Allors.Workspace.Adapters.Remote.IReadTransport
    {
        public const int MaxRequestBytes = 1024 * 1024;
        public const int MaxResponseBytes = 16 * 1024 * 1024;
        private readonly HttpClient httpClient;

        public ReadTransport(HttpClient httpClient) => this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public ReadRequest CaptureRequest(ReadRequest request) => ReadJson.DeserializeRequest(JsonConvert.SerializeObject(request));

        public int RequestByteCount(ReadRequest request) => System.Text.Encoding.UTF8.GetByteCount(JsonConvert.SerializeObject(request));

        public object CaptureResponse(object response, ReadRequest request)
        {
            var raw = JsonConvert.SerializeObject(response);
            if (System.Text.Encoding.UTF8.GetByteCount(raw) > 16 * 1024 * 1024)
            {
                throw new FormatException("The workspace read response exceeds 16 MiB. Request a smaller selection.");
            }

            var captured = ReadJson.DeserializeResponse(raw, request);

            return captured;
        }

        public Task<object> Read(ReadRequest request, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return this.Read(JsonConvert.SerializeObject(request), cancellationToken);
        }

        public async Task<object> Read(string rawRequest, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (rawRequest != null && Encoding.UTF8.GetByteCount(rawRequest) > MaxRequestBytes)
            {
                throw new FormatException("Reduce the workspace read request to at most 1 MiB before sending it.");
            }

            var request = ReadJson.DeserializeRequest(rawRequest);
            using var message = new HttpRequestMessage(HttpMethod.Post, new Uri("read", UriKind.Relative))
            {
                Content = new StringContent(rawRequest, Encoding.UTF8, "application/json"),
            };
            using var response = await this.httpClient.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            cancellationToken.ThrowIfCancellationRequested();
            if (response.Content.Headers.ContentLength > MaxResponseBytes)
            {
                throw new FormatException("The workspace read response exceeds 16 MiB. Request a smaller selection.");
            }

            using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var body = new MemoryStream();
            var buffer = new byte[8192];
            int count;
            while ((count = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0)
            {
                if (body.Length + count > MaxResponseBytes)
                {
                    throw new FormatException("The workspace read response exceeds 16 MiB. Request a smaller selection.");
                }

                body.Write(buffer, 0, count);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var result = ReadJson.DeserializeResponse(new UTF8Encoding(false, true).GetString(body.ToArray()), request);
            cancellationToken.ThrowIfCancellationRequested();
            return result;
        }
    }
}
