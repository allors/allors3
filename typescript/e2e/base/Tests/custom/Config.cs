namespace Tests.E2E
{
    using System.Globalization;
    using System.IO;
    using Allors.Database.Configuration;
    using Allors.Database.Configuration.Derivations.Default;
    using Allors.Database.Domain;
    using Allors.Database.Domain.Tests;
    using Allors.Database.Meta;
    using Microsoft.Extensions.Configuration;
    using Person = Allors.Database.Domain.Person;

    public static class Config
    {
        private static readonly MetaBuilder MetaBuilder = new MetaBuilder();

        static Config()
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddCrossPlatform(".");
            Configuration = configurationBuilder.Build();

            CultureInfo.CurrentCulture = new CultureInfo("nl-BE");
            MetaPopulation = MetaBuilder.Build();
            var rules = Rules.Create(MetaPopulation);
            Engine = new Engine(rules);

            var domainPrint = typeof(Person).Assembly.Fingerprint();
            var testPrint = typeof(Test).Assembly.Fingerprint();
            PopulationFileInfo = new FileInfo($"population.{domainPrint}.{testPrint}.xml");
        }

        public static IConfigurationRoot Configuration { get; }

        public static MetaPopulation MetaPopulation { get; }

        public static Engine Engine { get; }

        public static FileInfo PopulationFileInfo { get; }
    }
}
