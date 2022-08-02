namespace Tests.E2E
{
    using System.Xml;
    using Allors.Database;
    using Allors.Database.Adapters.Sql;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using NUnit.Framework;
    using Database = Allors.Database.Adapters.Sql.SqlClient.Database;

    [SetUpFixture]
    public class SetUpFixture
    {

        [OneTimeSetUp]
        public void Init()
        {
            Config.PopulationFileInfo.Refresh();

            if (!Config.PopulationFileInfo.Exists)
            {
                var database = new Database(
                    new DefaultDatabaseServices(Config.Engine),
                    new Configuration
                    {
                        ConnectionString = Config.Configuration["ConnectionStrings:DefaultConnection"],
                        ObjectFactory = new ObjectFactory(Config.MetaPopulation, typeof(Person)),
                    });

                database.Init();

                var config = new Allors.Database.Domain.Config();
                new Setup(database, config).Apply();

                using var stream = Config.PopulationFileInfo.Create();
                using var writer = XmlWriter.Create(stream);
                database.Save(writer);
            }
        }
    }
}
