// <copyright file="ProvisioningTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using Microsoft.Data.SqlClient;
    using Xunit;

    public class ProvisioningTest
    {
        // A program connects with a login of its own, not with the admin connection. Provisioning creates
        // the database of the program's connection string and the login that the connection string names,
        // and makes that login the owner of the database.
        [SqlLoginTheory]
        [InlineData("ProvisioningTest")]
        [InlineData("ProvisioningTest's")]
        [InlineData("ProvisioningTest]")]
        public void DropCreateForCreatesTheDatabaseAndItsOwner(string database)
        {
            const string login = "ProvisioningTestOwner";

            var connectionString = new SqlConnectionStringBuilder(Provisioning.ConnectionString(database))
            {
                IntegratedSecurity = false,
                UserID = login,
                Password = "provisioning",
                Pooling = false,
            }.ConnectionString;

            try
            {
                Provisioning.DropCreateFor(connectionString);

                using var connection = new SqlConnection(connectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT SUSER_SNAME(owner_sid) FROM sys.databases WHERE database_id = DB_ID()";
                var owner = (string)command.ExecuteScalar();

                Assert.Equal(login, owner);
            }
            finally
            {
                // The database returns to the admin connection, so that the login can be dropped.
                Provisioning.DropCreate(database);

                using var connection = new SqlConnection(Provisioning.ConnectionString("master"));
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = $"IF SUSER_ID('{login}') IS NOT NULL DROP LOGIN [{login}]";
                command.ExecuteNonQuery();
            }
        }
    }
}
