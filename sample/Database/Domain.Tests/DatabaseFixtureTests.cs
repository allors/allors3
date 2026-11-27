// <copyright file="DatabaseFixtureTests.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Tests
{
    using System;
    using Xunit;

    public class DatabaseFixtureTests
    {
        [Theory]
        [InlineData("sqlclient", "ALLORS_SQLCLIENT", "ALLORS_TEST_SQLCLIENT_CONNECTION")]
        [InlineData("npgsql", "ALLORS_NPGSQL", "ALLORS_TEST_NPGSQL_CONNECTION")]
        public void ServerAdapterRequiresItsAdminConnection(string adapter, string adminVariable, string legacyVariable)
        {
            const string adapterVariable = "ALLORS_TEST_ADAPTER";
            var previousAdapter = Environment.GetEnvironmentVariable(adapterVariable);
            var previousAdmin = Environment.GetEnvironmentVariable(adminVariable);
            var previousLegacy = Environment.GetEnvironmentVariable(legacyVariable);
            try
            {
                Environment.SetEnvironmentVariable(adapterVariable, adapter);
                Environment.SetEnvironmentVariable(adminVariable, null);
                Environment.SetEnvironmentVariable(legacyVariable, "invalid connection string");

                // Dispose a fixture created by mistake, so that its lock cannot keep the domain tests waiting.
                var error = Assert.Throws<InvalidOperationException>(() => new DatabaseFixture().Dispose());
                Assert.Contains(adminVariable, error.Message);
            }
            finally
            {
                Environment.SetEnvironmentVariable(adapterVariable, previousAdapter);
                Environment.SetEnvironmentVariable(adminVariable, previousAdmin);
                Environment.SetEnvironmentVariable(legacyVariable, previousLegacy);
            }
        }
    }
}
