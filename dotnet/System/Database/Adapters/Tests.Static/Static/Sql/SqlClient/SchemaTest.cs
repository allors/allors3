// <copyright file="SchemaTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using Xunit;

    public class SchemaTest : Adapters.Sql.SchemaTest, IClassFixture<Fixture<SchemaTest>>
    {
        private readonly Profile profile;

        public SchemaTest() => this.profile = new Profile(this.GetType().Name);

        protected override IProfile Profile => this.profile;

        public override void Dispose() => this.profile.Dispose();

        protected override void DropTable(string schema, string tableName) => this.profile.DropTable(schema, tableName);

        protected override bool ExistTable(string schema, string table) => this.profile.ExistTable(schema, table);

        protected override int ColumnCount(string schema, string table) => this.profile.ColumnCount(schema, table);

        protected override bool ExistColumn(string schema, string table, string column, ColumnTypes columnType) => this.profile.ExistColumn(schema, table, column, columnType);

        protected override bool ExistPrimaryKey(string schema, string table, string column) => this.profile.ExistPrimaryKey(schema, table, column);

        protected override bool ExistIndex(string schema, string table, string column) => this.profile.ExistIndex(schema, table, column);

        // The adapter sends statements only. Init creates the tables and the table types that those statements use:
        // no stored procedure, and no table type that nothing uses.
        [Fact]
        public void InitCreatesNoProceduresAndNoUnusedTableTypes()
        {
            foreach (var init in this.Inits)
            {
                init();

                Assert.Equal(0, (int)this.profile.ExecuteScalar("SELECT COUNT(*) FROM INFORMATION_SCHEMA.ROUTINES WHERE routine_schema = 'allors'"));
                Assert.Equal(0, (int)this.profile.ExecuteScalar("SELECT COUNT(*) FROM sys.table_types WHERE schema_id = SCHEMA_ID('allors') AND (name LIKE '[_]ti[_]%' OR name = '_t_vo')"));
            }
        }
    }
}
