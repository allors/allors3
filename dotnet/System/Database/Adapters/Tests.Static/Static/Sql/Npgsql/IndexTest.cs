// <copyright file="IndexTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using System.Globalization;
    using global::Npgsql;
    using Xunit;

    public class IndexTest : Sql.IndexTest, IClassFixture<Fixture<IndexTest>>
    {
        private readonly Profile profile;

        public IndexTest() => this.profile = new Profile(this.GetType().Name);

        public override void Dispose() => this.profile.Dispose();

        [Theory]
        [InlineData("idx_c1_c1many2one", "many2one", true)]
        [InlineData("idx_c1_c1c1allorsstring", "c1allorsstring", false)]
        public void QuotedIndexNamesAreDistinctFromTheNamesOfTheAdapter(string name, string column, bool wanted)
        {
            this.Rebuild();

            if (wanted)
            {
                this.DropIndex("c1", name);
            }
            else
            {
                this.ExecuteScalar($"CREATE INDEX {name} ON allors.c1 (c1allorsdouble)");
            }

            var quotedName = name.ToUpperInvariant();
            var quotedColumn = wanted ? "c1allorsdouble" : column;
            this.ExecuteScalar($"CREATE INDEX \"{quotedName}\" ON allors.c1 ({quotedColumn})");

            ((Database)this.CreateDatabase()).EnsureIndexes();

            Assert.True(this.ExistIndex(name));
            Assert.True(this.ExistIndex(quotedName));
            Assert.Equal(1, this.IndexCount("c1", column));
            Assert.Equal(1, this.IndexCount("c1", "c1allorsdouble"));
        }

        [Theory]
        [InlineData("ALTER TABLE allors.c1 ADD COLUMN \"MANY2ONE\" bigint", "allors.c1 (\"MANY2ONE\")")]
        [InlineData("CREATE TABLE allors.\"C1\" (o bigint PRIMARY KEY, many2one bigint)", "allors.\"C1\" (many2one)")]
        public void QuotedTableAndColumnNamesAreDistinctFromTheNamesOfTheAdapter(string create, string on)
        {
            this.Rebuild();
            this.DropIndex("c1", "idx_c1_c1many2one");

            try
            {
                // A column or a table whose name differs from the adapter's only by case, with an index of
                // somebody else.
                this.ExecuteScalar(create);
                this.ExecuteScalar($"CREATE INDEX made_by_hand ON {on}");

                ((Database)this.CreateDatabase()).EnsureIndexes();

                Assert.True(this.ExistIndex("idx_c1_c1many2one"));
                Assert.Equal(1, this.IndexCount("c1", "many2one"));
            }
            finally
            {
                this.ExecuteScalar("DROP TABLE IF EXISTS allors.\"C1\"");
            }
        }

        protected override IDatabase CreateDatabase() => this.profile.CreateDatabase();

        protected override object ExecuteScalar(string sql) => this.profile.ExecuteScalar(sql);

        protected override void WithIndexLock(Action action)
        {
            var database = (Database)this.CreateDatabase();
            using (var connection = new NpgsqlConnection(database.ConnectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                using (var command = new NpgsqlCommand("SELECT pg_advisory_xact_lock(hashtext('allors.indexes.allors'))", connection, transaction))
                {
                    command.ExecuteNonQuery();
                    action();
                }
            }
        }

        protected override int IndexCount(string table, string column) => Convert.ToInt32(this.profile.ExecuteScalar($@"
SELECT COUNT(*)
FROM pg_index i
JOIN pg_class t ON t.oid = i.indrelid
JOIN pg_namespace n ON n.oid = t.relnamespace
JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum = i.indkey[0]
WHERE n.nspname = 'allors' AND t.relname = '{table}' AND a.attname = '{column}'"), CultureInfo.InvariantCulture);

        protected override bool ExistIndex(string name) => Convert.ToInt32(this.profile.ExecuteScalar($"SELECT COUNT(*) FROM pg_indexes WHERE schemaname = 'allors' AND indexname = '{name}'"), CultureInfo.InvariantCulture) != 0;

        protected override void DropIndex(string table, string name) => this.profile.ExecuteScalar($"DROP INDEX allors.{name}");

        protected override void DropIndexes()
        {
            if (this.profile.ExecuteScalar(@"
SELECT string_agg(format('DROP INDEX %I.%I', n.nspname, c.relname), '; ')
FROM pg_index i
JOIN pg_class c ON c.oid = i.indexrelid
JOIN pg_class t ON t.oid = i.indrelid
JOIN pg_namespace n ON n.oid = t.relnamespace
WHERE n.nspname = 'allors' AND NOT i.indisprimary") is string statements)
            {
                this.profile.ExecuteScalar(statements);
            }
        }
    }
}
