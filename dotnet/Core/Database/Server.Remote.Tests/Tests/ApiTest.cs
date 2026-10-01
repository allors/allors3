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
    using Database.Adapters;
    using Database.Adapters.Sql;
    using Database.Domain;
    using Database.Configuration;
    using Database.Configuration.Derivations.Default;
    using Database.Meta;
    using Microsoft.Extensions.Configuration;
    using Xunit;
    using C1 = Database.Domain.C1;
    using ObjectFactory = Database.ObjectFactory;
    using User = Database.Domain.User;

    public abstract class ApiTest : IDisposable
    {
        public const string Url = "http://localhost:5000/allors/";
        public const string SetupUrl = "Test/Setup?population=full";

        // Test-only credential recognised by this (Core) test-harness server; see TestUserAuthenticationHandler.
        public const string TestUserHeaderName = "X-Allors-TestUser";

        protected ApiTest()
        {
            var configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.AddAllorsConfiguration("core", "commands");

            var configuration = configurationBuilder.Build();

            var metaPopulation = new MetaBuilder().Build();
            var rules = Rules.Create(metaPopulation);
            var engine = new Engine(rules);
            var database = new DatabaseBuilder(
                new DefaultDatabaseServices(engine),
                configuration,
                new ObjectFactory(metaPopulation, typeof(C1))).Build();

            this.HttpClientHandler = new HttpClientHandler();
            this.HttpClient = new HttpClient(this.HttpClientHandler)
            {
                BaseAddress = new Uri(Url),
            };

            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = this.HttpClient.GetAsync(SetupUrl).Result;

            Assert.True(response.IsSuccessStatusCode);

            this.Transaction = database.CreateTransaction();
        }

        public MetaPopulation M => this.Transaction.Database.Services.Get<Allors.Database.Meta.MetaPopulation>();

        public IConfigurationRoot Configuration { get; set; }

        protected ITransaction Transaction { get; private set; }

        protected HttpClient HttpClient { get; set; }

        protected HttpClientHandler HttpClientHandler { get; set; }

        protected User Administrator => new Users(this.Transaction).FindBy(this.M.User.UniqueId, Users.JaneId);

        public void Dispose()
        {
            this.Transaction.Rollback();
            this.Transaction = null;

            this.HttpClient.Dispose();
            this.HttpClient = null;
        }

        protected Task SignIn(User user)
        {
            // Authenticate with the test-only X-Allors-TestUser header, which carries the user's
            // UniqueId; the harness server resolves it to the same Allors user.
            this.HttpClient.DefaultRequestHeaders.Remove(TestUserHeaderName);
            this.HttpClient.DefaultRequestHeaders.Add(TestUserHeaderName, user.UniqueId.ToString());
            return Task.CompletedTask;
        }

        protected void SignOut() => this.HttpClient.DefaultRequestHeaders.Remove(TestUserHeaderName);

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
