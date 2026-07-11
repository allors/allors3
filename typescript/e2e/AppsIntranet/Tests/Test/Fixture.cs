namespace Tests
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Xml;
    using Allors.Database;
    using Allors.Database.Adapters;
    using Allors.Database.Configuration;
    using NUnit.Framework;
    using Person = Allors.Database.Domain.Person;

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

        public string[] Logins = new[] {"jane@example.com"};

        public IDatabase Init()
        {
            var database = new DatabaseBuilder(
                   new DefaultDatabaseServices(Config.Engine),
                   Config.Configuration,
                   new ObjectFactory(Config.MetaPopulation, typeof(Person))).Build();

            using var stream = Config.PopulationFileInfo.OpenRead();
            using var reader = XmlReader.Create(stream);
            database.Load(reader);

            var response = this.HttpClient.GetAsync(RestartUrl).Result;
            ClassicAssert.True(response.IsSuccessStatusCode);

            return database;
        }

        public void Dispose() => this.HttpClient.Dispose();
    }
}
