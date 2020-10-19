// <copyright file="Profile.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using Microsoft.Data.SqlClient;
    using System.Text;
    using Adapters;
    using Caching;
    using Domain;
    using Meta;
    using ObjectFactory = Allors.ObjectFactory;

    public class Profile : Adapters.Profile
    {
        private readonly string database;
        private readonly IConnectionFactory connectionFactory;
        private readonly ICacheFactory cacheFactory;

        private readonly Prefetchers prefetchers = new Prefetchers();

        public Profile(string database, IConnectionFactory connectionFactory = null, ICacheFactory cacheFactory = null)
        {
            this.database = database.ToLowerInvariant();
            this.connectionFactory = connectionFactory;
            this.cacheFactory = cacheFactory;
        }

        public override Action[] Markers
        {
            get
            {
                var markers = new List<Action>
                {
                    () => { },
                    () => this.Session.Commit(),
                };

                if (Settings.ExtraMarkers)
                {
                    markers.Add(
                        () =>
                        {
                            foreach (var @class in this.Session.Database.MetaPopulation.DatabaseClasses)
                            {
                                var prefetchPolicy = this.prefetchers[@class];
                                this.Session.Prefetch(prefetchPolicy, this.Session.Extent(@class).ToArray());
                            }
                        });
                }

                return markers.ToArray();
            }
        }

        protected string ConnectionString => $@"server=(localdb)\MSSQLLocalDB;database={this.database};Integrated Security=true";

        public override IDatabase CreateDatabase()
        {
            var metaPopulation = new MetaBuilder().Build();
            var scope = new DatabaseState();
            return new Database(scope, new Configuration
            {
                ObjectFactory = new ObjectFactory(metaPopulation, typeof(C1)),
                ConnectionString = this.ConnectionString,
                ConnectionFactory = this.connectionFactory,
                CacheFactory = this.cacheFactory,
            });
        }

        protected bool Match(ColumnTypes columnType, string dataType)
        {
            dataType = dataType.Trim().ToLowerInvariant();

            switch (columnType)
            {
                case ColumnTypes.ObjectId:
                    return dataType.Equals("int");
                case ColumnTypes.TypeId:
                    return dataType.Equals("uniqueidentifier");
                case ColumnTypes.CacheId:
                    return dataType.Equals("int");
                case ColumnTypes.Binary:
                    return dataType.Equals("varbinary");
                case ColumnTypes.Boolean:
                    return dataType.Equals("bit");
                case ColumnTypes.Decimal:
                    return dataType.Equals("decimal");
                case ColumnTypes.Float:
                    return dataType.Equals("float");
                case ColumnTypes.Integer:
                    return dataType.Equals("int");
                case ColumnTypes.String:
                    return dataType.Equals("nvarchar");
                case ColumnTypes.Unique:
                    return dataType.Equals("uniqueidentifier");
                default:
                    throw new Exception("Unsupported columntype " + columnType);
            }
        }

        public void DropProcedure(string procedure)
        {
            using (var connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    var sql = new StringBuilder();
                    sql.Append("DROP PROCEDURE " + procedure);

                    command.CommandText = sql.ToString();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void DropTable(string schema, string table)
        {
            using (var connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();
                var cmdText = @"
IF EXISTS (SELECT * FROM information_schema.tables WHERE table_name = @tableName AND table_schema = @tableSchema)
BEGIN
DROP TABLE " + schema + "." + table + @"
END
";
                using (var command = new SqlCommand(cmdText, connection))
                {
                    command.Parameters.Add("@tableName", SqlDbType.NVarChar).Value = table;
                    command.Parameters.Add("@tableSchema", SqlDbType.NVarChar).Value = schema;
                    command.ExecuteNonQuery();
                }
            }
        }

        public bool ExistTable(string schema, string table)
        {
            using (var connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    var cmdText = @"
SELECT COUNT(*) 
FROM information_schema.tables 
WHERE table_name = @tableName AND table_schema = @tableSchema";

                    command.CommandText = cmdText;

                    command.Parameters.Add("@tableName", SqlDbType.NVarChar).Value = table;
                    command.Parameters.Add("@tableSchema", SqlDbType.NVarChar).Value = schema;
                    var count = (int)command.ExecuteScalar();

                    return count != 0;
                }
            }
        }

        public int ColumnCount(string schema, string table)
        {
            using (var connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    var cmdText = @"
SELECT count(table_name)
FROM information_schema.columns
WHERE table_name = @tableName AND table_schema = @tableSchema";

                    command.CommandText = cmdText;
                    command.Parameters.Add("@tableName", SqlDbType.NVarChar).Value = table;
                    command.Parameters.Add("@tableSchema", SqlDbType.NVarChar).Value = schema;
                    var count = (int)command.ExecuteScalar();

                    return count;
                }
            }
        }

        public bool ExistColumn(string schema, string table, string column, ColumnTypes columnType)
        {
            using (var connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    var cmdText = @"
SELECT data_type
FROM information_schema.columns
WHERE table_name = @tableName AND table_schema = @tableSchema AND
column_name=@columnName";

                    command.CommandText = cmdText;

                    command.Parameters.Add("@tableName", SqlDbType.NVarChar).Value = table;
                    command.Parameters.Add("@tableSchema", SqlDbType.NVarChar).Value = schema;
                    command.Parameters.Add("@columnName", SqlDbType.NVarChar).Value = column;

                    var dataType = (string)command.ExecuteScalar();
                    if (string.IsNullOrWhiteSpace(dataType))
                    {
                        return false;
                    }

                    return this.Match(columnType, dataType);
                }
            }
        }

        public bool ExistIndex(string schema, string table, string column)
        {
            using (var connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    var cmdText = @"
SELECT COUNT(*)
FROM sys.indexes AS idx
JOIN sys.index_columns idxcol
ON idx.object_id = idxcol.object_id AND idx.index_id=idxcol.index_id
WHERE idx.type = 2 -- Non Clusterd
and key_ordinal = 1 -- 1 based

and object_name(idx.object_id) = @tableName
and object_schema_name(idx.object_id) = @tableSchema
and col_name(idx.object_id,idxcol.column_id) = @columnName";

                    command.CommandText = cmdText;
                    command.Parameters.Add("@tableName", SqlDbType.NVarChar).Value = table;
                    command.Parameters.Add("@tableSchema", SqlDbType.NVarChar).Value = schema;
                    command.Parameters.Add("@columnName", SqlDbType.NVarChar).Value = column;

                    var count = (int)command.ExecuteScalar();

                    return count != 0;
                }
            }
        }

        public bool ExistProcedure(string schema, string procedure)
        {
            using (var connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    var cmdText = @"
SELECT count(*)
FROM INFORMATION_SCHEMA.ROUTINES
WHERE routine_schema = @routineSchema 
AND routine_name=@routineName";

                    command.CommandText = cmdText;
                    command.Parameters.Add("@routineSchema", SqlDbType.NVarChar).Value = schema;
                    command.Parameters.Add("@routineName", SqlDbType.NVarChar).Value = procedure;

                    var count = (int)command.ExecuteScalar();

                    return count != 0;
                }
            }
        }

        public bool ExistPrimaryKey(string schema, string table, string column)
        {
            using (var connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    var cmdText = @"
SELECT count(*)
FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
WHERE OBJECTPROPERTY(OBJECT_ID(constraint_name), 'IsPrimaryKey') = 1
AND table_name = @tableName 
AND table_schema = @tableSchema 
AND column_name=@columnName";

                    command.CommandText = cmdText;
                    command.Parameters.Add("@tableName", SqlDbType.NVarChar).Value = table;
                    command.Parameters.Add("@tableSchema", SqlDbType.NVarChar).Value = schema;
                    command.Parameters.Add("@columnName", SqlDbType.NVarChar).Value = column;

                    var count = (int)command.ExecuteScalar();

                    return count != 0;
                }
            }
        }
    }
}