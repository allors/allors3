namespace Integrations
{
    using System;
    using System.Diagnostics;
    using Allors.Database;
    using Allors.Database.Domain;

  public class IntegrationFixture
    {
        public static void Setup(IDatabase database)
        {
            using (var transaction = database.CreateTransaction())
            {
                try
                {
                    var config = new Config { SetupSecurity = false };
                    new Setup(transaction.Database, config).Apply();
                    transaction.Commit();

                    var administrator = new PersonBuilder(transaction)
                        .WithFirstName("Jane")
                        .WithLastName("Doe")
                        .WithUserName("administrator")
                        .Build();

                    new UserGroups(transaction).Administrators.AddMember(administrator);

                    var administrators = new UserGroups(transaction).Administrators;
                    administrators.AddMember(administrator);

                    transaction.Derive(true);
                    transaction.Commit();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.StackTrace);
                    throw;
                }
            }
        }
    }
}
