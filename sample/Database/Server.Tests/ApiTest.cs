// <copyright file="ApiTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Server.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Allors.Database;
    using Allors.Database.Domain;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Protocol.Json.Auth;
    using Allors.Services;
    using Microsoft.AspNetCore.Mvc.Testing;
    using Microsoft.Extensions.DependencyInjection;
    using MetaPopulation = Allors.Database.Meta.MetaPopulation;

    // A test of the server over HTTP, in the process of the test. Every test starts a server of its own on the
    // Snapshot adapter and gives its database the population of the sample's setup. The requests name the host
    // localhost:5000, which the Startup maps to the workspace Default.
    public abstract class ApiTest : IDisposable
    {
        // The key has a character outside ASCII, so a server that validated the tokens with other bytes of the
        // key than those that sign them would refuse them.
        public const string Key = "The key that signs the bearer tokens of the tests, naïvely long";

        protected ApiTest(IDictionary<string, string> settings = null)
        {
            var serverSettings = new Dictionary<string, string>
            {
                ["adapter"] = "snapshot",
                ["JwtToken:Key"] = Key,
            };

            foreach (var setting in settings ?? new Dictionary<string, string>())
            {
                serverSettings[setting.Key] = setting.Value;
            }

            this.Factory = new ServerFactory(serverSettings);
            this.HttpClient = this.Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("http://localhost:5000/allors/"),
            });

            this.Database = this.Factory.Services.GetRequiredService<IDatabaseService>().Database;
            this.M = this.Database.Services.Get<MetaPopulation>();

            this.Database.Init();
            new Setup(this.Database, new Config()).Apply();
        }

        public ServerFactory Factory { get; }

        public HttpClient HttpClient { get; }

        public IDatabase Database { get; }

        public MetaPopulation M { get; }

        public void Dispose()
        {
            this.HttpClient.Dispose();
            this.Factory.Dispose();
        }

        // Signs in at the sign-in endpoint, and sends the bearer token with the requests that follow.
        protected async Task<AuthenticationTokenResponse> SignIn(string userName, string password)
        {
            var response = await this.PostAsJsonAsync("Authentication/Token", new AuthenticationTokenRequest { l = userName, p = password });
            response.EnsureSuccessStatusCode();

            var tokenResponse = await ReadAsAsync<AuthenticationTokenResponse>(response);
            this.HttpClient.DefaultRequestHeaders.Authorization = tokenResponse.a ? new AuthenticationHeaderValue("Bearer", tokenResponse.t) : null;
            return tokenResponse;
        }

        protected Task<PullResponse> Pull(PullRequest request) => this.Post<PullResponse>("pull", request);

        protected Task<SyncResponse> Sync(SyncRequest request) => this.Post<SyncResponse>("sync", request);

        protected Task<PushResponse> Push(PushRequest request) => this.Post<PushResponse>("push", request);

        protected async Task<HttpResponseMessage> PostAsJsonAsync(string uri, object request)
        {
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            return await this.HttpClient.PostAsync(new Uri(uri, UriKind.Relative), content);
        }

        private async Task<T> Post<T>(string uri, object request)
        {
            var response = await this.PostAsJsonAsync(uri, request);
            response.EnsureSuccessStatusCode();
            return await ReadAsAsync<T>(response);
        }

        private static async Task<T> ReadAsAsync<T>(HttpResponseMessage response) => JsonSerializer.Deserialize<T>(await response.Content.ReadAsStringAsync());
    }
}
