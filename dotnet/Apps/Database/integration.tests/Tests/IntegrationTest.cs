namespace Integrations
{
    using System;
    using System.Globalization;
    using Allors.Database;
    using Allors.Database.Adapters.Memory;
    using Allors.Database.Meta;
    using ObjectFactory = Allors.Database.ObjectFactory;

    public class IntegrationTest : IDisposable
    {
        public ITransaction Transaction { get; private set; }

        //protected ObjectFactory ObjectFactory => new ObjectFactory(MetaPopulation.Instance, typeof(User));

        public IntegrationTest()
        {
            this.Init(true);
        }

        public void Dispose()
        {
            this.Transaction.Rollback();
            this.Transaction = null;
        }

        protected void Init(bool setup)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");

            //var services = new ServiceCollection();
            //services.AddAllors();
            //var serviceProvider = services.BuildServiceProvider();

            //var database = new Database(serviceProvider, new Configuration { ObjectFactory = this.ObjectFactory });
            //database.Init();

            //if (setup)
            //{
            //    IntegrationFixture.Setup(database);
            //}

            //this.Transaction = database.CreateTransaction();
        }
    }
}
