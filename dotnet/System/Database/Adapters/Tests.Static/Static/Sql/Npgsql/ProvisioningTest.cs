// <copyright file="ProvisioningTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using global::Npgsql;
    using Xunit;

    public class ProvisioningTest
    {
        // The adapter opens a connection for every transaction. Without pooling each one is a new TCP
        // connection, and the suite in parallel runs out of ephemeral ports on macOS.
        [Fact]
        public void ConnectionStringPools()
        {
            var connectionString = Provisioning.ConnectionString("ProvisioningTest");

            var pooling = new NpgsqlConnectionStringBuilder(connectionString).Pooling;

            Assert.True(pooling);
        }

        // A program connects with a role of its own, not with the admin connection. Provisioning creates
        // the database of the program's connection string and the role that the connection string names,
        // and makes that role the owner of the database.
        [Fact]
        public void DropCreateForCreatesTheDatabaseAndItsOwner()
        {
            const string database = "provisioningtest";
            const string role = "ProvisioningTestOwner";

            var connectionString = new NpgsqlConnectionStringBuilder(Provisioning.ConnectionString(database))
            {
                Username = role,
                Password = "provisioning",
                Pooling = false,
            }.ConnectionString;

            try
            {
                Provisioning.DropCreateFor(connectionString);

                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT pg_get_userbyid(datdba) FROM pg_database WHERE datname = current_database()";
                var owner = (string)command.ExecuteScalar();

                Assert.Equal(role, owner);
            }
            finally
            {
                // The database returns to the admin connection, so that the role can be dropped.
                Provisioning.DropCreate(database);

                using var connection = new NpgsqlConnection(Provisioning.AdminConnectionString());
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = $"DROP ROLE IF EXISTS \"{role}\"";
                command.ExecuteNonQuery();
            }
        }
    }
}
