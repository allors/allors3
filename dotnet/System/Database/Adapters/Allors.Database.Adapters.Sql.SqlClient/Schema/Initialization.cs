// <copyright file="Initialization.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using Microsoft.Data.SqlClient;
    using System.Text;


    public class Initialization
    {
        private readonly Database database;
        private readonly Mapping mapping;

        private Validation validation;

        internal Initialization(Database database)
        {
            this.database = database;
            this.mapping = (Mapping)database.Mapping;
        }

        private string ConnectionString
        {
            get
            {
                var builder = new SqlConnectionStringBuilder(this.database.ConnectionString) { Pooling = false };
                return builder.ConnectionString;
            }
        }

        internal void Execute()
        {
            this.validation = new Validation(this.database);

            this.AllowSnapshotIsolation();

            if (this.validation.IsValid)
            {
                this.TruncateTables();
            }
            else
            {
                this.CreateSchema();

                // The table types go first. SQL Server does not drop a table type that another object of the
                // schema uses, and then Init stops before it has touched a table.
                this.DropTableTypes();

                this.DropTables();

                this.CreateTables();

                this.CreateTableTypes();
            }
        }

        private void AllowSnapshotIsolation()
        {
            using var connection = new SqlConnection(this.ConnectionString);
            connection.Open();
            try
            {
                // Asked first, and altered only when snapshot isolation is off: SQL Server refuses the statement
                // below to a login that may not alter the database, also when it has nothing to alter.
                using (var ask = new SqlCommand("SELECT snapshot_isolation_state FROM sys.databases WHERE database_id = DB_ID()", connection))
                {
                    if (ask.ExecuteScalar() is byte state && state != 0)
                    {
                        return;
                    }
                }

                // The database of the connection, without its name: a name can need brackets, and can hold a
                // quote or a bracket itself.
                using var command = new SqlCommand("ALTER DATABASE CURRENT SET ALLOW_SNAPSHOT_ISOLATION ON", connection);
                command.ExecuteNonQuery();
            }
            finally
            {
                connection.Close();
            }
        }

        private void CreateSchema()
        {
            if (!this.validation.Schema.Exists)
            {
                // CREATE SCHEMA must be in its own batch
                using var connection = new SqlConnection(this.ConnectionString);
                connection.Open();
                try
                {
                    var cmdText = $@"
CREATE SCHEMA {this.database.SchemaName}";
                    using var command = new SqlCommand(cmdText, connection);
                    command.ExecuteNonQuery();
                }
                finally
                {
                    connection.Close();
                }
            }
        }

        private void DropTableTypes()
        {
            using var connection = new SqlConnection(this.ConnectionString);
            connection.Open();
            try
            {
                foreach (var name in this.validation.Schema.TableTypeByName.Keys)
                {
                    using var command = new SqlCommand($"DROP TYPE {name}", connection);
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                connection.Close();
            }
        }

        private void CreateTableTypes()
        {
            using var connection = new SqlConnection(this.ConnectionString);
            connection.Open();
            try
            {
                foreach (var dictionaryEntry in this.mapping.TableTypeDefinitionByName)
                {
                    var definition = dictionaryEntry.Value;

                    using var command = new SqlCommand(definition, connection);
                    command.ExecuteNonQuery();
                }
            }
            finally
            {
                connection.Close();
            }
        }

        // Empties the tables in one transaction: when it fails, it has changed nothing. Only the tables that
        // hold a row are truncated.
        private void TruncateTables()
        {
            var sql = new StringBuilder();

            // The objects table numbers the objects. It is truncated whatever it holds: the numbering starts
            // again, as it does when the table is created.
            sql.Append("TRUNCATE TABLE " + this.mapping.TableNameForObjects + ";\n");

            foreach (var @class in this.mapping.Database.MetaPopulation.DatabaseClasses)
            {
                var tableName = this.mapping.TableNameForObjectByClass[@class];

                AppendTruncateTable(sql, tableName);
            }

            foreach (var relationType in this.mapping.Database.MetaPopulation.DatabaseRelationTypes)
            {
                var associationType = relationType.AssociationType;
                var roleType = relationType.RoleType;

                if (!roleType.ObjectType.IsUnit && ((associationType.IsMany && roleType.IsMany) || !relationType.ExistExclusiveDatabaseClasses))
                {
                    var tableName = this.mapping.TableNameForRelationByRelationType[relationType];
                    AppendTruncateTable(sql, tableName);
                }
            }

            using var connection = new SqlConnection(this.ConnectionString);
            connection.Open();
            try
            {
                // One batch. The transaction is committed here, once the batch has ended without an error:
                // after an error that ends its statement only, SQL Server goes on with the rest of a batch.
                using var transaction = connection.BeginTransaction();
                using var command = new SqlCommand(sql.ToString(), connection, transaction);
                command.ExecuteNonQuery();
                transaction.Commit();
            }
            finally
            {
                connection.Close();
            }
        }

        private void DropTables()
        {
            using var connection = new SqlConnection(this.ConnectionString);
            connection.Open();
            try
            {
                this.DropTable(connection, this.mapping.TableNameForObjects);

                foreach (var @class in this.mapping.Database.MetaPopulation.DatabaseClasses)
                {
                    var tableName = this.mapping.TableNameForObjectByClass[@class];

                    this.DropTable(connection, tableName);
                }

                foreach (var relationType in this.mapping.Database.MetaPopulation.DatabaseRelationTypes)
                {
                    var associationType = relationType.AssociationType;
                    var roleType = relationType.RoleType;

                    if (!roleType.ObjectType.IsUnit && ((associationType.IsMany && roleType.IsMany) || !relationType.ExistExclusiveDatabaseClasses))
                    {
                        var tableName = this.mapping.TableNameForRelationByRelationType[relationType];
                        this.DropTable(connection, tableName);
                    }
                }
            }
            finally
            {
                connection.Close();
            }
        }

        private void CreateTables()
        {
            using var connection = new SqlConnection(this.ConnectionString);
            connection.Open();
            try
            {
                {
                    var sql = new StringBuilder();
                    sql.Append($"CREATE TABLE {this.mapping.TableNameForObjects}\n");
                    sql.Append("(\n");
                    sql.Append($"{Sql.Mapping.ColumnNameForObject} {Mapping.SqlTypeForObject} IDENTITY(1,1) PRIMARY KEY,\n");
                    sql.Append($"{Sql.Mapping.ColumnNameForClass} {Mapping.SqlTypeForClass},\n");
                    sql.Append($"{Sql.Mapping.ColumnNameForVersion} {Mapping.SqlTypeForVersion}\n");
                    sql.Append(")\n");

                    using var command = new SqlCommand(sql.ToString(), connection);
                    command.ExecuteNonQuery();
                }

                foreach (var @class in this.mapping.Database.MetaPopulation.DatabaseClasses)
                {
                    var tableName = this.mapping.TableNameForObjectByClass[@class];

                    var sql = new StringBuilder();
                    sql.Append($"CREATE TABLE {tableName}\n");
                    sql.Append("(\n");
                    sql.Append($"{Sql.Mapping.ColumnNameForObject} {Mapping.SqlTypeForObject} PRIMARY KEY,\n");
                    sql.Append($"{Sql.Mapping.ColumnNameForClass} {Mapping.SqlTypeForClass}");

                    foreach (var associationType in @class.DatabaseAssociationTypes)
                    {
                        var relationType = associationType.RelationType;
                        var roleType = relationType.RoleType;
                        if (!(associationType.IsMany && roleType.IsMany) && relationType.ExistExclusiveDatabaseClasses && roleType.IsMany)
                        {
                            sql.Append(
                                $",\n{this.mapping.ColumnNameByRelationType[relationType]} {Mapping.SqlTypeForObject}");
                        }
                    }

                    foreach (var roleType in @class.DatabaseRoleTypes)
                    {
                        var relationType = roleType.RelationType;
                        var associationType3 = relationType.AssociationType;
                        if (roleType.ObjectType.IsUnit)
                        {
                            sql.Append(
                                $",\n{this.mapping.ColumnNameByRelationType[relationType]} {this.mapping.GetSqlType(roleType)}");
                        }
                        else if (!(associationType3.IsMany && roleType.IsMany) && relationType.ExistExclusiveDatabaseClasses && !roleType.IsMany)
                        {
                            sql.Append(
                                $",\n{this.mapping.ColumnNameByRelationType[relationType]} {Mapping.SqlTypeForObject}");
                        }
                    }

                    sql.Append(")\n");

                    using var command = new SqlCommand(sql.ToString(), connection);
                    command.ExecuteNonQuery();
                }

                foreach (var relationType in this.mapping.Database.MetaPopulation.DatabaseRelationTypes)
                {
                    var associationType = relationType.AssociationType;
                    var roleType = relationType.RoleType;

                    if (!roleType.ObjectType.IsUnit && ((associationType.IsMany && roleType.IsMany) || !relationType.ExistExclusiveDatabaseClasses))
                    {
                        var tableName = this.mapping.TableNameForRelationByRelationType[relationType];

                        var primaryKeyName = $"pk_{relationType.RoleType.SingularFullName.ToLowerInvariant()}";

                        var sql =
                            $@"CREATE TABLE {tableName}(
    {Sql.Mapping.ColumnNameForAssociation} {Mapping.SqlTypeForObject},
    {Sql.Mapping.ColumnNameForRole} {Mapping.SqlTypeForObject},
    {(relationType.RoleType.IsOne
                                ? $"CONSTRAINT {primaryKeyName} PRIMARY KEY ({Sql.Mapping.ColumnNameForAssociation})\n"
                                : $"CONSTRAINT {primaryKeyName} PRIMARY KEY ({Sql.Mapping.ColumnNameForAssociation}, {Sql.Mapping.ColumnNameForRole})\n")}

)";

                        using var command = new SqlCommand(sql, connection);
                        command.ExecuteNonQuery();
                    }
                }
            }
            finally
            {
                connection.Close();
            }
        }

        // The look takes an exclusive lock on the table, which it keeps until the transaction ends: it waits for
        // a transaction that holds a row in the table that is not committed, and no other transaction writes
        // to the table before the truncate. Without the lock, and with read committed snapshot on, the look
        // does not see such a row and does not wait for it.
        private static void AppendTruncateTable(StringBuilder sql, string tableName) =>
            sql.Append("IF EXISTS (SELECT * FROM " + tableName + " WITH (TABLOCKX)) TRUNCATE TABLE " + tableName + ";\n");

        private void DropTable(SqlConnection connection, string tableName)
        {
            if (!this.validation.MissingTableNames.Contains(tableName))
            {
                using var command = new SqlCommand($"DROP TABLE {tableName}", connection);
                command.ExecuteNonQuery();
            }
        }
    }
}
