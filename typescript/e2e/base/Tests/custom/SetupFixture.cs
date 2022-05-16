namespace Tests.E2E
{
    using System.Xml;
    using Allors.Database;
    using Allors.Database.Adapters.Sql;
    using Allors.Database.Configuration;
    using Allors.Database.Domain;
    using NUnit.Framework;
    using Database = Allors.Database.Adapters.Sql.SqlClient.Database;
    using Person = Allors.Database.Domain.Person;

    [SetUpFixture]
    public class SetUpFixture
    {

        [OneTimeSetUp]
        public void Init()
        {
            E2E.Config.PopulationFileInfo.Refresh();

            if (!E2E.Config.PopulationFileInfo.Exists)
            {
                var database = new Database(
                    new DefaultDatabaseServices(E2E.Config.Engine),
                    new Configuration
                    {
                        ConnectionString = E2E.Config.Configuration["ConnectionStrings:DefaultConnection"],
                        ObjectFactory = new ObjectFactory(E2E.Config.MetaPopulation, typeof(Person)),
                    });

                database.Init();

                var config = new Allors.Database.Domain.Config();
                new Setup(database, config).Apply();

                using var stream = E2E.Config.PopulationFileInfo.Create();
                using var writer = XmlWriter.Create(stream);
                database.Save(writer);
            }
        }
    }
}
