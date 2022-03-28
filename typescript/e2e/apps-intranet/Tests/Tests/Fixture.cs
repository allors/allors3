namespace Tests
{
    using System;
    using System.Globalization;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Allors.Database;
    using Allors.Database.Adapters.Sql;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Meta;
    using Microsoft.Extensions.Configuration;
    using NUnit.Framework;
    using Database = Allors.Database.Adapters.Sql.SqlClient.Database;

    public class Fixture : IDisposable
    {
        public const string Url = "http://localhost:5000/allors";
        public static readonly string InitUrl = $"{Url}/Test/Init";

        private static readonly MetaBuilder MetaBuilder = new MetaBuilder();

        public Fixture()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddCrossPlatform(".");
            this.Configuration = configurationBuilder.Build();

            CultureInfo.CurrentCulture = new CultureInfo("nl-BE");
            this.MetaPopulation = MetaBuilder.Build();
            var rules = Rules.Create(this.MetaPopulation);
            this.Engine = new Engine(rules);

            this.HttpClientHandler = new HttpClientHandler();
            this.HttpClient = new HttpClient(this.HttpClientHandler)
            {
                BaseAddress = new Uri(Url),
            };

            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public IConfigurationRoot Configuration { get; set; }

        public MetaPopulation MetaPopulation { get; set; }

        public Engine Engine { get; set; }

        public HttpClientHandler HttpClientHandler { get; set; }

        public HttpClient HttpClient { get; set; }

        public IDatabase Init()
        {
            var response = this.HttpClient.GetAsync(InitUrl).Result;

            Assert.True(response.IsSuccessStatusCode);

            var database = new Database(
                new DefaultDatabaseServices(this.Engine),
                new Configuration
                {
                    ConnectionString = this.Configuration["ConnectionStrings:DefaultConnection"],
                    ObjectFactory = new ObjectFactory(this.MetaPopulation, typeof(C1)),
                });

            database.Init();

            var config = new Config();
            new Setup(database, config).Apply();

            return database;
        }

        public void Dispose() => this.HttpClient.Dispose();
    }
}
