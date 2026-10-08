// <copyright file="HttpTransport.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection.Remote.SystemText
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Protocol.Json.SystemTextJson;
    using Polly;

    /// <summary>
    /// The transport over HTTP with System.Text.Json: every call posts its request to the route
    /// of the server, relative to the base address of the client, and retries a failed request.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "RCS1090:Add call to 'ConfigureAwait' (or vice versa).", Justification = "<Pending>")]
    public class HttpTransport : ITransport
    {
        public HttpTransport(HttpClient httpClient)
        {
            this.HttpClient = httpClient;
            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            this.UnitConvert = new UnitConvert();
        }

        public IUnitConvert UnitConvert { get; }

        public IObservable<ServerMessage> ServerMessages => null;

        public IAsyncPolicy Policy { get; set; } = Polly.Policy
            .Handle<HttpRequestException>()
            .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        public HttpClient HttpClient { get; }

        public async Task<PullResponse> PullAsync(string name, object args)
        {
            var uri = new Uri($"{name}/pull", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, args);
            response.EnsureSuccessStatusCode();
            return await this.ReadAsAsync<PullResponse>(response);
        }

        public async Task<PullResponse> PullAsync(PullRequest pullRequest)
        {
            var uri = new Uri("pull", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, pullRequest);
            response.EnsureSuccessStatusCode();
            return await this.ReadAsAsync<PullResponse>(response);
        }

        public async Task<SyncResponse> SyncAsync(SyncRequest syncRequest)
        {
            var uri = new Uri("sync", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, syncRequest);
            response.EnsureSuccessStatusCode();

            return await this.ReadAsAsync<SyncResponse>(response);
        }

        public async Task<PushResponse> PushAsync(PushRequest pushRequest)
        {
            var uri = new Uri("push", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, pushRequest);
            response.EnsureSuccessStatusCode();

            return await this.ReadAsAsync<PushResponse>(response);
        }

        public async Task<InvokeResponse> InvokeAsync(InvokeRequest invokeRequest)
        {
            var uri = new Uri("invoke", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, invokeRequest);
            response.EnsureSuccessStatusCode();

            return await this.ReadAsAsync<InvokeResponse>(response);
        }

        public async Task<AccessResponse> AccessAsync(AccessRequest accessRequest)
        {
            var uri = new Uri("access", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, accessRequest);
            response.EnsureSuccessStatusCode();

            return await this.ReadAsAsync<AccessResponse>(response);
        }

        public async Task<PermissionResponse> PermissionAsync(PermissionRequest permissionRequest)
        {
            var uri = new Uri("permission", UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, permissionRequest);
            response.EnsureSuccessStatusCode();

            return await this.ReadAsAsync<PermissionResponse>(response);
        }

        private async Task<HttpResponseMessage> PostAsJsonAsync(Uri uri, object args) =>
            await this.Policy.ExecuteAsync(
                async () =>
                {
                    // TODO: use SerializeToUtf8Bytes()
                    var json = JsonSerializer.Serialize(args);
                    return await this.HttpClient.PostAsync(
                        uri,
                        new StringContent(json, Encoding.UTF8, "application/json"));
                });

        private async Task<T> ReadAsAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
