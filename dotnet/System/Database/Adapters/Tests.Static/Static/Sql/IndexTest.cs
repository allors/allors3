// <copyright file="IndexTest.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Xml;
    using Meta;
    using Xunit;

    // The adapter ensures the indexes of its schema when it is asked to, at the end of Init and at the end of a
    // load. A transaction creates and drops no index: the first transaction of a database object reads the indexes
    // of the schema, and none opens while one is missing. Every composite relation gets an index, and a unit role
    // gets one when the domain marks it as indexed. The test domain marks C1.C1AllorsInteger, C1.Many2One and
    // Company.IndexedMany2ManyPersons, and not C1.C1AllorsString, C1.C1AllorsDouble, C1.C1C2one2one,
    // C1.C1C2one2manies and C1.C1C2many2manies.
    public abstract class IndexTest : IDisposable
    {
        private const string UnitColumn = "c1allorsinteger";
        private const string RoleColumn = "many2one";
        private const string RoleIndex = "idx_c1_c1many2one";
        private const string RelationTable = "companyindexedmany2manyperson";

        public abstract void Dispose();

        protected abstract IDatabase CreateDatabase();

        protected abstract object ExecuteScalar(string sql);

        // The number of indexes of the schema that lead with the column.
        protected abstract int IndexCount(string table, string column);

        protected abstract bool ExistIndex(string name);

        protected abstract void DropIndex(string table, string name);

        // Drops every index of the schema that is not the primary key of its table.
        protected abstract void DropIndexes();

        protected abstract void WithIndexLock(Action action);

        [Fact]
        public void InitCreatesTheIndexes()
        {
            this.Rebuild();

            Assert.Equal(1, this.IndexCount("c1", UnitColumn));
            Assert.Equal(1, this.IndexCount("c1", RoleColumn));
            Assert.Equal(1, this.IndexCount(RelationTable, "r"));
        }

        [Fact]
        public void EnsureIndexesCreatesThemWithoutATransaction()
        {
            var database = this.RebuildWithoutIndexes();

            Assert.Equal(0, this.IndexCount("c1", UnitColumn));
            Assert.Equal(0, this.IndexCount("c1", RoleColumn));
            Assert.Equal(0, this.IndexCount(RelationTable, "r"));

            database.EnsureIndexes();

            Assert.Equal(1, this.IndexCount("c1", UnitColumn));
            Assert.Equal(1, this.IndexCount("c1", RoleColumn));
            Assert.Equal(1, this.IndexCount(RelationTable, "r"));
        }

        [Fact]
        public void AMissingIndexRefusesTheTransactionUntilItIsCreated()
        {
            this.Rebuild();
            this.DropIndex("c1", RoleIndex);

            // The transaction creates nothing. Its message names the statement for the index and how to ensure
            // the indexes.
            var database = (Database)this.CreateDatabase();
            var exception = Assert.ThrowsAny<Exception>(() => database.CreateTransaction());
            Assert.Contains($"CREATE INDEX {RoleIndex} ON allors.c1 ({RoleColumn})", exception.Message);
            Assert.Contains("EnsureIndexes()", exception.Message);
            Assert.Equal(0, this.IndexCount("c1", RoleColumn));

            // The next transaction is refused as well.
            Assert.ThrowsAny<Exception>(() => database.CreateTransaction());

            // Another database object ensures the indexes, as a deployment does. The refusal is not kept.
            ((Database)this.CreateDatabase()).EnsureIndexes();

            using (database.CreateTransaction())
            {
            }

            Assert.True(this.ExistIndex(RoleIndex));
            Assert.Equal(1, this.IndexCount("c1", RoleColumn));
        }

        [Fact]
        public void AnIndexOfAnotherNameCounts()
        {
            this.Rebuild();
            this.DropIndex("c1", RoleIndex);
            this.ExecuteScalar($"CREATE INDEX made_by_hand ON allors.c1 ({RoleColumn})");

            var database = (Database)this.CreateDatabase();
            using (database.CreateTransaction())
            {
            }

            database.EnsureIndexes();

            Assert.False(this.ExistIndex(RoleIndex));
            Assert.Equal(1, this.IndexCount("c1", RoleColumn));
        }

        [Fact]
        public void AnIndexOfTheAdapterForAUnitRoleThatIsNotMarkedIsDropped()
        {
            this.Rebuild();

            // The name that the adapter gives to an index for C1.C1AllorsString, and an index of somebody else
            // on the same column.
            this.ExecuteScalar("CREATE INDEX idx_c1_c1c1allorsstring ON allors.c1 (c1allorsstring)");
            this.ExecuteScalar("CREATE INDEX made_by_hand ON allors.c1 (c1allorsstring)");

            // A transaction drops nothing.
            var database = (Database)this.CreateDatabase();
            using (database.CreateTransaction())
            {
            }

            Assert.True(this.ExistIndex("idx_c1_c1c1allorsstring"));

            database.EnsureIndexes();

            Assert.False(this.ExistIndex("idx_c1_c1c1allorsstring"));
            Assert.True(this.ExistIndex("made_by_hand"));
            Assert.Equal(1, this.IndexCount("c1", "c1allorsstring"));
        }

        [Fact]
        public void AnIndexThatCannotAcquireTheSchemaLockStays()
        {
            const string indexName = "idx_c1_c1c1allorsstring";
            this.Rebuild();
            this.ExecuteScalar($"CREATE INDEX {indexName} ON allors.c1 (c1allorsstring)");

            var database = (Database)this.CreateDatabase();
            Assert.True(database.IsValid);

            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    database.EnsureIndexes();
                }
                catch (Exception e)
                {
                    exception = e;
                }
            });

            var started = false;
            try
            {
                this.WithIndexLock(() =>
                {
                    thread.Start();
                    started = true;
                    Assert.True(thread.Join(TimeSpan.FromSeconds(10)), "Optional index cleanup did not finish while the schema lock was held.");
                    Assert.Null(exception);
                    Assert.True(this.ExistIndex(indexName));
                });
            }
            finally
            {
                // The helper releases the lock before a blocked worker is joined, even when the assertion fails.
                if (started)
                {
                    thread.Join();
                }
            }

            ((Database)this.CreateDatabase()).EnsureIndexes();

            Assert.False(this.ExistIndex(indexName));
        }

        [Fact]
        public void ACompositeRelationThatIsNotMarkedGetsAnIndex()
        {
            this.Rebuild();

            // C1.C1C2one2one is a column of the table of C1, and its index has the name of the adapter.
            Assert.True(this.ExistIndex("idx_c1_c1c1c2one2one"));
            Assert.Equal(1, this.IndexCount("c1", "c1c2one2one"));

            // C1.C1C2one2manies is a column of the table of C2, and C1.C1C2many2manies has a table of its own.
            Assert.Equal(1, this.IndexCount("c2", "c1wherec1c2one2many"));
            Assert.Equal(1, this.IndexCount("c1c1c2many2many", "r"));
        }

        [Fact]
        public void ANameThatIsTakenRefusesTheTransaction()
        {
            var database = this.Rebuild();
            this.DropIndex("c1", RoleIndex);

            // The name of the index for C1.Many2One, on another column.
            this.ExecuteScalar($"CREATE INDEX {RoleIndex} ON allors.c1 (c1allorsdouble)");

            // The message names the index that is needed and the column of the index that has its name.
            var exception = Assert.ThrowsAny<Exception>(() => this.CreateDatabase().CreateTransaction());
            Assert.Contains(RoleIndex, exception.Message);
            Assert.Contains(RoleColumn, exception.Message);
            Assert.Contains("c1allorsdouble", exception.Message);

            // Ensuring the indexes says the same.
            exception = Assert.ThrowsAny<Exception>(() => ((Database)this.CreateDatabase()).EnsureIndexes());
            Assert.Contains(RoleIndex, exception.Message);
            Assert.Contains(RoleColumn, exception.Message);
            Assert.Contains("c1allorsdouble", exception.Message);

            // So does Init, which ensures the indexes. The object that had the indexes before that Init is
            // refused a transaction as well.
            exception = Assert.ThrowsAny<Exception>(() => database.Init());
            Assert.Contains("c1allorsdouble", exception.Message);
            Assert.ThrowsAny<Exception>(() => database.CreateTransaction());

            this.DropIndex("c1", RoleIndex);
            ((Database)this.CreateDatabase()).EnsureIndexes();

            using (this.CreateDatabase().CreateTransaction())
            {
            }

            Assert.Equal(1, this.IndexCount("c1", RoleColumn));
        }

        [Fact]
        public void InitForgetsThatATransactionWasRefused()
        {
            this.Rebuild();
            this.ExecuteScalar("DROP TABLE allors.c1");

            var database = this.CreateDatabase();
            Assert.ThrowsAny<Exception>(() => database.CreateTransaction());

            database.Init();

            using (database.CreateTransaction())
            {
            }
        }

        [Fact]
        public void InitEnsuresTheIndexesAgain()
        {
            var database = this.Rebuild();
            Assert.Equal(1, this.IndexCount("c1", RoleColumn));

            // Init builds the schema again, and with it tables without indexes.
            this.ExecuteScalar("DROP TABLE allors.c1");
            database.Init();
            Assert.Equal(1, this.IndexCount("c1", RoleColumn));

            // Init of a valid schema only empties its tables. It brings back an index that the schema has lost.
            this.DropIndex("c1", RoleIndex);
            database.Init();
            Assert.Equal(1, this.IndexCount("c1", RoleColumn));

            using (database.CreateTransaction())
            {
            }
        }

        [Fact]
        public void LoadEnsuresTheIndexes()
        {
            var database = this.Rebuild();
            var m = (MetaPopulation)database.MetaPopulation;
            using (var transaction = database.CreateTransaction())
            {
                transaction.Create(m.C1);
                transaction.Commit();
            }

            var saved = new StringWriter();
            using (var writer = XmlWriter.Create(saved))
            {
                database.Save(writer);
            }

            // A schema that the load has to build again.
            this.ExecuteScalar("DROP TABLE allors.c1");

            var loaded = this.CreateDatabase();
            using (var reader = XmlReader.Create(new StringReader(saved.ToString())))
            {
                loaded.Load(reader);
            }

            Assert.Equal(1, this.IndexCount("c1", UnitColumn));
            Assert.Equal(1, this.IndexCount("c1", RoleColumn));
            Assert.Equal(1, this.IndexCount(RelationTable, "r"));

            using (var transaction = loaded.CreateTransaction())
            {
                Assert.Single(transaction.Extent(m.C1));
            }
        }

        // A load that fails initializes the database again, which ensures the indexes: a transaction opens.
        [Fact]
        public void ALoadThatFailsLeavesTheIndexes()
        {
            this.Rebuild();

            // A schema that the load has to build again, and a file that is no population.
            this.ExecuteScalar("DROP TABLE allors.c1");

            var database = this.CreateDatabase();
            using (var reader = XmlReader.Create(new StringReader("<allors/>")))
            {
                Assert.Throws<ArgumentException>(() => database.Load(reader));
            }

            Assert.Equal(1, this.IndexCount("c1", UnitColumn));
            Assert.Equal(1, this.IndexCount("c1", RoleColumn));
            Assert.Equal(1, this.IndexCount(RelationTable, "r"));

            using (database.CreateTransaction())
            {
            }
        }

        [Fact]
        public void DatabaseObjectsEnsureTheIndexesAtTheSameTime()
        {
            this.RebuildWithoutIndexes();

            var databases = Enumerable.Range(0, 4).Select(v => (Database)this.CreateDatabase()).ToArray();

            // Validated beforehand and at the same time, so that the pool holds a connection for every object:
            // they then read the indexes of the schema at the same moment and find them all missing.
            Assert.Empty(AtTheSameTime(databases, database => Assert.True(database.IsValid)));

            Assert.Empty(AtTheSameTime(databases, database => database.EnsureIndexes()));

            Assert.Equal(1, this.IndexCount("c1", UnitColumn));
            Assert.Equal(1, this.IndexCount("c1", RoleColumn));
            Assert.Equal(1, this.IndexCount(RelationTable, "r"));
        }

        // The exceptions of an action that every database object starts at the same moment, on a thread of its own.
        private static Exception[] AtTheSameTime(Database[] databases, Action<Database> action)
        {
            var exceptions = new ConcurrentQueue<Exception>();
            using (var barrier = new Barrier(databases.Length))
            {
                var threads = databases
                    .Select(database => new Thread(() =>
                    {
                        try
                        {
                            barrier.SignalAndWait();
                            action(database);
                        }
                        catch (Exception e)
                        {
                            exceptions.Enqueue(e);
                        }
                    }))
                    .ToArray();

                foreach (var thread in threads)
                {
                    thread.Start();
                }

                foreach (var thread in threads)
                {
                    thread.Join();
                }
            }

            return exceptions.ToArray();
        }

        // A schema that Init has just built: its tables are new, and have the indexes of the adapter and no other.
        protected Database Rebuild()
        {
            this.ExecuteScalar("DROP TABLE IF EXISTS allors.c1");
            var database = (Database)this.CreateDatabase();
            database.Init();
            return database;
        }

        // A schema that Init has just built, whose indexes were then dropped behind the adapter.
        private Database RebuildWithoutIndexes()
        {
            var database = this.Rebuild();
            this.DropIndexes();
            return database;
        }
    }
}
