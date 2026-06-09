// <copyright file="DatabaseProvisioning.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters
{
    using System;

    /// <summary>
    /// Provider-agnostic facade over the SQL adapters' database provisioning. The <c>adapter</c> string
    /// (<c>npgsql</c> or <c>sqlclient</c>) selects the implementation; the admin connection is read from
    /// the matching <c>ALLORS_NPGSQL</c> / <c>ALLORS_SQLCLIENT</c> environment variable.
    /// </summary>
    public static class DatabaseProvisioning
    {
        /// <summary>Drops <paramref name="database"/> if it exists and creates it fresh.</summary>
        public static void DropCreate(string adapter, string database)
        {
            switch (Normalize(adapter))
            {
                case "NPGSQL":
                    Sql.Npgsql.Provisioning.DropCreate(database);
                    break;
                case "SQLCLIENT":
                    Sql.SqlClient.Provisioning.DropCreate(database);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(adapter), adapter, "Expected 'npgsql' or 'sqlclient'.");
            }
        }

        /// <summary>A connection string for <paramref name="database"/>, derived from the admin connection.</summary>
        public static string ConnectionString(string adapter, string database) =>
            Normalize(adapter) switch
            {
                "NPGSQL" => Sql.Npgsql.Provisioning.ConnectionString(database),
                "SQLCLIENT" => Sql.SqlClient.Provisioning.ConnectionString(database),
                _ => throw new ArgumentOutOfRangeException(nameof(adapter), adapter, "Expected 'npgsql' or 'sqlclient'."),
            };

        /// <summary>The database name contained in <paramref name="connectionString"/>.</summary>
        public static string DatabaseName(string adapter, string connectionString) =>
            Normalize(adapter) switch
            {
                "NPGSQL" => Sql.Npgsql.Provisioning.DatabaseName(connectionString),
                "SQLCLIENT" => Sql.SqlClient.Provisioning.DatabaseName(connectionString),
                _ => throw new ArgumentOutOfRangeException(nameof(adapter), adapter, "Expected 'npgsql' or 'sqlclient'."),
            };

        private static string Normalize(string adapter) => adapter?.Trim().ToUpperInvariant();
    }
}
