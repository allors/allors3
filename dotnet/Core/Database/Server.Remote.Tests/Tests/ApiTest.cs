// <copyright file="ApiTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Protocol.Json.Auth;
    using Xunit;

    /// <summary>
    /// A test against the Core test server over HTTP. The server owns the database: every test starts from
    /// the full population through <c>Test/Setup</c> and creates the users it needs through
    /// <c>Test/CreateUser</c>, so the tests run on whichever adapter the server runs on, the Snapshot
    /// adapter included.
    /// </summary>
    public abstract class ApiTest : IDisposable
    {
        public const string Url = "http://localhost:5000/allors/";
        public const string SetupUrl = "Test/Setup?population=full";
        public const string CreateUserUrl = "Test/CreateUser";
        public const string LoginUrl = "TestAuthentication/Token";

        /// <summary>
        /// The user name of the administrator of the full population.
        /// </summary>
        public const string Administrator = "jane@example.com";

        protected ApiTest()
        {
            this.HttpClientHandler = new HttpClientHandler();
            this.HttpClient = new HttpClient(this.HttpClientHandler)
            {
                BaseAddress = new Uri(Url),
            };

            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = this.HttpClient.GetAsync(SetupUrl).Result;

            Assert.True(response.IsSuccessStatusCode);
        }

        protected HttpClient HttpClient { get; set; }

        protected HttpClientHandler HttpClientHandler { get; set; }

        public void Dispose()
        {
            this.HttpClient.Dispose();
            this.HttpClient = null;
        }

        /// <summary>
        /// Creates a user on the server: a person with the user name, with the password when one is given,
        /// and with lockout enabled when asked.
        /// </summary>
        protected void CreateUser(string userName, string password = null, bool lockoutEnabled = false)
        {
            var query = $"{CreateUserUrl}?userName={Uri.EscapeDataString(userName)}&lockoutEnabled={lockoutEnabled}";
            if (password != null)
            {
                query += $"&password={Uri.EscapeDataString(password)}";
            }

            var response = this.HttpClient.PostAsync(new Uri(query, UriKind.Relative), null).Result;
            Assert.True(response.IsSuccessStatusCode, response.Content.ReadAsStringAsync().Result);
        }

        protected async Task SignIn(string userName)
        {
            var args = new AuthenticationTokenRequest
            {
                l = userName,
            };

            var uri = new Uri(LoginUrl, UriKind.Relative);
            var response = await this.PostAsJsonAsync(uri, args);
            var signInResponse = await this.ReadAsAsync<AuthenticationTokenResponse>(response);
            this.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", signInResponse.t);
        }

        protected void SignOut() => this.HttpClient.DefaultRequestHeaders.Authorization = null;

        protected Stream GetResource(string name)
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            return assembly.GetManifestResourceStream(name);
        }

        protected async Task<HttpResponseMessage> PostAsJsonAsync(Uri uri, object args)
        {
            var json = JsonSerializer.Serialize(args);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            return await this.HttpClient.PostAsync(uri, content);
        }

        protected async Task<T> ReadAsAsync<T>(HttpResponseMessage response)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}
