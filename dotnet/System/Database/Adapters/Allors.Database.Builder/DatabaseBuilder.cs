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
    using System.Data;
    using Microsoft.Extensions.Configuration;
    using Memory;

    public class DatabaseBuilder
    {
        private readonly IDatabaseServices scope;
        private readonly IConfiguration configuration;
        private readonly ObjectFactory objectFactory;
        private readonly IsolationLevel? isolationLevel;
        private readonly int? commandTimeout;

        public DatabaseBuilder(IDatabaseServices scope, IConfiguration configuration, ObjectFactory objectFactory, IsolationLevel? isolationLevel = null, int? commandTimeout = null)
        {
            this.scope = scope;
            this.configuration = configuration;
            this.objectFactory = objectFactory;
            this.isolationLevel = isolationLevel;
            this.commandTimeout = commandTimeout;
        }

        public IDatabase Build()
        {
            var adapter = this.configuration["adapter"]?.Trim().ToUpperInvariant();
            var connectionString = this.configuration["ConnectionStrings:DefaultConnection"];

            switch (adapter)
            {
                case "MEMORY":
                    return new Memory.Database(this.scope, new Memory.Configuration
                    {
                        ObjectFactory = this.objectFactory,
                    });

                case "NPGSQL":

                    return new Sql.Npgsql.Database(this.scope, new Sql.Configuration
                    {
                        ObjectFactory = this.objectFactory,
                        ConnectionString = connectionString,
                        IsolationLevel = this.isolationLevel,
                        CommandTimeout = this.commandTimeout,
                    });

                case "SQLCLIENT":

                    return new Sql.SqlClient.Database(this.scope, new Sql.Configuration
                    {
                        ObjectFactory = this.objectFactory,
                        ConnectionString = connectionString,
                        IsolationLevel = this.isolationLevel,
                        CommandTimeout = this.commandTimeout,
                    });

                default:
                    throw new ArgumentOutOfRangeException(adapter);
            }
        }
    }
}
