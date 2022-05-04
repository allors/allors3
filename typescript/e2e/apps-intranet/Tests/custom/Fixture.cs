namespace Tests
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Xml;
    using Allors.Database;
    using Allors.Database.Adapters.Sql;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;
    using Allors.Database.Domain.Tests;
    using Allors.Database.Meta;
    using Microsoft.Extensions.Configuration;
    using NUnit.Framework;
    using Database = Allors.Database.Adapters.Sql.SqlClient.Database;
    using Person = Allors.Database.Domain.Person;

    public class Fixture : IDisposable
    {
        public const string Url = "http://localhost:5000/allors";
        public static readonly string RestartUrl = $"{Url}/Test/Restart";

        private static readonly MetaBuilder MetaBuilder = new MetaBuilder();

        private static readonly FileInfo populationFileInfo;
        private static string population;

        static Fixture()
        {
            var domainPrint = typeof(Person).Assembly.Fingerprint();
            var testPrint = typeof(Test).Assembly.Fingerprint();
            var testPopulationPrint = typeof(Marker).Assembly.Fingerprint();
            populationFileInfo = new FileInfo($"population.{domainPrint}.{testPrint}.{testPopulationPrint}.xml");
        }

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
            var response = this.HttpClient.GetAsync(RestartUrl).Result;
            
            Assert.True(response.IsSuccessStatusCode);

            var database = new Database(
                   new DefaultDatabaseServices(this.Engine),
                   new Configuration
                   {
                       ConnectionString = this.Configuration["ConnectionStrings:DefaultConnection"],
                       ObjectFactory = new ObjectFactory(this.MetaPopulation, typeof(Person)),
                   });

            // Population
            if (population == null && populationFileInfo.Exists)
            {
                population = File.ReadAllText(populationFileInfo.FullName);
            }

            if (population != null)
            {
                using var stringReader = new StringReader(population);
                using var reader = XmlReader.Create(stringReader);
                database.Load(reader);
            }
            else
            {
                database.Init();

                var config = new Config();
                new Setup(database, config).Apply();

                using var transaction = database.CreateTransaction();
                new IntranetPopulation(transaction, null, this.MetaPopulation).Execute();
                transaction.Commit();

                using var stringWriter = new StringWriter();
                using (var writer = XmlWriter.Create(stringWriter))
                {
                    database.Save(writer);
                }

                population = stringWriter.ToString();
                File.WriteAllText(populationFileInfo.FullName, population);
            }

            return database;
        }

        public void Dispose() => this.HttpClient.Dispose();
    }
}
