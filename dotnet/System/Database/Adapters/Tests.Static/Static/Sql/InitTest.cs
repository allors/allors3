// <copyright file="InitTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;
    using System.Globalization;
    using Meta;
    using Xunit;
    using C1 = Domain.C1;
    using C2 = Domain.C2;

    // Init of a valid schema empties the tables in one transaction: the objects table, where the numbering of
    // the objects starts again, and the tables that hold data. A table without data is left as it is. The facts
    // take the names of the tables from the mapping.
    public abstract class InitTest : IDisposable
    {
        private const string DropReferringTable = "DROP TABLE allors.refers_to_c2";

        private Database names;

        protected MetaPopulation M => (MetaPopulation)this.Names.MetaPopulation;

        // A table that refers to the table of C2. Neither server truncates a table that a foreign key
        // references: an Init that truncates the table of C2 fails.
        private string CreateReferringTable => $"CREATE TABLE allors.refers_to_c2 (r bigint REFERENCES {this.Table(this.M.C2)} (o))";

        // A database object that is asked for its mapping only.
        private Database Names => this.names ??= (Database)this.CreateDatabase();

        public abstract void Dispose();

        protected abstract IDatabase CreateDatabase();

        protected abstract object ExecuteScalar(string sql);

        [Fact]
        public void InitLeavesATableWithoutDataAsItIs()
        {
            var database = this.CreateDatabase();
            database.Init();

            using (var transaction = database.CreateTransaction())
            {
                C1.Create(transaction);
                transaction.Commit();
            }

            this.ExecuteScalar(this.CreateReferringTable);
            try
            {
                database.Init();
            }
            finally
            {
                this.ExecuteScalar(DropReferringTable);
            }

            Assert.Equal(0, this.Count(this.TableOfObjects));
            Assert.Equal(0, this.Count(this.Table(this.M.C1)));
        }

        [Fact]
        public void AnInitThatFailsChangesNothing()
        {
            var database = this.CreateDatabase();
            database.Init();

            using (var transaction = database.CreateTransaction())
            {
                var c1 = C1.Create(transaction);
                var c2 = C2.Create(transaction);
                c1.AddC1C2many2many(c2);
                transaction.Commit();
            }

            // The table of C2 holds a row, so Init truncates it, and fails.
            this.ExecuteScalar(this.CreateReferringTable);
            try
            {
                Assert.ThrowsAny<Exception>(() => database.Init());
            }
            finally
            {
                this.ExecuteScalar(DropReferringTable);
            }

            Assert.Equal(2, this.Count(this.TableOfObjects));
            Assert.Equal(1, this.Count(this.Table(this.M.C1)));
            Assert.Equal(1, this.Count(this.Table(this.M.C2)));
            Assert.Equal(1, this.Count(this.Table(this.M.C1.C1C2many2manies)));

            // The numbering goes on where it was.
            using (var transaction = database.CreateTransaction())
            {
                Assert.Equal(3, C1.Create(transaction).Id);
            }
        }

        // The objects table, and the table of a class or of a relation, as the mapping names them.
        protected string TableOfObjects => this.Names.Mapping.TableNameForObjects;

        protected string Table(IClass @class) => this.Names.Mapping.TableNameForObjectByClass[@class];

        protected string Table(IRoleType roleType) => this.Names.Mapping.TableNameForRelationByRelationType[roleType.RelationType];

        private int Count(string table) => Convert.ToInt32(this.ExecuteScalar($"SELECT COUNT(*) FROM {table}"), CultureInfo.InvariantCulture);
    }
}
