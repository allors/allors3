// <copyright file="ApiTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Server.Tests
{
    using System;
    using System.Net.Http;
    using System.Net.Http.Headers;
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

    // The base of the tests against the published Identity test server: it resets and populates the
    // server's database, and signs users in the way the Identity plug-in does, with a cookie.
    public abstract class ApiTest : IDisposable
    {
        public const string Url = "http://localhost:5000/allors/";
        public const string SetupUrl = "Test/Setup";

        protected ApiTest()
        {
            var configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.AddAllorsConfiguration("identity", "commands");

            var configuration = configurationBuilder.Build();

            var metaPopulation = new MetaBuilder().Build();
            var rules = Rules.Create(metaPopulation);
            var engine = new Engine(rules);
            var database = new DatabaseBuilder(
                new DefaultDatabaseServices(engine),
                configuration,
                new ObjectFactory(metaPopulation, typeof(User))).Build();

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

        public MetaPopulation M => this.Transaction.Database.Services.Get<MetaPopulation>();

        protected ITransaction Transaction { get; private set; }

        protected HttpClient HttpClient { get; set; }

        protected HttpClientHandler HttpClientHandler { get; set; }

        // Identity signs users in by user name, so this tree knows its administrator by it.
        protected User Administrator => new Users(this.Transaction).FindBy(this.M.User.UserName, "jane@example.com");

        public void Dispose()
        {
            this.Transaction.Rollback();
            this.Transaction = null;

            this.HttpClient.Dispose();
            this.HttpClient = null;
        }

        // Logs in through the real Identity Razor login page (GET to obtain the antiforgery token,
        // then POST the form) and returns a cookie-bearing client — no bearer token involved.
        protected async Task<HttpClient> SignInWithCookieAsync(string userName, string password)
        {
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new System.Net.CookieContainer(),
                AllowAutoRedirect = false,
            };
            var client = new HttpClient(handler) { BaseAddress = new Uri("http://localhost:5000/") };

            await LoginWithCookieAsync(client, userName, password);
            return client;
        }

        // Performs the Identity Razor form login on a caller-supplied cookie-bearing client (base
        // address = site root), so a test can visit pages anonymously first and inspect its own
        // CookieContainer across the sign-in transition.
        protected static async Task LoginWithCookieAsync(HttpClient client, string userName, string password)
        {
            var loginUri = new Uri("Identity/Account/Login", UriKind.Relative);
            var getResponse = await client.GetAsync(loginUri);
            var getBody = await getResponse.Content.ReadAsStringAsync();
            var token = System.Text.RegularExpressions.Regex.Match(
                getBody,
                "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;

            var form = new System.Collections.Generic.Dictionary<string, string>
            {
                ["Input.UserName"] = userName,
                ["Input.Password"] = password,
                ["Input.RememberMe"] = "false",
                ["__RequestVerificationToken"] = token,
            };

            await client.PostAsync(loginUri, new FormUrlEncodedContent(form));
        }

        // True when the Identity form login for these credentials yields an authenticated session,
        // verified by reaching the authenticated UserInfo endpoint with the resulting cookie.
        protected async Task<bool> CookieLoginSucceedsAsync(string userName, string password)
        {
            var client = await this.SignInWithCookieAsync(userName, password);
            var response = await client.GetAsync(new Uri("allors/UserInfo", UriKind.Relative));
            return response.IsSuccessStatusCode;
        }
    }
}
