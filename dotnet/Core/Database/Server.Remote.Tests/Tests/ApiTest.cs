// <copyright file="ApiTest.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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
    using Database.Adapters.Sql;
    using Database.Domain;
    using Database.Configuration;
    using Database.Configuration.Derivations.Default;
    using Database.Meta;
    using Microsoft.Extensions.Configuration;
    using Protocol.Json.Auth;
    using Xunit;
    using C1 = Database.Domain.C1;
    using Database = Database.Adapters.Sql.SqlClient.Database;
    using ObjectFactory = Database.ObjectFactory;
    using Path = System.IO.Path;
    using User = Database.Domain.User;

    public abstract class ApiTest : IDisposable
    {
        public const string Url = "http://localhost:5000/allors/";
        public const string SetupUrl = "Test/Setup?population=full";
        public const string LoginUrl = "TestAuthentication/Token";

        protected ApiTest()
        {
            var configurationBuilder = new ConfigurationBuilder();

            const string root = "/config/core";
            configurationBuilder.AddCrossPlatform(".");
            configurationBuilder.AddCrossPlatform(root);
            configurationBuilder.AddCrossPlatform(Path.Combine(root, "commands"));
            configurationBuilder.AddEnvironmentVariables();

            var configuration = configurationBuilder.Build();

            var metaPopulation = new MetaBuilder().Build();
            var rules = Rules.Create(metaPopulation);
            var engine = new Engine(rules);
            var database = new Database(
                new DefaultDatabaseServices(engine),
                new Configuration
                {
                    ConnectionString = configuration["ConnectionStrings:DefaultConnection"],
                    ObjectFactory = new ObjectFactory(metaPopulation, typeof(C1)),
                });

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

        protected User Administrator => new Users(this.Transaction).FindBy(this.M.User.UserName, "jane@example.com");

        public void Dispose()
        {
            this.Transaction.Rollback();
            this.Transaction = null;

            this.HttpClient.Dispose();
            this.HttpClient = null;
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
