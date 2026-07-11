namespace Tests
{
    using System.Xml;
    using Allors.Database;
    using Allors.Database.Adapters;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using NUnit.Framework;
    using Person = Allors.Database.Domain.Person;

    [SetUpFixture]
    public class SetUpFixture
    {

        [OneTimeSetUp]
        public void Init()
        {
            Config.PopulationFileInfo.Refresh();

            if (!Config.PopulationFileInfo.Exists)
            {
                var database = new DatabaseBuilder(
                    new DefaultDatabaseServices(Config.Engine),
                    Config.Configuration,
                    new ObjectFactory(Config.MetaPopulation, typeof(Person))).Build();

                database.Init();

                var config = new Allors.Database.Domain.Config();
                new Setup(database, config).Apply();

                using var transaction = database.CreateTransaction();
                new IntranetPopulation(transaction, null, Config.MetaPopulation).Execute();
                transaction.Commit();

                using var stream = Config.PopulationFileInfo.Create();
                using var writer = XmlWriter.Create(stream);
                database.Save(writer);
            }
        }
    }
}
