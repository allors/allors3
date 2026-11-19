// <copyright file="InitTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;
    using System.Globalization;
    using Xunit;
    using C1 = Domain.C1;
    using C2 = Domain.C2;

    // Init of a valid schema empties the tables in one transaction: the objects table, where the numbering of
    // the objects starts again, and the tables that hold data. A table without data is left as it is.
    public abstract class InitTest : IDisposable
    {
        // A table that refers to the table of C2. Neither server truncates a table that a foreign key
        // references: an Init that truncates the table of C2 fails.
        private const string CreateReferringTable = "CREATE TABLE allors.refers_to_c2 (r bigint REFERENCES allors.c2 (o))";
        private const string DropReferringTable = "DROP TABLE allors.refers_to_c2";

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

            this.ExecuteScalar(CreateReferringTable);
            try
            {
                database.Init();
            }
            finally
            {
                this.ExecuteScalar(DropReferringTable);
            }

            Assert.Equal(0, this.Count("_o"));
            Assert.Equal(0, this.Count("c1"));
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
            this.ExecuteScalar(CreateReferringTable);
            try
            {
                Assert.ThrowsAny<Exception>(() => database.Init());
            }
            finally
            {
                this.ExecuteScalar(DropReferringTable);
            }

            Assert.Equal(2, this.Count("_o"));
            Assert.Equal(1, this.Count("c1"));
            Assert.Equal(1, this.Count("c2"));
            Assert.Equal(1, this.Count("c1c1c2many2many"));

            // The numbering goes on where it was.
            using (var transaction = database.CreateTransaction())
            {
                Assert.Equal(3, C1.Create(transaction).Id);
            }
        }

        private int Count(string table) => Convert.ToInt32(this.ExecuteScalar($"SELECT COUNT(*) FROM allors.{table}"), CultureInfo.InvariantCulture);
    }
}
