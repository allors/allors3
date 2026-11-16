// <copyright file="SchemaTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System.Linq;
    using Domain;
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

        protected override bool ExistProcedure(string schema, string procedure) => this.profile.ExistProcedure(schema, procedure);

        protected override bool ExistIndex(string schema, string table, string column) => this.profile.ExistIndex(schema, table, column);

        protected override void DropProcedure(string schema, string procedure) => this.profile.DropProcedure(procedure);

        // The schema is validated when the first transaction of a database is created. From then on the adapter
        // sends its statements itself: a transaction creates, reads, changes and deletes objects in a schema
        // whose procedures are gone.
        [Fact]
        public void TransactionDoesNotNeedTheProcedures()
        {
            foreach (var init in this.Inits)
            {
                init();

                var database = this.Profile.Database;
                var m = database.Context().M;

                this.profile.DropProcedures("allors");

                var population = new TestPopulation(this.Transaction);
                var c4s = this.Transaction.Create(m.C4, 2);
                this.Transaction.Commit();

                // The commit made the versions of the objects unknown: their roles and associations are read from
                // the database.
                Assert.Equal("c1b", population.C1B.Name);
                Assert.Equal(population.C2B, population.C1B.C1C2one2one);
                Assert.Equal(population.C2B, population.C1B.I12C2one2one);
                Assert.Equal(new[] { population.C2C, population.C2D }, population.C1C.C1C2one2manies.OrderBy(v => v.Id));
                Assert.Equal(new[] { population.C2B, population.C2C, population.C2D }, population.C1D.C1C2many2manies.OrderBy(v => v.Id));
                Assert.Equal(population.C1B, population.C2B.C1WhereC1C2one2one);
                Assert.Equal(new[] { population.C1C, population.C1D }, population.C2C.C1sWhereC1C2many2one.OrderBy(v => v.Id));
                Assert.Equal(new[] { population.C1B, population.C1C, population.C1D }, population.C2B.C1sWhereC1C2many2many.OrderBy(v => v.Id));

                using (var transaction = database.CreateTransaction())
                {
                    var c1s = transaction.Instantiate(new[] { population.C1B.Id, population.C1C.Id, population.C1D.Id });
                    Assert.Equal(3, c1s.Length);

                    // Every role and every association of every class.
                    var prefetchers = new Prefetchers();
                    foreach (var @class in database.MetaPopulation.DatabaseClasses)
                    {
                        transaction.Prefetch(prefetchers[@class], transaction.Extent(@class).ToArray());
                    }

                    var c1B = (C1)c1s[0];
                    var c1C = (C1)c1s[1];
                    var c1D = (C1)c1s[2];

                    c1B.C1AllorsString = "changed";
                    c1B.RemoveC1C2one2one();
                    c1C.RemoveC1C2one2many((C2)transaction.Instantiate(population.C2C.Id));
                    c1D.RemoveC1C2many2manies();
                    transaction.Instantiate(c4s[0].Id).Strategy.Delete();

                    transaction.Commit();
                }

                using (var transaction = database.CreateTransaction())
                {
                    var c1s = transaction.Instantiate(new[] { population.C1B.Id, population.C1C.Id, population.C1D.Id });

                    Assert.Equal("changed", ((C1)c1s[0]).C1AllorsString);
                    Assert.Null(((C1)c1s[0]).C1C2one2one);
                    Assert.Equal(new[] { population.C2D.Id }, ((C1)c1s[1]).C1C2one2manies.Select(v => v.Id));
                    Assert.Empty(((C1)c1s[2]).C1C2many2manies);
                    Assert.Null(transaction.Instantiate(c4s[0].Id));
                    Assert.NotNull(transaction.Instantiate(c4s[1].Id));
                }
            }
        }

        [Fact(Skip = "Explicit")]
        public void Recover()
        {
            // this.InitAndCreateTransaction();

            // this.DropProcedure("_GC");

            // var repository = this.CreateDatabase();

            // var exceptionThrown = false;
            // try
            // {
            //    repository.CreateTransaction();
            // }
            // catch
            // {
            //    exceptionThrown = true;
            // }

            // Assert.True(exceptionThrown);

            // ((Database.SqlClient.Database)repository).Schema.Recover();

            // Assert.True(this.ExistProcedure("_GC"));

            // exceptionThrown = false;
            // try
            // {
            //    repository.CreateTransaction();
            // }
            // catch
            // {
            //    exceptionThrown = true;
            // }

            // Assert.False(exceptionThrown);
        }
    }
}
