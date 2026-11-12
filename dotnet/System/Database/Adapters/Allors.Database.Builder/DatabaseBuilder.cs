// <copyright file="DatabaseBuilder.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//   Defines the AllorsStrategySql type.
// </summary>

namespace Allors.Database.Adapters
{
    using System;
    using Microsoft.Extensions.Configuration;

    public class DatabaseBuilder
    {
        private readonly IDatabaseServices scope;
        private readonly IConfiguration configuration;
        private readonly ObjectFactory objectFactory;
        private readonly int? commandTimeout;

        public DatabaseBuilder(IDatabaseServices scope, IConfiguration configuration, ObjectFactory objectFactory, int? commandTimeout = null)
        {
            this.scope = scope;
            this.configuration = configuration;
            this.objectFactory = objectFactory;
            this.commandTimeout = commandTimeout;
        }

        public IDatabase Build()
        {
            var adapter = this.configuration["adapter"]?.Trim().ToUpperInvariant();
            var connectionString = this.configuration["ConnectionStrings:DefaultConnection"];

            switch (adapter)
            {
                case "MEMORY":
                    throw new NotImplementedException();

                case "SNAPSHOT":

                    return new Snapshot.Database(this.scope, new Snapshot.Configuration
                    {
                        ObjectFactory = this.objectFactory,
                    });

                case "NPGSQL":

                    return new Sql.Npgsql.Database(this.scope, new Sql.Configuration
                    {
                        ObjectFactory = this.objectFactory,
                        ConnectionString = RequireConnectionString(connectionString),
                        CommandTimeout = this.commandTimeout,
                    });

                case "SQLCLIENT":

                    return new Sql.SqlClient.Database(this.scope, new Sql.Configuration
                    {
                        ObjectFactory = this.objectFactory,
                        ConnectionString = RequireConnectionString(connectionString),
                        CommandTimeout = this.commandTimeout,
                    });

                default:
                    throw UnknownAdapter(this.configuration["adapter"]);
            }
        }

        /// <summary>
        /// Whether the adapter that <paramref name="adapter"/> names keeps its data in a database server, which
        /// a program reaches through <c>ConnectionStrings:DefaultConnection</c> and manages through
        /// <see cref="DatabaseProvisioning"/>. The <c>snapshot</c> adapter keeps its data in memory and needs
        /// neither.
        /// </summary>
        public static bool HasDatabaseServer(string adapter)
        {
            switch (adapter?.Trim().ToUpperInvariant())
            {
                case "SNAPSHOT":
                    return false;

                case "NPGSQL":
                case "SQLCLIENT":
                    return true;

                default:
                    throw UnknownAdapter(adapter);
            }
        }

        internal static ArgumentOutOfRangeException UnknownAdapter(string adapter) =>
            string.IsNullOrWhiteSpace(adapter)
                ? new ArgumentOutOfRangeException(nameof(adapter), "The setting 'adapter' has no value. Set it to 'snapshot', 'sqlclient' or 'npgsql'.")
                : new ArgumentOutOfRangeException(nameof(adapter), $"The setting 'adapter' is '{adapter}'. Set it to 'snapshot', 'sqlclient' or 'npgsql'.");

        internal static string RequireConnectionString(string connectionString) =>
            string.IsNullOrWhiteSpace(connectionString)
                ? throw new InvalidOperationException("The setting 'ConnectionStrings:DefaultConnection' has no value. Set it to the connection string of the database.")
                : connectionString;
    }
}
