// <copyright file="IndexTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using System.Globalization;
    using global::Npgsql;
    using Meta;
    using Xunit;

    public class IndexTest : Sql.IndexTest, IClassFixture<Fixture<IndexTest>>
    {
        private readonly Profile profile;

        public IndexTest() => this.profile = new Profile(this.GetType().Name);

        public override void Dispose() => this.profile.Dispose();

        // The index for C1.Many2One, which the adapter wants, and the name that it gives to an index for
        // C1.C1AllorsString, which it does not want.
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void QuotedIndexNamesAreDistinctFromTheNamesOfTheAdapter(bool wanted)
        {
            this.Rebuild();

            var name = wanted ? this.RoleIndex : this.StringIndex;
            var column = wanted ? this.RoleColumn : this.StringColumn;

            if (wanted)
            {
                this.DropIndex(this.TableOfC1, name);
            }
            else
            {
                this.ExecuteScalar($"CREATE INDEX {name} ON allors.{this.TableOfC1} ({this.DoubleColumn})");
            }

            var quotedName = name.ToUpperInvariant();
            var quotedColumn = wanted ? this.DoubleColumn : column;
            this.ExecuteScalar($"CREATE INDEX \"{quotedName}\" ON allors.{this.TableOfC1} ({quotedColumn})");

            ((Database)this.CreateDatabase()).EnsureIndexes();

            Assert.True(this.ExistIndex(name));
            Assert.True(this.ExistIndex(quotedName));
            Assert.Equal(1, this.IndexCount(this.TableOfC1, column));
            Assert.Equal(1, this.IndexCount(this.TableOfC1, this.DoubleColumn));
        }

        // The column of C1.Many2One and the table of C1, in upper case.
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void QuotedTableAndColumnNamesAreDistinctFromTheNamesOfTheAdapter(bool column)
        {
            this.Rebuild();
            this.DropIndex(this.TableOfC1, this.RoleIndex);

            var quotedTable = this.TableOfC1.ToUpperInvariant();
            var quotedColumn = this.RoleColumn.ToUpperInvariant();
            var create = column
                ? $"ALTER TABLE allors.{this.TableOfC1} ADD COLUMN \"{quotedColumn}\" bigint"
                : $"CREATE TABLE allors.\"{quotedTable}\" (o bigint PRIMARY KEY, {this.RoleColumn} bigint)";
            var on = column
                ? $"allors.{this.TableOfC1} (\"{quotedColumn}\")"
                : $"allors.\"{quotedTable}\" ({this.RoleColumn})";

            try
            {
                // A column or a table whose name differs from the adapter's only by case, with an index of
                // somebody else.
                this.ExecuteScalar(create);
                this.ExecuteScalar($"CREATE INDEX made_by_hand ON {on}");

                ((Database)this.CreateDatabase()).EnsureIndexes();

                Assert.True(this.ExistIndex(this.RoleIndex));
                Assert.Equal(1, this.IndexCount(this.TableOfC1, this.RoleColumn));
            }
            finally
            {
                this.ExecuteScalar($"DROP TABLE IF EXISTS allors.\"{quotedTable}\"");
            }
        }

        protected override IDatabase CreateDatabase(MetaPopulation metaPopulation = null) => this.profile.CreateDatabase(metaPopulation: metaPopulation);

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

        protected override bool IsIndexLockAwaited() => Convert.ToInt32(this.profile.ExecuteScalar(@"
SELECT COUNT(*)
FROM pg_locks l
JOIN pg_database d ON d.oid = l.database
WHERE d.datname = current_database() AND l.locktype = 'advisory' AND NOT l.granted"), CultureInfo.InvariantCulture) != 0;

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
