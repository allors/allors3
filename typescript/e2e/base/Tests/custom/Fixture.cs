namespace Tests.E2E
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Xml;
    using Allors.Database;
    using Allors.Database.Adapters.Sql;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using NUnit.Framework;
    using Database = Allors.Database.Adapters.Sql.SqlClient.Database;

    public class Fixture : IDisposable
    {
        public const string Url = "http://localhost:5000/allors";
        public static readonly string RestartUrl = $"{Url}/Test/Restart";

        public Fixture()
        {
            this.HttpClientHandler = new HttpClientHandler();
            this.HttpClient = new HttpClient(this.HttpClientHandler)
            {
                BaseAddress = new Uri(Url),
            };

            this.HttpClient.DefaultRequestHeaders.Accept.Clear();
            this.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public HttpClientHandler HttpClientHandler { get; set; }

        public HttpClient HttpClient { get; set; }

        public string[] Logins { get; } = new[] { "jane@example.com" };

        public IDatabase Init()
        {
            var database = new Database(
                new DefaultDatabaseServices(Config.Engine),
                new Configuration
                {
                    ConnectionString = Config.Configuration["ConnectionStrings:DefaultConnection"],
                    ObjectFactory = new ObjectFactory(Config.MetaPopulation, typeof(Person)),
                });

            using var stream = Config.PopulationFileInfo.OpenRead();
            using var reader = XmlReader.Create(stream);
            database.Load(reader);

            var config = new Allors.Database.Domain.Config();
            new Setup(database, config).Apply();

            var response = this.HttpClient.GetAsync(RestartUrl).Result;
            Assert.True(response.IsSuccessStatusCode);

            return database;
        }

        public void Dispose() => this.HttpClient.Dispose();
    }
}
