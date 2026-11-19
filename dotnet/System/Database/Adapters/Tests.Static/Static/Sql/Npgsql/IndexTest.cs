// <copyright file="IndexTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.Npgsql
{
    using System;
    using System.Globalization;
    using Xunit;

    public class IndexTest : Sql.IndexTest, IClassFixture<Fixture<IndexTest>>
    {
        private readonly Profile profile;

        public IndexTest() => this.profile = new Profile(this.GetType().Name);

        public override void Dispose() => this.profile.Dispose();

        protected override IDatabase CreateDatabase() => this.profile.CreateDatabase();

        protected override object ExecuteScalar(string sql) => this.profile.ExecuteScalar(sql);

        protected override int IndexCount(string table, string column) => Convert.ToInt32(this.profile.ExecuteScalar($@"
SELECT COUNT(*)
FROM pg_index i
JOIN pg_class t ON t.oid = i.indrelid
JOIN pg_namespace n ON n.oid = t.relnamespace
JOIN pg_attribute a ON a.attrelid = t.oid AND a.attnum = i.indkey[0]
WHERE n.nspname = 'allors' AND t.relname = '{table}' AND a.attname = '{column}'"), CultureInfo.InvariantCulture);

        protected override bool ExistIndex(string name) => Convert.ToInt32(this.profile.ExecuteScalar($"SELECT COUNT(*) FROM pg_indexes WHERE schemaname = 'allors' AND indexname = '{name}'"), CultureInfo.InvariantCulture) != 0;

        protected override void DropIndex(string table, string name) => this.profile.ExecuteScalar($"DROP INDEX allors.{name}");
    }
}
