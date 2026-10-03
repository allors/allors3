// <copyright file="ApiTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using Database;
    using Database.Adapters;
    using Database.Configuration;
    using Database.Configuration.Derivations.Default;
    using Database.Domain;
    using Database.Meta;
    using Microsoft.Extensions.Configuration;
    using Xunit;
    using ObjectFactory = Database.ObjectFactory;
    using User = Database.Domain.User;

    // The base of the tests against the published Entra test server, which signs in against its fake
    // Entra: it resets the server's database, signs accounts in the way a browser does, through the
    // sign-in endpoint and the fake's pages, and gets access tokens the way a client does, from the
    // fake's token endpoint.
    public abstract class ApiTest : IDisposable
    {
        public const string Origin = "http://localhost:5000";
        public const string Url = Origin + "/allors/";
        public const string SetupUrl = "Test/Setup";

        protected ApiTest()
        {
            var configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.AddAllorsConfiguration("entra", "commands");

            this.Configuration = configurationBuilder.Build();

            var metaPopulation = new MetaBuilder().Build();
            var rules = Rules.Create(metaPopulation);
            var engine = new Engine(rules);
            var database = new DatabaseBuilder(
                new DefaultDatabaseServices(engine),
                this.Configuration,
                new ObjectFactory(metaPopulation, typeof(User))).Build();

            this.HttpClientHandler = new HttpClientHandler { UseCookies = false };
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

        public MetaPopulation M => this.Transaction.Database.Services.Get<MetaPopulation>();

        protected IConfiguration Configuration { get; }

        // The tenant and the registration the server is configured for; the fake takes them from the
        // same configuration.
        protected Guid TenantId => Guid.Parse(this.Configuration["Entra:TenantId"]);

        protected string ClientId => this.Configuration["Entra:ClientId"];

        protected ITransaction Transaction { get; private set; }

        protected HttpClient HttpClient { get; set; }

        protected HttpClientHandler HttpClientHandler { get; set; }

        public void Dispose()
        {
            this.Transaction.Rollback();
            this.Transaction = null;

            this.HttpClient.Dispose();
            this.HttpClient = null;
        }

        // The user an account's Entra identity has in the server's database, or null. Read fresh: the
        // test's transaction sees the database as it was when it began, not what the server committed
        // since, until it is rolled back.
        protected User FindUser(FakeEntraAccount account)
        {
            this.Transaction.Rollback();
            return new Users(this.Transaction).FindByEntraIdentity(this.TenantId, account.ObjectId);
        }

        protected User[] AllUsers()
        {
            this.Transaction.Rollback();
            return new Users(this.Transaction).Extent().ToArray();
        }

        // A browser: it keeps cookies and shows every redirect, so a test can look at each step.
        protected static (HttpClient Client, CookieContainer Jar) NewBrowser()
        {
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer(),
                AllowAutoRedirect = false,
            };
            return (new HttpClient(handler) { BaseAddress = new Uri(Origin + "/") }, handler.CookieContainer);
        }

        // Signs the browser in as the account the way a person does: the sign-in endpoint sends it to
        // Entra, the fake's page lets it pick the account, and the code comes back through the callback
        // to the sign-in endpoint, which sends it on to the returnUrl. Returns the last response, which
        // is the response of the returnUrl when the account is admitted.
        protected static async Task<HttpResponseMessage> SignInAsync(HttpClient browser, FakeEntraAccount account, string returnUrl = "/allors/UserInfo")
        {
            var response = await browser.GetAsync("entra/sign-in?returnUrl=" + Uri.EscapeDataString(returnUrl));
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/oauth2/v2.0/authorize", response.Headers.Location?.ToString(), StringComparison.Ordinal);

            response = await browser.GetAsync(response.Headers.Location + "&account=" + account.Id);
            return await FollowRedirectsAsync(browser, response);
        }

        protected static async Task<HttpResponseMessage> FollowRedirectsAsync(HttpClient browser, HttpResponseMessage response, int maximum = 6)
        {
            for (var i = 0; i < maximum && (response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.Found || response.StatusCode == HttpStatusCode.SeeOther) && response.Headers.Location != null; i++)
            {
                response = await browser.GetAsync(response.Headers.Location);
            }

            return response;
        }

        // The token a client gets for a person with the password grant, from the fake's token endpoint;
        // a test may hand the fake its knobs for a token Microsoft would never issue.
        protected Task<string> PersonTokenAsync(FakeEntraAccount account, params (string Name, string Value)[] knobs) =>
            this.PersonTokenAsync(this.TenantId, account, knobs);

        // The same from another tenant of the fake, whose issuer is not the application's.
        protected Task<string> PersonTokenAsync(Guid tenant, FakeEntraAccount account, params (string Name, string Value)[] knobs) =>
            this.TokenAsync(tenant, new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = this.ClientId,
                ["username"] = account.UserName,
                ["password"] = account.Password,
                ["scope"] = $"openid profile email api://{this.ClientId}/access_as_user",
            }, knobs);

        // The token a program gets with its own credentials.
        protected Task<string> ProgramTokenAsync(params (string Name, string Value)[] knobs) =>
            this.TokenAsync(this.TenantId, new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = FakeEntraAccounts.Program.ClientId,
                ["client_secret"] = FakeEntraAccounts.Program.ClientSecret,
                ["scope"] = $"api://{this.ClientId}/.default",
            }, knobs);

        protected static HttpRequestMessage Bearer(HttpMethod method, string path, string token, HttpContent content = null)
        {
            var request = new HttpRequestMessage(method, new Uri(Origin + path)) { Content = content };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return request;
        }

        protected static StringContent EmptyPull() => new("{}", Encoding.UTF8, "application/json");

        protected static async Task<JsonElement> JsonAsync(HttpResponseMessage response) =>
            JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        protected static string Cookie(CookieContainer jar, string name) => jar.GetCookies(new Uri(Origin + "/"))[name]?.Value;

        private async Task<string> TokenAsync(Guid tenant, Dictionary<string, string> form, (string Name, string Value)[] knobs)
        {
            foreach (var (name, value) in knobs)
            {
                form[name] = value;
            }

            using var client = new HttpClient();
            var response = await client.PostAsync($"{Origin}{FakeEntra.PathPrefix}/{tenant}/oauth2/v2.0/token", new FormUrlEncodedContent(form));
            var json = await JsonAsync(response);
            Assert.True(response.IsSuccessStatusCode, $"The fake's token endpoint answered {(int)response.StatusCode}: {json}");
            return json.GetProperty("access_token").GetString();
        }
    }
}
