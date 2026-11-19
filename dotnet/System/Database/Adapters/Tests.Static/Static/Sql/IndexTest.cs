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

    // The adapter ensures the indexes of its schema before the first transaction of a database object, when it is
    // asked to, and at the end of a load. The test domain marks C1.C1AllorsInteger, C1.Many2One and
    // Company.IndexedMany2ManyPersons as indexed, and not C1.C1AllorsString, C1.C1AllorsDouble and C1.C1C2one2one.
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

        protected abstract void WithIndexLock(Action action);

        [Fact]
        public void InitCreatesNoIndexAndTheFirstTransactionCreatesThem()
        {
            var database = this.Rebuild();

            Assert.Equal(0, this.IndexCount("c1", UnitColumn));
            Assert.Equal(0, this.IndexCount("c1", RoleColumn));
            Assert.Equal(0, this.IndexCount(RelationTable, "r"));

            using (database.CreateTransaction())
            {
            }

            Assert.Equal(1, this.IndexCount("c1", UnitColumn));
            Assert.Equal(1, this.IndexCount("c1", RoleColumn));
            Assert.Equal(1, this.IndexCount(RelationTable, "r"));
        }

        [Fact]
        public void EnsureIndexesCreatesThemWithoutATransaction()
        {
            var database = this.Rebuild();

            database.EnsureIndexes();

            Assert.Equal(1, this.IndexCount("c1", UnitColumn));
            Assert.Equal(1, this.IndexCount("c1", RoleColumn));
            Assert.Equal(1, this.IndexCount(RelationTable, "r"));
        }

        [Fact]
        public void AMissingIndexIsCreatedBeforeTheFirstTransaction()
        {
            this.Rebuild().EnsureIndexes();
            this.DropIndex("c1", RoleIndex);
            Assert.Equal(0, this.IndexCount("c1", RoleColumn));

            using (this.CreateDatabase().CreateTransaction())
            {
            }

            Assert.True(this.ExistIndex(RoleIndex));
            Assert.Equal(1, this.IndexCount("c1", RoleColumn));
        }

        [Fact]
        public void AnIndexOfAnotherNameCounts()
        {
            this.Rebuild().EnsureIndexes();
            this.DropIndex("c1", RoleIndex);
            this.ExecuteScalar($"CREATE INDEX made_by_hand ON allors.c1 ({RoleColumn})");

            using (this.CreateDatabase().CreateTransaction())
            {
            }

            Assert.False(this.ExistIndex(RoleIndex));
            Assert.Equal(1, this.IndexCount("c1", RoleColumn));
        }

        [Fact]
        public void AnIndexOfTheAdapterForAUnitRoleThatIsNotMarkedIsDropped()
        {
            this.Rebuild().EnsureIndexes();

            // The name that the adapter gives to an index for C1.C1AllorsString, and an index of somebody else
            // on the same column.
            this.ExecuteScalar("CREATE INDEX idx_c1_c1c1allorsstring ON allors.c1 (c1allorsstring)");
            this.ExecuteScalar("CREATE INDEX made_by_hand ON allors.c1 (c1allorsstring)");

            using (this.CreateDatabase().CreateTransaction())
            {
            }

            Assert.False(this.ExistIndex("idx_c1_c1c1allorsstring"));
            Assert.True(this.ExistIndex("made_by_hand"));
            Assert.Equal(1, this.IndexCount("c1", "c1allorsstring"));
        }

        [Fact]
        public void AnIndexThatCannotAcquireTheSchemaLockStays()
        {
            const string indexName = "idx_c1_c1c1allorsstring";
            this.Rebuild().EnsureIndexes();
            this.ExecuteScalar($"CREATE INDEX {indexName} ON allors.c1 (c1allorsstring)");

            var database = (Database)this.CreateDatabase();
            Assert.True(database.IsValid);

            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    using (database.CreateTransaction())
                    {
                    }
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

            using (this.CreateDatabase().CreateTransaction())
            {
            }

            Assert.False(this.ExistIndex(indexName));
        }

        [Fact]
        public void AnIndexOnACompositeRelationThatIsNotMarkedStays()
        {
            this.Rebuild().EnsureIndexes();

            // The name that the adapter gives to an index for C1.C1C2one2one.
            this.ExecuteScalar("CREATE INDEX idx_c1_c1c1c2one2one ON allors.c1 (c1c2one2one)");

            using (this.CreateDatabase().CreateTransaction())
            {
            }

            Assert.True(this.ExistIndex("idx_c1_c1c1c2one2one"));
        }

        [Fact]
        public void ANameThatIsTakenRefusesTheTransaction()
        {
            this.Rebuild();

            // The name of the index for C1.Many2One, on another column.
            this.ExecuteScalar($"CREATE INDEX {RoleIndex} ON allors.c1 (c1allorsdouble)");

            // The message names the index that is needed and the column of the index that has its name.
            var exception = Assert.ThrowsAny<Exception>(() => this.CreateDatabase().CreateTransaction());
            Assert.Contains(RoleIndex, exception.Message);
            Assert.Contains(RoleColumn, exception.Message);
            Assert.Contains("c1allorsdouble", exception.Message);

            this.DropIndex("c1", RoleIndex);

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
        public void TheFirstTransactionAfterInitEnsuresTheIndexesAgain()
        {
            var database = this.Rebuild();
            using (database.CreateTransaction())
            {
            }

            Assert.Equal(1, this.IndexCount("c1", RoleColumn));

            // Init builds the schema again, and with it tables without indexes.
            this.ExecuteScalar("DROP TABLE allors.c1");
            database.Init();
            Assert.Equal(0, this.IndexCount("c1", RoleColumn));

            using (database.CreateTransaction())
            {
            }

            Assert.Equal(1, this.IndexCount("c1", RoleColumn));
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

        [Fact]
        public void DatabaseObjectsEnsureTheIndexesAtTheSameTime()
        {
            this.Rebuild();

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

        // A schema that Init has just built: its tables are new, and so are without the indexes of an earlier fact.
        protected Database Rebuild()
        {
            this.ExecuteScalar("DROP TABLE IF EXISTS allors.c1");
            var database = (Database)this.CreateDatabase();
            database.Init();
            return database;
        }
    }
}
