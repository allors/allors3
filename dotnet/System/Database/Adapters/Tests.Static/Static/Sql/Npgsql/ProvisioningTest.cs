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
    }
}
