// <copyright file="ApiTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

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
    using Database;
    using Database.Meta;
    using Protocol.Json.Auth;
    using Services;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;
    using User = Database.Domain.User;

    public abstract class ApiTest : IDisposable
    {
        public const string SetupUrl = "allors/Test/Setup?population=full";
        public const string LoginUrl = "allors/TestAuthentication/Token";

        private readonly AllorsWebApplicationFactory factory;

        protected ApiTest()
        {
            this.factory = new AllorsWebApplicationFactory();

            this.HttpClient = this.factory.CreateClient();
            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = this.HttpClient.GetAsync(SetupUrl).Result;
            Assert.True(response.IsSuccessStatusCode);

            var database = this.factory.Services.GetRequiredService<IDatabaseService>().Database;
            this.Transaction = database.CreateTransaction();
        }

        public MetaPopulation M => this.Transaction.Database.Services.Get<MetaPopulation>();

        protected ITransaction Transaction { get; private set; }

        protected HttpClient HttpClient { get; set; }

        protected User Administrator => new Database.Domain.Users(this.Transaction).FindBy(this.M.User.UserName, "jane@example.com");

        public void Dispose()
        {
            this.Transaction.Rollback();
            this.Transaction = null;

            this.HttpClient.Dispose();
            this.HttpClient = null;

            this.factory.Dispose();
        }

        protected async Task SignIn(User user)
        {
            var args = new AuthenticationTokenRequest
            {
                l = user.UserName,
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
