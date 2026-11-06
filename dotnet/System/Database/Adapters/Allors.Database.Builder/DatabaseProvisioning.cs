// <copyright file="DatabaseProvisioning.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters
{
    /// <summary>
    /// Database management for the connection that a program is configured with. The <c>adapter</c>
    /// setting selects the SQL adapter whose <c>Provisioning</c> does the work on the admin connection:
    /// <c>ALLORS_SQLCLIENT</c> for <c>sqlclient</c>, <c>ALLORS_NPGSQL</c> for <c>npgsql</c>.
    /// </summary>
    public static class DatabaseProvisioning
    {
        /// <summary>The database name contained in <paramref name="connectionString"/>.</summary>
        public static string DatabaseName(string adapter, string connectionString)
        {
            connectionString = DatabaseBuilder.RequireConnectionString(connectionString);

            switch (adapter?.Trim().ToUpperInvariant())
            {
                case "NPGSQL":
                    return Sql.Npgsql.Provisioning.DatabaseName(connectionString);

                case "SQLCLIENT":
                    return Sql.SqlClient.Provisioning.DatabaseName(connectionString);

                default:
                    throw DatabaseBuilder.UnknownAdapter(adapter);
            }
        }

        /// <summary>
        /// Drops and creates the database of <paramref name="connectionString"/> and makes the login that
        /// the connection string names the owner of the database, creating the login when the server does
        /// not know it.
        /// </summary>
        public static void DropCreate(string adapter, string connectionString)
        {
            connectionString = DatabaseBuilder.RequireConnectionString(connectionString);

            switch (adapter?.Trim().ToUpperInvariant())
            {
                case "NPGSQL":
                    Sql.Npgsql.Provisioning.DropCreateFor(connectionString);
                    break;

                case "SQLCLIENT":
                    Sql.SqlClient.Provisioning.DropCreateFor(connectionString);
                    break;

                default:
                    throw DatabaseBuilder.UnknownAdapter(adapter);
            }
        }
    }
}
