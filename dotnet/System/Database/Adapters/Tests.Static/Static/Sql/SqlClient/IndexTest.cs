// <copyright file="IndexTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using System.Globalization;
    using Microsoft.Data.SqlClient;
    using Meta;
    using Xunit;

    public class IndexTest : Sql.IndexTest, IClassFixture<Fixture<IndexTest>>
    {
        private readonly Profile profile;

        public IndexTest() => this.profile = new Profile(this.GetType().Name);

        public override void Dispose() => this.profile.Dispose();

        protected override IDatabase CreateDatabase(MetaPopulation metaPopulation = null) => this.profile.CreateDatabase(metaPopulation: metaPopulation);

        protected override object ExecuteScalar(string sql) => this.profile.ExecuteScalar(sql);

        protected override int IndexCount(string table, string column) => Convert.ToInt32(this.profile.ExecuteScalar($@"
SELECT COUNT(*)
FROM sys.indexes i
JOIN sys.tables t ON t.object_id = i.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id AND ic.key_ordinal = 1
JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
WHERE s.name = 'allors' AND t.name = '{table}' AND c.name = '{column}'"), CultureInfo.InvariantCulture);

        protected override bool ExistIndex(string name) => Convert.ToInt32(this.profile.ExecuteScalar($@"
SELECT COUNT(*)
FROM sys.indexes i
JOIN sys.tables t ON t.object_id = i.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = 'allors' AND i.name = '{name}'"), CultureInfo.InvariantCulture) != 0;

        protected override void DropIndex(string table, string name) => this.profile.ExecuteScalar($"DROP INDEX {name} ON allors.{table}");

        protected override void DropIndexes()
        {
            if (this.profile.ExecuteScalar(@"
SELECT STRING_AGG(CAST(N'DROP INDEX ' + QUOTENAME(i.name) + N' ON ' + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name) AS nvarchar(max)), N'; ')
FROM sys.indexes i
JOIN sys.tables t ON t.object_id = i.object_id
JOIN sys.schemas s ON s.schema_id = t.schema_id
WHERE s.name = 'allors' AND i.type = 2 AND i.is_primary_key = 0") is string statements)
            {
                this.profile.ExecuteScalar(statements);
            }
        }

        protected override void WithIndexLock(Action action)
        {
            using var connection = new SqlConnection(((Database)this.CreateDatabase()).ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            using var command = new SqlCommand(@"
DECLARE @result int;
EXEC @result = sp_getapplock @Resource = 'allors.indexes.allors', @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000;
SELECT @result;", connection, transaction);
            Assert.True((int)command.ExecuteScalar() >= 0, "The test could not acquire the schema lock.");
            action();
        }

        protected override bool IsIndexLockAwaited() => Convert.ToInt32(this.profile.ExecuteScalar(@"
SELECT COUNT(*)
FROM sys.dm_tran_locks
WHERE resource_database_id = DB_ID() AND resource_type = 'APPLICATION' AND request_status = 'WAIT'"), CultureInfo.InvariantCulture) != 0;
    }
}
